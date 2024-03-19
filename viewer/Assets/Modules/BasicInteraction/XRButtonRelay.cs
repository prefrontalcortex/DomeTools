using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace pfc.Fulldome
{
    public class XRButtonRelay : MonoBehaviour
    {
        public XRNode node;
        private InputDevice device => InputDevices.GetDeviceAtXRNode(node);
        public List<EventButton> buttons;
        
        [System.Serializable]
        public class EventButton
        {
            public InputHelpers.Button button;
            private float threshold = 0.3f;
            public UnityEvent OnDown, OnUp;

            private bool value;
            
            public void Update(InputDevice device, XRButtonRelay relay)
            {
                device.IsPressed(button, out var isPressed, threshold);
                if (value != isPressed)
                    if(isPressed) OnDown.Invoke();
                    else OnUp.Invoke();
                value = isPressed;
            }
        }

        private void Update()
        {
            foreach(var button in buttons)
                button.Update(device,this);
        }
    }
}