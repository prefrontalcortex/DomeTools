namespace UnityEngine.XR.OpenXR
{
    /// <summary>
    /// Custom yield instruction that waits for the OpenXRRestarter to finish if it is running.
    /// </summary>
    internal sealed class WaitForRestartFinish : CustomYieldInstruction
    {
        private float m_Timeout = 0;

        public WaitForRestartFinish(float timeout = 5.0f)
        {
            m_Timeout = Time.realtimeSinceStartup + timeout;
        }

        public override bool keepWaiting
        {
            get
            {
                // Wait until the restarter is finished
                if (!OpenXRRestarter.Instance.isRunning)
                {
                    return false;
                }

                // Did we time out waiting?
                if (Time.realtimeSinceStartup > m_Timeout)
                {
                    Debug.LogError("WaitForRestartFinish: Timeout");
                    return false;
                }

                return true;
            }
        }
    }
}
