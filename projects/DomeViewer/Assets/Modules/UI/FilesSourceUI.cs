// #define DOME_VIEWER_DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using Directory = System.IO.Directory;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
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
        
#if UNITY_ANDROID
        // Only list formats that Android / Quest can actually decode.
        private static string[] videoExtensions = {".mp4", ".mov"};
        private static string[] imageExtensions = {".png", ".jpg", ".jpeg"};
#else
        private static string[] videoExtensions = {".mp4", ".mov", ".avi", ".wmv", ".mkv"};
        private static string[] imageExtensions = {".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff"};
#endif
        private static List<string> extensions = videoExtensions.Concat(imageExtensions).ToList();
      
        private static string userAssetsPath => Application.persistentDataPath;
        private const string UserAssetKey = "pfc-open-persistent-data-path";
        private const string PickFileKey = "pfc-pick-file";

        // Files chosen with the system file picker in this session (absolutePath is a content:// uri).
        private static readonly List<FileEntry> pickedFiles = new List<FileEntry>();

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
            
            files = files
                .Distinct()
                .OrderByDescending(x => x.type)
                .ThenBy(x => x.absolutePath)
                .ToList();
            
            files.Add(new FileEntry() { type = null, displayName = "Place assets in " + Application.persistentDataPath, absolutePath = UserAssetKey });
            if (AndroidFilePicker.IsSupported)
            {
                files.Add(new FileEntry() { type = null, displayName = "Open file from this device…", absolutePath = PickFileKey });
                files.AddRange(pickedFiles);
            }

            // restore absolute path on this platform
            var streamingAssetsData = GetStreamingAssets();
            foreach (var f in streamingAssetsData)
            {
                if (!f.absolutePath.StartsWith(Application.streamingAssetsPath))
                    f.absolutePath = Path.Combine(Application.streamingAssetsPath, f.absolutePath);
            }
            
            files.AddRange(streamingAssetsData);
            
            Log("All files:\n" + string.Join("\n", files.Select(f => f.ToString()).ToArray()));
           
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
                    // The selected file is gone: show "None" and stop playing it.
                    dropdown.SetValueWithoutNotify(0);
                    if (MediaSourceSelectionContainer.currentSelection.path == currentPath)
                        MediaSourceSelectionContainer.Deselect();
                }
            }
        }

        // A file that cannot be played is reset to "None" so the UI doesn't pretend it is playing.
        private void FailSelection(string path, string reason)
        {
            Debug.LogWarning($"Could not play {path}: {reason}");
            if (MediaSourceSelectionContainer.currentSelection.path != path) return;
            if (dropdown) dropdown.SetValueWithoutNotify(0);
            MediaSourceSelectionContainer.Deselect();
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            if (pickedVideo != null)
            {
                Debug.LogWarning($"Playing {pickedVideo.displayName} failed (attempt {pickedVideoAttempt}): {message}");
                PlayPickedVideo(pickedVideoAttempt + 1);
                return;
            }
            FailSelection(MediaSourceSelectionContainer.currentSelection.path, message);
        }

        // --- Files chosen with the Android system file picker -------------------------------------

        private FileEntry pickedVideo;
        private int pickedVideoAttempt;

        private void SelectInDropdown(string path)
        {
            var index = dropdown.options.FindIndex(f => (f as FileOptionData)?.absolutePath == path);
            dropdown.SetValueWithoutNotify(Mathf.Max(index, 0));
            dropdown.RefreshShownValue();
        }

        private void OnFilePicked(string uri, string name)
        {
            Log($"Picked {name} ({uri})");
            var entry = pickedFiles.Find(f => f.absolutePath == uri);
            if (entry == null)
            {
                entry = new FileEntry { type = "Device", displayName = name, absolutePath = uri };
                pickedFiles.Add(entry);
            }
            CollectFilesAndFillDropdown();
            SelectInDropdown(uri);
            ActivatePickedFile(entry);
        }

        private void OnFilePickFailed(string reason)
        {
            if (reason != null) Debug.LogWarning($"Could not open the file picker: {reason}");
            // Cancelled or failed: show what is actually playing again.
            SelectInDropdown(MediaSourceSelectionContainer.currentSelection.path);
        }

        private void ActivatePickedFile(FileEntry entry)
        {
            var ext = Path.GetExtension(entry.displayName).ToLower();
            if (videoExtensions.Contains(ext))
            {
                pickedVideo = entry;
                MediaSourceSelectionContainer.SetSelection(videoPlayer.gameObject, entry.absolutePath);
                PlayPickedVideo(0);
            }
            else if (imageExtensions.Contains(ext))
            {
                pickedVideo = null;
                AndroidFilePicker.CopyToCache(entry.absolutePath, entry.displayName, (file, _) =>
                {
                    imagePlayer.path = file;
                    MediaSourceSelectionContainer.SetSelection(imagePlayer.gameObject, entry.absolutePath);
                    imagePlayer.Set();
                }, reason => FailSelection(entry.absolutePath, reason));
            }
            else
            {
                FailSelection(entry.absolutePath, $"unsupported file type '{ext}'");
                SelectInDropdown(MediaSourceSelectionContainer.currentSelection.path);
            }
        }

        // The video player cannot open content:// uris (tested on Quest 3, Unity 6000.3), so the file is
        // streamed from a local http server, which starts at once even for very large files.
        // 0 = local stream, 1 = copy into the app cache as a fallback.
        private void PlayPickedVideo(int attempt)
        {
            var entry = pickedVideo;
            pickedVideoAttempt = attempt;
            switch (attempt)
            {
                case 0:
                    var stream = AndroidFilePicker.Serve(entry.absolutePath, entry.displayName);
                    if (stream == null) PlayPickedVideo(1);
                    else StartVideo(stream);
                    break;
                case 1:
                    Debug.Log($"Copying {entry.displayName} into the app cache");
                    AndroidFilePicker.CopyToCache(entry.absolutePath, entry.displayName, (file, _) =>
                    {
                        if (pickedVideo == entry) StartVideo($"file://{file}");
                    }, reason => { pickedVideo = null; FailSelection(entry.absolutePath, reason); });
                    break;
                default:
                    pickedVideo = null;
                    FailSelection(entry.absolutePath, "the video could not be played");
                    break;
            }
        }

        private void StartVideo(string url)
        {
            videoPlayer.Stop();
            videoPlayer.url = url;
            videoPlayer.Play();
            if (!isActiveAndEnabled) return;
            StopCoroutine(nameof(WatchPickedVideo));
            StartCoroutine(nameof(WatchPickedVideo));
        }

        // Some failures never raise errorReceived: move on if the video isn't prepared after a while.
        private System.Collections.IEnumerator WatchPickedVideo()
        {
            var attempt = pickedVideoAttempt;
            var entry = pickedVideo;
            yield return new WaitForSecondsRealtime(10f);
            if (pickedVideo != entry || pickedVideoAttempt != attempt || videoPlayer.isPrepared) yield break;
            Debug.LogWarning($"Playing {entry.displayName} timed out (attempt {attempt})");
            PlayPickedVideo(attempt + 1);
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
            if (videoPlayer) videoPlayer.errorReceived += OnVideoError;
            if (imagePlayer) imagePlayer.LoadFailed += FailSelection;
        }

        private void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(ActivateObjectFromIndex);
            if (videoPlayer) videoPlayer.errorReceived -= OnVideoError;
            if (imagePlayer) imagePlayer.LoadFailed -= FailSelection;
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
            Log($"Activating: {path}");
            if (path == UserAssetKey)
            {
                // Open explorer / finder etc.
                Application.OpenURL(userAssetsPath);
                return;
            }

            if (path == PickFileKey)
            {
                AndroidFilePicker.Pick(new[] {"video/*", "image/*"}, OnFilePicked, OnFilePickFailed);
                return;
            }

            var picked = pickedFiles.Find(f => f.absolutePath == path);
            if (picked != null)
            {
                ActivatePickedFile(picked);
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            if (videoExtensions.Contains(ext))
            {
                pickedVideo = null;
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
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not get folders from {path}: {e.Message}");
                return eligibleFiles;
            }
            foreach(var dir in directories)
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
        [ContextMenu("Update Now")]
        internal void UpdateFilesFromStreamingAssets()
        {
            // collect list of StreamingAssets as flat file list
            streamingAssets = GetEligibleFiles("Demo", Application.streamingAssetsPath, Application.streamingAssetsPath, extensions);
            foreach (var s in streamingAssets)
            {
                // truncate the StreamingAssets path so we can append it again at runtime for the correct platform
                s.absolutePath = s.absolutePath.Substring(Application.streamingAssetsPath.Length + 1);
            }
            
            EditorUtility.SetDirty(this);
            Log("Collected StreamingAssets files: " + streamingAssets.Count + "\n" + string.Join("\n", streamingAssets.Select(f => f.ToString()).ToArray()));
        }
#endif

        [Conditional("DOME_VIEWER_DEBUG")]
        private static void Log(object message)
        {
            Debug.Log(message);
        }
    }

#if UNITY_EDITOR
    internal class Postprocessor: IProcessSceneWithReport
    {
        public int callbackOrder => 0;
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var filesSourceUI = Object.FindObjectsByType<FilesSourceUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var ui in filesSourceUI)
                ui.UpdateFilesFromStreamingAssets();
        }
    }
#endif
}