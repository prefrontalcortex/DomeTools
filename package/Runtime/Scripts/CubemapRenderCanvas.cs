using UnityEngine;
#if HAVE_URP || HAVE_HDRP
using UnityEngine.Rendering;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace pfc.Fulldome
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CubemapRenderCanvas : MonoBehaviour
    {
#if HAVE_URP || HAVE_HDRP
        private Camera c;
        
        private void OnEnable()
        {
            c = GetComponent<Camera>();
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (cam != c) return;
            ScriptableRenderContext.EmitGeometryForCamera(cam);
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }
#endif
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(CubemapRenderCanvas))]
    public class CubemapRenderCanvasEditor : Editor
    {
        public override void OnInspectorGUI()
        {
#if HAVE_URP || HAVE_HDRP
            if (!GraphicsSettings.currentRenderPipeline)
                EditorGUILayout.HelpBox("World-space Canvas rendering for cubemaps is only supported in URP and HDRP.", MessageType.Warning);
            else
#endif
                EditorGUILayout.HelpBox("This component enables support for rendering world-space canvases to cubemaps in URP and HDRP.", MessageType.None);
        }
    }
#endif
}