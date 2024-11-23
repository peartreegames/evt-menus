using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PeartreeGames.Evt.Menus.Editor
{
    [CustomEditor(typeof(EvtMenuObject))]
    public class EvtMenuObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty _prefabProperty;
        private SerializedProperty _sceneNameProperty;
        private SerializedProperty _transitionProperty;
        private SerializedProperty _canBeClosedProperty;
        private SerializedProperty _destroyOnCloseProperty;
        private SerializedProperty _disablePreviousProperty;

        private VisualElement _transitionField;
        private VisualElement _rootElement;
        private EvtMenuObject _menu;
        private ListView _listView;

        private void OnEnable()
        {
            _menu = (EvtMenuObject) target;
            _prefabProperty = serializedObject.FindProperty("prefab");
            _sceneNameProperty = serializedObject.FindProperty("sceneName");
            _transitionProperty = serializedObject.FindProperty("transition");
            _transitionProperty.managedReferenceValue ??= new EvtSimpleTransition();
            _canBeClosedProperty = serializedObject.FindProperty("canBeClosed");
            _destroyOnCloseProperty = serializedObject.FindProperty("destroyOnClose");
            _disablePreviousProperty = serializedObject.FindProperty("disablePreviousMenus");
            serializedObject.ApplyModifiedProperties();
        }

        private VisualElement CreateProperty(SerializedProperty prop)
        {
            var field = new PropertyField(prop);
            field.Bind(serializedObject);
            return field;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var elem = new VisualElement();
            elem.styleSheets.Add(Resources.Load<StyleSheet>("EvtMenus"));
            _rootElement = elem;
            
            var buttons = new VisualElement();
            buttons.AddToClassList("flex-grow");
            buttons.AddToClassList("flex-row");
            var open = new Toggle("IsOpen") {value = ((EvtMenuObject) target).IsOpen};
            open.SetEnabled(false);
            open.AddToClassList("flex-grow");
            buttons.Add(open);
            buttons.Add(new Button(() => _menu.Open(null, true)) {text = "Open"});
            buttons.Add(new Button(() => _menu.Close()) {text = "Close"});
            elem.Add(buttons);
            
            elem.Add(CreateProperty(_prefabProperty));
            elem.Add(CreateProperty(_sceneNameProperty));
            _transitionField = CreateProperty(_transitionProperty);
            elem.Add(CreateTransitionDropdown());
            elem.Add(_transitionField);
            elem.Add(CreateProperty(_canBeClosedProperty));
            elem.Add(CreateProperty(_destroyOnCloseProperty));
            elem.Add(CreateProperty(_disablePreviousProperty));

            return elem;
        }

        private DropdownField CreateTransitionDropdown()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var transitions = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var transitionList = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EvtTransition))).ToList();
                transitions.AddRange(transitionList);
            }

            var dropdown = new DropdownField
            {
                choices = transitions.Select(t => t.Name).ToList(),
                value = _transitionProperty.managedReferenceValue.GetType().Name
            };
            dropdown.RegisterValueChangedCallback(change =>
            {
                _transitionField.RemoveFromHierarchy();
                _transitionProperty.managedReferenceValue =
                    Activator.CreateInstance(transitions.Find(t => t.Name == change.newValue));
                _transitionField = CreateProperty(_transitionProperty);
                _rootElement.Insert(3, _transitionField);
                serializedObject.ApplyModifiedProperties();
            });
            return dropdown;
        }
    }
}