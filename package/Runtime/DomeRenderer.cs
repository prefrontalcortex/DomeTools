using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
{
    public class DomeRenderer : MonoBehaviour
    {
        public static event System.Action<DomeRenderer> OnDomeRendererChanged;
        
        [Header("Target Objects")]
        public DomeWarp domeWarping;
        public RealtimeCubemap cubemapRendering;
        public RenderTexture domeContentTexture;
        public RenderTexture domeMasterTexture;
        
        public void SetMode(bool useDomeWarping)
        {
            if (domeWarping) domeWarping.gameObject.SetActive(useDomeWarping);
            if (cubemapRendering) cubemapRendering.gameObject.SetActive(!useDomeWarping);
        }

        private void OnValidate()
        {
            if (domeContentTexture && domeContentTexture.dimension != UnityEngine.Rendering.TextureDimension.Tex2D)
            {
                Debug.LogWarning("Dome Content Texture must be a 2D Texture.");
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "Set RT");
                if (domeContentTexture)
                    EditorUtility.SetDirty(this);
#endif
                domeContentTexture = null;
            }
            
            if (!domeContentTexture)
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "Set RT");
#endif
                if (domeWarping && domeWarping.targetTexture)
                    domeContentTexture = domeWarping.targetTexture;
                else if (cubemapRendering && cubemapRendering.targetTexture)
                    domeContentTexture = cubemapRendering.targetTexture;
#if UNITY_EDITOR
                if (domeContentTexture)
                    EditorUtility.SetDirty(this);
#endif
            }
        }

        public void SetContentRenderTexture(RenderTexture rt)
        {
#if UNITY_EDITOR
            const string undoMsg = "Set Content Render Texture";
            Undo.RegisterCompleteObjectUndo(this, undoMsg);
#endif
            domeContentTexture = rt;
            
            if (domeWarping)
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(domeWarping, undoMsg);
#endif
                domeWarping.targetTexture = rt;
            }

            if (cubemapRendering)
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(cubemapRendering, undoMsg);
#endif
                cubemapRendering.targetTexture = rt;
            }
            OnDomeRendererChanged?.Invoke(this);
        }

        public void SetOutputRenderTexture(RenderTexture objectReferenceValue)
        {
#if UNITY_EDITOR
            const string undoMsg = "Set Output Render Texture";
            Undo.RegisterCompleteObjectUndo(this, undoMsg);
#endif
            domeMasterTexture = objectReferenceValue;
            OnDomeRendererChanged?.Invoke(this);
        }
    }

    public class ExposedProperty
    {
        public string path;
        public string name = null;
        public Vector2 range = Vector2.zero;
    }
}


