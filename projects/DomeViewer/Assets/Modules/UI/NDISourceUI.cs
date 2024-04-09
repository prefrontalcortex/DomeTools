using System;
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
        
        public static event Action<string> onNdiChanged;
        
        private void OnEnable()
        {
            RefreshSources();
            if (immediatelyChooseAvailableStream && string.IsNullOrEmpty(receiver.ndiName))
                receiver.ndiName = NdiFinder.EnumerateSourceNames().FirstOrDefault();
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
        
        private void RefreshSources()
        {
            options.Clear();
            options.Add(new NdiOptionData("None"));
            
            foreach (var source in NdiFinder.EnumerateSourceNames())
                options.Add(new NdiOptionData(source));
            
            sources.options = options;
        }
    }
}
