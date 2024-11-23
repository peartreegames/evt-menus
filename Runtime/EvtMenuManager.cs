using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeartreeGames.Evt.Menus
{
    public class EvtMenuManager : MonoBehaviour
    {
        private HashSet<EvtMenuObject> _menus;
        private static EvtMenuManager Instance { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var manager = new GameObject("EvtMenuManager").AddComponent<EvtMenuManager>();
            manager._menus = new HashSet<EvtMenuObject>();
            Instance = manager;
            DontDestroyOnLoad(Instance);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public static bool Contains(EvtMenuObject menu) => Instance != null && Instance._menus.Contains(menu);
        public static void Add(EvtMenuObject menu) => Instance._menus.Add(menu);
        public static void Remove(EvtMenuObject menu) => Instance._menus.Remove(menu);
    }
}