using System;
using System.Collections.Generic;
using System.Linq;
using OscJack;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
#if HAVE_NDI
using Klak.Ndi;
#endif
#if HAVE_OSCJACK
// using OscJack;
#endif

namespace pfc.DomeTools
{
    [CustomEditor(typeof(DomeTools))]
    public class DomeToolsEditor : Editor
    {
        private static class Styles
        {
            public static GUIStyle heading;
            public static GUIStyle miniLabelBreak;
            public static GUIStyle richTextLabel;
            public static GUIStyle linkButton;

            static Styles()
            {
                heading = new GUIStyle(EditorStyles.largeLabel);
                heading.fontStyle = FontStyle.Bold;
                heading.fontSize = 32;
                heading.padding.bottom = -5;
                
                miniLabelBreak = new GUIStyle(EditorStyles.miniLabel);
                miniLabelBreak.wordWrap = true;
                miniLabelBreak.richText = true;
                
                richTextLabel = new GUIStyle(EditorStyles.label);
                richTextLabel.richText = true;
                richTextLabel.wordWrap = true;
                
                linkButton = new GUIStyle(EditorStyles.linkLabel);
            }
        }
        
        private static readonly GUIContent CameraSettings = new GUIContent("Camera Settings", "Adjust the rendering method (Dome Warp or Cubemap) and other camera settings.");
        private static readonly GUIContent DomeMasterSettings = new GUIContent("Dome Output Settings", "Adjust UI configuration for the Dome Master output image, like title, producer, logo and more.");
        private static readonly GUIContent NdiInstall = new GUIContent("Install NDI", "NDI is recommended for sending the Dome output over network to a dome (physical or virtual).");

        private void OnEnable()
        {
            DomeRendererEditor.CollectGameViews(gameViews);
        }

        private List<EditorWindow> gameViews = new List<EditorWindow>();
        
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
            
            // - the camera rig
            // - NDI package and NDI output
            // - Audio output
            // - a single audio listener (and it is on the NDI Sender)
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label(new GUIContent("Scene Checks", "Verify that your scene is set up to create interactive fulldome content."), EditorStyles.boldLabel);
            GUILayout.Label("The sections below will help you verify that your scene is set up correctly for Dome Master rendering, NDI video output, and NDI audio.", Styles.miniLabelBreak);
            EditorGUILayout.Space();
            GUILayout.Label("Dome Master Rendering".ToUpper(), EditorStyles.miniBoldLabel);
            var haveDomeRenderer = t.GetComponentInChildren<DomeRenderer>();
            var warningIsOff = gameViews.All(DomeRendererEditor.GameViewHidesNoCameraWarning);
            if (haveDomeRenderer && warningIsOff)
            {
                Utils.DrawCheck("Dome Creator is fully configured.");
            }
            else 
            {
                Utils.DrawCheck("Dome Camera Rig is set up", haveDomeRenderer);
                Utils.DrawCheck("Current Render Pipeline is supported"); // all supported 🎉
                Utils.DrawCheck("\"Warn if no cameras rendering\" is disabled", warningIsOff, () => gameViews.ForEach(DomeRendererEditor.HideGameViewNoCameraRenderingWarning));
            }
            
            EditorGUILayout.Space();
            GUILayout.Label("NDI Video Output".ToUpper(), EditorStyles.miniBoldLabel);
#if HAVE_NDI
            var ndiSenderInScene = FindFirstObjectByType<NdiSender>();
            var ndiFullySetUp = ndiSenderInScene && ndiSenderInScene.sourceTexture;
            if (ndiFullySetUp)
            {
                var resolution = ndiSenderInScene.sourceTexture.width + "x" + ndiSenderInScene.sourceTexture.height;
                var senderName = ndiSenderInScene.ndiName;
                Utils.DrawCheck($"NDI Video Output is fully configured. You're sending with a resolution of {resolution} and the NDI stream is named \"{senderName}\".");
            }
            else
            {
                Utils.DrawCheck("NDI Package is installed");
                Utils.DrawCheck("NDI Sender is configured", ndiSenderInScene, () =>
                {
                    var go = new GameObject("NDI Sender").AddComponent<NdiSender>();
                    go.captureMethod = CaptureMethod.Texture;
                    go.sourceTexture = t.GetComponentInChildren<DomeRenderer>().domeMasterTexture;
                    go.ndiName = "Dome Master";
                }, "Create NDI Sender and have it send the Dome Master texture");    
            }
#else
            ndiFullySetUp |= Utils.DrawCheck("NDI Package is not installed", false, InstallNDIPackageInspector.Install, "Installs KlakNDI (jp.keijiro.klak.ndi) for sending the dome output over network.");
#endif

            EditorGUILayout.Space();
            GUILayout.Label("NDI Audio Output".ToUpper(), EditorStyles.miniBoldLabel);
#if HAVE_NDI
            var haveOneAudioSource = audioListenersInScene.Length == 1;
            var audioListenerOnSender = ndiSenderInScene && ndiSenderInScene.GetComponent<AudioListener>();
            // check project settings
            var AudioSpatializerExpectedName = "Dummy Spatializer (NDI)";
            var haveCustomSpatializer = AudioSettings.GetSpatializerPluginName() == AudioSpatializerExpectedName;
            var audioFullySetUp = haveOneAudioSource && audioListenerOnSender && haveCustomSpatializer;
            
