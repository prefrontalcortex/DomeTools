using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace pfc.DomeTools
{
    [CustomEditor(typeof(TestPattern))]
    public class TestPatternEditor: Editor
    {
        private List<Texture> _textures;
        
        private void OnEnable()
        {
            var t = target as TestPattern;
            if (!t) return;
            
            var folder = AssetDatabase.GUIDToAssetPath(t.rootFolderGuid);
            var textures = AssetDatabase.FindAssets("t:Texture", new[] {folder});
            _textures = new List<Texture>();
            foreach (var texture in textures)
            {
                var path = AssetDatabase.GUIDToAssetPath(texture);
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (tex) _textures.Add(tex);
            }
        }

        public override void OnInspectorGUI()
        {
            var t = target as TestPattern;
            if (!t) return;
            
            EditorGUI.BeginChangeCheck();
            var currentIndex = _textures.IndexOf(t.currentTexture);
            if (currentIndex < 0) currentIndex = 0;
            var index = EditorGUILayout.Popup("Texture", currentIndex, _textures.Select(x => x.name).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Change Test Pattern Texture");
                t.SetTexture(_textures[index]);
                EditorUtility.SetDirty(t);
            }
        }
    }
}
