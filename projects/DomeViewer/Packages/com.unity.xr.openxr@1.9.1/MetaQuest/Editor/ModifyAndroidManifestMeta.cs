using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

#if XR_MGMT_4_4_0_OR_NEWER
using Unity.XR.Management.AndroidManifest.Editor;
#endif

namespace UnityEditor.XR.OpenXR.Features.MetaQuestSupport
{
    internal class ModifyAndroidManifestMeta : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType => typeof(MetaQuestFeature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
            ProcessSystemSplashScreen(path);

#if !XR_MGMT_4_4_0_OR_NEWER
            var androidManifest = new AndroidManifest(GetManifestPath(path));
            androidManifest.AddMetaData();
            androidManifest.Save();
#endif
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

#if XR_MGMT_4_4_0_OR_NEWER
        public override ManifestRequirement ProvideManifestRequirement()
        {
            var elementsToRemove = new List<ManifestElement>()
            {
                new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "uses-permission" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.permission.BLUETOOTH" }
                    }
                }
            };

            if (ForceRemoveInternetPermission())
            {
                elementsToRemove.Add(new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "uses-permission" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.permission.INTERNET" }
                    }
                });
            }

            var elementsToAdd = new List<ManifestElement>()
            {
                new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "uses-feature" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.hardware.vr.headtracking" },
                        { "required", "true" },
                        { "version", "1" }
                    }
                },
                new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "application", "meta-data" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.oculus.supportedDevices" },
                        { "value", GetMetaSupportedDevices() }
                    }
                },
                new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "application", "activity", "meta-data" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.oculus.vr.focusaware" },
                        { "value", "true" }
                    }
                },
                new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "application", "activity", "intent-filter", "category" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.oculus.intent.category.VR" }
                    }
                }
            };

            if (SystemSplashScreen() != null)
            {
                elementsToAdd.Add(new ManifestElement()
                {
                    ElementPath = new List<string> { "manifest", "application", "meta-data" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.oculus.ossplash" },
                        { "value", "true" }
                    }
                });
            }

            return new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>()
                {
                    typeof(OpenXRLoader)
                },
                NewElements = elementsToAdd,
                RemoveElements = elementsToRemove
            };
        }

