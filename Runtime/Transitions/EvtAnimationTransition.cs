using System;
using System.Collections;
using UnityEngine;

namespace PeartreeGames.Evt.Menus
{
    [Serializable]
    public class EvtAnimationTransition : EvtTransition
    {
        private static readonly int ShowHash = Animator.StringToHash("Show");
        private static readonly int HideHash = Animator.StringToHash("Hide");

        public override void Init(EvtMenu menu)
        {
            menu.GetComponent<Animator>().SetTrigger(HideHash);
        }

        public override IEnumerator Show(EvtMenu menu)
        {
            var anim = menu.GetComponent<Animator>();
            anim.SetTrigger(ShowHash);
            yield return null;
            while (anim.IsInTransition(0))
            {
                yield return null;
            }
        }

        public override IEnumerator Hide(EvtMenu menu)
        {
            var anim = menu.GetComponent<Animator>();
            anim.SetTrigger(HideHash);
            yield return null;
            while (anim.IsInTransition(0))
            {
                yield return null;
            }
        }
    }
}