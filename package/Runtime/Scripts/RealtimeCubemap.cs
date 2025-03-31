using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace pfc.DomeTools {
    
    [ExecuteInEditMode]
    public class RealtimeCubemap : MonoBehaviour
    {
        public RenderTexture cubemap;
        public RenderTexture targetTexture;
        public Shader blitShader;
        
        [Range(120, 360)]
        public float viewAngle = 210;

        internal static readonly ExposedProperty[] ExposedProperties = new[]
        {
            new ExposedProperty() { path = nameof(viewAngle) },
            new ExposedProperty() { path = nameof(cubemapSize), range = new Vector2(128, 4096)},
        };

        [FormerlySerializedAs("size")]
        public int cubemapSize = 1024;
        
        private Material blitMaterial;
        private Camera c;
        
        // Update is called once per frame
        void Update ()
        {
            if (!c) c = GetComponent<Camera>();
            if (!c) return;
            
            if (!cubemap || cubemap.width != cubemapSize)
            {
                var _size = Mathf.ClosestPowerOfTwo(cubemapSize);
                if (!cubemap)
                {
                    // get nearest power of two
                    cubemap = new RenderTexture(_size, _size, 0, RenderTextureFormat.ARGB32);
                }
                else
                { 
                    cubemap.Release();
                    cubemap.width = _size;
                    cubemap.height = _size;
                }
                cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
                cubemap.Create();
            }
            
            // Render all faces
            var mask = 63;
            // exclude bottom if possible. This is a rough number since there's also a bit of antialiasing etc going on
            if (viewAngle <= 250)
                mask &= ~(1 << (int)CubemapFace.NegativeY);
            c.RenderToCubemap(cubemap, mask, Camera.MonoOrStereoscopicEye.Mono);
            targetTexture.IncrementUpdateCount();

            StartCoroutine(_BlitAtEndOfFrame());
        }
        
        IEnumerator _BlitAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            if (!blitMaterial)
            {
                blitMaterial = new Material(blitShader);
                blitMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            
            if (blitMaterial)
            {
                blitMaterial.SetFloat("_Angle", viewAngle); 
                blitMaterial.SetTexture("_CubeTex", cubemap);
                var mat = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
                blitMaterial.SetMatrix("_CubeToDome_WorldTransform", mat);
                Shader.SetGlobalMatrix("_CubeToDome_WorldTransform", mat);
                
                // HDRP uses the BiRP shader graph as well. HDRP would be pass 6 and requires custom matrices.
                // See https://forum.unity.com/threads/graphics-blit-with-hdrp-shadergraph.724112/#post-8201697
                var target = targetTexture;
                Graphics.SetRenderTarget(targetTexture);
                GL.Clear(true, true, Color.clear);
                Graphics.Blit(null, targetTexture, blitMaterial, 0);
                Graphics.SetRenderTarget(target);
            }
        }

        private void OnDisable()
        {
            if (cubemap)
                cubemap.Release();
            if (blitMaterial)
                DestroyImmediate(blitMaterial); 
        }
    }
}