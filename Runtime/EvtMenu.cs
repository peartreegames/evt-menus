using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeartreeGames.Evt.Menus
{
    public class EvtMenu : MonoBehaviour, ICancelHandler
    {
        public RectTransform container;
        private bool _isFocused;
        public EvtMenuObject MenuObject { get; set; }
        protected ISelectHandler PreviousSelection { get; set; }

        public void Focus()
        {
            if (_isFocused) return;
            _isFocused = true;
            PreviousSelection ??= GetComponentInChildren<ISelectHandler>();
            EventSystem.current.SetSelectedGameObject((PreviousSelection as MonoBehaviour)?.gameObject);
        }

        public void Blur()
        {
            if (!_isFocused) return;
            _isFocused = false;
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            var items = GetComponentsInChildren<ISelectHandler>();
            PreviousSelection = Array.Find(items, item => (item as MonoBehaviour)?.gameObject == selected);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnCancel(BaseEventData eventData) => MenuObject.Close();
    }
}