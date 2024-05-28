using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
#if HAVE_NDI
using Klak.Ndi;
#endif

namespace pfc.Fulldome
{

    [CustomEditor(typeof(DomeTools))]
    public class DomeToolsEditor : Editor
    {
        private static class Styles
        {
            public static GUIStyle heading;
            public static GUIStyle miniLabelBreak;
            public static GUIStyle richTextLabel;

            static Styles()
            {
                heading = new GUIStyle(EditorStyles.largeLabel);
                heading.fontStyle = FontStyle.Bold;
                heading.fontSize = 32;
                heading.padding.bottom = -5;
                
                miniLabelBreak = new GUIStyle(EditorStyles.miniLabel);
                miniLabelBreak.wordWrap = true;
                
                richTextLabel = new GUIStyle(EditorStyles.label);
                richTextLabel.richText = true;
                richTextLabel.wordWrap = true;
            }
        }
        
        private static readonly GUIContent CameraSettings = new GUIContent("Camera Settings", "Adjust the rendering method (Dome Warp or Cubemap) and other camera settings.");
        private static readonly GUIContent DomeMasterSettings = new GUIContent("Dome Output Settings", "Adjust UI configuration for the Dome Master output image, like title, producer, logo and more.");
        private static readonly GUIContent NdiInstall = new GUIContent("Install NDI", "NDI is recommended for sending the Dome output over network to a dome (physical or virtual).");
        
        public override void OnInspectorGUI()
        {
            var t = target as DomeTools;
            if (!t) return;
            
            GUILayout.Label("Dome Tools", Styles.heading);
            GUILayout.Label("by prefrontal cortex", EditorStyles.label);
            EditorGUILayout.Space();
            GUILayout.Label("Use <b>Dome Creator</b> to produce real-time, interactive fulldome content. Stream it over the network with <b>NewTek NDI</b> and view your content in <b>Dome Viewer</b> and other NDI-enabled applications.", Styles.richTextLabel);
            if (EditorGUILayout.LinkButton("Learn more"))
                Application.OpenURL(DomeTools.DocumentationURL);
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            // Quick selection
            if (GUILayout.Button(CameraSettings))
            {
                Selection.activeGameObject = t.GetComponentInChildren<DomeRenderer>()?.gameObject;
            }
            if (GUILayout.Button(DomeMasterSettings))
            {
                Selection.activeGameObject = t.GetComponentInChildren<DomeMasterUI>()?.gameObject;
            }
#if HAVE_NDI
            if (GUILayout.Button("NDI Settings"))
            {
                Selection.activeGameObject = t.GetComponentInChildren<NdiSender>()?.gameObject;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Quick Options", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Once you start play mode, the Dome Master will be visible as NDI source in NDI-enabled applications across your local network, including Dome Viewer.", Styles.miniLabelBreak);
            
            EditorGUILayout.Space();
            if (!EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Enter Play Mode\nto start the NDI stream",GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    EditorApplication.isPlaying = true;
            }
            else
            {
                if (GUILayout.Button("Exit Play Mode\nto stop the NDI stream",GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    EditorApplication.isPlaying = false;
            }
#else
            if (GUILayout.Button(NdiInstall))
            {
                InstallNDIPackageInspector.Install();
            }
#endif
            
            // Check that we have
            var audioListenersInScene = FindObjectsOfType<AudioListener>();
            var ndiSenderInScene = FindFirstObjectByType<NdiSender>();
            
            // - the camera rig
            // - NDI package and NDI output
            // - Audio output
            // - a single audio listener (and it is on the NDI Sender)
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label(new GUIContent("Scene Checks", "Verify that your scene is set up to create interactive fulldome content."), EditorStyles.boldLabel);
#if HAVE_NDI
            Utils.DrawCheck("NDI Package is installed");
            Utils.DrawCheck("NDI Output is configured", ndiSenderInScene, () =>
            {
                var go = new GameObject("NDI Sender").AddComponent<NdiSender>();
                go.captureMethod = CaptureMethod.Texture;
                go.sourceTexture = t.GetComponentInChildren<DomeRenderer>().domeMasterTexture;
                go.ndiName = "Dome Master";
            }, "Create NDI Sender and have it send the Dome Master texture");
#else
            Utils.DrawCheck("NDI Package is not installed", false, InstallNDIPackageInspector.Install, "Installs KlakNDI (jp.keijiro.klak.ndi) for sending the dome output over network.");
#endif
            Utils.DrawCheck("Current Render Pipeline is supported"); // all supported 🎉
            Utils.DrawCheck("Dome Camera Rig is set up", t.GetComponentInChildren<DomeRenderer>());

            var haveOneAudioSource = audioListenersInScene.Length == 1;
            var audioListenerOnSender = ndiSenderInScene && ndiSenderInScene.GetComponent<AudioListener>();
            Utils.DrawCheck("Exactly one Audio Listener in scene", haveOneAudioSource);
#if HAVE_NDI
            Utils.DrawCheck("Audio Listener is on NDI Sender", audioListenerOnSender, () =>
            {
                foreach (var t1 in audioListenersInScene)
                    DestroyImmediate(t1);
                FindObjectOfType<NdiSender>().gameObject.AddComponent<AudioListener>();
            }, "Remove Audio Listener(s) and place an Audio Listener on the NDI Sender");
#endif
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (EditorGUILayout.LinkButton("Show Samples in Package Manager"))
                UnityEditor.PackageManager.UI.Window.Open("com.pfc.dome-tools");
            if (EditorGUILayout.LinkButton("Open the Documentation"))
                Application.OpenURL(DomeTools.DocumentationURL);
#if HAVE_NDI
            if (EditorGUILayout.LinkButton("Download NDI Tools"))
                Application.OpenURL("https://ndi.video/tools/download/");
#endif
        }
    }
}