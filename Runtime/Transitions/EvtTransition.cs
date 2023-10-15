using System;
using System.Collections;

namespace PeartreeGames.Evt.Menus
{
    public abstract class EvtTransition
    {
        public abstract void Init(EvtMenu menu);
        public abstract IEnumerator Show(EvtMenu menu);
        public abstract IEnumerator Hide(EvtMenu menu);
    }

    [Serializable]
    public class EvtSimpleTransition : EvtTransition
    {
        public override void Init(EvtMenu menu)
        {
            menu.gameObject.SetActive(false);
        }

        public override IEnumerator Show(EvtMenu menu)
        {
            menu.gameObject.SetActive(true);
            yield break;
        }

        public override IEnumerator Hide(EvtMenu menu)
        {
            menu.gameObject.SetActive(false);
            yield break;
        }
    }
}