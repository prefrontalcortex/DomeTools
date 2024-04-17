using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR
{
    /// <summary>
    /// Small utility class for reading, updating and writing boot config.
    /// </summary>
    internal class BootConfig
    {
        private const string XrBootSettingsKey = "xr-boot-settings";

        private readonly Dictionary<string, string> bootConfigSettings;
        private readonly string buildTargetName;

        public BootConfig(BuildReport report) : this(report.summary.platform)
        { }

        public BootConfig(BuildTarget target)
        {
            bootConfigSettings = new Dictionary<string, string>();
            buildTargetName = BuildPipeline.GetBuildTargetName(target);
        }

        public void ReadBootConfig()
        {
            bootConfigSettings.Clear();

            string xrBootSettings = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, XrBootSettingsKey);
            if (!string.IsNullOrEmpty(xrBootSettings))
            {
                // boot settings string format
                // <boot setting>:<value>[;<boot setting>:<value>]*
                var bootSettings = xrBootSettings.Split(';');
                foreach (var bootSetting in bootSettings)
                {
                    var setting = bootSetting.Split(':');
                    if (setting.Length == 2 && !string.IsNullOrEmpty(setting[0]) && !string.IsNullOrEmpty(setting[1]))
                    {
                        bootConfigSettings.Add(setting[0], setting[1]);
                    }
                }
            }
        }

        public Dictionary<string, string> Settings => bootConfigSettings;

        public void SetValueForKey(string key, string value) => bootConfigSettings[key] = value;

        public bool TryGetValue(string key, out string value) => bootConfigSettings.TryGetValue(key, out value);

        public void ClearEntryForKeyAndValue(string key, string value)
        {
            if (bootConfigSettings.TryGetValue(key, out string dictValue) && dictValue == value)
            {
                bootConfigSettings.Remove(key);
            }
        }

        public bool CheckValuePairExists(string key, string value)
        {
            var foundKey = bootConfigSettings.TryGetValue(key, out string result);

            return foundKey && value.Equals(result);
        }

        public void WriteBootConfig()
        {
            // boot settings string format
            // <boot setting>:<value>[;<boot setting>:<value>]*
            bool firstEntry = true;
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in bootConfigSettings)
            {
                if (!firstEntry)
                {
                    sb.Append(";");
                }

                sb.Append($"{kvp.Key}:{kvp.Value}");
                firstEntry = false;
            }

            EditorUserBuildSettings.SetPlatformSettings(buildTargetName, XrBootSettingsKey, sb.ToString());
        }
    }
}
