using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    public class TypeSelectorDropdown : AdvancedDropdown
    {
        // TODO : Searching and adding 'UnityEngine' assemblies just makes this lag horribly
        // Fix this by either implementing an 'OptimizedSearchDropdown' or something similar
        // For other types it works fine though.
        // OptimizedAdvancedDropdown :
        // General optimizations to do :
        // A : Do a 'Recycling GUI' thing, that is, only call 'OnGUI' for in-any-way visible rects
        // For the scroll bar thing, check if the value has changed and check the ranges of batched rect indices (instead of checking all rects to see if it contains?)
        // Or make the constant height sized rects a lerp function with index and show those indices?
        // Scroll bar height will be displayed using a discard GUI area specified for the scroll bar.
        // B : Searching algorithm : Because 'OptimizedAdvancedDropdown' will handle large batches of strings, use aho-corasick or anything better idk.
        //     Make searching asynchronous and the result list be filled in by an editor coroutine, pressing enter while search results are loading would wait until the searching is done
        //     If the user presses enter again proceed with the requested option.
        // Extra Features :
        // - (in priority order)
        // Allow rich text
        // Allow the ability to set the maximum size (but only in height? or in both axis?)
        // Allow custom fonts
        // -
        // For the first version though, it will only draw as if it was 'AdvancedDropdown'
        // Allow for each 'AdvancedDropdownItem' to be able to define it's own 'GetPropertyHeight' and 'OnGUI'
        // -

        /// <summary>
        /// An 'AdvancedDropdownItem' that contains extra data.
        /// </summary>
        public class Item : AdvancedDropdownItem
        {
            /// <summary>
            /// Assembly qualified (and forklift certified) name for the given type.
            /// </summary>
            public string assemblyQualifiedName;

            public Item(string name) : base(name)
            { }
            public Item(string name, string typeAssemblyQualifiedName) : base(name)
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
            AdvDropdownElementLineStyle.richText = true;
            AdvDropdownElementHeaderStyle.richText = true;
            
            AdvancedDropdownItem rootItem = new AdvancedDropdownItem("Type Categories");
            rootItem.AddChild(new Item("None", string.Empty));

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

                    Item categoryChildItem = new Item($"{typeIdentifier} | <color=white>{t.FullName}</color>", t.AssemblyQualifiedName);
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

        public TypeSelectorDropdown(AdvancedDropdownState state) : base(state) { }
        public TypeSelectorDropdown(AdvancedDropdownState state, Type selected) : base(state)
        {
            m_selectedType = selected;
        }
        public TypeSelectorDropdown(AdvancedDropdownState state, string selectedAssemblyQualifiedName) : base(state)
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
        private static TypeSelectorDropdown typeSelector;

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
                typeSelector = new TypeSelectorDropdown(new AdvancedDropdownState(), sPropTypeName.stringValue);
                typeSelector.Show(dropdownSelfRect);

                // Copy the 'SerializedObject' + 'SerializedProperty'
                SerializedObject mainSo = new SerializedObject(property.serializedObject.targetObjects);
                SerializedProperty spTypeNameCopy = mainSo.FindProperty(sPropTypeName.propertyPath);
                typeSelector.onItemSelected = (AdvancedDropdownItem item) =>
                {
                    if (!(item is TypeSelectorDropdown.Item typeItem))
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
