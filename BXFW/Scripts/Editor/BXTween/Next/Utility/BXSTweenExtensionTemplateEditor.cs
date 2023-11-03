using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using BXFW.Tools.Editor;

namespace BXFW.Tweening.Next.Editor
{
    [CustomPropertyDrawer(typeof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate))]
    public class BXSTweenExtensionTemplateEditor : PropertyDrawer
    {
        /// <summary>
        /// A field info selector. Also adds the ability to select get+set properties.
        /// <br>Can only select public instance fields.</br>
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

        private const float NonUniqueValuesWarningBoxHeight = 36;
        private const string ChildListExpandedKey = "[BXSTwExtGenEditor].isChildListExpanded";

        private readonly PropertyRectContext mainCtx = new PropertyRectContext(2);

        private BXSTweenExtensionGeneratorTask.ExtensionClassTemplate currentTargetTemplate;
        private SerializedProperty currentArrayProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            currentTargetTemplate = (BXSTweenExtensionGeneratorTask.ExtensionClassTemplate)property.GetTarget().value;
            currentArrayProperty = property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.extensionMethods));

            if (!property.isExpanded)
                return height;

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(BXSTweenExtensionGeneratorTask.ExtensionClassTemplate.targetType)));

            // warning
            bool allMethodNamesUnique = currentTargetTemplate.extensionMethods.Cast((t) => t.MethodName).Distinct().Count() == currentTargetTemplate.extensionMethods.Count;
            if (!allMethodNamesUnique)
            {
                height += NonUniqueValuesWarningBoxHeight + mainCtx.Padding;
            }

            // list (height)
            // collapse + size fields
            height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            bool childListExpanded = property.GetLong(ChildListExpandedKey) != 0;
            for (int i = 0; i < currentArrayProperty.arraySize && childListExpanded; i++)
            {
                // extensionMethods.Array.data[index]
                SerializedProperty elementProperty = currentArrayProperty.GetArrayElementAtIndex(i);
                // 'm_MethodName'
                SerializedProperty methodNameProperty = elementProperty.FindPropertyRelative($"m_{nameof(BXSTweenExtensionGeneratorTask.ExtensionMethodTemplate.MethodName)}");
                height += EditorGUI.GetPropertyHeight(methodNameProperty) + mainCtx.Padding;

                // Draw a dropdown selector for method info in the given type from ClassExtensionTemplate
                height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            // Draw dropdown to uncollapse extension template
            property.isExpanded = EditorGUI.Foldout(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
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
            // I found my new favorite unity class, it's called ReorderableList and it's very buggy and nice.
            // Iterate all values on array property and draw their property fields manually
            // Draw a foldout + array size controller
            Rect mainFoldoutRect = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            // array size controller size is 150
            Rect foldoutElementRect = new Rect(mainFoldoutRect)
            {
                width = mainFoldoutRect.width - 160
            };
            Rect foldoutArraySizeRect = new Rect(mainFoldoutRect)
            {
                x = mainFoldoutRect.x + foldoutElementRect.width + 10,
                width = 150,
            };
            EditorGUI.BeginChangeCheck();
            bool childListExpanded = EditorGUI.Foldout(foldoutElementRect, property.GetLong(ChildListExpandedKey) != 0, "Methods");

            // Do + and - buttons to add/remove elements
            Rect arraySizeIncBtnRect = new Rect(foldoutArraySizeRect)
            {
                width = 25,
            };
            Rect arraySizeDecBtnRect = new Rect(foldoutArraySizeRect)
            {
                x = arraySizeIncBtnRect.x + 30,
                width = 25
            };
            Rect arraySizeIntFieldRect = new Rect(foldoutArraySizeRect)
            {
                x = arraySizeDecBtnRect.x + 30,
                width = 90
            };
            int propertyArraySize = Mathf.Max(EditorGUI.IntField(arraySizeIntFieldRect, currentArrayProperty.arraySize), 0);
            if (GUI.Button(arraySizeIncBtnRect, "+"))
            {
                propertyArraySize++;
            }
            if (GUI.Button(arraySizeDecBtnRect, "-"))
            {
                propertyArraySize--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                currentArrayProperty.arraySize = propertyArraySize;
                property.SetLong(ChildListExpandedKey, childListExpanded ? 1 : 0);
            }

            Rect indentedRect = new Rect(position)
            {
                width = position.width - 30,
                x = position.x + 30,
            };
            for (int i = 0; i < currentArrayProperty.arraySize && childListExpanded; i++)
            {
                // extensionMethods.Array.data[index]
                SerializedProperty elementProperty = currentArrayProperty.GetArrayElementAtIndex(i);
                // 'm_MethodName'
                SerializedProperty methodNameProperty = elementProperty.FindPropertyRelative($"m_{nameof(BXSTweenExtensionGeneratorTask.ExtensionMethodTemplate.MethodName)}");
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentedRect, methodNameProperty), methodNameProperty);

                // Draw a dropdown selector for method info in the given type from ClassExtensionTemplate
                // If the parent ClassExtensionTemplate doesn't have a type show a warning dropdown
                SerializedProperty targetMemberNameProperty = elementProperty.FindPropertyRelative("m_TargetMemberName");
                Rect baseDropdownAreaRect = mainCtx.GetPropertyRect(indentedRect, EditorGUIUtility.singleLineHeight);
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
            }

            EditorGUI.EndProperty();
        }
    }
}
