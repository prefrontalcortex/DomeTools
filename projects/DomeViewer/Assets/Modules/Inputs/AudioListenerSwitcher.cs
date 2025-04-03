using System;
using UnityEngine;

public class AudioListenerSwitcher : MonoBehaviour
{
    [SerializeField] private AudioListener vrMode;
    [SerializeField] private AudioListener desktopMode;

    private AudioListener currentActive
    {
        get
        {
            if (vrMode == null || desktopMode == null)
                return null;

            if (vrMode.enabled)
                return vrMode;
            else
                return desktopMode;
        }
    }

    private Transform vrTransform;
    private Transform desktopTransform;
    private Pose lastVrPose;
    private Pose lastDesktopPose;

    private void OnEnable()
    {
        vrTransform = vrMode.transform;
        desktopTransform = desktopMode.transform;
        
        lastVrPose = new Pose(vrTransform.position, vrTransform.rotation);
        lastDesktopPose = new Pose(desktopTransform.position, desktopTransform.rotation);
        
        vrMode.enabled = false;
        desktopMode.enabled = true;
    }

    private void Update()
    {
        if (currentActive == desktopMode)
        {
            var vrPose = new Pose(vrTransform.position, vrTransform.rotation);
            if (lastVrPose != vrPose)
            {
                vrMode.enabled = true;
                desktopMode.enabled = false;
            }
            lastVrPose = vrPose;
        }
        else
        {
            var desktopPose = new Pose(desktopTransform.position, desktopTransform.rotation);
            if (lastDesktopPose != desktopPose)
            {
                vrMode.enabled = false;
                desktopMode.enabled = true;
            }
            lastDesktopPose = desktopPose;
        }
        
    }
}
