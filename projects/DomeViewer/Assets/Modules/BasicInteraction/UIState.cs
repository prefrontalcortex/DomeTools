using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
{
    public class UIState : MonoBehaviour
    {
        // We assume that all UI panels are children of this root object.
        public GameObject uiRoot;

        private void Start()
        {
            // find the first enabled panel and set it as the active panel.
            // this ensures there's NEVER two panels active at the same time.
            for (var i = 0; i < uiRoot.transform.childCount; i++)
            {
                var child = uiRoot.transform.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                SetUIPanel(child.gameObject);
                return;
            }

            SetUIPanel(null);
        }

        // Set the active panel. This will disable all other panels.
        public void SetUIPanel(GameObject panel)
        {
            // if this panel is already on, we close them all
            if (panel && panel.activeSelf)
            {
                SetUIPanel(null);
                return;
            }

            // disable all roots when they're not the specified panel
            for (var i = 0; i < uiRoot.transform.childCount; i++)
            {
                var child = uiRoot.transform.GetChild(i);
                child.gameObject.SetActive(child.gameObject == panel);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UIState))]
    public class UIStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            var t = (UIState)target;
            for (var i = 0; i < t.uiRoot.transform.childCount; i++)
            {
                var child = t.uiRoot.transform.GetChild(i);
                if (GUILayout.Button(child.name))
                {
                    t.SetUIPanel(child.gameObject);
                }
            }
        }
    }
#endif
}
