using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeartreeGames.Evt.Menus
{
    [CreateAssetMenu(fileName = "menu_", menuName = "Evt/Menu", order = 0)]
    public class EvtMenuObject : ScriptableObject
    {
        public EvtMenu Menu { get; set; }
        [SerializeField] private EvtMenu prefab;
        [SerializeField] private string sceneName;
        [SerializeReference] private EvtTransition transition;
        [SerializeField] private bool canBeClosed = true;
        [SerializeField] private bool destroyOnClose;
        [SerializeField] private bool disablePreviousMenus;

        private EvtMenuObject _previousObject;
        private IEnumerator _coroutine;
        [NonSerialized] public bool IsOpen;

        public event Action<EvtMenu> OnOpen;
        public event Action<EvtMenu> OnClose;

        private void OnEnable()
        {
            IsOpen = false;
        }

        public void Open() => Open(null, true);
        public void Open(EvtMenuObject previous) => Open(previous, true);
        public void Open(EvtMenuObject previous, bool focus)
        {
            if (IsOpen) return;
            IsOpen = true;
            if (Menu == null)
            {
                Menu = Instantiate(prefab);
                transition.Init(Menu);
                Menu.MenuObject = this;
                if (sceneName != string.Empty)
                {
                    var scene = SceneManager.GetSceneByName(sceneName);
                    if (scene.IsValid() && scene.isLoaded) SceneManager.MoveGameObjectToScene(Menu.gameObject, scene);
                }
            }

            if (_previousObject != this) _previousObject = previous;
            OnOpen?.Invoke(Menu);
            Menu.gameObject.SetActive(true);
            if (_coroutine != null) Menu.StopCoroutine(_coroutine);
            _coroutine = Show(focus);
            Menu.StartCoroutine(_coroutine);
        }

        private IEnumerator Show(bool focus)
        {
            yield return transition.Show(Menu);
            if (focus) Menu.Focus();
        }

        public void Close()
        {
            if (!canBeClosed || !IsOpen) return;
            IsOpen = false;
            OnClose?.Invoke(Menu);
            if (_coroutine != null) Menu.StopCoroutine(_coroutine);
            _coroutine = Hide();
            Menu.StartCoroutine(_coroutine);
        }

        private IEnumerator Hide()
        {
            yield return transition.Hide(Menu);
            Menu.Blur();
            if (_previousObject != null) _previousObject.Open(_previousObject._previousObject, true);
            if (destroyOnClose)
            {
                Destroy(Menu.gameObject);
                Menu = null;
            }
            else Menu.gameObject.SetActive(false);
        }
    }
}