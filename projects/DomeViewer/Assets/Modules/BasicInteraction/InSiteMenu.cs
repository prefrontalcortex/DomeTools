using UnityEngine;

namespace pfc.Fulldome
{
    public class InSiteMenu : MonoBehaviour
    {
        public static InSiteMenu Instance { get; private set; }
        
        public Transform navigationMenu;
        public Vector3 navigationMenuOffset;

        private float currentScale = 1;
        private Vector3 originalMenuOffset, originalMenuScale;
        
        public void ScaleMenu(float scaleFactor, bool updateImmediately)
        {
            // Debug.Log("Set menu scale " + scaleFactor, this);
            currentScale = scaleFactor;
            if(updateImmediately)
                SetNavigationMenu(navigationMenu.gameObject.activeSelf);
        }

        public void ToggleNavigationMenu()
        {
            SetNavigationMenu(!navigationMenu.gameObject.activeSelf);
        }

        private void SetNavigationMenu(bool value)
        {
            switch (value)
            {
                case true:
                    Transform cam = gameObject.transform;
                    var canvas = navigationMenu.GetComponent<Canvas>();
                    if (canvas && canvas.worldCamera)
                        cam = canvas.worldCamera.transform;
                    
                    var dir = cam.forward;
                    dir.y = 0f;
                    dir = dir.normalized;
                    var rot = Quaternion.LookRotation(dir, Vector3.up);
                    var pos = cam.position + rot * originalMenuOffset * currentScale;
                    navigationMenu.position = pos;
                    navigationMenu.rotation = rot;
                    navigationMenu.localScale = originalMenuScale * currentScale;
                    navigationMenu.gameObject.SetActive(true);
                    break;
                case false:
                    navigationMenu.gameObject.SetActive(false);
                    break;
            }
        }

        private void Start()
        {
            originalMenuOffset = navigationMenuOffset;
            originalMenuScale = navigationMenu.localScale;
            navigationMenu.gameObject.SetActive(true);
            navigationMenu.gameObject.SetActive(false);
            SetNavigationMenu(false);
        }

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
