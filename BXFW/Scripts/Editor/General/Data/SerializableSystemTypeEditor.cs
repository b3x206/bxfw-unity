using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    public class TypeSelectorControl : AdvancedDropdown
    {
        /// <summary>
        /// An 'AdvancedDropdownItem' that contains extra data.
        /// </summary>
        public class TypeDropdownItem : AdvancedDropdownItem
        {
            /// <summary>
            /// Assembly qualified (and forklift certified) name for the given type.
            /// </summary>
            public string assemblyQualifiedName;

            public TypeDropdownItem(string name) : base(name)
            { }
            public TypeDropdownItem(string name, string typeAssemblyQualifiedName) : base(name)
            {
                assemblyQualifiedName = typeAssemblyQualifiedName;
            }
        }

        private static readonly GUIStyle AdvDropdownElementLineStyle = "DD ItemStyle";
        private static readonly GUIStyle AdvDropdownElementHeaderStyle = "DD HeaderStyle";

        // Both text instances are not rich text.
        public Action<AdvancedDropdownItem> onItemSelected;
        private Type m_selectedType;
        /// <summary>
        /// List of assembly flags to filter.
        /// </summary>
        public AssemblyFlags filterFlags = AssemblyFlags.All & ~AssemblyFlags.Dynamic;

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem rootItem = new AdvancedDropdownItem("Type Categories");

            AdvDropdownElementLineStyle.richText = true;
            AdvDropdownElementHeaderStyle.richText = true;

            foreach (KeyValuePair<AssemblyFlags, Type[]> domainCategoryType in TypeListProvider.DomainTypesList)
            {
                if ((domainCategoryType.Key & filterFlags) != domainCategoryType.Key)
                    continue;

                AdvancedDropdownItem categoryItem = new AdvancedDropdownItem($"<color=#a2d4a3>{domainCategoryType.Key}</color>");
                rootItem.AddChild(categoryItem);

                foreach (Type t in domainCategoryType.Value)
                {
                    if (!t.IsPublic)
                        continue;

                    string typeIdentifier = string.Empty;
                    if (t.IsClass)
                    {
                        typeIdentifier = "<color=#4ec9b0>C</color>";
                    }
                    else if (t.IsEnum)
                    {
                        // Enum is also value type, so do it before?
                        typeIdentifier = "<color=#b8d797>E</color>";
                    }
                    else if (t.IsValueType)
                    {
                        typeIdentifier = "<color=#86b86a>S</color>";
                    }
                    else if (t.IsInterface)
                    {
                        typeIdentifier = "<color=#b8d7a3>I</color>";
                    }

                    TypeDropdownItem categoryChildItem = new TypeDropdownItem($"{typeIdentifier} | <color=white>{t.Name}</color>", t.AssemblyQualifiedName);
                    categoryChildItem.enabled = t == m_selectedType;
                    categoryItem.AddChild(categoryChildItem);
                }
            }

            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            AdvDropdownElementLineStyle.richText = false;
            AdvDropdownElementHeaderStyle.richText = false;

            onItemSelected?.Invoke(item);
        }

        public TypeSelectorControl(AdvancedDropdownState state) : base(state) { }
        public TypeSelectorControl(AdvancedDropdownState state, Type selected) : base(state)
        {
            m_selectedType = selected;
        }
        public TypeSelectorControl(AdvancedDropdownState state, string selectedAssemblyQualifiedName) : base(state)
        {
            if (string.IsNullOrWhiteSpace(selectedAssemblyQualifiedName))
                return;

            //m_selectedType = TypeListProvider.GetDomainTypesByPredicate((t) => t.AssemblyQualifiedName == selectedAssemblyQualifiedName).First();
            m_selectedType = Type.GetType(selectedAssemblyQualifiedName);
        }
    }

    [CustomPropertyDrawer(typeof(SerializableSystemType), true)]
    public class SerializableSystemTypeEditor : PropertyDrawer
    {
        private PropertyRectContext mainCtx = new PropertyRectContext(2f);
        private static TypeSelectorControl typeSelector;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + mainCtx.Padding;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            // Draw a dropdown selector
            // The dropdown selector will summon the advanced dropdown
            Rect paddedPosition = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            Rect dropdownLabelRect = new Rect(paddedPosition)
            {
                width = EditorGUIUtility.labelWidth
            };
            Rect dropdownSelfRect = new Rect(paddedPosition)
            {
                x = position.x + dropdownLabelRect.width,
                width = Mathf.Max(paddedPosition.width - dropdownLabelRect.width, EditorGUIUtility.fieldWidth),
            };

            SerializedProperty sPropTypeName = property.FindPropertyRelative("m_AssemblyQualifiedName");
            SerializableSystemType target = ((SerializableSystemType)property.GetTarget().value);
            EditorGUI.LabelField(dropdownLabelRect, label);
            bool openWindow = EditorGUI.DropdownButton(
                dropdownSelfRect, 
                new GUIContent(target.Type?.FullName ?? "<null>", label.tooltip), 
                FocusType.Keyboard
            );

            if (openWindow)
            {
                typeSelector = new TypeSelectorControl(new AdvancedDropdownState(), sPropTypeName.stringValue);
                typeSelector.Show(dropdownSelfRect);

                // Copy the 'SerializedObject' + 'SerializedProperty'
                SerializedObject mainSo = new SerializedObject(property.serializedObject.targetObjects);
                SerializedProperty spTypeNameCopy = mainSo.FindProperty(sPropTypeName.propertyPath);
                typeSelector.onItemSelected = (AdvancedDropdownItem item) =>
                {
                    if (!(item is TypeSelectorControl.TypeDropdownItem typeItem))
                        return;

                    spTypeNameCopy.stringValue = typeItem.assemblyQualifiedName;
                    mainSo.ApplyModifiedProperties();

                    spTypeNameCopy.Dispose();
                    mainSo.Dispose();
                };
            }

            EditorGUI.EndProperty();
        }
    }
}
