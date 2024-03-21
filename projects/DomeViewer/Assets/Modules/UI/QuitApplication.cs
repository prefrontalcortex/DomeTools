using UnityEngine;
using UnityEngine.InputSystem;

namespace pfc.Fulldome
{
    public class QuitApplication : MonoBehaviour
    {
        public GameObject quitMenu;
        public InputActionReference QuitAction;

        private void OnEnable()
        {
            if (QuitAction) QuitAction.action.Enable();
            if (QuitAction) QuitAction.action.performed += ToggleUI;
        }

        private void OnDisable()
        {
            if (QuitAction) QuitAction.action.Disable();
            if (QuitAction) QuitAction.action.performed -= ToggleUI;
        }

        private void Start()
        {
            quitMenu.SetActive(false);
        }

        public void ToggleUI(InputAction.CallbackContext callbackContext)
        {
            quitMenu.SetActive(!quitMenu.activeSelf);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
