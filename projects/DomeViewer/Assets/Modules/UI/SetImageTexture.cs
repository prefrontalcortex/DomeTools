using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace pfc.Fulldome
{
    [ExecuteInEditMode]
    public class SetImageTexture : MonoBehaviour
    {
        public Texture2D texture;
        public string path;
        public RenderTexture target;

        /// <summary>Raised with (path, reason) when an image could not be loaded.</summary>
        public event Action<string, string> LoadFailed;

        private void OnValidate()
        {
            if (enabled) Set();
        }

        private void OnEnable()
        {
            Set();
        }

        public void Set()
        {
            // fallback to the provided texture
            if (string.IsNullOrEmpty(path) && texture && target)
            {
                Graphics.Blit(texture, target);
                return;
            }

            if (string.IsNullOrEmpty(path)) return;

            // A slow load must not finish after a newer one and overwrite it.
            StopAllCoroutines();
            StartCoroutine(LoadTexture());
        }

        private IEnumerator LoadTexture()
        {
            var loadingPath = path;
            // Local files need the file:// scheme, otherwise UnityWebRequest treats the path as a host (fails on Android).
            var url = loadingPath.Contains("://") ? loadingPath : $"file://{loadingPath}";
            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load texture from {url}: {request.error}");
                LoadFailed?.Invoke(loadingPath, request.error);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(request);
            var maxSize = SystemInfo.maxTextureSize;
            if (tex.width > maxSize || tex.height > maxSize)
            {
                var reason = $"image is {tex.width}x{tex.height}, this device supports at most {maxSize}x{maxSize}";
                Debug.LogError($"Failed to load texture from {url}: {reason}");
                if (Application.isPlaying) Destroy(tex);
                else DestroyImmediate(tex);
                LoadFailed?.Invoke(loadingPath, reason);
                yield break;
            }

            Graphics.Blit(tex, target);
            // the render texture holds a copy now, free the downloaded one
            if (Application.isPlaying) Destroy(tex);
            else DestroyImmediate(tex);
        }
    }

#if UNITY_EDITOR
    internal class TextureUpdateHelper: AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            // For some reason, Unity clears the render texture on save,
            // so we're hooking into the save callbacks to restore the texture.
            EditorApplication.delayCall += () =>
            {
                var obj = Object.FindObjectsByType<SetImageTexture>(FindObjectsSortMode.None);
                foreach (var o in obj)
                {
                    if (!o) continue;
                    o.Set();
                }
            };
            return paths;
        }
    }
#endif
}