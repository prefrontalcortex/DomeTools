using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Directory = System.IO.Directory;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace pfc.Fulldome
{
    public class FilesSourceUI : MonoBehaviour
    {
        private Dropdown dropdown;
        public VideoPlayer videoPlayer;
        public SetImageTexture imagePlayer;
        
        private static string[] videoExtensions = {".mp4", ".mov", ".avi", ".wmv", ".mkv"};
        private static string[] imageExtensions = {".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff"};
        private static List<string> extensions = videoExtensions.Concat(imageExtensions).ToList();
      
        private static string userAssetsPath => Application.persistentDataPath;
        private const string UserAssetKey = "pfc-open-persistent-data-path";
        
        private void Start()
        {
            CollectFilesAndFillDropdown();
        }

        private void CollectFilesAndFillDropdown()
        {
            var currentPath = default(string);
            // If we already have options, we want to remember which file was selected before updating the dropdown options
            if (dropdown.options.Any())
                currentPath = (dropdown.options[dropdown.value] as FileOptionData)?.absolutePath;
            
            var files = GetEligibleFiles("User", userAssetsPath, userAssetsPath, extensions);
            files.Add(new FileEntry() { type = "User", displayName = "Open " + Application.persistentDataPath, absolutePath = UserAssetKey });
            files.AddRange(GetStreamingAssets());
            
            files = files
                .Distinct()
                .OrderByDescending(x => x.type)
                .ThenBy(x => x.absolutePath)
                .ToList();
           
            dropdown.options = new List<Dropdown.OptionData>() { new FileOptionData(null, "None", null) };
            dropdown.options.AddRange(
                files
                    .Select(f => new FileOptionData(f.type, f.displayName, f.absolutePath))
                    .ToList()
            );
            
            // Restore previously selected file
            if (currentPath != null)
            {
                var index = dropdown.options.FindIndex(f => (f as FileOptionData)?.absolutePath == currentPath);
                if (index != -1)
                {
                    dropdown.SetValueWithoutNotify(index);
                }
                else
                {
                    dropdown.SetValueWithoutNotify(0);
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) return;
            if (!dropdown) return;
            CollectFilesAndFillDropdown();
        }

        private void OnEnable()
        {
            dropdown = GetComponentInChildren<Dropdown>();
            dropdown.onValueChanged.AddListener(ActivateObjectFromIndex);
        }

        private void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(ActivateObjectFromIndex);
        }
        
        private void ActivateObjectFromIndex(int index)
        {
            if (index == 0)
            {
                MediaSourceSelectionContainer.Deselect();
                return;
            }
            var path = (dropdown.options[index] as FileOptionData)!.absolutePath;
            ActivateObjectFromPath(path);
        }
        
        private void ActivateObjectFromPath(string path)
        {
            Debug.Log($"Activating: {path}");
            if (path == UserAssetKey)
            {
                // Open explorer / finder etc.
                Application.OpenURL(userAssetsPath);
                return;
            }
            
            var ext = Path.GetExtension(path).ToLower();
            if (videoExtensions.Contains(ext))
            {
                videoPlayer.url = $"file://{path}";
                MediaSourceSelectionContainer.SetSelection(videoPlayer.gameObject, path);
                videoPlayer.Play();
            }
            else if (imageExtensions.Contains(ext))
            {
                imagePlayer.path = path;
                MediaSourceSelectionContainer.SetSelection(imagePlayer.gameObject, path);
                imagePlayer.Set();
            }
        }

        private static List<FileEntry> GetEligibleFiles(string type, string rootPath, string path, List<string> eligibleExtensions)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not get files from {path}: {e.Message}");
                return new List<FileEntry>();
            }
            
            var eligibleFiles = new List<FileEntry>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (eligibleExtensions.Contains(ext))
                {
                    var f = file.Replace('\\', '/');
                    eligibleFiles.Add(new FileEntry
                    {
                        displayName = f.Substring(rootPath.Length + 1),
                        absolutePath = f,
                        type = type,
                    });
                }
            }
            foreach(var dir in Directory.GetDirectories(path))
            {
                eligibleFiles.AddRange(GetEligibleFiles(type, rootPath, dir, eligibleExtensions));
            }
            
            return eligibleFiles;
        }

        [Serializable]
        public class FileEntry
        {
            public string type;
            public string displayName;
            public string absolutePath;

            public override string ToString() => $"{displayName} ({absolutePath}) [{type}]";
        }
        
        public class FileOptionData : Dropdown.OptionData
        {
            public string absolutePath;

            public FileOptionData(string type, string displayName, string absolutePath)
            {
                this.absolutePath = absolutePath;

                var file = Path.GetFileName(displayName);
                var dir = Path.GetDirectoryName(displayName)?.Replace("\\", "/") + "/";
                
                this.text = (string.IsNullOrEmpty(type) ? "" : $"<size=\"9px\">[{type.ToUpper()}]</size>  ") + (dir != "/" ? $"<color=\"#999\">{dir}</color>" : "") + $"{file}";
            }
        }

        [SerializeField] 
        private List<FileEntry> streamingAssets;
        
        private List<FileEntry> GetStreamingAssets()
        {
            return streamingAssets;
        }

#if UNITY_EDITOR
        internal void UpdateFilesFromStreamingAssets()
        {
            // collect list of StreamingAssets as flat file list
            streamingAssets = GetEligibleFiles("Demo", Application.streamingAssetsPath, Application.streamingAssetsPath, extensions);
            Debug.Log("Added StreamingAssets to FilesSourceUI:\n" + string.Join("\n", streamingAssets));
        }
#endif
    }

#if UNITY_EDITOR
    internal class Postprocessor: IProcessSceneWithReport
    {
        public int callbackOrder => 0;
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // find FilesSourceUI in the scene
            var filesSourceUI = Object.FindAnyObjectByType<FilesSourceUI>();
            if (!filesSourceUI) return;
            
            filesSourceUI.UpdateFilesFromStreamingAssets();
        }
    }
#endif
}