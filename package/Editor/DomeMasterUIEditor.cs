using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace pfc.Fulldome
{
    [CustomEditor(typeof(DomeMasterUI))]
    public class DomeMasterUIEditor : UnityEditor.Editor
    {
        private Dictionary<Object, SerializedObject> _serializedObjects = new Dictionary<Object, SerializedObject>();
        private Dictionary<(SerializedObject, string), SerializedProperty> _serializedProperties = new Dictionary<(SerializedObject, string), SerializedProperty>();
        
        private SerializedObject GetSerializedObject(Object obj)
        {
            if (!obj) return null;
            if (!_serializedObjects.ContainsKey(obj))
                _serializedObjects[obj] = new SerializedObject(obj);
            _serializedObjects[obj].Update();
            return _serializedObjects[obj];
        }
        
        private SerializedProperty GetSerializedProperty(SerializedObject obj, string path)
        {
            if (obj == null) return null;
            var key = (obj, path);
            if (!_serializedProperties.ContainsKey(key))
                _serializedProperties[key] = obj.FindProperty(path);
            return _serializedProperties[key];
        }
        
        public override void OnInspectorGUI()
        {
            var t = (DomeMasterUI) target;
            
            foreach (var text in t.texts)
            {
                if (!text) continue;
                var so = GetSerializedObject(text);
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(GetSerializedProperty(so, "m_Enabled"), new GUIContent(text.name));
                EditorGUILayout.PropertyField(GetSerializedProperty(so, "m_Text"), new GUIContent("Text"));
                if (EditorGUI.EndChangeCheck())
                    so.ApplyModifiedProperties();
            }
            
            foreach (var image in t.images)
            {
                if (!image) continue;
                var so = GetSerializedObject(image);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(GetSerializedProperty(so, "m_Enabled"), new GUIContent(image.name));
                EditorGUILayout.PropertyField(GetSerializedProperty(so, "m_Sprite"), new GUIContent("Image"));
                if (EditorGUI.EndChangeCheck())
                    so.ApplyModifiedProperties();
            }
            
            foreach (var obj in t.objects)
            {
                if (!obj) continue;
                var so = GetSerializedObject(obj);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(GetSerializedProperty(so, "m_IsActive"), new GUIContent(obj.name));
                if (EditorGUI.EndChangeCheck())
                    so.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
