using Klak.Ndi;
using UnityEngine;

public class SetSpeakerDistance : MonoBehaviour
{
    [SerializeField] private NdiReceiver _receiver;
    [SerializeField] private float _distance = 10.0f;
    [SerializeField] private bool _onEnable = true;

    public void SetDistance()
    {
        _receiver.virtualSpeakerDistances = _distance;
    }
    
    private void OnEnable()
    {
        if (_onEnable)
            SetDistance();
    }
}
