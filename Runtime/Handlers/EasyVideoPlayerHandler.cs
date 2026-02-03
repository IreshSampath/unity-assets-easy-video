using UnityEngine;
using UnityEngine.Video;
using System.Collections;

namespace GAG.EasyVideo
{
    public class EasyVideoPlayerHandler : MonoBehaviour
    {
        [SerializeField] VideoPlayer _videoPlayer;
        [SerializeField] EasyVideoFadeTransition _fade;
        [SerializeField] EasyVideoResolutionHandler _resolutionHandler;
        bool _firstPlay = true;

        void Awake()
        {
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
        }
        
        void OnVideoPrepared(VideoPlayer vp)
        {
            // _resolutionHandler?.Apply(vp, _videoPlayer.targetTexture == null
            //     ? GetComponentInChildren<UnityEngine.UI.RawImage>()
            //     : null);
            
        }
        
        public IEnumerator Play(string path, bool loop, bool useTransition)
        {
            // First play = no fade
            if (_firstPlay)
            {
                _firstPlay = false;
                PlayInternal(path, loop);
                yield break;
            }

            if (useTransition && _fade != null)
                yield return _fade.FadeOutRoutine();

            _videoPlayer.Stop();

            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = path;
            _videoPlayer.isLooping = loop;

            _videoPlayer.Prepare();
            while (!_videoPlayer.isPrepared)
                yield return null;

            _videoPlayer.Play();

            if (useTransition && _fade != null)
                yield return _fade.FadeInRoutine();
        }

        void PlayInternal(string path, bool loop)
        {
            Stop();
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = path;
            _videoPlayer.isLooping = loop;
            _videoPlayer.Play();
        }

        public void Stop()
        {
            if (_videoPlayer.isPlaying)
                _videoPlayer.Stop();

            _resolutionHandler?.Release();
        }
    }
}