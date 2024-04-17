using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.XR.Management.AndroidManifest.Editor;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;

public class AndroidManifestTests
{
    private string tempProjectPath;
    private string xrManifestTemplateFilePath;
    private string xrLibraryManifestFilePath;
    private string unityLibraryManifestFilePath;
    private DirectoryInfo dirInfo;
    private XRManagerSettings mockXrSettings;
    private Type supportedLoaderType;

    [SetUp]
    public void SetUp()
    {
        tempProjectPath = FileUtil.GetUniqueTempPathInProject();
        dirInfo = Directory.CreateDirectory(tempProjectPath);

        var xrPackagePath = dirInfo.CreateSubdirectory(string.Join(Path.DirectorySeparatorChar.ToString(), "xrPackage", "xrmanifest.androidlib"));
        var xrLibraryPath = dirInfo.CreateSubdirectory("xrmanifest.androidlib");
        var unityLibraryPath = dirInfo.CreateSubdirectory(string.Join(Path.DirectorySeparatorChar.ToString(), "src", "main"));

        xrManifestTemplateFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), xrPackagePath.FullName,  "AndroidManifest.xml");
        xrLibraryManifestFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), xrLibraryPath.FullName, "AndroidManifest.xml");
        unityLibraryManifestFilePath = string.Join(Path.DirectorySeparatorChar.ToString(), unityLibraryPath.FullName, "AndroidManifest.xml");

        CreateMockManifestDocument(xrManifestTemplateFilePath);
        CreateMockManifestDocument(xrLibraryManifestFilePath);
        CreateMockManifestDocument(unityLibraryManifestFilePath);

        mockXrSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
        supportedLoaderType = typeof(MockXrLoader);
        mockXrSettings.TrySetLoaders(new List<XRLoader>
        {
            ScriptableObject.CreateInstance<MockXrLoader>()
        });
    }

    [TearDown]
    public void TearDown()
    {
        dirInfo.Delete(true);
        ScriptableObject.DestroyImmediate(mockXrSettings);
    }

    [Test]
    public void AndroidManifestProcessor_AddOneNewManifestElement()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath = new List<string> { "manifest", "application", "meta-data" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "custom-data" },
            { "value", "test-data" },
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedLibraryManifest = GetXrLibraryManifest();
        var nodes = updatedLibraryManifest.SelectNodes(string.Join("/", newElementPath));
        Assert.AreEqual(
            1,
            nodes.Count,
            "Additional elements exist in the Manifest when expecting 1");

        var attributeList = nodes[0].Attributes;
        Assert.AreEqual(
            newElementAttributes.Count,
            attributeList.Count,
            "Attribute count in element doesn't match expected count");

        AssertAttributesAreEqual(nodes[0].Name, newElementAttributes, attributeList);
    }

    [Test]
    public void AndroidManifestProcessor_AddTwoNewManifestElements()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath = new List<string> { "manifest", "application", "meta-data" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "custom-data" },
            { "value", "test-data" },
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    },
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedLibraryManifest = GetXrLibraryManifest();
        var nodes = updatedLibraryManifest.SelectNodes(string.Join("/", newElementPath));
        Assert.AreEqual(
            2,
            nodes.Count,
            "Additional elements exist in the Manifest when expecting 2");

        foreach(XmlElement node in nodes)
        {
            var attributeList = node.Attributes;
            Assert.AreEqual(
                newElementAttributes.Count,
                attributeList.Count,
                "Attribute count in element doesn't match expected count");

            AssertAttributesAreEqual(node.Name, newElementAttributes, attributeList);
        }
    }

    [Test]
    public void AndroidManifestProcessor_CreateSingleNewManifestElementFromTwoOverridenElements()
    {
        // Use the Assert class to test conditions
        var processor = CreateProcessor();

        // Initialize data
        var overrideElementPath = new List<string> { "manifest", "application", "activity" };
        var overrideElement1Attributes = new Dictionary<string, string>();
        var overrideElement2Attributes = new Dictionary<string, string>();
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                OverrideElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = overrideElementPath,
                        Attributes = overrideElement1Attributes
                    },
                    new ManifestElement()
                    {
                        ElementPath = overrideElementPath,
                        Attributes = overrideElement2Attributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedLibraryManifest = GetXrLibraryManifest();
        var nodes = updatedLibraryManifest.SelectNodes(string.Join("/", overrideElementPath));
        Assert.AreEqual(
            1,
            nodes.Count,
            "Additional elements exist in the Manifest when expecting 1");

        var attributeList = nodes[0].Attributes;
        var expectedElementAttrributes = MergeDictionaries(overrideElement1Attributes, overrideElement2Attributes);
        Assert.AreEqual(
            expectedElementAttrributes.Count,
            attributeList.Count,
            $"Attribute count in element doesn't match expected {expectedElementAttrributes.Count}");

        AssertAttributesAreEqual(nodes[0].Name, expectedElementAttrributes, attributeList);
    }


    [Test]
    public void AndroidManifestProcessor_UpdateExistingElementWithOverridenElement()
    {
        // Use the Assert class to test conditions
        var processor = CreateProcessor();

        // Initialize data
        var overrideElementPath = new List<string> { "manifest", "application", "activity" };
        var existingElementAttributes = new Dictionary<string, string>()
        {
            { "name", "com.test.app" }
        };
        var overrideElementAttributes = new Dictionary<string, string>()
        {
            { "isGame", "true" },
            { "testOnly", "true" },
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                OverrideElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = overrideElementPath,
                        Attributes = overrideElementAttributes
                    }
                }
            })
        };

        // Prepare test document
        var libManifest = GetXrLibraryManifest();
        libManifest.CreateNewElement(overrideElementPath, existingElementAttributes);
        libManifest.Save();

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedLibraryManifest = GetXrLibraryManifest();
        var nodes = updatedLibraryManifest.SelectNodes(string.Join("/", overrideElementPath));
        Assert.AreEqual(
            1,
            nodes.Count,
            "Additional elements exist in the Manifest when expecting 1");

        var attributeList = nodes[0].Attributes;
        var expectedElementAttrributes = MergeDictionaries(existingElementAttributes, overrideElementAttributes);
        Assert.AreEqual(
            expectedElementAttrributes.Count,
            attributeList.Count,
            $"Attribute count {attributeList.Count} in element doesn't match expected {expectedElementAttrributes.Count}");

        AssertAttributesAreEqual(nodes[0].Name, expectedElementAttrributes, attributeList);
    }

    [Test]
    public void AndroidManifestProcessor_DeleteExistingManifestElement()
    {
        var processor = CreateProcessor();

        // Initialize data
        var deletedElementPath = new List<string> { "manifest", "uses-permission" };
        var deletedElementAttributes = new Dictionary<string, string>()
        {
            { "name", "BLUETOOTH" }
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                RemoveElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = deletedElementPath,
                        Attributes = deletedElementAttributes
                    }
                }
            })
        };

        // Prepare test document
        var appManifest = GetUnityLibraryManifest();
        appManifest.CreateNewElement(deletedElementPath, deletedElementAttributes);
        appManifest.Save();

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedAppManifest = GetXrLibraryManifest();
        var removedElementPath = string.Join("/", deletedElementPath);
        var removedNodes = updatedAppManifest.SelectNodes(removedElementPath);
        Assert.AreEqual(
            0,
            removedNodes.Count,
            $"Expected element in path \"{removedElementPath}\" wasn't deleted");
    }

    [Test]
    public void AndroidManifestProcessor_DontModifyManifestIfNoSupportedLoadersAdded()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath = new List<string> { "manifest", "application", "meta-data" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "custom-data" },
            { "value", "test-data" },
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    typeof(object) // Dummy object representing an inactive loader
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        // Validate
        var updatedLibraryManifest = GetXrLibraryManifest();
        var nodes = updatedLibraryManifest.SelectNodes(string.Join("/", newElementPath));
        Assert.AreEqual(
            0,
            nodes.Count,
            "Elements exist in the Manifest when expecting 0");
    }

