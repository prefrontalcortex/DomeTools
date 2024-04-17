using UnityEditor;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    [CustomEditor(typeof(DPadInteraction))]
    internal class DPadInteractionCustomEditor : Editor
    {
        private SerializedProperty forceThresholdLeft;
        private SerializedProperty forceThresholdReleaseLeft;
        private SerializedProperty centerRegionLeft;
        private SerializedProperty wedgeAngleLeft;
        private SerializedProperty isStickyLeft;

        private SerializedProperty forceThresholdRight;
        private SerializedProperty forceThresholdReleaseRight;
        private SerializedProperty centerRegionRight;
        private SerializedProperty wedgeAngleRight;
        private SerializedProperty isStickyRight;

        static GUIContent s_ForceThresholdLabelLeft = EditorGUIUtility.TrTextContent("ForceThreshold");
        static GUIContent s_ForceThresholdReleaseLabelLeft = EditorGUIUtility.TrTextContent("ForceThresholdRelease");
        static GUIContent s_CenterRegionLeft = EditorGUIUtility.TrTextContent("centerRegion");
        static GUIContent s_WedgeAngleLeft = EditorGUIUtility.TrTextContent("wedgeAngle");
        static GUIContent s_IsStickyLeft = EditorGUIUtility.TrTextContent("isSticky");

        static GUIContent s_ForceThresholdLabelRight = EditorGUIUtility.TrTextContent("ForceThreshold");
        static GUIContent s_ForceThresholdReleaseLabelRight = EditorGUIUtility.TrTextContent("ForceThresholdRelease");
        static GUIContent s_CenterRegionRight = EditorGUIUtility.TrTextContent("centerRegion");
        static GUIContent s_WedgeAngleRight = EditorGUIUtility.TrTextContent("wedgeAngle");
        static GUIContent s_IsStickyRight = EditorGUIUtility.TrTextContent("isSticky");


        void OnEnable()
        {
            forceThresholdLeft = serializedObject.FindProperty("forceThresholdLeft");
            forceThresholdReleaseLeft = serializedObject.FindProperty("forceThresholdReleaseLeft");
            centerRegionLeft = serializedObject.FindProperty("centerRegionLeft");
            wedgeAngleLeft = serializedObject.FindProperty("wedgeAngleLeft");
            isStickyLeft = serializedObject.FindProperty("isStickyLeft");

            forceThresholdRight = serializedObject.FindProperty("forceThresholdRight");
            forceThresholdReleaseRight = serializedObject.FindProperty("forceThresholdReleaseRight");
            centerRegionRight = serializedObject.FindProperty("centerRegionRight");
            wedgeAngleRight = serializedObject.FindProperty("wedgeAngleRight");
            isStickyRight = serializedObject.FindProperty("isStickyRight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Dpad Bindings Custom Values For Left Controller:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(forceThresholdLeft, s_ForceThresholdLabelLeft);
            EditorGUILayout.PropertyField(forceThresholdReleaseLeft, s_ForceThresholdReleaseLabelLeft);
            EditorGUILayout.PropertyField(centerRegionLeft, s_CenterRegionLeft);
            EditorGUILayout.PropertyField(wedgeAngleLeft, s_WedgeAngleLeft);
            EditorGUILayout.PropertyField(isStickyLeft, s_IsStickyLeft);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dpad Bindings Custom Values For Right Controller:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(forceThresholdRight, s_ForceThresholdLabelRight);
            EditorGUILayout.PropertyField(forceThresholdReleaseRight, s_ForceThresholdReleaseLabelRight);
            EditorGUILayout.PropertyField(centerRegionRight, s_CenterRegionRight);
            EditorGUILayout.PropertyField(wedgeAngleRight, s_WedgeAngleRight);
            EditorGUILayout.PropertyField(isStickyRight, s_IsStickyRight);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
