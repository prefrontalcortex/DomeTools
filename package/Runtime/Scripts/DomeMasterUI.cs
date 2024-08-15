using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if HAVE_NDI
using Klak.Ndi;
#endif

namespace pfc.DomeTools
{
    [ExecuteAlways]
    public class DomeMasterUI : MonoBehaviour
    {
        public DomeRenderer domeRenderer;
        public RawImage domeContentOutput;
        public Camera domeOutputCamera;
#if HAVE_NDI
        public NdiSender ndiSender;
#endif
        // public SpoutSender spoutSender;
        
        public List<Text> texts;
        public List<Image> images;
        public List<GameObject> objects;

        private void SetTargetTextureFromDomeRenderer()
        {
            if (!domeRenderer)
            {
                domeRenderer = FindAnyObjectByType<DomeRenderer>();
#if UNITY_EDITOR
                if (domeRenderer)
                    EditorUtility.SetDirty(this);
#endif
            }

            if (!domeRenderer || !domeRenderer.domeWarping || !domeContentOutput) return;
            if (domeContentOutput.texture == domeRenderer.domeWarping.targetTexture) return;
#if UNITY_EDITOR
            EditorUtility.SetDirty(domeContentOutput);
#endif
            domeContentOutput.texture = domeRenderer.domeWarping.targetTexture;

            if (domeOutputCamera && domeRenderer.domeMasterTexture)
            {
                domeOutputCamera.targetTexture = domeRenderer.domeMasterTexture;
#if UNITY_EDITOR
                EditorUtility.SetDirty(domeOutputCamera);
#endif
#if HAVE_NDI
                if (ndiSender)
                {
                    ndiSender.sourceTexture = domeRenderer.domeMasterTexture;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(ndiSender);
#endif
                }   
#endif
            }
        }

        private void OnEnable()
        {
            DomeRenderer.OnDomeRendererChanged += x => SetTargetTextureFromDomeRenderer();
            SetTargetTextureFromDomeRenderer();
        }

        private void OnValidate() => SetTargetTextureFromDomeRenderer();

        private void Update()
        {   
            // correct scale so that we always have world scale = 1
            var scale = transform.lossyScale;
            transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        }
    }
}