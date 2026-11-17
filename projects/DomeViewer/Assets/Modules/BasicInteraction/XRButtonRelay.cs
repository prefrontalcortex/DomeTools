using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace pfc.Fulldome
{
    public class XRButtonRelay : MonoBehaviour
    {
        public XRNode node;
        private InputDevice device => InputDevices.GetDeviceAtXRNode(node);
        public List<EventButton> buttons;

        public enum XRButtonType
        {
            Trigger,
            Grip,
            PrimaryButton,
            SecondaryButton,
            MenuButton
        }

        [System.Serializable]
        public class EventButton
        {
            public XRButtonType button;
            [Tooltip("Threshold only used for analog inputs (Trigger, Grip)")]
            public float threshold = 0.3f;
            public UnityEvent OnDown, OnUp;

            private bool value;

            public void Update(InputDevice device, XRButtonRelay relay)
            {
                bool isPressed = false;

                if (!device.isValid)
                {
                    // If device lost, ensure we send an Up event if previously pressed.
                    if (value)
                    {
                        OnUp.Invoke();
                        value = false;
                    }
                    return;
                }

                switch (button)
                {
                    case XRButtonType.Trigger:
                        if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal))
                            isPressed = triggerVal >= threshold;
                        break;
                    case XRButtonType.Grip:
                        if (device.TryGetFeatureValue(CommonUsages.grip, out float gripVal))
                            isPressed = gripVal >= threshold;
                        break;
                    case XRButtonType.PrimaryButton:
                        device.TryGetFeatureValue(CommonUsages.primaryButton, out isPressed);
                        break;
                    case XRButtonType.SecondaryButton:
                        device.TryGetFeatureValue(CommonUsages.secondaryButton, out isPressed);
                        break;
                    case XRButtonType.MenuButton:
                        device.TryGetFeatureValue(CommonUsages.menuButton, out isPressed);
                        break;
                }

                if (value != isPressed)
                {
                    if (isPressed) OnDown.Invoke();
                    else OnUp.Invoke();
                }

                value = isPressed;
            }
        }

        private void Update()
        {
            foreach (var button in buttons)
                button.Update(device, this);
        }
    }
}