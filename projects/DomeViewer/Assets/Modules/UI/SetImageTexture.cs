using UnityEngine;
using System.IO;
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
            if ((string.IsNullOrEmpty(path) || !File.Exists(path)) && texture && target)
            {
                Graphics.Blit(texture, target);
                return;
            }
            
            if (!File.Exists(path))
            {
                Debug.LogWarning($"No File at {path}!");
                return;
            }
            
            var raw = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(raw);
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