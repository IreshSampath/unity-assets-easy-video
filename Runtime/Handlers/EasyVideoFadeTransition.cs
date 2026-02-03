using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GAG.EasyVideo
{
    public class EasyVideoFadeTransition : MonoBehaviour
    {
        [SerializeField] RawImage _fadeImage;
        [SerializeField] float _fadeDuration = 0.4f;

        Coroutine _routine;

        readonly Color _white = Color.white;
        readonly Color _black = Color.black;

        void Awake()
        {
            if (_fadeImage != null)
                _fadeImage.color = _white;
        }

        public void FadeOut()
        {
            Restart(Fade(_white, _black));
        }

        public void FadeIn()
        {
            Restart(Fade(_black, _white));
        }

        public IEnumerator FadeOutRoutine()
        {
            yield return Fade(_white, _black);
        }

        public IEnumerator FadeInRoutine()
        {
            yield return Fade(_black, _white);
        }

        IEnumerator Fade(Color from, Color to)
        {
            float t = 0f;

            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _fadeImage.color = Color.Lerp(from, to, t / _fadeDuration);
                yield return null;
            }

            _fadeImage.color = to;
        }

        void Restart(IEnumerator routine)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(routine);
        }
    }
}