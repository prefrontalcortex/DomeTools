using System;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.SubsystemsImplementation;
#endif

namespace UnityEngine.XR.Management.Tests.Standalone
{
#if UNITY_2020_2_OR_NEWER
    public class StandaloneSubsystem : SubsystemWithProvider<StandaloneSubsystem, StandaloneSubsystemDescriptor, StandaloneSubsystem.Provider>
    {
        public class Provider : SubsystemProvider<StandaloneSubsystem>
        {
            public event Action startCalled;
            public event Action stopCalled;
            public event Action destroyCalled;

            public override void Start()
            {
                if (startCalled != null)
                    startCalled.Invoke();
            }

            public override void Stop()
            {
                if (stopCalled != null)
                    stopCalled.Invoke();
            }

            public override void Destroy()
            {
                if (destroyCalled != null)
                    destroyCalled.Invoke();
            }
        }
    }

    public class StandaloneSubsystemImpl : StandaloneSubsystem
    {
        public class ProviderImpl : Provider{ }
    }
#else
    public class StandaloneSubsystem : Subsystem
    {
        private bool isRunning = false;
        public override bool running
        {
            get { return isRunning; }
        }

        public event Action startCalled;
        public event Action stopCalled;
        public event Action destroyCalled;

        public override void Start()
        {
            isRunning = true;
            if (startCalled != null)
                startCalled.Invoke();
        }

        public override void Stop()
        {
            isRunning = false;
            if (stopCalled != null)
                stopCalled.Invoke();
        }

        protected override void OnDestroy()
        {
            isRunning = false;
            if (destroyCalled != null)
                destroyCalled.Invoke();
        }
    }
#endif
}
