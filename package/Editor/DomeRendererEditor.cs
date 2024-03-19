using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
#if HAVE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace pfc.Fulldome
{
    [CustomEditor(typeof(DomeRenderer))]
    public class DomeRendererEditor : UnityEditor.Editor
    {
        private List<EditorWindow> gameViews = new List<EditorWindow>();
        private List<Camera> cams = new List<Camera>();
        private void OnEnable()
        {
            gameViews.Clear();
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var w in allWindows)
            {
                if (!w) continue;
                if (w.GetType().Name != "GameView") continue;
                gameViews.Add(w);
            }

            cams.Clear();
            cams.AddRange(FindObjectsByType<Camera>(FindObjectsSortMode.None));
        }
        
        private List<Camera> notAllowedCams = new List<Camera>();

        public override void OnInspectorGUI()
        {
            // Draw enum to switch between different modes
            var t = (DomeRenderer) target;
            
            // Error checking.
            
            notAllowedCams.Clear();
            // Check how many cameras are rendering. This has a performance impact.
            foreach (var cam in cams)
            {
                // Check if the camera is rendering
                if (!cam) continue;
                if (!cam.isActiveAndEnabled) continue;
                // Check if it's one of ours
                // Orthographic camera with UI layer is allowed
                if (cam.orthographic && cam.cullingMask == 1 << 5) continue;
                // Has ShiftedPerspective or RealtimeCubemap
                if (cam.GetComponent<ShiftedPerspective>() || cam.GetComponent<RealtimeCubemap>()) continue;
                notAllowedCams.Add(cam);
            }
            
            // Check if "Warn if No Cameras are rendering" is disabled
            foreach (var w in gameViews)
            {
                if (!w) continue;
                if (w.GetType().Name != "GameView") continue;
                var warnProp = w.GetType().GetField("m_NoCameraWarning", (BindingFlags)(-1));
                if (warnProp == null) continue;
                var warnValue = (bool) warnProp.GetValue(w);
                if (warnValue)
                {
                    EditorGUILayout.HelpBox("Please disable \"Warn if No Cameras Rendering\" in the Game View.", MessageType.Warning);
                    if (GUILayout.Button("Fix"))
                    {
                        warnProp.SetValue(w, false);
                        EditorUtility.SetDirty(w);
                        RepaintForSure();
                    }
                }
            }
            
            // Check if in HDRP
            // If yes, ensure we have
            // - Project Settings > HDRP Default Settings > Custom Post Process Orders > After Post Process > Dome Warp Volume
            // - A Global Volume with a Dome Warp Volume component
#if UNITY_2021_3_OR_NEWER
#if HAVE_HDRP && false
            if (GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset)
            {
                // Check that DomeWarpVolume is in the custom postprocess passes list
                var rpAsset = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
                var customPasses = rpAsset.GetType().GetField("afterPostProcessCustomPostProcesses", (BindingFlags)(-1));
                var stringArray = customPasses?.GetValue(rpAsset) as List<string>;
                var expectedTypeName = "DomeWarpVolume, pfc.Fulldome.HDRP, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                if (stringArray != null && !stringArray.Contains(expectedTypeName))
                {
                    EditorGUILayout.HelpBox("Custom Postprocess Passes don't contain DomeWarpVolume", MessageType.Warning);
                    if (GUILayout.Button("Fix"))
                    {
                        stringArray.Add(expectedTypeName);
                        customPasses.SetValue(rpAsset, stringArray);
                        EditorUtility.SetDirty(rpAsset);
                        RepaintForSure();
                    }
                }
                
                /*
                // Check that we have a global volume that contains a DomeWarpVolume
                var volume = FindObjectsByType<Volume>(FindObjectsSortMode.None);
                var foundVolume = false;
                foreach (var vol in volume)
                {
                    if (!vol || !vol.sharedProfile) continue;
                    var effects = vol.sharedProfile.components;
                    if (effects == null) continue;
                    foreach (var effect in effects)
                    {
                        if (effect.GetType().AssemblyQualifiedName == expectedTypeName)
                        {
                            foundVolume = true;
                            break;
                        }
                    }
                    if (foundVolume) break;
                }

                if (!foundVolume)
                {
                    EditorGUILayout.HelpBox("Scene doesn't contain a Global Volume with a DomeWarpVolume", MessageType.Warning);
                    if (GUILayout.Button("Fix"))
                    {
                        var go = new GameObject("Global Volume");
                        var vol = go.AddComponent<Volume>();
                        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath("1fa14c9ffc8e8ad478ba19e7b07dbab4"));
                        vol.sharedProfile = profile;
                        EditorUtility.SetDirty(vol);
                        RepaintForSure();
                    }
                }
                */
            }
#endif
#endif
            if (!t.domeWarping && !t.cubemapRendering)
            {
                EditorGUILayout.HelpBox("No cameras assigned", MessageType.Warning);
                return;
            }

            var selectedIndex = t.domeWarping && t.domeWarping.gameObject.activeSelf ? 0 : 1;
            var newIndex = EditorGUILayout.Popup(new GUIContent("Mode"), selectedIndex, new[]
            {
                new GUIContent("Dome Warping"),
                new GUIContent("Cubemap Rendering"),
            });
            
            if (selectedIndex != newIndex)
            {
                Undo.RegisterFullObjectHierarchyUndo(t, "Change Dome Renderer Mode");
                t.SetMode(newIndex == 0);
                EditorUtility.SetDirty(t.domeWarping);
                EditorUtility.SetDirty(t.cubemapRendering);
                RepaintForSure();
            }

            EditorGUI.indentLevel++;
            // Pull in the respective inspectors
            if (t.domeWarping && t.domeWarping.gameObject.activeSelf)
            {
                DrawProperty(
                    t.domeWarping.GetComponent<DomeWarp>(), 
                    ref _domeWarpingSerializedObject, 
                    DomeWarp.ExposedProperties);
            }
            else if (t.cubemapRendering && t.cubemapRendering.gameObject.activeSelf)
            {
                DrawProperty(
                    t.cubemapRendering.GetComponent<RealtimeCubemap>(), 
                    ref _cubemapRenderingSerializedObject, 
                    RealtimeCubemap.ExposedProperties);
            }
            EditorGUI.indentLevel--;

            EditorGUI.BeginChangeCheck();
            var domeContentRT = serializedObject.FindProperty(nameof(DomeRenderer.domeContentTexture));
            EditorGUILayout.PropertyField(domeContentRT);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                t.SetContentRenderTexture(domeContentRT.objectReferenceValue as RenderTexture);
                RepaintForSure();
            }
            
            EditorGUI.BeginChangeCheck();
            var domeMasterRT = serializedObject.FindProperty(nameof(DomeRenderer.domeMasterTexture));
            /*
            // UI for this texture only makes sense if size can be adjusted individually 
            EditorGUILayout.PropertyField(domeMasterRT);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                t.SetOutputRenderTexture(domeMasterRT.objectReferenceValue as RenderTexture);
                RepaintForSure();
            }
            */
            
            EditorGUI.indentLevel++;
            if (domeContentRT.objectReferenceValue)
            {
                EditorGUI.BeginChangeCheck();
                var rt = (RenderTexture) domeContentRT.objectReferenceValue;
                var rt2 = (RenderTexture) domeMasterRT.objectReferenceValue;
                
                var canBeEdited = true;
                if (AssetDatabase.Contains(rt)) {
                    var path = AssetDatabase.GetAssetPath(rt);
                    var package = PackageInfo.FindForAssetPath(path);
                    if (package != null)
                    {
                        if (package.source != PackageSource.Embedded && package.source != PackageSource.Local)
                        {
                            canBeEdited = false;
                        }
                    }
                }

                // for debugging
                // canBeEdited = false;
                
                if (!canBeEdited)
                {
                    EditorGUILayout.HelpBox("This RenderTexture can't be edited since it's part of an immutable package.", MessageType.Info);
                    if (GUILayout.Button("Create a copy for editing"))
                    {
                        var copy = Instantiate(rt);
                        AssetDatabase.CreateAsset(copy, "Assets/Dome Content Texture.asset");
                        t.domeContentTexture = copy;
                        
                        var copy2 = Instantiate(rt2);
                        AssetDatabase.CreateAsset(copy2, "Assets/Dome Output Texture.asset");
                        t.domeMasterTexture = copy2;
                        
                        t.SetContentRenderTexture(copy);
                        t.SetOutputRenderTexture(copy2);
                    }
                }
                EditorGUI.BeginDisabledGroup(!canBeEdited);
                var size = Mathf.Min(rt.width, rt.height);
                var newSize = EditorGUILayout.IntPopup("Size", size, new[] { "128", "256", "512", "1024", "2048", "4096", "8192" }, new[] { 128, 256, 512, 1024, 2048, 4096, 8192 });
                if (EditorGUI.EndChangeCheck())
                {
                    rt.Release();
                    rt.width = newSize;
                    rt.height = newSize;
                    rt.Create();
                    if (rt2)
                    {
                        rt2.Release();
                        rt2.width = newSize;
                        rt2.height = newSize;
                        rt2.Create();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;
            
            if (notAllowedCams.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("There are additional cameras rendering in your scene, make sure this is intended.\n" + string.Join("\n", notAllowedCams), MessageType.Info);
            }
        }

        private void OnSceneGUI()
        {
            var t = (DomeRenderer) target;
            var tr = t.transform;
            Handles.DrawWireArc(tr.position, tr.up, tr.forward, 360, 0.2f * tr.lossyScale.x);
            
            // draw horizontal disc slices
            // var slices = 8;
            // for (var i = 0; i <= slices; i++)
            // {
            //     var normalizedSlicePosition = (float) (i / (float) slices);   
            //     var slicePosition = tr.position + tr.up * normalizedSlicePosition;
            //     var radiusAtSlicePosition = Mathf.Sqrt(1 - normalizedSlicePosition * normalizedSlicePosition);
            //     Handles.DrawWireArc(slicePosition, tr.up, tr.forward, 360, radiusAtSlicePosition);
            // }

            var cam = (t.cubemapRendering.gameObject.activeSelf ? t.cubemapRendering as MonoBehaviour : t.domeWarping as MonoBehaviour).GetComponent<Camera>();
            var trp = cam.transform.position;
            Handles.DrawDottedLine(tr.position, trp, 5f);
            
            if (t.domeWarping.gameObject.activeSelf)
            {
                if(!cam) return;
			
                Vector3[] frustumCorners = new Vector3[4];
                cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

                for (int i = 0; i < 4; i++)
                {
                    var worldSpaceCorner = cam.transform.TransformVector(frustumCorners[i]);
                    Handles.color = c1;
                    Handles.DrawLine(trp, worldSpaceCorner);
                    Handles.color = c2;
                    var center = cam.transform.TransformVector(Vector3.Lerp(frustumCorners[i], frustumCorners[(i + 1) % 4], 0.5f));
                    Handles.DrawLine(trp, center);
                    var quarter = cam.transform.TransformVector(Vector3.Lerp(frustumCorners[i], frustumCorners[(i + 1) % 4], 0.25f));
                    Handles.DrawLine(trp, quarter);
                    var threeQuarter = cam.transform.TransformVector(Vector3.Lerp(frustumCorners[i], frustumCorners[(i + 1) % 4], 0.75f));
                    Handles.DrawLine(trp, threeQuarter);
                }
                
                // could also draw arcs at opposite corners
                
                // draw quad
                var shiftPerspective = cam.GetComponent<ShiftedPerspective>();
                var quad = shiftPerspective.screenQuad;
                var s = quad.lossyScale;
                s.y = 0;
                Handles.color = c1;
                var mat = Handles.matrix;
                Handles.matrix = quad.localToWorldMatrix;
                Handles.DrawWireCube(Vector3.zero, new Vector3(1,1,0));
                Handles.matrix = mat;
            }
            else
            {
                Handles.color = c1;

                var radius = tr.lossyScale.x * 0.5f;
                // // rotate vertical line by viewAngle
                var viewAngle = t.cubemapRendering.GetComponent<RealtimeCubemap>().viewAngle;
                var a = (viewAngle) / 2f * Mathf.Deg2Rad;
                var v = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0);
                for (var i = 0; i < 8; i++)
                {
                    var q = Quaternion.AngleAxis(i * 45, Vector3.up);
                    var qv = cam.transform.TransformPoint(q * v * 0.5f);
                    Handles.DrawLine(trp, qv);
                    
                    // draw arc
                    Handles.DrawWireArc(trp, cam.transform.TransformDirection(q * Vector3.forward), cam.transform.TransformDirection(q * v), viewAngle, radius);
                }

                void DrawRingForAngle(float angleInDeg)
                {
                    var a = angleInDeg / 2f * Mathf.Deg2Rad;
                    var cosA = Mathf.Cos(a);
                    var downVec = new Vector3(0, cosA, 0);
                    var radiusAtSlicePosition = Mathf.Sqrt(1 - cosA * cosA) * radius;
                    Handles.DrawWireDisc(cam.transform.TransformPoint(downVec * 0.5f), tr.up, radiusAtSlicePosition);
                }
                
                var step = 45 / 2f;
                var angle = step;
                while (angle < viewAngle)
                {
                    Handles.color = c2;
                    DrawRingForAngle(angle);
                    angle += step;
                }

                Handles.color = c1;
                DrawRingForAngle(viewAngle);
            }
        }

        private readonly Color c1 = new Color(1, 1, 1, 0.5f);
        private readonly Color c2 = new Color(1, 1, 1, 0.2f);

        static void RepaintForSure()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            InternalEditorUtility.RepaintAllViews();
            EditorApplication.delayCall += () =>
            {
                EditorApplication.QueuePlayerLoopUpdate();
                EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
                InternalEditorUtility.RepaintAllViews();
            };
        }
        
        static void DrawProperty<T>(T component, ref SerializedObject serializedObject, params ExposedProperty[] propertyName) where T : Component
        {
            if (serializedObject == null)
                serializedObject = new SerializedObject(component);
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            foreach (var prop in propertyName)
            {
                var parts = prop.path.Split('.');
                var firstPart = parts[0];
                var startAt = 1;
                // special handling for access to components
                if (firstPart == "transform")
                {
                    serializedObject = new SerializedObject(component.transform);
                    startAt = 2;
                    firstPart = parts[1];
                }
                var property = serializedObject.FindProperty(firstPart);
                for (var i = startAt; i < parts.Length; i++)
                {
                    try
                    {
                        property = property.FindPropertyRelative(parts[i]);
                    }
                    catch
                    {
                        Debug.Log("Failed to find property " + prop + " in " + component);
                    }
                }
                if (property != null)
                {
                    if (property.propertyType == SerializedPropertyType.Float && prop.range != Vector2.zero)
                    {
                        var value = property.floatValue;
                        EditorGUI.BeginChangeCheck();
                        value = EditorGUILayout.Slider(prop.name ?? property.displayName, value, prop.range.x, prop.range.y);
                        if (EditorGUI.EndChangeCheck())
                            property.floatValue = value;
                    }
                    else if (property.propertyType == SerializedPropertyType.Integer && prop.range != Vector2.zero &&
                             Mathf.IsPowerOfTwo((int)prop.range.x) && Mathf.IsPowerOfTwo((int)prop.range.y))
                    {
                        var value = property.intValue;
                        EditorGUI.BeginChangeCheck();
                        PowerOfTwoNames(prop.range, ref _names, ref _values);
                        value = EditorGUILayout.IntPopup(prop.name ?? property.displayName, value, _names, _values);
                        if (EditorGUI.EndChangeCheck())
                            property.intValue = value;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RepaintForSure();
            }
        }
        
        static string[] _names = new string[0];
        static int[] _values = new int[0];

        static void PowerOfTwoNames(Vector2 range, ref string[] names, ref int[] values)
        {
            int start = (int) range.x;
            int end = (int) range.y;
            int count = 0;
            for (int i = start; i <= end; i *= 2)
                count++;
            if (names.Length != count) names = new string[count];
            if (values.Length != count) values = new int[count];
            for (int i = start; i <= end; i *= 2) {
                names[--count] = i.ToString();
                values[count] = i;
            }
        }
        
        private SerializedObject _domeWarpingSerializedObject;
        private SerializedObject _cubemapRenderingSerializedObject;
    }
}
