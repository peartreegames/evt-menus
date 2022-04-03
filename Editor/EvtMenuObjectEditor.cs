using System;
using System.Collections.Generic;
using System.Linq;
using PeartreeGames.EvtVariables;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PeartreeGames.EvtMenus.Editor
{
    [CustomEditor(typeof(EvtMenuObject))]
    public class EvtMenuObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty prefabProperty;
        private SerializedProperty sceneNameProperty;
        private SerializedProperty transitionProperty;
        private SerializedProperty canBeClosedProperty;
        private SerializedProperty destroyOnCloseProperty;
        private SerializedProperty disablePreviousProperty;

        private VisualElement transitionField;
        private VisualElement rootElement;
        private EvtMenuObject menu;
        private ListView _listView;
        private List<EvtVariableObject> variableList;

        private void OnEnable()
        {
            menu = (EvtMenuObject) target;
            prefabProperty = serializedObject.FindProperty("prefab");
            sceneNameProperty = serializedObject.FindProperty("sceneName");
            transitionProperty = serializedObject.FindProperty("transition");
            transitionProperty.managedReferenceValue ??= new EvtMenuSimpleTransition();
            canBeClosedProperty = serializedObject.FindProperty("canBeClosed");
            destroyOnCloseProperty = serializedObject.FindProperty("destroyOnClose");
            disablePreviousProperty = serializedObject.FindProperty("disablePreviousMenus");
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
            rootElement = elem;
            elem.Add(CreateProperty(prefabProperty));
            elem.Add(CreateProperty(sceneNameProperty));
            transitionField = CreateProperty(transitionProperty);
            elem.Add(CreateTransitionDropdown());
            elem.Add(transitionField);
            elem.Add(CreateProperty(canBeClosedProperty));
            elem.Add(CreateProperty(destroyOnCloseProperty));
            elem.Add(CreateProperty(disablePreviousProperty));
            
            var list = CreateVariableList();
            elem.Add(list);

            elem.Add(new Button(() => menu.Open(null, true)) {text = "Open"});
            elem.Add(new Button(menu.Close) {text = "Close"});

            return elem;
        }

        private ListView CreateVariableList()
        {
            RefreshVariableList();
            void BindItem(VisualElement elem, int i)
            {
                if (i >= variableList.Count) return;
                var objField = elem.Q<ObjectField>();
                objField.value = variableList[i];
                elem.Q<TextField>().value = objField.value != null ? objField.value.name : string.Empty;
            }

            _listView = new ListView(variableList, 20, CreateVariableListItem, BindItem)
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
            variableData.Insert(0, new VariableData()
            {
                Path = "Add Variable",
                TypeName = null
            });
            var dropdown = new DropdownField()
            {
                choices = variableData.Select(v => v.Path).ToList(),
                value = "Add Variable"
            };
            dropdown.RegisterValueChangedCallback(change =>
            {
                var variable =
                    CreateInstance(variableData.Find(v => v.Path == change.newValue).TypeName) as EvtVariableObject;
                AddVariable(variable);
                RefreshVariableList();
                dropdown.SetValueWithoutNotify("Add Variable");
            });
            return dropdown;
        }

        private class VariableData
        {
            public string Path;
            public string TypeName;
        }

        private static List<VariableData> GetEvtVariableData()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var variables = new List<VariableData>();
            foreach (var assembly in assemblies)
            {
                var list = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EvtVariableObject)))
                    .SelectMany(t => t.GetCustomAttributesData().Where(
                        attr => attr.AttributeType == typeof(CreateAssetMenuAttribute)).Select(attr =>
                        new VariableData()
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
            itemElem.AddToClassList("variable-item");
            var text = new TextField();
            text.AddToClassList("variable-name");
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
                AssetDatabase.SetLabels(obj.value, new[]{ text.value});
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
            var path = AssetDatabase.GetAssetPath(menu);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            variableList = AssetDatabase.LoadAllAssetsAtPath(path).OfType<EvtVariableObject>().Reverse().ToList();
            if (_listView == null) return;
            _listView.itemsSource = variableList;
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
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EvtMenuTransition))).ToList();
                transitions.AddRange(transitionList);
            }

            var dropdown = new DropdownField()
            {
                choices = transitions.Select(t => t.Name).ToList(),
                value = transitionProperty.managedReferenceValue.GetType().Name
            };
            dropdown.RegisterValueChangedCallback(change =>
            {
                transitionField.RemoveFromHierarchy();
                transitionProperty.managedReferenceValue = Activator.CreateInstance(transitions.Find(t => t.Name == change.newValue));
                transitionField = CreateProperty(transitionProperty);
                rootElement.Insert(3, transitionField);
                serializedObject.ApplyModifiedProperties();
            });
            return dropdown;
        }
    }
}