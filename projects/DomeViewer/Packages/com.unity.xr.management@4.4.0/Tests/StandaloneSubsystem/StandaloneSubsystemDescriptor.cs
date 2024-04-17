
using System;
#if UNITY_2020_2_OR_NEWER
using UnityEngine.SubsystemsImplementation;
#endif

namespace UnityEngine.XR.Management.Tests.Standalone
{
#if UNITY_2020_2_OR_NEWER
    public class StandaloneSubsystemParams
    {
        public string id { get; set;}
        public Type subsystemTypeOverride { get; set; }
        public Type providerType { get; set; }
    }

    public class StandaloneSubsystemDescriptor : SubsystemDescriptorWithProvider<StandaloneSubsystem, StandaloneSubsystem.Provider>
    {
        public static void Create(StandaloneSubsystemParams descriptorParams)
        {
            var descriptor = new StandaloneSubsystemDescriptor(descriptorParams);
            SubsystemDescriptorStore.RegisterDescriptor(descriptor);
        }

        StandaloneSubsystemDescriptor(StandaloneSubsystemParams descriptorParams)
        {
            id = descriptorParams.id;
            subsystemTypeOverride = descriptorParams.subsystemTypeOverride;
            providerType = descriptorParams.providerType;
        }
    }
#else
    namespace Providing
    {
        public class StandaloneSubsystemParams
        {
            public string id { get; set;}
            public System.Type subsystemImplementationType { get; set; }

            public StandaloneSubsystemParams(string id, System.Type subsystemImplType)
            {
                this.id = id;
                this.subsystemImplementationType = subsystemImplType;
            }
        }
    }

    public class StandaloneSubsystemDescriptor : SubsystemDescriptor<StandaloneSubsystem>
    {
        public static void Create(Providing.StandaloneSubsystemParams parms)
        {
#if USE_LEGACY_SUBSYS_REGISTRATION
            SubsystemRegistration.CreateDescriptor(new StandaloneSubsystemDescriptor(parms.id, parms.subsystemImplementationType));
#endif
        }

        public StandaloneSubsystemDescriptor(string id, System.Type subsystemImplType)
        {
            this.id = id;
            this.subsystemImplementationType = subsystemImplType;
        }
    }
#endif 
}
