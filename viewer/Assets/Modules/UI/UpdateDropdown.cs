using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace pfc.Fulldome
{
    public class UpdateDropdown : MonoBehaviour
    {
        private Dropdown dropdown;
        public GameObject[] targets;
        
        private void OnEnable()
        {
            dropdown = GetComponentInChildren<Dropdown>();
            MediaSourceSelectionContainer.OnSelection.AddListener(SetDropdownValue);
            SetDropdownValue(MediaSourceSelectionContainer.currentSelection);
        }

        private void OnDisable()
        {
            MediaSourceSelectionContainer.OnSelection.RemoveListener(SetDropdownValue);
        }

        private void SetDropdownValue((GameObject go, string path) arg0)
        {
            // we expect value 0 to be None
            if(!arg0.go || !targets.Contains(arg0.go)) dropdown.SetValueWithoutNotify(0);
            else
            {
                var path = arg0.path;
                var options = dropdown.options;
                for (var i = 0; i < options.Count; i++)
                {
                    var option = options[i];
                    if (!path.Contains(option.text)) continue;
                    dropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
        }

        
    }
}