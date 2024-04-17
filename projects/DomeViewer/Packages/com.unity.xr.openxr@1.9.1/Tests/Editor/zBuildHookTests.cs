using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    // If you change this file, be sure to run the "no players" tests on yamato.
    // APV jobs don't include "players" such as standalone, so `BuildMockPlayer()`
    // will fail during APV.  The "no players" job on yamato will catch this.
    internal class zBuildHookTests : OpenXRLoaderSetup
    {
        internal static BuildReport BuildMockPlayer()
        {
            BuildPlayerOptions opts = new BuildPlayerOptions();
#if UNITY_EDITOR_WIN
            opts.target = BuildTarget.StandaloneWindows64;
#elif UNITY_EDITOR_OSX
            opts.target = BuildTarget.StandaloneOSX;
#endif
            if (File.Exists("Assets/main.unity"))
                opts.scenes = new string[] { "Assets/main.unity" };
            opts.targetGroup = BuildTargetGroup.Standalone;
            opts.locationPathName = "mocktest/mocktest.exe";

            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, opts.target);
            var report = BuildPipeline.BuildPlayer(opts);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            return report;
        }

        [Test]
        public void PrePostCallbacksAreReceived()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            var result = BuildMockPlayer();

            if (Environment.GetEnvironmentVariable("UNITY_OPENXR_YAMATO") == "1")
                Assert.IsTrue(result.summary.result == BuildResult.Succeeded);
            else if (result.summary.result != BuildResult.Succeeded)
                return;

            Assert.IsTrue(preprocessCalled);
            Assert.IsTrue(postprocessCalled);
        }

        [Test]
        public void NoBuildCallbacksFeatureDisabled()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            // Disable mock runtime, no callbacks should occur during build
            EnableFeature<MockRuntime>(false);
            BuildMockPlayer();
            Assert.IsFalse(preprocessCalled);
            Assert.IsFalse(postprocessCalled);
        }

        [Test]
        public void NoBuildCallbacksOpenXRDisabled()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BuildCallbacks.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            // Remove OpenXR Loader, no callbacks should occur during build
            var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>());
            BuildMockPlayer();
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>(loaders));
            Assert.IsFalse(preprocessCalled);
            Assert.IsFalse(postprocessCalled);
        }

        [Test]
        public void VerifyBootConfigWrite()
        {
            bool preprocessCalled = false;
            bool postprocessCalled = false;

            BootConfigTests.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnPreprocessBuildExt")
                {
                    preprocessCalled = true;
                }

                if (methodName == "OnPostprocessBuildExt")
                {
                    postprocessCalled = true;
                }

                return true;
            };

            var result = BuildMockPlayer();
            BuildMockPlayer();
            Assert.IsFalse(preprocessCalled);
            Assert.IsFalse(postprocessCalled);

            BootConfigTests.EnsureCleanupFromLastRun();
        }

        private bool HasOpenXRLibraries(BuildReport report)
        {
            var path = Path.GetDirectoryName(report.summary.outputPath);
            var dir = new DirectoryInfo(path);

            var ext = "dll";
            if (Application.platform == RuntimePlatform.OSXEditor)
                ext = "dylib";

            var dlls = dir.EnumerateFiles($"*.{ext}", SearchOption.AllDirectories).Select(s => s.Name).ToList();
            return dlls.Contains($"openxr_loader.{ext}") || dlls.Contains($"UnityOpenXR.{ext}");
        }

        [Test]
        public void VerifyBuildOutputLibraries()
        {
            var resultWithOpenXR = BuildMockPlayer();

            // Disable this test if we're not running our openxr yamato infrastructure
            if (resultWithOpenXR.summary.result != BuildResult.Succeeded && Environment.GetEnvironmentVariable("UNITY_OPENXR_YAMATO") != "1")
                return;

            Assert.IsTrue(HasOpenXRLibraries(resultWithOpenXR));

            // Remove OpenXR Loader
            XRGeneralSettings.Instance.Manager.TrySetLoaders(new List<XRLoader>());
            var resultWithoutOpenXR = BuildMockPlayer();
            Assert.IsFalse(HasOpenXRLibraries(resultWithoutOpenXR));
        }

        internal class BuildCallbacks : OpenXRFeatureBuildHooks
        {
            [NonSerialized] internal static Func<string, object, bool> TestCallback = (methodName, param) => true;

            public override int callbackOrder => 1;
            public override Type featureType => typeof(MockRuntime);

            protected override void OnPreprocessBuildExt(BuildReport report)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, report);
            }

            protected override void OnPostGenerateGradleAndroidProjectExt(string path)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, path);
            }

            protected override void OnPostprocessBuildExt(BuildReport report)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, report);
            }

            protected override void OnProcessBootConfigExt(BuildReport report, BootConfigBuilder builder)
            {
                TestCallback(MethodBase.GetCurrentMethod().Name, report);
            }
        }

        internal class BootConfigTests : OpenXRFeatureBuildHooks
        {
            [NonSerialized] internal static Func<string, object, bool> TestCallback = (methodName, param) => true;

            public override int callbackOrder => 1;
            public override Type featureType => typeof(MockRuntime);

            // For this test, we want to ensure that the last run actually cleans up any settings that we've
            // stored into the boot settings of the EditorUserBuildSettings. We need the last run BuildReport
            // in order to check the EditorUserBuildSettings.
            private static BuildReport s_lastRunBuildReport;

            protected override void OnPreprocessBuildExt(BuildReport report)
            {
            }

            protected override void OnPostGenerateGradleAndroidProjectExt(string path)
            {
            }

            protected override void OnPostprocessBuildExt(BuildReport report)
            {
                // check to see if we've got the boot config written into the UserSettings
                var bootConfig = new BootConfig(report);
                bootConfig.ReadBootConfig();
                Assert.IsTrue(bootConfig.TryGetValue("key-01", out var key01Value));
                Assert.AreEqual(key01Value, "primary test value");
                Assert.IsTrue(bootConfig.TryGetValue("key-02", out var key02Value));
                Assert.AreEqual(key02Value, "secondary test value");
                Assert.IsTrue(bootConfig.TryGetValue("key-03", out var key03Value));
                Assert.AreEqual(key03Value, "1");
                Assert.IsTrue(bootConfig.TryGetValue("key-04", out var key04Value));
                Assert.AreEqual(key04Value, "0");

                s_lastRunBuildReport = report;
            }

            protected override void OnProcessBootConfigExt(BuildReport report, BootConfigBuilder builder)
            {
                // Now we set some boot config values and check to make sure they're there.
                builder.SetBootConfigValue("key-01", "primary test value");
                builder.SetBootConfigValue("key-02", "secondary test value");
                builder.SetBootConfigBoolean("key-03", true);
                builder.SetBootConfigBoolean("key-04", false);
                Assert.IsTrue(builder.TryGetBootConfigValue("key-01", out var result01));
                Assert.AreEqual(result01, "primary test value");
                Assert.IsTrue(builder.TryGetBootConfigValue("key-02", out var result02));
                Assert.AreEqual(result02, "secondary test value");
                Assert.IsTrue(builder.TryGetBootConfigBoolean("key-03", out var result03));
                Assert.IsTrue(result03);
                Assert.IsTrue(builder.TryGetBootConfigBoolean("key-04", out var result04));
                Assert.IsFalse(result04);
                builder.SetBootConfigValue("key-05", "remove-me");
                Assert.IsTrue(builder.TryRemoveBootConfigEntry("key-05"));
                Assert.IsFalse(builder.TryGetBootConfigValue("key-05", out var result05));
                Assert.IsFalse(builder.TryGetBootConfigBoolean("key-999", out var result06));
                Assert.IsFalse(result06);
            }

            public static void EnsureCleanupFromLastRun()
            {
                // make sure that the UserSettings doesn't hold any additional configs we've previously written
                if (s_lastRunBuildReport == null)
                    return;

                var bootConfig = new BootConfig(s_lastRunBuildReport);
                bootConfig.ReadBootConfig();
                Assert.IsFalse(bootConfig.TryGetValue("key-01", out var key01Value));
                Assert.IsFalse(bootConfig.TryGetValue("key-02", out var key02Value));
                Assert.IsFalse(bootConfig.TryGetValue("key-03", out var key03Value));
                Assert.IsFalse(bootConfig.TryGetValue("key-04", out var key04Value));
                Assert.IsFalse(bootConfig.TryGetValue("key-05", out var key05Value));

                s_lastRunBuildReport = null;
            }
        }
    }
}
