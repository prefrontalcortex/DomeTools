using UnityEngine;
namespace pfc.Fulldome
{
    public class XRMenuAnchor : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.zero;
        public float lerpSpeed = 5.0f;

        void Update()
        {
            if (target != null)
            {
                Vector3 targetPosition = target.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            }
        }
    }
}