            if (audioFullySetUp)
            {
                var audioSetupType = ndiSenderInScene.audioMode.ToString();
                Utils.DrawCheck($"NDI Audio Output is fully configured. You're sending {audioSetupType}.");
            }
            else {
                Utils.DrawCheck("Exactly one Audio Listener in scene", haveOneAudioSource);
                Utils.DrawCheck("Audio Listener is on NDI Sender", audioListenerOnSender, () =>
                {
                    foreach (var t1 in audioListenersInScene)
                        DestroyImmediate(t1);
                    FindObjectOfType<NdiSender>().gameObject.AddComponent<AudioListener>();
                }, "Remove Audio Listener(s) and place an Audio Listener on the NDI Sender");
                Utils.DrawCheck($"Audio Spatializer is set to \"{AudioSpatializerExpectedName}\"", haveCustomSpatializer, () =>
                {
                    AudioSettings.SetSpatializerPluginName(AudioSpatializerExpectedName);
                    if (AudioSettings.GetSpatializerPluginName() != AudioSpatializerExpectedName)
                    {
                        Debug.LogError($"Failed to set the Audio Spatializer to \"{AudioSpatializerExpectedName}\". Please set it manually in Project Settings > Audio.");
                    }
                });
            }
#endif
            
            EditorGUILayout.Space();
            GUILayout.Label("ADM Object-Based Audio Output (optional)".ToUpper(), EditorStyles.miniBoldLabel);
            
            /*
            GUILayout.Label(
                "Audio can be streamed either from virtual microphones (for example, a virtual 7.1 recording setup), " +
                "or as object-based audio where each source becomes its own channel. " +
                "Audio source positions are then sent over OSC with the ADM protocol.", Styles.miniLabelBreak);
            "Applications supporting object-based audio and ADM include", Styles.miniLabelBreak);
            EditorGUI.indentLevel++;
            if (EditorGUILayout.LinkButton("Spatial Audio Processor by New Audio Technology"))
                Application.OpenURL("https://www.newaudiotechnology.com/products/spatial-audio-designer-processor/");
            if (EditorGUILayout.LinkButton("L-ISA by L-Acoustics"))
                Application.OpenURL("https://l-isa.l-acoustics.com/");
            EditorGUI.indentLevel--;
            */

#if HAVE_OSCJACK
            var oscSenderInScene = FindFirstObjectByType<AdmOscSender>();
            var hasConfig = oscSenderInScene && oscSenderInScene._connection;
            var audioModeIsObjectBased = ndiSenderInScene && ndiSenderInScene.audioMode == NdiSender.AudioMode.ObjectBased;
            var admFullySetUp = oscSenderInScene && hasConfig && audioModeIsObjectBased;
            
            if (admFullySetUp)
            {
                var hostAndPort = (oscSenderInScene && oscSenderInScene._connection) ? oscSenderInScene._connection.host + ":" + oscSenderInScene._connection.port : "<none>";
                Utils.DrawCheck($"Object-Based Audio Output is fully configured. You're sending ADM over OSC to {hostAndPort}");
            }
            else {
                
                Utils.DrawCheck("Virtual Audio Mode is set to \"Object Based\"", audioModeIsObjectBased, () =>
                {
                    ndiSenderInScene.audioMode = NdiSender.AudioMode.ObjectBased;
                    EditorUtility.SetDirty(ndiSenderInScene);
                }, "Set Virtual Audio Mode to \"Object Based\"", "Change");
                Utils.DrawCheck("OSCJack Package is installed");
                Utils.DrawCheck("ADM OSC Sender in scene", oscSenderInScene, () =>
                {
                    new GameObject("ADM OSC Sender for object-based audio").AddComponent<AdmOscSender>();
                    Undo.RegisterCreatedObjectUndo(t, "Create ADM OSC Sender");
                });
                if (oscSenderInScene)
                {
                    Utils.DrawCheck("ADM OSC Sender has configuration", hasConfig, () =>
                    {
                        var config = CreateInstance<OscConnection>();
                        AssetDatabase.CreateAsset(config, "Assets/ADM OSC Sender Configuration.asset");
                        Undo.RegisterCreatedObjectUndo(config, "Create OSC Connection Configuration Asset");
                        Undo.RecordObject(oscSenderInScene, "Assign OSC Connection Configuration");
                        oscSenderInScene._connection = config;
                        EditorUtility.SetDirty(oscSenderInScene);
                    }, "Create and assign OSC Connection Configuration");
                }
            }
#else
            admFullySetUp |=  Utils.DrawCheck("OSCJack Package is not installed", false, InstallOSCJackPackageInspector.Install, "Installs OSCJack (jp.keijiro.osc-jack) for object-based audio ADM support.");
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
