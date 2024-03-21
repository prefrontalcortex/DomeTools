using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace pfc.Fulldome
{
    public class ExternalCameraController : MonoBehaviour
    {
        public Camera ExternalCamera = null;
        public GameObject focusObj = null;
        public InputActionReference LookAction;
        public InputActionReference ActivateLookAction;
        public InputActionReference ZoomAction;
        
        public float zoomMultiplier = 0.0005f;
        public float rotateSpeed = 0.05f;
        public bool setDisableOnStart = false;
        public float zoomLerpFactor = 5;
        public AnimationCurve zoomAmountToFocusOffset;
        public Vector2 zoomMinMax = new Vector2(1, 30);

        public Vector2 rotationLimit = new Vector2(-60,60);

        private void OnEnable()
        {
            ExternalCamera.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            ExternalCamera.gameObject.SetActive(false);
        }

        private void Start()
        {
            if(setDisableOnStart) gameObject.SetActive(false);
            
            if(LookAction)LookAction.action.Enable();
            if(ActivateLookAction)ActivateLookAction.action.Enable();
            if(ZoomAction)ZoomAction.action.Enable();

            if(LookAction)LookAction.action.performed += ctx => lookBuffer += ctx.ReadValue<Vector2>();
            if(ZoomAction)ZoomAction.action.performed += ctx => fovBuffer += ctx.ReadValue<Vector2>();
            if(ActivateLookAction)ActivateLookAction.action.performed += ctx => canLook = !EventSystem.current.IsPointerOverGameObject();
            if(ActivateLookAction)ActivateLookAction.action.canceled += ctx => canLook = false;

            zoomAmount = 1;
            targetZoomAmoumt = 1;

            SetCameraPos();
        }

        bool canLook = false;
        Vector2 lookBuffer = Vector2.zero, fovBuffer = Vector2.zero;

        private void Update()
        {
            if(ZoomAction) FOV(fovBuffer, Time.deltaTime);
            if(canLook) OrbitCam(lookBuffer, Time.deltaTime);
            lookBuffer = Vector2.zero;
            fovBuffer = Vector2.zero;
        }

        private void OrbitCam(Vector2 mouseDelta, float deltaTime)
        {
            if (!isActiveAndEnabled) return;
            if (focusObj == null) return;
            
            var scaledRotateSpeed = rotateSpeed * deltaTime;
            
            //rotate camera around focusObj
            var pos = focusObj.transform.position;

            var localOffset = transform.position - pos;

            var forward = localOffset;
            forward.y = 0;
            forward.Normalize();
            
            localOffset = Quaternion.AngleAxis(mouseDelta.x * scaledRotateSpeed, Vector3.up) * localOffset;
            var finaLocalOffset = Quaternion.AngleAxis(-mouseDelta.y * scaledRotateSpeed, transform.right) * localOffset;
            
            var angleDelta = Vector3.SignedAngle(forward, finaLocalOffset, transform.right);
            var isOK = angleDelta > rotationLimit.x && angleDelta < rotationLimit.y;

            transform.position = pos + (isOK ? finaLocalOffset : localOffset);
            transform.LookAt(pos + new Vector3(0, zoomAmountToFocusOffset.Evaluate(zoomAmount), 0));
            
            // stop the smooth zoom coroutine if we're rotating,
            // otherwise we're getting conflicting behaviour. Better would be one update loop applying both. 
            if (smoothZoomCoroutine != null)
            {
                StopCoroutine(smoothZoomCoroutine);
                smoothZoomCoroutine = null;
            }
        }
        
        private void FOV(Vector2 value, float deltaTime)
        {
            if (!isActiveAndEnabled) return;
            targetZoomAmoumt -= value.y * zoomMultiplier * deltaTime;
            targetZoomAmoumt = Mathf.Clamp(targetZoomAmoumt, 0, 1);
            endSmoothZoomTime = Time.fixedTime + 1.0f;
            if (smoothZoomCoroutine == null) smoothZoomCoroutine = StartCoroutine(LerpZoom());
        }


        // range 0..1
        private float targetZoomAmoumt;
        private float zoomAmount;
        private float endSmoothZoomTime = 0;
        private Coroutine smoothZoomCoroutine = null;

        private void SetCameraPos(float lerp = 1)
        {
            zoomAmount = Mathf.Lerp(zoomAmount, targetZoomAmoumt, lerp * 5);
            var zoomDistance = Mathf.Lerp(zoomMinMax.x, zoomMinMax.y, zoomAmount);
            var center = focusObj.transform.position + Vector3.up * zoomAmountToFocusOffset.Evaluate(zoomAmount);
            var targetPos = center - transform.forward * zoomDistance;
            
            var distToCenter = Vector3.Distance(ExternalCamera.transform.position, center);
            var targetDistToCenter = Vector3.Distance(targetPos, center);
            ExternalCamera.transform.position = Vector3.Lerp(ExternalCamera.transform.position, targetPos, lerp);
            var lerpedDistance = Mathf.Lerp(distToCenter, targetDistToCenter, lerp);
            ExternalCamera.transform.LookAt(center);
            ExternalCamera.transform.position = center - ExternalCamera.transform.forward * lerpedDistance;
        }
        
        private IEnumerator LerpZoom()
        {
            
            while (Time.time < endSmoothZoomTime)
            {
                SetCameraPos(Time.deltaTime * zoomLerpFactor);
                yield return null;
            }

            {
                SetCameraPos();
                smoothZoomCoroutine = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (focusObj == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, focusObj.transform.position);
        }

        private void OnValidate()
        {
            if (focusObj == null) return;
            transform.LookAt(focusObj.transform);
        }

        private void OnDrawGizmosSelected()
        {
            if (focusObj == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, focusObj.transform.position);
        }
    }
}
