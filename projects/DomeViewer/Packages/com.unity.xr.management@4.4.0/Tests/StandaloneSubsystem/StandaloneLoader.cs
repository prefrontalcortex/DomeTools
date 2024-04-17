using System.Collections.Generic;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.SubsystemsImplementation.Extensions;
#endif

namespace UnityEngine.XR.Management.Tests.Standalone
{
    public class StandaloneLoader : XRLoaderHelper
    {
        static List<StandaloneSubsystemDescriptor> s_StandaloneSubsystemDescriptors = new List<StandaloneSubsystemDescriptor>();

        public StandaloneSubsystem standaloneSubsystem => GetLoadedSubsystem<StandaloneSubsystem>();

        public bool started { get; protected set; }
        public bool stopped { get; protected set; }
        public bool deInitialized { get; protected set; }

        void OnStartCalled()
        {
            started = true;
        }

        void OnStopCalled()
        {
            stopped = true;
        }

        void OnDestroyCalled()
        {
            deInitialized = true;
        }

        public override bool Initialize()
        {
            started = false;
            stopped = false;
            deInitialized = false;

            CreateSubsystem<StandaloneSubsystemDescriptor, StandaloneSubsystem>(s_StandaloneSubsystemDescriptors, "Standalone Subsystem");
            if (standaloneSubsystem == null)
                return false;

#if UNITY_2020_2_OR_NEWER
            var provider = standaloneSubsystem.GetProvider();

            if (provider == null)
				return false;

            provider.startCalled += OnStartCalled;
            provider.stopCalled += OnStopCalled;
            provider.destroyCalled += OnDestroyCalled;
#elif USE_LEGACY_SUBSYS_REGISTRATION
            standaloneSubsystem.startCalled += OnStartCalled;
            standaloneSubsystem.stopCalled += OnStopCalled;
            standaloneSubsystem.destroyCalled += OnDestroyCalled;
#endif

            return true;
        }

        public override bool Start()
        {
            if (standaloneSubsystem != null)
                StartSubsystem<StandaloneSubsystem>();
            return true;
        }

        public override bool Stop()
        {
            if (standaloneSubsystem != null)
                StopSubsystem<StandaloneSubsystem>();
            return true;
        }

        public override bool Deinitialize()
        {
            DestroySubsystem<StandaloneSubsystem>();
            if (standaloneSubsystem != null)
            {
#if UNITY_2020_2_OR_NEWER
                var provider = standaloneSubsystem.GetProvider();

                if (provider != null)
                {
                    provider.startCalled -= OnStartCalled;
                    provider.stopCalled -= OnStopCalled;
                    provider.destroyCalled -= OnDestroyCalled;
                }
#elif USE_LEGACY_SUBSYS_REGISTRATION
                standaloneSubsystem.startCalled -= OnStartCalled;
                standaloneSubsystem.stopCalled -= OnStopCalled;
                standaloneSubsystem.destroyCalled -= OnDestroyCalled;
#endif
            }
            return base.Deinitialize();
        }

    }
}
