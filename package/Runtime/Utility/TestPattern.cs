using UnityEngine;

namespace pfc.Fulldome
{
    [ExecuteAlways]
    public class TestPattern : MonoBehaviour
    {
        [HideInInspector]
        public string rootFolderGuid = "ce5ea5a30554b5a4bbae639f8d9e574d";
        public Texture currentTexture;
        
        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            if (!_renderer) return;
            _renderer.hideFlags = HideFlags.HideInInspector;
            var _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter) _meshFilter.hideFlags = HideFlags.HideInInspector;
            
            _propertyBlock = new MaterialPropertyBlock();
            SetTexture(currentTexture);
        }

        private void OnDisable()
        {
            if (!_renderer) return;
            _renderer.SetPropertyBlock(null);
            _renderer.hideFlags = HideFlags.None;
            var _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter) _meshFilter.hideFlags = HideFlags.None;
        }

        public void SetTexture(Texture texture)
        {
            currentTexture = texture;
            if (!_renderer) return;
            _propertyBlock.SetTexture(BaseMap, texture);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}