using System;
using System.Collections.Generic;
using UnityEngine;

namespace pfc.Fulldome
{
    public class SetDomeMaterials : MonoBehaviour
    {
        public Material mat;
        public Renderer[] domeRenderer = Array.Empty<Renderer>();

        private List<Material> cached = new List<Material>();
        private void OnEnable() => Set();
        private void OnDisable() => Unset();

        private void Set()
        {
            cached.Clear();
            if (!mat) return;
            for (var i = 0; i < domeRenderer.Length; i++)
            {
                var rend = domeRenderer[i];
                if (!rend) continue;
                cached.Add(rend.material);
                rend.material = mat;
            }
        }

        private void Unset()
        {
            if (!mat || cached.Count != domeRenderer.Length) return;
            for (var i = 0; i < domeRenderer.Length; i++)
            {
                var rend = domeRenderer[i];
                if (rend) rend.material = cached[i];
            }
        }
    }
}