#if UNITY_2021_1_OR_NEWER
    [Test]
    public void AndroidManifestProcessor_AddNewIntentsOnlyInUnityLibraryManifest()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath =
            new List<string> { "manifest", "application", "activity", "intent-filter", "category" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "com.oculus.intent.category.VR" }
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        var elementPath = string.Join("/", newElementPath);

        // Validate that the intent is created in Unity library manifest
        var unityLibManifest = GetUnityLibraryManifest();
        var addedNodes = unityLibManifest.SelectNodes(elementPath);
        Assert.AreEqual(
            1,
            addedNodes.Count,
            $"Expected new element in path \"{elementPath}\" in Unity Library manifest");

        // Validate that the intent isn't created in XR Library manifest
        var xrLibManifest = GetXrLibraryManifest();
        var emptyNodes = xrLibManifest.SelectNodes(elementPath);
        Assert.AreEqual(
            0,
            emptyNodes.Count,
            $"Expected no new element in path \"{elementPath}\" in XR Library manifest");
    }

    [Test]
    public void AndroidManifestProcessor_KeepOnlyOneIntentOfTheSameType()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath =
            new List<string> { "manifest", "application", "activity", "intent-filter", "category" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "com.oculus.intent.category.VR" }
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Prepare test document
        var appManifest = GetUnityLibraryManifest();
        appManifest.CreateNewElement(newElementPath, newElementAttributes);
        appManifest.Save();

        // Execute
        processor.ProcessManifestRequirements(providers);

        var elementPath = string.Join("/", newElementPath);

        // Validate that only one intent of the same kind is in the manifest
        var unityLibManifest = GetUnityLibraryManifest();
        var addedNodes = unityLibManifest.SelectNodes(elementPath);
        Assert.AreEqual(
            1,
            addedNodes.Count,
            $"Expected only 1 element in path \"{elementPath}\" in Unity Library manifest");

    }

    [Test]
    public void AndroidManifestProcessor_AddManyIntentsOfTheSameTypeButKeepOnlyOne()
    {
        var processor = CreateProcessor();

        // Initialize data
        var newElementPath =
            new List<string> { "manifest", "application", "activity", "intent-filter", "category" };
        var newElementAttributes = new Dictionary<string, string>()
        {
            { "name", "com.oculus.intent.category.VR" }
        };
        var providers = new List<IAndroidManifestRequirementProvider>()
        {
            new MockManifestRequirementProvider(new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>
                {
                    supportedLoaderType
                },
                NewElements = new List<ManifestElement>()
                {
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    },
                    new ManifestElement()
                    {
                        ElementPath = newElementPath,
                        Attributes = newElementAttributes
                    }
                }
            })
        };

        // Execute
        processor.ProcessManifestRequirements(providers);

        var elementPath = string.Join("/", newElementPath);

        // Validate that only one intent of the same kind is in the manifest
        var unityLibManifest = GetUnityLibraryManifest();
        var addedNodes = unityLibManifest.SelectNodes(elementPath);
        Assert.AreEqual(
            1,
            addedNodes.Count,
            $"Expected only 1 element in path \"{elementPath}\" in Unity Library manifest");

    }
