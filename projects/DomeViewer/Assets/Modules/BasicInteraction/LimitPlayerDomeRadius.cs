using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace pfc.Fulldome
{
    public class LimitPlayerDomeRadius : MonoBehaviour
    {
        [SerializeField]
        public float allowedRadius = 15f;
        
        [SerializeField]
        XROrigin XrPlayer = null;

        // Update is called once per frame
        void Update()
        {
            if (XrPlayer == null){return;}
            Vector3 directionToPlayer = XrPlayer.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > allowedRadius){
                Vector3 clampedPosition = transform.position + directionToPlayer.normalized * allowedRadius;
                XrPlayer.transform.position = clampedPosition;
            }
        }
    }
}