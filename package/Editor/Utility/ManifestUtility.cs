using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace pfc.DomeTools
{
    public class ManifestUtility
    {
        public static string GetPathToManifest()
        {
            var path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Packages/manifest.json";
            if (!File.Exists(path))
            {
                Debug.LogError("manifest.json not found");
                return null;
            }
            return path;
        }
        
        static Dictionary<string,bool> packageAvailabilityCache = new Dictionary<string, bool>();
        public static bool CheckIfPackageAvailable(string package)
        {
            var path = GetPathToManifest();
            if (path == null) return false;

            if (packageAvailabilityCache.TryGetValue(package, out var available)) return available;
            //adding new packages should trigger domain reloads and clear the cache
            
            //this is slow and should probably be cached
            var manifest = File.ReadAllText(path);
            var startIndex = manifest.IndexOf("\"dependencies\": {");
            manifest = manifest.Substring(startIndex);
            var endIndex = manifest.IndexOf("}");
            manifest = manifest.Substring(0, endIndex);
            packageAvailabilityCache[package] = manifest.Contains($"\"{package}\"");
            return manifest.Contains($"\"{package}\"");
        }
        
        public static void AddPackage(string package, string version = null)
        {
            UnityEditor.PackageManager.Client.Add(package + (string.IsNullOrWhiteSpace(version) ? "" : "@" + version));
        }

        public static void AddOrReplacePackage(string package, string version, string packageToReplace)
        {
            var path = GetPathToManifest();
            if (path == null) return;

            var manifest = File.ReadAllText(path);
            var targetDependency = $"\"{package}\": \"{version}\"";
            var targetPattern = new Regex($"\"{Regex.Escape(package)}\"\\s*:\\s*\"[^\"]*\"");
            var replacementPattern = new Regex($"\"{Regex.Escape(packageToReplace)}\"\\s*:\\s*\"[^\"]*\"");

            if (targetPattern.IsMatch(manifest))
            {
                manifest = targetPattern.Replace(manifest, targetDependency, 1);
                File.WriteAllText(path, manifest);
                UnityEditor.PackageManager.Client.Resolve();
                return;
            }

            if (replacementPattern.IsMatch(manifest))
            {
                manifest = replacementPattern.Replace(manifest, targetDependency, 1);
                File.WriteAllText(path, manifest);
                UnityEditor.PackageManager.Client.Resolve();
                return;
            }

            AddPackage(package, version);
        }
        
        public static bool CheckIfScopedRegistryAvailable(string url)
        {
            var path = GetPathToManifest();
            if (path == null) return false;
            
            //this is slow and should probably be cached
            var manifest = File.ReadAllText(path);
            return manifest.Contains($"\"{url}\"");
        }
        
        public static void AddScopedRegistry(string name, string url, string scope, bool resolve = true)
        {
            var path = GetPathToManifest();
            if (path == null) return;
            
            var manifest = File.ReadAllText(path);
            if (manifest.Contains($"\"{url}\""))
            {
                if (TryAddScopeToRegistry(ref manifest, url, scope))
                {
                    File.WriteAllText(path, manifest);
                    if (resolve) UnityEditor.PackageManager.Client.Resolve();
                }
                else if (!manifest.Contains($"\"{scope}\""))
                {
                    Debug.LogError($"Could not add scope {scope} to the existing registry {url} in manifest.json.");
                }
                return;
            }

            if (!manifest.Contains("\"scopedRegistries\": ["))
            {
                int startIndex = manifest.LastIndexOf("}");
                manifest = manifest.Insert(startIndex, ",\n"+"\"scopedRegistries\": [\n\n  ]\n");
            }
            
            var lines = manifest.Split('\n');
            var newLines = new string[lines.Length + 1];
            var hasPreRegisteredRegistries = CheckCountOfScopedRegistries(manifest) > 0;
            for (var i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
                if (lines[i].Contains("\"scopedRegistries\": ["))
                {
                    //check if we need ',' at the end of the last line

                    newLines[i + 1] = $"    {{\n      \"name\": \"{name}\",\n      \"url\": \"{url}\",\n      \"scopes\": [ \"{scope}\" ]\n    }}"
                                      +(hasPreRegisteredRegistries?",":"");
                    newLines[i + 1] += "\n"+lines[i + 1];
                    i++;
                }
                
            }
            File.WriteAllText(path, string.Join("\n", newLines));
            if (resolve) UnityEditor.PackageManager.Client.Resolve();
        }

        internal static bool TryAddScopeToRegistry(ref string manifest, string url, string scope)
        {
            var urlIndex = manifest.IndexOf($"\"{url}\"", System.StringComparison.Ordinal);
            if (urlIndex < 0) return false;

            var registryStart = manifest.LastIndexOf('{', urlIndex);
            var registryEnd = manifest.IndexOf('}', urlIndex);
            if (registryStart < 0 || registryEnd < 0) return false;

            var scopesIndex = manifest.IndexOf("\"scopes\"", registryStart, registryEnd - registryStart, System.StringComparison.Ordinal);
            if (scopesIndex < 0) return false;

            var scopesStart = manifest.IndexOf('[', scopesIndex);
            var scopesEnd = manifest.IndexOf(']', scopesStart);
            if (scopesStart < 0 || scopesEnd < 0 || scopesEnd > registryEnd) return false;

            var scopes = manifest.Substring(scopesStart + 1, scopesEnd - scopesStart - 1);
            if (scopes.Contains($"\"{scope}\"")) return false;

            var trimmedScopes = scopes.TrimEnd();
            var hasScopes = !string.IsNullOrWhiteSpace(trimmedScopes);
            var separator = hasScopes ? "," : string.Empty;
            var insertionIndex = scopesStart + 1 + trimmedScopes.Length;
            manifest = manifest.Insert(insertionIndex, $"{separator}\n        \"{scope}\"");
            return true;
        }

        static int CheckCountOfScopedRegistries(string manifest)
        {
            var startIndex = manifest.IndexOf("\"scopedRegistries\": [");
            var endIndex = manifest.IndexOf("]", startIndex);
            var scopedRegistries = manifest.Substring(startIndex, endIndex - startIndex);
            var lines = scopedRegistries.Split('\n');
            var registryCount = 0;
            foreach (var line in lines)
            {
                if (line.Contains("\"name\":")) registryCount++;
            }
            return registryCount;
        }
    }
}
