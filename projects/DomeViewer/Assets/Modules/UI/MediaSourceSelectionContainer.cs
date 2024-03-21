using UnityEngine;
using UnityEngine.Events;

namespace pfc.Fulldome
{
    public class MediaSourceSelectionContainer : MonoBehaviour
    {
        public GameObject defaultSelection;

        public static MediaSourceSelectionContainer Instance
        {
            get
            {
                if (!_instance) _instance = FindObjectOfType<MediaSourceSelectionContainer>();
                return _instance;
            }
        }

        private static MediaSourceSelectionContainer _instance;
        public static UnityEvent<(GameObject go, string path)> OnSelection = new UnityEvent<(GameObject, string)>();
        public static (GameObject go, string path) currentSelection = (null, "");

        private void OnEnable()
        {
            SetSelection(defaultSelection, "");
        }

        public static void SetSelection(GameObject selection, string path)
        {
            for (int i = 0; i < Instance.transform.childCount; i++)
            {
                var child = Instance.transform.GetChild(i);
                if (child.gameObject != selection) child.gameObject.SetActive(false);
                else child.gameObject.SetActive(true);
            }

            currentSelection = (selection, path);
            OnSelection.Invoke((selection, path));
        }

        public static void Deselect()
        {
            SetSelection(Instance.defaultSelection, "");
        }
    }
}