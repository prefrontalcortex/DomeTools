using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Klak.Ndi;
using UnityEngine;
using UnityEngine.UI;

namespace pfc.Fulldome
{
    public class NDISourceUI : MonoBehaviour
    {
        public NdiReceiver receiver;
        public Button refresh;
        public Dropdown sources;

        public bool immediatelyChooseAvailableStream = true;

        [Tooltip("Seconds between automatic source list updates while this UI is visible. 0 disables it.")]
        public float autoRefreshInterval = 3f;

        public static event Action<string> onNdiChanged;

        private void OnEnable()
        {
            RefreshSources();
            if (immediatelyChooseAvailableStream && string.IsNullOrEmpty(receiver.ndiName))
                receiver.ndiName = NdiFinder.EnumerateSourceNames().FirstOrDefault();

            if (autoRefreshInterval > 0)
                pollRoutine = StartCoroutine(PollSources());
        }

        private void OnDisable()
        {
            if (pollRoutine != null) StopCoroutine(pollRoutine);
            pollRoutine = null;
        }

        private void Start()
        {
            refresh.onClick.AddListener(RefreshSources);
            sources.onValueChanged.AddListener(SourceChanged);
        }

        private void SourceChanged(int arg0)
        {
            if (arg0 == 0)
            {
                MediaSourceSelectionContainer.Deselect();
                return;
            }
            var option = sources.options[arg0] as NdiOptionData;
            if (option == null) return;
            
            var newSourceName = option.sourceName;
            
            Debug.Log("Changing source to " +  newSourceName);
            receiver.ndiName = newSourceName;
            MediaSourceSelectionContainer.SetSelection(receiver.gameObject, newSourceName);
            onNdiChanged?.Invoke(newSourceName);
        }

        private class NdiOptionData : Dropdown.OptionData
        {
            public string sourceName;
            
            public NdiOptionData(string sourceName)
            {
                this.sourceName = sourceName;
                
                if (this.sourceName.Contains("(") && this.sourceName.Contains(")"))
                {
                    var parts = this.sourceName.Split(new[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        this.text = $"<color=\"#999\">{parts[0]}</color> {parts[1]}";
                    }
                    else
                    {
                        this.text = this.sourceName;                        
                    }
                }
                else
                {
                    this.text = this.sourceName;
                }
            }
        }
        
        private List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        private readonly List<string> lastNames = new List<string>();
        private Coroutine pollRoutine;

        private void RefreshSources()
        {
            var names = NdiFinder.EnumerateSourceNames().ToList();

            // Nothing changed: leave the dropdown alone (avoids flicker while polling).
            if (options.Count > 0 && names.SequenceEqual(lastNames)) return;
            lastNames.Clear();
            lastNames.AddRange(names);

            options.Clear();
            options.Add(new NdiOptionData("None"));
            foreach (var source in names)
                options.Add(new NdiOptionData(source));

            sources.options = options;

            // The dropdown value is an index: keep showing the currently received source by name.
            // The receiver itself is never touched, so a source that briefly disappears keeps
            // reconnecting on its own.
            var index = string.IsNullOrEmpty(receiver.ndiName) ? 0 : names.IndexOf(receiver.ndiName) + 1;
            sources.SetValueWithoutNotify(index);
            sources.RefreshShownValue();
        }

        private IEnumerator PollSources()
        {
            var wait = new WaitForSecondsRealtime(autoRefreshInterval);
            while (true)
            {
                yield return wait;
                // Don't rebuild the list while the user is looking at it.
                if (!IsDropdownOpen) RefreshSources();
            }
        }

        // The legacy UI Dropdown has no public "is open" flag, but it spawns a "Dropdown List" child while open.
        private bool IsDropdownOpen => sources.transform.Find("Dropdown List") != null;
    }
}
