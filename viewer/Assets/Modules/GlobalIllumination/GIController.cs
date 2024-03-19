using UnityEngine;

namespace pfc.Fulldome
{
    [ExecuteAlways]
    public class GIController : MonoBehaviour
    {
        public Texture source;
        public Transform giTarget;
        private static readonly int GlobalIlluminationSource = Shader.PropertyToID("_GlobalIlluminationSource");
        private static readonly int GlobalIlluminationTransform = Shader.PropertyToID("_GlobalIlluminationTransform");

        private void OnEnable() => SetTexture();
        private void Update() => SetTexture();
        private void OnValidate() => SetTexture();

        private void SetTexture()
        {
            Shader.SetGlobalTexture(GlobalIlluminationSource, source);
            Shader.SetGlobalMatrix(GlobalIlluminationTransform, giTarget.worldToLocalMatrix);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1, 0.05f);
            Gizmos.matrix = giTarget.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        }
    }
}