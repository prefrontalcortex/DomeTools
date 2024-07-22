using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace pfc.Fulldome
{
    public class DesktopController : MonoBehaviour
    {
        public Camera FirstPersonCamera = null;
        public float moveSpeed;
        [Range(0.01f, 1f)]
        public float damping = 0.85f;
        public Vector2 fovMinMax = new Vector2(15, 120);
        public float fovLerpSpeed = 1f;
        public float fovMultiplier = 0.05f;

        [Range(1f, 100f)]
        public float rotateSpeed = 50f;
        public float rotationLerpSpeed = 10f;
        
        private Vector2 m_Rotation;
        public InputActionReference LookAction;
        public InputActionReference MoveAction;
        public InputActionReference ActivateLookAction;
        public InputActionReference FOVAction;

        public bool setDisableOnStart = false;

        private float endSmoothZoomTime = 0;
        private float targetFov;
        private Coroutine smoothZoomCoroutine = null;
        public Vector2 rotationLimit = new Vector2(-85,85);

        private void FOV(Vector2 value)
        {
            if (!isActiveAndEnabled || FirstPersonCamera == null) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //add lerp in coroutine to smooth out the zoom
            targetFov = Mathf.Clamp(FirstPersonCamera.fieldOfView - value.y * fovMultiplier, fovMinMax.x, fovMinMax.y);
            endSmoothZoomTime = Time.time + 1.0f;
            if (smoothZoomCoroutine == null) 
                smoothZoomCoroutine = StartCoroutine(SmoothZoom());
        }

        private IEnumerator SmoothZoom()
        {
            while (Time.time < endSmoothZoomTime)
            {
                float currentFOV = FirstPersonCamera.fieldOfView;
                FirstPersonCamera.fieldOfView = Mathf.Clamp(Mathf.Lerp(currentFOV, targetFov, fovLerpSpeed * Time.deltaTime), fovMinMax.x, fovMinMax.y);
                yield return null;
            }

            FirstPersonCamera.fieldOfView = targetFov;
            smoothZoomCoroutine = null;
        }

        private void Start()
        {
            if (setDisableOnStart) gameObject.SetActive(false);

            if (LookAction) LookAction.action.Enable();
            if (MoveAction) MoveAction.action.Enable();
            if (ActivateLookAction) ActivateLookAction.action.Enable();
            if (FOVAction) FOVAction.action.Enable();

            if (LookAction) LookAction.action.performed += ctx => lookBuffer += ctx.ReadValue<Vector2>();
            if (FOVAction) FOVAction.action.performed += ctx => fovBuffer += ctx.ReadValue<Vector2>();
            ActivateLookAction.action.performed += ctx => canLook = !EventSystem.current.IsPointerOverGameObject();
            ActivateLookAction.action.canceled += ctx => canLook = false;
            
            // initial rotation values to match the camera setup
            targetRotation = transform.rotation;
            targetEuler = transform.eulerAngles;
        }

        private Vector2 lookBuffer, fovBuffer;
        private bool canLook;
        private Vector3 velocity;
        private Quaternion targetRotation = Quaternion.identity;
        private Vector3 targetEuler = Vector3.zero;

        private void FixedUpdate()
        {
            Move(MoveAction.action.ReadValue<Vector2>(), Time.fixedDeltaTime);
            if (LookAction && canLook) Look(lookBuffer, Time.fixedDeltaTime);
            lookBuffer = Vector2.zero;
        }

        void Update()
        {
            if (FOVAction) FOV(fovBuffer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            fovBuffer = Vector2.zero;
        }

        
        private void Move(Vector2 direction, float deltaTime)
        {
            if (!isActiveAndEnabled) return;

            var scaledMoveSpeed = moveSpeed * deltaTime;
            var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
            velocity += move * scaledMoveSpeed;
            velocity *= damping;
            transform.position += velocity;
        }

        
        private void Look(Vector2 rotate, float deltaTime)
        {
            if (!isActiveAndEnabled || !canLook) return;

            if (rotate.sqrMagnitude < 0.00001) return;

            var horizontalFwd = transform.forward;
            horizontalFwd.y = 0;
            var horizontalRight = transform.right;
            horizontalRight.y = 0;
            var angleToHorizon = Vector3.SignedAngle(transform.forward, horizontalFwd, horizontalRight);
            var absAngleToHorizon = Mathf.Abs(angleToHorizon);

            var scaledRotateSpeed = rotateSpeed * deltaTime;

            // adjust by field of view
            scaledRotateSpeed *= FirstPersonCamera.fieldOfView / 60f;
            targetEuler.y += rotate.x * scaledRotateSpeed;
            // transform.Rotate(Vector3.up, rotate.x * scaledRotateSpeed, Space.World);
            var angleChange = rotate.y * scaledRotateSpeed;

            const float maxAngle = 88;
            const float minAngle = 70;
            var angleChangeFactor = Mathf.Clamp01(Mathf.Lerp(minAngle, maxAngle, absAngleToHorizon));

            if (angleToHorizon > maxAngle && angleChange > 0) return;
            if (angleToHorizon < -maxAngle && angleChange < 0) return;

            // smoothen out when we're close to the edges
            if (angleToHorizon > minAngle && angleChange > 0 || angleToHorizon < -minAngle && angleChange < 0)
                angleChange *= angleChangeFactor;

            targetEuler.x -= angleChange;
            // transform.Rotate(transform.right, -angleChange, Space.World);
            targetEuler.x = Mathf.Clamp(targetEuler.x, rotationLimit.x, rotationLimit.y);
            targetRotation = Quaternion.Euler(targetEuler.x, targetEuler.y, targetEuler.z);
        }
    }
}
