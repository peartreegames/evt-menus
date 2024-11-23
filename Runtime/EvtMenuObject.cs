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
        public bool IsOpen => EvtMenuManager.Contains(this);
        public bool IsTransitioning { get; private set; }

        public event Action<EvtMenu> OnOpen;
        public event Action<EvtMenu> OnClose;

        public void Open() => Open(null, true);
        public void Open(EvtMenuObject previous) => Open(previous, true);
        public void Open(EvtMenuObject previous, bool focus)
        {
            if (IsOpen) return;
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

            OnOpen?.Invoke(Menu);
            Menu.gameObject.SetActive(true);
            _previousObject = previous;
            if (_coroutine != null) Menu.StopCoroutine(_coroutine);
            _coroutine = Show(focus);
            Menu.StartCoroutine(_coroutine);
        }

        private IEnumerator Show(bool focus)
        {
            EvtMenuManager.Add(this);
            if (disablePreviousMenus && _previousObject != null && _previousObject != this) _previousObject.Menu.StartCoroutine(_previousObject.Hide());
            IsTransitioning = true;
            yield return transition.Show(Menu);
            IsTransitioning = false;
            if (focus) Menu.Focus();
        }

        public void Close(bool openPrevious = true)
        {
            if (!canBeClosed || !IsOpen) return;
            OnClose?.Invoke(Menu);
            if (_coroutine != null) Menu.StopCoroutine(_coroutine);
            _coroutine = Hide(openPrevious);
            Menu.StartCoroutine(_coroutine);
        }

        private IEnumerator Hide(bool openPrevious = true)
        {
            EvtMenuManager.Remove(this);
            IsTransitioning = true;
            yield return transition.Hide(Menu);
            IsTransitioning = false;
            Menu.Blur();
            if (openPrevious && _previousObject != null) _previousObject.Open(_previousObject._previousObject, true);
            if (destroyOnClose)
            {
                Destroy(Menu.gameObject);
                Menu = null;
            }
            else Menu.gameObject.SetActive(false);
        }
    }
}