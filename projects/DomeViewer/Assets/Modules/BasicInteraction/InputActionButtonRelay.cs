using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace pfc.Fulldome
{
    public class InputActionButtonRelay : MonoBehaviour
    {
        public InputAction menuAction;
        public InputActionReference menuActionRef;
        public UnityEvent OnPress;
        
        public void Awake()
        {
            if (menuAction != null) menuAction.performed+=ctx=> Press();
            if (menuActionRef)
                menuActionRef.action.performed+=ctx=> Press();
        }
        
        public void OnEnable()
        {
            if (menuAction != null) menuAction.Enable();
            if (menuActionRef)
                menuActionRef.asset.Enable();
        }
        
        public void OnDisable()
        {
            if (menuAction != null) menuAction.Disable();
        }

        private void Press()
        {
            OnPress.Invoke();
        }
    }
}
