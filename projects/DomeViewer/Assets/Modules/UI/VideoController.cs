using UnityEngine;
using UnityEngine.Video;
using Application = UnityEngine.Application;

namespace pfc.Fulldome
{
    public class VideoController : MonoBehaviour
    {
        public string url;
        private string Url => Application.streamingAssetsPath + url;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(url)) return;
            StartPlayer();
        }

        private void StartPlayer()
        {
            var videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.url = Url;
            videoPlayer.Play();
        }
    }
}