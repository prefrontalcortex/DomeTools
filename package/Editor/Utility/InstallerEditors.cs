using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
{
    [CustomEditor(typeof(InstallNDIPackage))]
    public class InstallNDIPackageInspector : Editor
    {
        public override void OnInspectorGUI()
        {
#if HAVE_NDI
            Utils.DrawCheck("NDI (jp.keijiro.klak.ndi) is installed.");
#else
            EditorGUILayout.HelpBox("Sending the dome texture over the network requires the NDI package. Install it with the button below or using Package Manager.", MessageType.Info);
            if (GUILayout.Button("Install NDI Package"))
            {
                if(!ManifestUtility.CheckIfScopedRegistryAvailable("org.nuget.system")) 
                    ManifestUtility.AddScopedRegistry("OpenUPM","https://package.openupm.com","org.nuget.system");
                if(!ManifestUtility.CheckIfScopedRegistryAvailable("jp.keijiro")) 
                    ManifestUtility.AddScopedRegistry("Keijiro", "https://registry.npmjs.com", "jp.keijiro");
                ManifestUtility.AddPackage("jp.keijiro.klak.ndi");
            }
#endif
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
                    if(!ManifestUtility.CheckIfScopedRegistryAvailable("org.nuget.system")) 
                        ManifestUtility.AddScopedRegistry("OpenUPM","https://package.openupm.com","org.nuget.system");
                    if(!ManifestUtility.CheckIfScopedRegistryAvailable("jp.keijiro")) 
                        ManifestUtility.AddScopedRegistry("Keijiro", "https://registry.npmjs.com", "jp.keijiro");
                    ManifestUtility.AddPackage("jp.keijiro.klak.spout");
                }
            }
        }
    }

    internal class Utils
    {
        public static void DrawCheck(string label)
        {            
            var rect = EditorGUILayout.GetControlRect();
            rect.width = 20;
            rect.height = 20;
            EditorGUI.LabelField(rect, "✓");
            rect.x += 20;
            rect.width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 20;
            EditorGUI.LabelField(rect, label);
        }
    }
}
