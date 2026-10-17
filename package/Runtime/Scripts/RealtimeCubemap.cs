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
            
            c.RenderToCubemap(cubemap, GetFaceMask(), Camera.MonoOrStereoscopicEye.Mono);
            targetTexture.IncrementUpdateCount();

            StartCoroutine(_BlitAtEndOfFrame());
        }
        
        // Largest angle between a cube face normal and any direction on that face (the face corners).
        private const float FaceHalfAngle = 54.7356f;
        // Extra margin for filtering / antialiasing at the edge of the visible area.
        private const float FaceMargin = 3f;

        private static readonly (CubemapFace face, Vector3 normal)[] Faces =
        {
            (CubemapFace.PositiveX, Vector3.right),
            (CubemapFace.NegativeX, Vector3.left),
            (CubemapFace.PositiveY, Vector3.up),
            (CubemapFace.NegativeY, Vector3.down),
            (CubemapFace.PositiveZ, Vector3.forward),
            (CubemapFace.NegativeZ, Vector3.back),
        };

        // The dome center looks along the local up axis of this transform (see UVToDirection in DomemasterInclude.cginc)
        // and covers viewAngle / 2 around it. Only render the faces that are at least partly inside that cone.
        private int GetFaceMask()
        {
            var halfAngle = viewAngle * 0.5f;
            var axis = transform.rotation * Vector3.up;
            var mask = 0;
            foreach (var (face, normal) in Faces)
            {
                if (Vector3.Angle(axis, normal) - FaceHalfAngle - FaceMargin <= halfAngle)
                    mask |= 1 << (int)face;
            }
            return mask;
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