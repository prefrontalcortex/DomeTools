using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.Build.Reporting;
using UnityEditor.VersionControl;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.Mock;
using Assert = UnityEngine.Assertions.Assert;
using UnityEngine.XR.OpenXR.Tests;
using static UnityEditor.XR.OpenXR.Tests.OpenXREditorTestHelpers;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class BootConfigEditorTests : OpenXRLoaderSetup
    {
        [Test]
        public void TestCanCreateBootConfigAndroid()
        {
            TestBuildTarget(BuildTarget.Android);
        }

        [Test]
        public void TestCanCreateBootConfigWindows()
        {
            TestBuildTarget(BuildTarget.StandaloneWindows);
            TestBuildTarget(BuildTarget.StandaloneWindows64);
        }

        private void TestBuildTarget(BuildTarget buildTarget)
        {
            var bootConfig = new BootConfig(buildTarget);
            bootConfig.ReadBootConfig();

            // Check to see that we do not have the following key in the boot config
            Assert.IsFalse(bootConfig.TryGetValue("xr-sample-bootconfig-key01", out string value));
            Assert.AreEqual(value, null);

            // Check to see that we can store a key and retrieve it.
            bootConfig.SetValueForKey("xr-sample-bootconfig-key02", "primary value");
            Assert.IsTrue(bootConfig.TryGetValue("xr-sample-bootconfig-key02", out string key02value));
            Assert.AreEqual(key02value, "primary value");
            Assert.IsTrue(bootConfig.CheckValuePairExists("xr-sample-bootconfig-key02", "primary value"));

            // check to see that we can write the keys to the boot config and ensure that we can
            // retrieve the stored values
            bootConfig.WriteBootConfig();

            var cloneBootConfig = new BootConfig(buildTarget);
            cloneBootConfig.ReadBootConfig();
            Assert.IsTrue(cloneBootConfig.TryGetValue("xr-sample-bootconfig-key02", out key02value));
            Assert.AreEqual(key02value, "primary value");
            Assert.IsTrue(cloneBootConfig.CheckValuePairExists("xr-sample-bootconfig-key02", "primary value"));

        }
    }
}
