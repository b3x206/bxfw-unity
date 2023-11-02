using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using BXFW.Tools.Editor;

namespace BXFW.Tweening.Next.Editor
{
    [CustomPropertyDrawer(typeof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate))]
    public class BXSTweenExtensionTemplateEditor : PropertyDrawer
    {
        /// <summary>
        /// A field info selector. Also adds the ability to select get+set properties.
        /// </summary>
        public class FieldInfoSelectorDropdown : AdvancedDropdown
        {
            /// <summary>
            /// Item that contains extra data for selected.
            /// </summary>
            public class Item : AdvancedDropdownItem
            {
                /// <summary>
                /// The member info that this item contains.
                /// </summary>
                public readonly MemberInfo memberInfo;

                public Item(string name, MemberInfo info) : base(name)
                {
                    memberInfo = info;
                }
            }

            public Action<AdvancedDropdownItem> onItemSelected;
            private readonly Type targetType;

            private static readonly GUIStyle AdvDropdownElementLineStyle = "DD ItemStyle";
            private static readonly GUIStyle AdvDropdownElementHeaderStyle = "DD HeaderStyle";

            protected override AdvancedDropdownItem BuildRoot()
            {
                AdvDropdownElementLineStyle.richText = true;
                AdvDropdownElementHeaderStyle.richText = true;

                AdvancedDropdownItem rootItem = new AdvancedDropdownItem("Select Field Info");

                // Only draw public fields + properties with get+set
                foreach (MemberInfo member in targetType.GetMembers())
                {
                    if ((member.MemberType & MemberTypes.Field) != MemberTypes.Field && (member.MemberType & MemberTypes.Property) != MemberTypes.Property)
                    {
                        continue;
                    }
                    string memberTypeName = "|unknown type|";
                    if (member is PropertyInfo prop)
                    {
                        if (!prop.CanRead || !prop.CanWrite)
                        {
                            continue;
                        }
                        memberTypeName = prop.PropertyType.Name;
                    }
                    if (member is FieldInfo field)
                    {
                        memberTypeName = field.FieldType.Name;
                    }

                    Item memberItem = new Item($"<color=#2e9fa4>{memberTypeName}</color> {member.Name}", member);
                    rootItem.AddChild(memberItem);
                }

                return rootItem;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                onItemSelected?.Invoke(item);

                AdvDropdownElementLineStyle.richText = false;
                AdvDropdownElementHeaderStyle.richText = false;
            }

            public FieldInfoSelectorDropdown(AdvancedDropdownState state, Type target) : base(state)
            {
                targetType = target;
            }
        }

        private const float NonUniqueValuesWarningBoxHeight = 42;

        private readonly PropertyRectContext mainCtx = new PropertyRectContext(2);
        private readonly static Dictionary<string, ReorderableList> currentListsDict = new Dictionary<string, ReorderableList>();

        private string currentPropertyId;
        private BXSTweenExtensionGeneratorTask.ExtensionClassTemplate currentTargetTemplate;
        private SerializedProperty currentArrayProperty;
        private ReorderableList CurrentPropertyList => currentListsDict.GetValueOrDefault(string.IsNullOrEmpty(currentPropertyId) ? "0xdeadbeef" : currentPropertyId);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            currentPropertyId = property.GetIDString();
            currentTargetTemplate = (BXSTweenExtensionGeneratorTask.ExtensionClassTemplate)property.GetTarget().value;

            currentArrayProperty = property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.extensionMethods));

            if (CurrentPropertyList == null)
            {
                // Create the registry on dictionary as this property gets from it
                // Resizing the 'ReorderableList' does not resize the array
                ReorderableList list = new ReorderableList(property.serializedObject, currentArrayProperty)
                {
                    drawHeaderCallback = DrawExtensionTemplateListHeader,
                    elementHeightCallback = GetExtensionMethodTemplateHeight,
                    drawElementCallback = DrawExtensionMethodTemplate,
                };

                currentListsDict.Add(currentPropertyId, list);
            }

            if (!property.isExpanded)
                return height;

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.targetType)));

            // warning
            bool allMethodNamesUnique = currentTargetTemplate.extensionMethods.Cast((t) => t.MethodName).Distinct().Count() == currentTargetTemplate.extensionMethods.Count;
            if (!allMethodNamesUnique)
            {
                height += NonUniqueValuesWarningBoxHeight + mainCtx.Padding;
            }

            if (CurrentPropertyList.serializedProperty.IsDisposed() || CurrentPropertyList.serializedProperty.serializedObject.IsDisposed())
            {
                CurrentPropertyList.serializedProperty = currentArrayProperty;
            }

            height += CurrentPropertyList.GetHeight();

            return height;
        }

        private readonly PropertyRectContext listCtx = new PropertyRectContext(2);
        private void DrawExtensionTemplateListHeader(Rect position)
        {
            EditorGUI.LabelField(position, "Field Info List");
        }
        private float GetExtensionMethodTemplateHeight(int index)
        {
            // 'm_MethodName'
            float height = EditorGUI.GetPropertyHeight(currentArrayProperty.GetArrayElementAtIndex(index).FindPropertyRelative($"m_{nameof(BXSTweenExtensionGeneratorTask.ExtensionMethodTemplate.MethodName)}"));
            // 'm_TargetMemberName'
            height += EditorGUIUtility.singleLineHeight + listCtx.Padding;
            return height;
        }

        private void DrawExtensionMethodTemplate(Rect rect, int index, bool isActive, bool isFocused)
        {
            listCtx.Reset();
            // extensionMethods.Array.data[index]
            SerializedProperty elementProperty = currentArrayProperty.GetArrayElementAtIndex(index);
            // 'm_MethodName'
            SerializedProperty methodNameProperty = elementProperty.FindPropertyRelative($"m_{nameof(BXSTweenExtensionGeneratorTask.ExtensionMethodTemplate.MethodName)}");
            EditorGUI.PropertyField(listCtx.GetPropertyRect(rect, methodNameProperty), methodNameProperty);

            // Draw a dropdown selector for method info in the given type from ClassExtensionTemplate
            // If the parent ClassExtensionTemplate doesn't have a type show a warning dropdown
            SerializedProperty targetMemberNameProperty = elementProperty.FindPropertyRelative("m_TargetMemberName");
            Rect baseDropdownAreaRect = listCtx.GetPropertyRect(rect, EditorGUIUtility.singleLineHeight);
            Rect dropdownLabelRect = new Rect(baseDropdownAreaRect)
            {
                width = EditorGUIUtility.labelWidth,
            };
            Rect dropdownSelfRect = new Rect(baseDropdownAreaRect)
            {
                width = Mathf.Max(baseDropdownAreaRect.width - dropdownLabelRect.width),
                x = baseDropdownAreaRect.x + dropdownLabelRect.width
            };
            EditorGUI.LabelField(dropdownLabelRect, "Target Field");
            using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(currentTargetTemplate.targetType.Type == null))
            {
                string displayString = string.IsNullOrWhiteSpace(targetMemberNameProperty.stringValue) ? "<null>" : targetMemberNameProperty.stringValue;
                string tooltipString = currentTargetTemplate.targetType.Type == null ? "Assign a type to the parent target template to be able to edit this value." : targetMemberNameProperty.tooltip;
                if (EditorGUI.DropdownButton(dropdownSelfRect, new GUIContent(displayString, tooltipString), FocusType.Passive))
                {
                    //SerializedObject copySo = new SerializedObject(targetMemberNameProperty.serializedObject.targetObjects);
                    //SerializedProperty memberNameCopyProperty = copySo.FindProperty(targetMemberNameProperty.propertyPath);
                    
                    FieldInfoSelectorDropdown dropdown = new FieldInfoSelectorDropdown(new AdvancedDropdownState(), currentTargetTemplate.targetType.Type);
                    dropdown.Show(dropdownSelfRect);
                    dropdown.onItemSelected = (AdvancedDropdownItem item) =>
                    {
                        if (!(item is FieldInfoSelectorDropdown.Item fieldItem))
                            return;

                        targetMemberNameProperty.stringValue = fieldItem.memberInfo.Name;
                        targetMemberNameProperty.serializedObject.ApplyModifiedProperties();

                        /*
                        memberNameCopyProperty.stringValue = fieldItem.memberInfo.Name;
                        copySo.ApplyModifiedProperties();

                        copySo.Dispose();
                        memberNameCopyProperty.Dispose();
                        */
                    };
                }
            }
            currentArrayProperty.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            // Draw dropdown to uncollapse extension template
            property.isExpanded = EditorGUI.Foldout(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                return;
            }

            // Draw the type selector
            SerializedProperty targetTypeProperty = property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.targetType));
            EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, targetTypeProperty), targetTypeProperty);

            // Draw warning if applicable
            currentTargetTemplate = (BXSTweenExtensionGeneratorTask.ExtensionClassTemplate)property.GetTarget().value; // This is null for some reason
            bool allMethodNamesUnique = currentTargetTemplate.extensionMethods.Cast((t) => t.MethodName).Distinct().Count() == currentTargetTemplate.extensionMethods.Count;
            if (!allMethodNamesUnique)
            {
                EditorGUI.HelpBox(
                    mainCtx.GetPropertyRect(position, NonUniqueValuesWarningBoxHeight), 
                    "Given method names are not unique. The given Method names inserted will be checked for uniqueness and a index identifier will be appended if the method name isn't unique.",
                    MessageType.Warning
                );
            }

            // Depending on the type selected, draw a ReorderableList where we can add valid MemberInfo's from given class
            // The member info shall either be a property with getter and setter or be a field
            currentArrayProperty = property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.extensionMethods));
            if (CurrentPropertyList == null)
            {
                currentPropertyId = property.GetIDString();
            }

            if (CurrentPropertyList.serializedProperty.IsDisposed() || CurrentPropertyList.serializedProperty.serializedObject.IsDisposed())
            {
                CurrentPropertyList.serializedProperty = currentArrayProperty;
            }
            float listHeight = CurrentPropertyList.GetHeight();
            CurrentPropertyList.DoList(mainCtx.GetPropertyRect(position, listHeight));
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
}
