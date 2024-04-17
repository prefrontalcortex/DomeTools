using System;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Tests
{
    /// <summary>
    /// Custom yield instruction that waits for a null OpenXRLoaderBase after initialization has started.
    /// </summary>
    internal sealed class WaitForNullXRLoader : CustomYieldInstruction
    {
        private float m_Timeout = 0;

        public bool m_startListening = false;

        public WaitForNullXRLoader(float timeout = 5.0f)
        {
            m_Timeout = Time.realtimeSinceStartup + timeout;
        }

        public void StartListening()
        {
            m_startListening = true;
        }

        public override bool keepWaiting
        {
            get
            {
                // Wait until the coroutine is done
                if (m_startListening && OpenXRLoaderBase.Instance == null)
                {
                    return false;
                }

                // Did we time out waiting?
                if (Time.realtimeSinceStartup > m_Timeout)
                {
                    Assert.Fail("WaitForDestroyInstanceCall: Timeout");
                    return false;
                }

                return true;
            }
        }
    }
}
