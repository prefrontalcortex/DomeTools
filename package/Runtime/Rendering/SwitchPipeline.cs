using UnityEngine;
using UnityEngine.Rendering;

namespace pfc.DomeTools
{
    [ExecuteAlways]
    public class SwitchPipeline : MonoBehaviour
    {
        public RenderPipelineAsset asset;

        // Start is called before the first frame update
        void OnEnable()
        {
            GraphicsSettings.defaultRenderPipeline = asset;
        }
    }
}