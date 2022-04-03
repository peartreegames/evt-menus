using System;
using System.Collections;
using UnityEngine;

namespace PeartreeGames.EvtMenus
{
    [Serializable]
    public class EvtMenuScaleTransition : EvtMenuTransition
    {
        [SerializeField] private Vector3 openScale;
        [SerializeField] private Vector3 closeScale;
        [SerializeField] private float openDuration;
        [SerializeField] private float closeDuration;
        [SerializeField] private Easing openEasing;
        [SerializeField] private Easing closeEasing;
        public override void Init(EvtMenu menu)
        {
            menu.container.localScale = closeScale;
        }
        
        private IEnumerator Execute(EvtMenu menu, Vector3 start, Vector3 end, float duration, Func<float, float> ease)
        {
            var diff = end - start;
            var current = menu.container.localScale - start;
            var elapsedTime = Vector3.Dot(current, diff) / Vector3.Dot(diff, diff) * duration;
            while (elapsedTime < duration)
            {
                menu.container.localScale = Vector3.Lerp(start, end, ease(elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            menu.container.localScale = end;
        }
        public override IEnumerator Show(EvtMenu menu)
        {
            yield return Execute(menu, closeScale, openScale, openDuration, openEasing.GetFunction());
        }
        
        public override IEnumerator Hide(EvtMenu menu)
        {
            yield return Execute(menu, openScale, closeScale, closeDuration, closeEasing.GetFunction());
        }
    }
}