#endif

    private AndroidManifestDocument GetXrLibraryManifest()
    {
#if UNITY_2021_1_OR_NEWER
        return new AndroidManifestDocument(xrLibraryManifestFilePath);
#else
        // Unity 2020 and lower use the same manifest for XR entries as the rest of the app
        return GetUnityLibraryManifest();
#endif
    }

    private AndroidManifestDocument GetUnityLibraryManifest()
    {
        return new AndroidManifestDocument(unityLibraryManifestFilePath);
    }

    private AndroidManifestProcessor CreateProcessor()
    {
#if UNITY_2021_1_OR_NEWER
        return new AndroidManifestProcessor(
            tempProjectPath,
            tempProjectPath,
            mockXrSettings);
#else
        return new AndroidManifestProcessor(tempProjectPath, mockXrSettings);
#endif
    }

    private void CreateMockManifestDocument(string filePath)
    {
        var manifestDocument = new AndroidManifestDocument();
        var manifestNode = manifestDocument.CreateElement("manifest");
        manifestNode.SetAttribute("xmlns:android", "http://schemas.android.com/apk/res/android");
        manifestDocument.AppendChild(manifestNode);
        var applicationNode = manifestDocument.CreateElement("application");
        manifestNode.AppendChild(applicationNode);
        manifestDocument.SaveAs(filePath);
    }

    private void AssertAttributesAreEqual(
        string elementName,
        Dictionary<string, string> expectedAttributes,
        XmlAttributeCollection attributes)
    {
        foreach (XmlAttribute attrib in attributes)
        {
            var attributeName = attrib.Name.Split(':').Last(); // Values are returned with preffixed namespace name, pick only the attribute name
            if (!expectedAttributes.Contains(new KeyValuePair<string, string>(attributeName, attrib.Value)))
            {
                Assert.Fail($"Unexpected attribute \"{attrib.Name}\" " +
                    $"with value \"{attrib.Value}\" found in element {elementName}");
            }
        }
    }

    private Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
    {
        return new List<Dictionary<TKey, TValue>> { dict1, dict2 }
        .SelectMany(dict => dict)
        .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private class MockManifestRequirementProvider : IAndroidManifestRequirementProvider
    {
        private readonly ManifestRequirement requirement;

        public MockManifestRequirementProvider(ManifestRequirement mockRequirments)
        {
            requirement = mockRequirments;
        }

        public ManifestRequirement ProvideManifestRequirement()
        {
            return requirement;
        }
    }

    private class MockXrLoader : XRLoader
    {
        public override T GetLoadedSubsystem<T>()
        {
            throw new NotImplementedException();
        }
    }
}
