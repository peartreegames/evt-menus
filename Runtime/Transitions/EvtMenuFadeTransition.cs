using System;
using System.Collections;
using UnityEngine;

namespace PeartreeGames.EvtMenus
{
    [Serializable]
    public class EvtMenuFadeTransition : EvtMenuTransition
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;
        [SerializeField] private Easing openEasing;
        [SerializeField] private Easing closeEasing;
        public override void Init(EvtMenu menu)
        {
            if (!menu.container.TryGetComponent(out _canvasGroup))
                _canvasGroup = menu.container.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }
        
        private IEnumerator Execute(EvtMenu menu, float start, float end, float duration, Func<float, float> ease)
        {
            var current = _canvasGroup.alpha;
            var elapsedTime = Mathf.InverseLerp(start, end, current) * duration;
            while (elapsedTime < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(start, end, ease(elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _canvasGroup.alpha = end;
        }
        
        public override IEnumerator Show(EvtMenu menu)
        {
            yield return Execute(menu, 0f, 1f, openDuration, openEasing.GetFunction());
        }
        
        public override IEnumerator Hide(EvtMenu menu)
        {
            yield return Execute(menu, 1f, 0f, closeDuration, closeEasing.GetFunction());
        }
    }
}