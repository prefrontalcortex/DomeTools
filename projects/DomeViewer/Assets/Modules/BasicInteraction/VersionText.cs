using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace pfc.Fulldome
{
    public class VersionText : MonoBehaviour { }

#if UNITY_EDITOR
    internal class VersionUpdater : IProcessSceneWithReport
    {
        public int callbackOrder { get; }
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var versionTexts = Object.FindObjectsByType<VersionText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in versionTexts)
            {
                var textComponent = text.GetComponent<Text>();
                if (!textComponent) continue;
                var timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
                textComponent.text = Application.version + "   " + "r" + PlayerSettings.Android.bundleVersionCode + "   " + timestamp;
            }
        }
    }
#endif
}