using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeartreeGames.EvtMenus
{
    public class EvtMenu : MonoBehaviour, ICancelHandler
    {
        public RectTransform container;
        private ISelectHandler _previousSelection;
        private bool _isFocused;
        public EvtMenuObject MenuObject { get; set; }

        public void Focus()
        {
            if (_isFocused) return;
            _isFocused = true;
            _previousSelection ??= GetComponentInChildren<ISelectHandler>();
            EventSystem.current.SetSelectedGameObject((_previousSelection as MonoBehaviour)?.gameObject);
        }

        public void Blur()
        {
            if (!_isFocused) return;
            _isFocused = false;
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            var items = GetComponentsInChildren<ISelectHandler>();
            _previousSelection = Array.Find(items, item => (item as MonoBehaviour)?.gameObject == selected);
        }

        public void OnCancel(BaseEventData eventData) => MenuObject.Close();
    }
}