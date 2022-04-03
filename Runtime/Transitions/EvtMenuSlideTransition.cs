using System;
using System.Collections;
using UnityEngine;

namespace PeartreeGames.EvtMenus
{
    [Serializable]
    public class EvtMenuSlideTransition : EvtMenuTransition
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
        
        private IEnumerator Execute(EvtMenu menu, Vector2 start, Vector2 end, float duration, Func<float, float> ease)
        {
            var diff = end - start;
            var current = menu.container.anchoredPosition - start;
            var elapsedTime = Vector2.Dot(current, diff) / Vector2.Dot(diff, diff) * duration;
            while (elapsedTime < duration)
            {
                menu.container.anchoredPosition = Vector2.Lerp(start, end, ease(elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            menu.container.anchoredPosition = end;
        }
        public override IEnumerator Show(EvtMenu menu)
        {
            yield return Execute(menu, closePosition, openPosition, openDuration, openEasing.GetFunction());
        }
        
        public override IEnumerator Hide(EvtMenu menu)
        {
            yield return Execute(menu, openPosition, closePosition, closeDuration, closeEasing.GetFunction());
        }
    }
}