#endif

        private static string GetMetaSupportedDevices()
        {
            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var questFeature = androidOpenXRSettings.GetFeature<MetaQuestFeature>();

            if (questFeature != null)
            {
                List<string> deviceList = new List<string>();
                foreach (var device in questFeature.targetDevices)
                {
                    if (device.active && device.enabled)
                        deviceList.Add(device.manifestName);
                }

                if (deviceList.Count > 0)
                {
                    return string.Join("|", deviceList.ToArray());
                }
                else
                {
                    UnityEngine.Debug.LogWarning("No target devices selected in Meta Quest Support Feature. No devices will be listed as supported in the application Android manifest.");
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private static bool ForceRemoveInternetPermission()
        {
            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var questFeature = androidOpenXRSettings.GetFeature<MetaQuestFeature>();

            if (questFeature == null || !questFeature.enabled)
                return false; // By default the permission is retained

            return questFeature.forceRemoveInternetPermission;
        }

        private static Texture2D SystemSplashScreen()
        {
            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var questFeature = androidOpenXRSettings.GetFeature<MetaQuestFeature>();

            if (questFeature == null || !questFeature.enabled)
                return null;

            return questFeature.systemSplashScreen;
        }

        private static void ProcessSystemSplashScreen(string gradlePath)
        {
            var systemSplashScreen = SystemSplashScreen();
            if (systemSplashScreen == null)
                return;

            string splashScreenAssetPath = AssetDatabase.GetAssetPath(systemSplashScreen);
            string sourcePath = splashScreenAssetPath;
            string targetFolder = Path.Combine(gradlePath, "src/main/assets");
            string targetPath = targetFolder + "/vr_splash.png";

            // copy the splash over into the gradle folder and make sure it's not read only
            FileUtil.ReplaceFile(sourcePath, targetPath);
            FileInfo targetInfo = new FileInfo(targetPath);
            targetInfo.IsReadOnly = false;
        }

#if !XR_MGMT_4_4_0_OR_NEWER
        private string _manifestFilePath;

        private string GetManifestPath(string basePath)
        {
            if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;

            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            _manifestFilePath = pathBuilder.ToString();

            return _manifestFilePath;
        }

        private class AndroidXmlDocument : XmlDocument
        {
            private string m_Path;
            protected XmlNamespaceManager nsMgr;
            public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

            public AndroidXmlDocument(string path)
            {
                m_Path = path;
                using (var reader = new XmlTextReader(m_Path))
                {
                    reader.Read();
                    Load(reader);
                }

                nsMgr = new XmlNamespaceManager(NameTable);
                nsMgr.AddNamespace("android", AndroidXmlNamespace);
            }

            public string Save()
            {
                return SaveAs(m_Path);
            }

            public string SaveAs(string path)
            {
                using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
                {
                    writer.Formatting = Formatting.Indented;
                    Save(writer);
                }

                return path;
            }
        }

        private class AndroidManifest : AndroidXmlDocument
        {
            private readonly XmlElement ApplicationElement;
            private readonly XmlElement ActivityIntentFilterElement;
            private readonly XmlElement ActivityElement;
            private readonly XmlElement ManifestElement;

            public AndroidManifest(string path) : base(path)
            {
                ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
                ActivityIntentFilterElement = SelectSingleNode("/manifest/application/activity/intent-filter") as XmlElement;
                ActivityElement = SelectSingleNode("manifest/application/activity") as XmlElement;
                ManifestElement = SelectSingleNode("/manifest") as XmlElement;
            }

            private XmlAttribute CreateAndroidAttribute(string key, string value)
            {
                XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
                attr.Value = value;
                return attr;
            }

            private void UpdateOrCreateAttribute(XmlElement xmlParentElement, string tag, string name, params (string name, string value)[] attributes)
            {
                var xmlNodeList = xmlParentElement.SelectNodes(tag);
                XmlElement targetNode = null;

                // Check all XmlNodes to see if a node with matching name already exists.
                foreach (XmlNode node in xmlNodeList)
                {
                    XmlAttribute nameAttr = (XmlAttribute)node.Attributes.GetNamedItem("name", AndroidXmlNamespace);
                    if (nameAttr != null && nameAttr.Value.Equals(name))
                    {
                        targetNode = (XmlElement)node;
                        break;
                    }
                }

                // If node exists, update the attribute values if they are present or create new ones as requested. Else, create new XmlElement.
                if (targetNode != null)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        XmlAttribute attr = (XmlAttribute)targetNode.Attributes.GetNamedItem(attributes[i].name, AndroidXmlNamespace);
                        if (attr != null)
                        {
                            attr.Value = attributes[i].value;
                        }
                        else
                        {
                            targetNode.SetAttribute(attributes[i].name, AndroidXmlNamespace, attributes[i].value);
                        }
                    }
                }
                else
                {
                    XmlElement newElement = CreateElement(tag);
                    newElement.SetAttribute("name", AndroidXmlNamespace, name);
                    for (int i = 0; i < attributes.Length; i++)
                        newElement.SetAttribute(attributes[i].name, AndroidXmlNamespace, attributes[i].value);
                    xmlParentElement.AppendChild(newElement);
                }
            }

            void RemoveNameValueElementInTag(string parentPath, string tag, string name, string value)
            {
                var xmlNodeList = this.SelectNodes(parentPath + "/" + tag);

                foreach (XmlNode node in xmlNodeList)
                {
                    var attributeList = ((XmlElement)node).Attributes;

                    foreach (XmlAttribute attrib in attributeList)
                    {
                        if (attrib.Name == name && attrib.Value == value)
                        {
                            node.ParentNode?.RemoveChild(node);
                        }
                    }
                }
            }

            internal void AddMetaData()
            {
                string supportedDevices = GetMetaSupportedDevices();

                UpdateOrCreateAttribute(ActivityIntentFilterElement,
                    "category", "com.oculus.intent.category.VR"
                );

                UpdateOrCreateAttribute(ActivityElement,
                    "meta-data", "com.oculus.vr.focusaware",
                    new (string name, string value)[]
                    {
                        ("value", "true")
                    });

                UpdateOrCreateAttribute(ApplicationElement,
                    "meta-data", "com.oculus.supportedDevices",
                    new (string name, string value)[]
                    {
                        ("value", supportedDevices)
                    });

                UpdateOrCreateAttribute(ManifestElement,
                    "uses-feature", "android.hardware.vr.headtracking",
                    new (string name, string value)[]
                    {
                        ("required", "true"),
                        ("version", "1")
                    });

                if (SystemSplashScreen() != null)
                {
                    UpdateOrCreateAttribute(ApplicationElement,
                        "meta-data", "com.oculus.ossplash",
                        new (string name, string value)[]
                        {
                            ("value", "true")
                        });
                }

                // if the Microphone class is used in a project, the BLUETOOTH permission is automatically added to the manifest
                // we remove it here since it will cause projects to fail Meta cert
                // this shouldn't affect Bluetooth HID devices, which don't need the permission
                RemoveNameValueElementInTag("/manifest", "uses-permission", "android:name", "android.permission.BLUETOOTH");
            }
        }
#endif
    }
}
