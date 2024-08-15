using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
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
        
        public static bool CheckIfScopedRegistryAvailable(string url)
        {
            var path = GetPathToManifest();
            if (path == null) return false;
            
            //this is slow and should probably be cached
            var manifest = File.ReadAllText(path);
            return manifest.Contains($"\"{url}\"");
        }
        
        public static void AddScopedRegistry(string name, string url, string scope)
        {
            var path = GetPathToManifest();
            if (path == null) return;
            
            var manifest = File.ReadAllText(path);
            if (manifest.Contains($"\"{url}\""))
            {
                Debug.LogWarning($"Scoped registry {url} already exists in manifest.json");
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
            UnityEditor.PackageManager.Client.Resolve();
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