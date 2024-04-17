using System;
using NUnit.Framework;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR;
using Unity.XR.CoreUtils.Editor;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class OpenXRValidationTests
    {
        internal class FakeFeature : OpenXRFeature
        {
        }

        /// <summary>
        /// Test that IsRuleEnabled will be true at the correct time for a BuildValidationRule.
        /// </summary>
        [Test]
        public void IsRuleEnabledTest()
        {
            // Create a validation rule that is enabled when FakeFeature is active
            OpenXRFeature.ValidationRule testRule = new OpenXRFeature.ValidationRule(ScriptableObject.CreateInstance<FakeFeature>())
            {
                message = "Fake feature message.",
                checkPredicate = () => true,
                fixIt = () => { },
                error = false,
                errorEnteringPlaymode = false
            };

            // Create the build validation rule for Standalone (arbitrarily picked)
            BuildValidationRule buildValidationRule = OpenXRProjectValidationRulesSetup.ConvertRuleToBuildValidationRule(testRule, BuildTargetGroup.Standalone);

            // Since the feature isn't in the active Standalone settings, the rule should not be enabled.
            Assert.IsFalse(buildValidationRule.IsRuleEnabled());

            // Temporarily add an enabled FakeFeature to the Standalone settings, and then restore the settings when the test is done.
            // The build validation rule should be enabled when we add the feature to the Standalone settings.
            OpenXRSettings standaloneSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
            OpenXRFeature firstStandaloneSetting = standaloneSettings.features[0];
            try
            {
                FakeFeature fakeFeature = ScriptableObject.CreateInstance<FakeFeature>();
                fakeFeature.enabled = true;
                standaloneSettings.features[0] = fakeFeature;
                Assert.IsTrue(buildValidationRule.IsRuleEnabled());
            }
            finally
            {
                standaloneSettings.features[0] = firstStandaloneSetting;
            }

            // Create another build validation rule for something else other than Standalone.
            // The build validation rule should not be enabled when we add the feature to the Standalone group.
            buildValidationRule = OpenXRProjectValidationRulesSetup.ConvertRuleToBuildValidationRule(testRule, BuildTargetGroup.WSA);
            standaloneSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
            firstStandaloneSetting = standaloneSettings.features[0];
            try
            {
                FakeFeature fakeFeature = ScriptableObject.CreateInstance<FakeFeature>();
                fakeFeature.enabled = true;
                standaloneSettings.features[0] = fakeFeature;
                Assert.IsFalse(buildValidationRule.IsRuleEnabled());
            }
            finally
            {
                standaloneSettings.features[0] = firstStandaloneSetting;
            }
        }
    }
}
