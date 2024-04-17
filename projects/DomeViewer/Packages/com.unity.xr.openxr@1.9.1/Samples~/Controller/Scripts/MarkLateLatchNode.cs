using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.InputSystem;

/// <summary>
/// Example code for Vulkan Late latching support. For use cases example, please check out Controller Sample - Head object, LeftHand/Aim object and RightHand/Grip object.
/// See: https://docs.unity3d.com/ScriptReference/XR.XRDisplaySubsystem.MarkTransformLateLatched.html
/// </summary>
public class MarkLateLatchNode : MonoBehaviour
{
    private XRDisplaySubsystem m_DisplaySubsystem = null;
    public UnityEngine.XR.XRDisplaySubsystem.LateLatchNode lateLatchNode;
    //Set one pose type InputAction for each hand to be late latched - e.g.: Aim or Grip. For head node, leave it unset.
    [SerializeField] private InputActionReference _ActionReference = null;

    // Start is called before the first frame update
    void Start()
    {
        List<XRDisplaySubsystem> subsys = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(subsys);

        if (subsys.Count >= 1)
            m_DisplaySubsystem = subsys[0];
        if (_ActionReference != null)
        {
            bool result = OpenXRInput.TrySetControllerLateLatchAction(_ActionReference.action);
            Debug.LogFormat("TrySetControllerLateLatchAction returns {0} for action {1}.", result, _ActionReference);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_DisplaySubsystem != null)
        {
            transform.position += new Vector3(0.00001f, 0, 0);
            Quaternion rot = transform.rotation;
            rot.x += 0.00001f;
            transform.rotation = rot;
            m_DisplaySubsystem.MarkTransformLateLatched(transform, lateLatchNode);
        }
    }
}
