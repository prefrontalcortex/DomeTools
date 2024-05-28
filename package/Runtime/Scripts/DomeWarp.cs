using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace pfc.Fulldome
{
    [ExecuteAlways]
    public class DomeWarp : MonoBehaviour
    {
        [HideInInspector]
        public Shader shader;
        public Material mat;
        public Material matHDRP;
        public RenderTexture targetTexture;

        public float DomeRadius => dome.localScale.x;
        public Vector3 CameraOffset => transform.localPosition;
        public Transform dome;
            
        internal static readonly ExposedProperty[] ExposedProperties = new[]
        {
            new ExposedProperty() { path = "transform.m_LocalPosition.y", name = "Height Offset", range = new Vector2(-2, -0.01f) },
        };
        
        [Header("Stretch Debugging")]
        public bool debugStretching = false;
        [Range(1f, 2f)]
        public float allowedUndersampling = 2f;
        [Range(0.0001f, 0.1f)]
        public float allowedPerfectRange = 0.01f;
        
        private Camera cam;
        private RenderTexture tempRT;
        
        private static readonly int AllowedUndersampling = Shader.PropertyToID("_AllowedUndersampling");
        private static readonly int AllowedPerfectRange = Shader.PropertyToID("_AllowedPerfectRange");
        private static readonly int Offset = Shader.PropertyToID("_CameraOffset");
        private static readonly int WorldToDomeCam = Shader.PropertyToID("_WorldToDomeCam");
        private static readonly int DomeCamToWorld = Shader.PropertyToID("_DomeCamToWorld");

        private void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            AssignTempTarget();
        }
        
        private void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        // Only called for SRPs
        private void OnEndCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            if (arg2 != cam) return;
            SetMaterialParams(mat);
            SetMaterialParams(matHDRP);
            mat.mainTexture = arg2.targetTexture;
            if (matHDRP) matHDRP.mainTexture = arg2.targetTexture;
            Graphics.Blit(arg2.targetTexture, targetTexture, mat, 0);
        }

        private void AssignTempTarget()
        {
            if (tempRT) RenderTexture.ReleaseTemporary(tempRT);
            tempRT = RenderTexture.GetTemporary(targetTexture.descriptor);
            tempRT.hideFlags = HideFlags.DontSave;
            if (!cam) cam = GetComponent<Camera>();
            cam.targetTexture = tempRT;
        }
        
        // should only be called when no camera component
        private void Update()
        {
            if (targetTexture && (!tempRT || tempRT.width != targetTexture.width || tempRT.height != targetTexture.height))
                AssignTempTarget();
            
            SetMaterialParams(mat);
            SetMaterialParams(matHDRP);
        }

        // only works with a camera component
        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!cam) cam =GetComponent<Camera>();

            if (!shader)
            {
                Graphics.Blit(src, dest);
                return;
            }

            if (!mat) mat = new Material(shader);

            mat.mainTexture = src;
            if(matHDRP) matHDRP.mainTexture = src;

            SetMaterialParams(mat);
            SetMaterialParams(matHDRP);

            //mat.SetFloat("_from", from);
            //mat.SetFloat("_to", to);

            // mat.SetFloat("")
            Graphics.Blit(src, targetTexture, mat);
            Graphics.Blit(src, dest);
        }

        void SetMaterialParams(Material material)
        {
            if(!material) return;
            if(!dome) return;

            if (!debugStretching)
            {
                if (!material.IsKeywordEnabled("MODE_DOME")) material.EnableKeyword("MODE_DOME");
                if (material.IsKeywordEnabled("MODE_DEBUG_STRETCH")) material.DisableKeyword("MODE_DEBUG_STRETCH");
            }
            else
            {
                if(material.IsKeywordEnabled("MODE_DOME")) material.DisableKeyword("MODE_DOME");
                if(!material.IsKeywordEnabled("MODE_DEBUG_STRETCH")) material.EnableKeyword("MODE_DEBUG_STRETCH");
            }

            material.SetFloat(AllowedUndersampling, allowedUndersampling);
            material.SetFloat(AllowedPerfectRange, allowedPerfectRange);
            material.SetVector(Offset, CameraOffset / DomeRadius);
            
            Shader.SetGlobalMatrix(WorldToDomeCam, transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(DomeCamToWorld, transform.localToWorldMatrix);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(DomeWarp))]
    public class DomemasterFisheyeEditor : Editor
    {
        SerializedObject transformObject;
        SerializedProperty localPositionProperty;
        SerializedProperty yProperty;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawExtraOptions();
        }

        public void DrawExtraOptions()
        {
            if (transformObject == null)
            {
                var t = (DomeWarp) target;
                var transform = t.transform;
                transformObject = new SerializedObject(transform);
                localPositionProperty = transformObject.FindProperty("m_LocalPosition");
                yProperty = localPositionProperty.FindPropertyRelative("y");
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(yProperty);
            if (EditorGUI.EndChangeCheck())
                transformObject.ApplyModifiedProperties();
        }
    }
#endif
}