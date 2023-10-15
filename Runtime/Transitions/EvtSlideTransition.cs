using System;
using System.Collections;
using UnityEngine;

namespace PeartreeGames.Evt.Menus
{
    [Serializable]
    public class EvtSlideTransition : EvtTransition
    {
        [SerializeField] private Vector2 openPosition;
        [SerializeField] private Vector2 closePosition;
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;
        [SerializeField] private Easing openEasing;
        [SerializeField] private Easing closeEasing;
        public override void Init(EvtMenu menu)
        {
            menu.container.anchoredPosition = closePosition;
        }
        
        public static IEnumerator Execute(RectTransform container, Vector2 start, Vector2 end, float duration, Func<float, float> ease)
        {
            var diff = end - start;
            var current = container.anchoredPosition - start;
            var elapsedTime = Vector2.Dot(current, diff) / Vector2.Dot(diff, diff) * duration;
            while (elapsedTime < duration)
            {
                container.anchoredPosition = Vector2.Lerp(start, end, ease(elapsedTime / duration));
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            container.anchoredPosition = end;
        }
        public override IEnumerator Show(EvtMenu menu)
        {
            yield return Execute(menu.container, closePosition, openPosition, openDuration, openEasing.GetFunction());
        }
        
        public override IEnumerator Hide(EvtMenu menu)
        {
            yield return Execute(menu.container, openPosition, closePosition, closeDuration, closeEasing.GetFunction());
        }
    }
}