using Klak.Ndi;
using UnityEngine;

namespace pfc.Fulldome
{
    [RequireComponent(typeof(NdiReceiver))]
    [ExecuteAlways]
    public class NDIDefaultTexture : MonoBehaviour
    {
        public Texture2D defaultTexture;
        private RenderTexture rt
        {
            get
            {
                if (!_rt) _rt = GetComponent<NdiReceiver>().targetTexture;
                return _rt;
            }
        }
        private RenderTexture _rt;
        private NdiReceiver receiver
        {
            get
            {
                if (!_receiver) _receiver = GetComponent<NdiReceiver>();
                return _receiver;
            }
        }
        private NdiReceiver _receiver;

        void LateUpdate()
        {
            if (!string.IsNullOrEmpty(receiver.ndiName) && receiver.ndiName != "None") return;
            if (!rt || !defaultTexture) return;
            Graphics.Blit(defaultTexture, rt);
        }
    }
}
