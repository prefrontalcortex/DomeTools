using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Directory = System.IO.Directory;

namespace pfc.Fulldome
{
    public class FilesSourceUI : MonoBehaviour
    {
        private Dropdown dd;
        public VideoPlayer videoPlayer;
        public SetImageTexture imagePlayer;
        
        static string[] videoExtensions = {".mp4", ".mov", ".avi", ".wmv", ".mkv"};
        static string[] imageExtensions = {".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff"};
        
#if UNITY_ANDROID && !UNITY_EDITOR
        static string userAssetsPath = "/sdcard/Download";
#else
        static string userAssetsPath = Application.streamingAssetsPath;
#endif
        
        private void OnEnable()
        {
            dd = GetComponentInChildren<Dropdown>();
            var files = GetEligibleFiles(userAssetsPath, videoExtensions.Concat(imageExtensions).ToList());
            // Debug.Log($"Cool! We got {files.Count} files from {userAssetsPath}");
            dd.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData("None") };
            dd.options.AddRange(
                files
                    .Select(f => new Dropdown.OptionData(f.Substring(userAssetsPath.Length+1)))
                    .ToList()
                );
            dd.onValueChanged.AddListener(ActivateObjectFromIndex);
        }

        private void OnDisable()
        {
            dd.onValueChanged.RemoveListener(ActivateObjectFromIndex);
        }
        
        private void ActivateObjectFromIndex(int index)
        {
            if (index == 0)
            {
                MediaSourceSelectionContainer.Deselect();
                return;
            }
            var path = dd.options[index].text;
            ActivateObjectFromPath(userAssetsPath+"/"+path);
        }
        
        private void ActivateObjectFromPath(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            if (videoExtensions.Contains(ext))
            {
                videoPlayer.url = "file://"+path;
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

        List<string> GetEligibleFiles(string path, List<string> eligibleExtensions)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(path);
                //create a string from the array elements
                // Debug.Log($"All Eligible Files in {path} : {string.Join(",", files)}");
            }
            catch(Exception e)
            {
                Debug.LogWarning("Could not get files from "+path+": "+e.Message);
                return new List<string>();
            }
            var eligibleFiles = new List<string>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (eligibleExtensions.Contains(ext)) eligibleFiles.Add(file);
            }
            foreach(var dir in Directory.GetDirectories(path))
            {
                eligibleFiles.AddRange(GetEligibleFiles(dir, eligibleExtensions));
            }
            return eligibleFiles;
        }
    }
}