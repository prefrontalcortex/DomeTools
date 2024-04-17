using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// The Boot Config builder exposes a centralized call-site for populating BootConfig options.
    /// </summary>
    public class BootConfigBuilder
    {
        private struct SettingEntry
        {
            public bool IsDirty;
            public string Setting;
        }

        private readonly Dictionary<string, SettingEntry> _bootConfigSettings;

        /// <summary>
        /// Internal constructor. Should only ever get called inside this assembly. More to the point, it should only ever
        /// get called inside <see cref="OpenXRFeatureBuildHooks"/>
        /// </summary>
        internal BootConfigBuilder()
        {
            _bootConfigSettings = new Dictionary<string, SettingEntry>();
        }

        /// <summary>
        /// Populate the boot config settings from the current EditorUserBuildSettings based on the BuildReport.
        /// If <see cref="SetBootConfigValue"/> or <see cref="SetBootConfigBoolean"/> have been called before this call, we do not overwrite
        /// the value set, as we assume that these were meant to be the new, updated values.
        /// </summary>
        /// <param name="report">The BuildReport load the bootconfig from.</param>
        internal void ReadBootConfig(BuildReport report)
        {
            var bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (var setting in bootConfig.Settings)
            {
                // only update the boot config if the key doesn't currently live in _bootConfigSettings
                // We may have updated _bootConfigSettings before we've called `ReadBootConfig`. If that is the case,
                // this value overrides what's in the boot config.
                if (!_bootConfigSettings.TryGetValue(setting.Key, out var entry))
                    _bootConfigSettings[setting.Key] = new SettingEntry { IsDirty = false, Setting = setting.Value };
            }
        }

        /// <summary>
        /// To ensure we don't have any lingering values carried over into the next build, we clear out the current
        /// boot config as part of the post build step.
        /// Any setting that we have added via a <see cref="SetBootConfigValue"/> or <see cref="SetBootConfigBoolean"/> will be cleaned up.
        /// Any setting that was already in the boot config will not be removed.
        /// </summary>
        /// <param name="report"></param>
        internal void ClearAndWriteBootConfig(BuildReport report)
        {
            var bootConfig = new BootConfig(report);

            bootConfig.ReadBootConfig();

            foreach (var entry in _bootConfigSettings)
            {
                if (entry.Value.IsDirty)
                    bootConfig.ClearEntryForKeyAndValue(entry.Key, entry.Value.Setting);
            }

            bootConfig.WriteBootConfig();

            _bootConfigSettings.Clear();
        }

        /// <summary>
        /// Write the current boot config.
        /// Since we can override the <see cref="IPostprocessBuildWithReport.OnPostprocessBuild"/>, <see cref="IPreprocessBuildWithReport.OnPreprocessBuild"/> methods,
        /// we cannot guarantee this method will get called, nor the order in which this method can be called. If you override these methods,
        /// unless you call the base methods last, you'll want to invoke this method manually.
        /// </summary>
        /// <param name="report">Build report that we want to write</param>
        internal void WriteBootConfig(BuildReport report)
        {
            // don't bother writing out if there's no boot config settings, or there isn't an OpenXR loader active.
            if (_bootConfigSettings.Count <= 0)
                return;

            var bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfig();

            foreach (var entry in _bootConfigSettings)
            {
                // We only want to clean up the entries that we've added in this build processor.
                // Any other entries, we want to leave as is.
                if (entry.Value.IsDirty)
                    bootConfig.SetValueForKey(entry.Key, entry.Value.Setting);
            }

            bootConfig.WriteBootConfig();
        }

        /// <summary>
        /// Method for setting a specific boot config option, given the key and the string value to store.
        /// </summary>
        /// <param name="key">Key of the value to be stored</param>
        /// <param name="value">String value to write to the key</param>
        /// <returns>True if we are able to set the config value, otherwise returns false</returns>
        public bool SetBootConfigValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Cannot write a boot config value with an empty key.");
                return false;
            }

            _bootConfigSettings[key] = new SettingEntry { IsDirty = true, Setting = value };
            return true;
        }

        /// <summary>
        /// Method for setting a specific BOOLEAN config option. This method ensures a consistent method for writing a boolean value
        /// </summary>
        /// <param name="key">Key of the value to be stored</param>
        /// <param name="value">Boolean value to set</param>
        /// <returns>If the `key` existing in the boot config and it's "1", return true. Otherwise return false.</returns>
        public bool SetBootConfigBoolean(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Cannot write a boot config with an empty key");
                return false;
            }

            _bootConfigSettings[key] = new SettingEntry { IsDirty = true, Setting = value ? "1" : "0" };
            return true;
        }

        /// <summary>
        /// Get a config value from the boot config, given a specific key.
        /// </summary>
        /// <param name="key">Key we want to locate in the boot config</param>
        /// <param name="value">Where we store the result.</param>
        /// <returns>true if we find the key in the bootconfig, otherwise we return false</returns>
        public bool TryGetBootConfigValue(string key, out string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Cannot write a boot config with an empty key");
                value = null;
                return false;
            }

            bool result = _bootConfigSettings.TryGetValue(key, out var entry);
            value = result ? entry.Setting : null;
            return result;
        }

        /// <summary>
        /// Return a boolean based on the value stored at `key`
        /// </summary>
        /// <param name="key">key to look for in the boot config</param>
        /// <param name="value">Where we store the result.</param>
        /// <returns>true if we find the key in the bootconfig, otherwise we return false</returns>
        public bool TryGetBootConfigBoolean(string key, out bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Cannot perform a look up with a null or empty string key");
                value = false;
                return false;
            }

            bool result = _bootConfigSettings.TryGetValue(key, out var entry);
            value = result && entry.Setting.Equals("1");
            return result;
        }

        /// <summary>
        /// Try and remove an entry from the boot config.
        /// </summary>
        /// <param name="key">The key to attempt to remove</param>
        /// <returns>true if we were able to remove the boot config entry, otherwise false</returns>
        public bool TryRemoveBootConfigEntry(string key)
        {
            if (string.IsNullOrEmpty(key) || !_bootConfigSettings.ContainsKey(key))
            {
                return false;
            }

            return _bootConfigSettings.Remove(key);
        }
    }
}
