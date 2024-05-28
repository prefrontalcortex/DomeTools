using System;
using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
{
    [CustomEditor(typeof(InstallNDIPackage))]
    public class InstallNDIPackageInspector : Editor
    {
        // This is the prefrontal cortex fork with NDI Audio support.
        private const string KlakNdiForkVersion = "https://github.com/prefrontalcortex/KlakNDI.git?path=jp.keijiro.klak.ndi#8f8e7f9120f2b0334b7fd5430b6101ff6538c2e7";
        
        public override void OnInspectorGUI()
        {
#if HAVE_NDI
            Utils.DrawCheck("NDI (jp.keijiro.klak.ndi) is installed.");
#else
            EditorGUILayout.HelpBox("Sending the dome texture over the network requires the NDI package. Install it with the button below or using Package Manager.", MessageType.Info);
            if (GUILayout.Button("Install NDI Package"))
            {
                Install();
            }
#endif
        }

        public static void Install()
        {
            if(!ManifestUtility.CheckIfScopedRegistryAvailable("org.nuget.system")) 
                ManifestUtility.AddScopedRegistry("OpenUPM","https://package.openupm.com","org.nuget.system");
            if(!ManifestUtility.CheckIfScopedRegistryAvailable("jp.keijiro")) 
                ManifestUtility.AddScopedRegistry("Keijiro", "https://registry.npmjs.com", "jp.keijiro");
            ManifestUtility.AddPackage("jp.keijiro.klak.ndi", KlakNdiForkVersion);
        }
    }
    
    [CustomEditor(typeof(InstallSpoutPackage))]
    public class InstallSpoutPackageInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (ManifestUtility.CheckIfPackageAvailable("jp.keijiro.klak.spout"))
            {
                Utils.DrawCheck("Spout (jp.keijiro.klak.spout) is installed.");
            }
            else
            {
                EditorGUILayout.HelpBox("Sending the dome texture to other software on the same computer requires the Spout package. Install it with the button below or using Package Manager.", MessageType.Info);
                if(GUILayout.Button("Install Spout Package"))
                {
                    Install();
                }
            }
        }

        public static void Install()
        {
            if(!ManifestUtility.CheckIfScopedRegistryAvailable("org.nuget.system")) 
                ManifestUtility.AddScopedRegistry("OpenUPM","https://package.openupm.com","org.nuget.system");
            if(!ManifestUtility.CheckIfScopedRegistryAvailable("jp.keijiro")) 
                ManifestUtility.AddScopedRegistry("Keijiro", "https://registry.npmjs.com", "jp.keijiro");
            ManifestUtility.AddPackage("jp.keijiro.klak.spout");
        }
    }

    internal class Utils
    {
        public static void DrawCheck(string label, bool check = true, Action fixAction = null, string fixTooltip = null)
        {            
            var rect = EditorGUILayout.GetControlRect();
            var w = rect.width;
            rect.width = 20;
            rect.height = 20;
            var needsFixButton = !check && fixAction != null;
            EditorGUI.LabelField(rect, check ? "✓" : "✗");
            rect.x += 20;
            rect.width = w - 20;
            if (needsFixButton)
                rect.width -= 50;
            EditorGUI.LabelField(rect, label);
            if (needsFixButton)
            {
                rect.x += rect.width;
                rect.width = 50;
                if (GUI.Button(rect, new GUIContent("Fix", fixTooltip)))
                    fixAction();
            }
        }
    }
}
