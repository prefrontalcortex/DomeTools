using UnityEngine;

namespace pfc.Fulldome
{
    [ExecuteAlways]
    public class GITarget : MonoBehaviour
    {
        private GIController _controller;

        private void OnEnable()
        {
            if (!_controller) _controller = FindAnyObjectByType<GIController>();
            _controller.giTarget = transform;
        }
    }
}