using System;
using UnityEditor;
using UnityEngine;

namespace pfc.DomeTools
{
    [CustomEditor(typeof(InstallNDIPackage))]
    public class InstallNDIPackageEditor : Editor
    {
        // This is the prefrontal cortex fork with NDI Audio support.
        private const string KlakNdiForkVersion = "https://github.com/prefrontalcortex/KlakNDI.git?path=jp.keijiro.klak.ndi#c0169a634c3282e3a596180801c1191c6b3dc887";
        
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
    public class InstallSpoutPackageEditor : Editor
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
    
    [CustomEditor(typeof(InstallOSCJackPackage))]
    public class InstallOSCJackPackageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (ManifestUtility.CheckIfPackageAvailable("jp.keijiro.osc-jack"))
            {
                Utils.DrawCheck("OSC Jack (jp.keijiro.osc-jack) is installed.");
            }
            else
            {
                EditorGUILayout.HelpBox("Sending virtual audio object positions over ADM/OSC requires the OSC Jack package. Install it with the button below or using Package Manager.", MessageType.Info);
                if(GUILayout.Button("Install OSC Jack Package"))
                {
                    Install();
                }
            }
        }

        public static void Install()
        {
            if(!ManifestUtility.CheckIfScopedRegistryAvailable("jp.keijiro")) 
                ManifestUtility.AddScopedRegistry("Keijiro", "https://registry.npmjs.com", "jp.keijiro");
            ManifestUtility.AddPackage("jp.keijiro.osc-jack");
        }
    }

    internal class Utils
    {
        private static GUIStyle labelWithBreaks;
        public static bool DrawCheck(string label, bool check = true, Action fixAction = null, string fixTooltip = null, string fixLabel = "Fix")
        {            
            if (labelWithBreaks == null)
            {
                labelWithBreaks = new GUIStyle(EditorStyles.label);
                labelWithBreaks.wordWrap = true;
            }
            
            EditorGUILayout.BeginHorizontal();
            var content = new GUIContent(label);
            var needsFixButton = !check && fixAction != null;
            EditorGUILayout.LabelField(check ? "✓" : "✗", GUILayout.Width(20));
            EditorGUILayout.LabelField(content, labelWithBreaks);
            if (needsFixButton)
            {
                var fixContent = new GUIContent(fixLabel, fixTooltip);
                var size = EditorStyles.label.CalcSize(fixContent);
                if (GUILayout.Button(fixContent, GUILayout.Width(size.x + 12)))
                    fixAction();
            }
            EditorGUILayout.EndHorizontal();
            return check;
        }
    }
}
