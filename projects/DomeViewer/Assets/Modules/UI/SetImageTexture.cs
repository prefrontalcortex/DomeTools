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

            StartCoroutine(LoadTexture());
        }
        
        private IEnumerator LoadTexture()
        {
            var request = UnityWebRequestTexture.GetTexture(path);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load texture from {path}: {request.error}");
                yield break;
            }
            
            var tex = DownloadHandlerTexture.GetContent(request);
            Graphics.Blit(tex, target);
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