using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using UnityEngine;

namespace UnityEditor.XR.Management
{
    internal static class EditorUtilities
    {
        internal static readonly string[] s_DefaultGeneralSettingsPath = {"XR"};
        internal static readonly string[] s_DefaultLoaderPath = {"XR","Loaders"};
        internal static readonly string[] s_DefaultSettingsPath = {"XR","Settings"};

        internal static bool AssetDatabaseHasInstanceOfType(string type)
        {
            var assets = AssetDatabase.FindAssets(String.Format("t:{0}", type));
            return assets.Any();
        }

        internal static T GetInstanceOfTypeFromAssetDatabase<T>() where T : class
        {
            var assets = AssetDatabase.FindAssets(String.Format("t:{0}", typeof(T).Name));
            if (assets.Any())
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
                return asset as T;
            }
            return null;
        }

        internal static ScriptableObject GetInstanceOfTypeWithNameFromAssetDatabase(string typeName)
        {
            var assets = AssetDatabase.FindAssets(String.Format("t:{0}", typeName));
            if (assets.Any())
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject));
                return asset as ScriptableObject;
            }
            return null;
        }

        internal static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach( var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), true) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }

        internal static string TypeNameToString(Type type)
        {
            return type == null ? "" : TypeNameToString(type.FullName);
        }

        internal static string TypeNameToString(string type)
        {
            string[] typeParts = type.Split(new char[] { '.' });
            if (!typeParts.Any())
                return String.Empty;

            for (int i = 0; i < typeParts.Length; ++i)
            {
                typeParts[i] = typeParts[i].TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            }

            string[] words = Regex.Matches(typeParts.Last(), "[\\w\\d]+")
                .OfType<Match>()
                .Select(m => m.Value)
                .ToArray();
            return string.Join("", words);
        }

        internal static ScriptableObject CreateScriptableObjectInstance(string typeName, string path)
        {
            ScriptableObject obj = ScriptableObject.CreateInstance(typeName) as ScriptableObject;
            if (obj != null)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string fileName = String.Format("{0}.asset", EditorUtilities.TypeNameToString(typeName));
                    string targetPath = Path.Combine(path, fileName);
                    AssetDatabase.CreateAsset(obj, targetPath);
                    AssetDatabase.SaveAssets();
                    return obj;
                }
            }

            Debug.LogError($"We were unable to create an instance of the requested type {typeName}. Please make sure that all packages are updated to support this version of XR Plug-In Management. See the Unity documentation for XR Plug-In Management for information on resolving this issue.");
            return null;
        }

        /// <summary>
        /// Retrieves the path of the XR Management package's Android manifest XML template.
        /// </summary>
        /// <param name="packageName">Package name in Java convention (eg com.package.module).</param>
        /// <returns><see cref="string"/> of the XML file path.</returns>
        internal static string GetPackagePath(string packageName)
        {
#if UNITY_2021_3_OR_NEWER
            var xrManagementPackageInfo = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                .Where(package => package.name == packageName)
                .First();
            return xrManagementPackageInfo.resolvedPath;
#else
            throw new System.NotSupportedException("XR Management package couldn't be found, please make sure you have the XR Management package added to your project through the Package Management window.");
#endif // UNITY_2021_3_OR_NEWER
        }
    }
}
