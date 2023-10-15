using System;
using System.Collections.Generic;
using System.Linq;
using PeartreeGames.Evt.Variables;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
        private List<EvtVariable> _variableList;

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
            buttons.Add(new Button(_menu.Close) {text = "Close"});
            elem.Add(buttons);
            
            elem.Add(CreateProperty(_prefabProperty));
            elem.Add(CreateProperty(_sceneNameProperty));
            _transitionField = CreateProperty(_transitionProperty);
            elem.Add(CreateTransitionDropdown());
            elem.Add(_transitionField);
            elem.Add(CreateProperty(_canBeClosedProperty));
            elem.Add(CreateProperty(_destroyOnCloseProperty));
            elem.Add(CreateProperty(_disablePreviousProperty));

            var list = CreateVariableList();
            elem.Add(list);


            return elem;
        }

        private ListView CreateVariableList()
        {
            RefreshVariableList();

            void BindItem(VisualElement elem, int i)
            {
                if (i >= _variableList.Count) return;
                var objField = elem.Q<ObjectField>();
                objField.value = _variableList[i];
                elem.Q<TextField>().value = objField.value != null ? objField.value.name : string.Empty;
            }

            _listView = new ListView(_variableList, 20, CreateVariableListItem, BindItem)
            {
                showFoldoutHeader = true,
                headerTitle = "Variables",
                showBoundCollectionSize = false,
                showBorder = true
            };
            _listView.Q<Foldout>().Add(CreateVariableDropdown());
            return _listView;
        }

        private VisualElement CreateVariableDropdown()
        {
            var variableData = GetEvtVariableData();
            variableData.Insert(0, new EvtVariableData
            {
                Path = "Add Variable",
                TypeName = null
            });
            var dropdown = new DropdownField
            {
                choices = variableData.Select(v => v.Path).ToList(),
                value = "Add Variable"
            };
            dropdown.RegisterValueChangedCallback(change =>
            {
                var variable =
                    CreateInstance(variableData.Find(v => v.Path == change.newValue).TypeName) as EvtVariable;
                AddVariable(variable);
                RefreshVariableList();
                dropdown.SetValueWithoutNotify("Add Variable");
            });
            return dropdown;
        }

        private class EvtVariableData
        {
            public string Path;
            public string TypeName;
        }

        private static List<EvtVariableData> GetEvtVariableData()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var variables = new List<EvtVariableData>();
            foreach (var assembly in assemblies)
            {
                var list = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EvtVariable)))
                    .SelectMany(t => t.GetCustomAttributesData().Where(
                        attr => attr.AttributeType == typeof(CreateAssetMenuAttribute)).Select(attr =>
                        new EvtVariableData
                        {
                            Path = attr.NamedArguments?.FirstOrDefault(ar => ar.MemberName == "menuName").TypedValue
                                .Value.ToString().Replace("Evt/", ""),
                            TypeName = t.Name
                        })).ToList();
                variables.AddRange(list);
            }

            return variables;
        }

        private VisualElement CreateVariableListItem()
        {
            var itemElem = new VisualElement();
            itemElem.AddToClassList("flex-row");
            var text = new TextField();
            text.AddToClassList("flex-grow");
            itemElem.Add(text);
            var obj = new ObjectField();
            obj.SetEnabled(false);
            itemElem.Add(obj);
            itemElem.Add(new Button(() =>
            {
                RemoveVariable(obj.value);
                RefreshVariableList();
            }) {text = "X"});
            text.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return) return;
                AssetDatabase.ClearLabels(obj.value);
                obj.value.name = text.value;
                AssetDatabase.SetLabels(obj.value, new[] {text.value});
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj.value));
            });
            text.RegisterCallback<BlurEvent>(_ =>
            {
                if (obj.value.name == text.value) return;
                text.value = obj.value.name;
            });
            return itemElem;
        }

        private void RefreshVariableList()
        {
            var path = AssetDatabase.GetAssetPath(_menu);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            _variableList = AssetDatabase.LoadAllAssetsAtPath(path).OfType<EvtVariable>().Reverse().ToList();
            if (_listView == null) return;
            _listView.itemsSource = _variableList;
            _listView.RefreshItems();
        }

        private void AddVariable(Object variable) => AssetDatabase.AddObjectToAsset(variable, target);
        private void RemoveVariable(Object variable) => AssetDatabase.RemoveObjectFromAsset(variable);

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