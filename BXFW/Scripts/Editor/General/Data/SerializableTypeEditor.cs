using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        private readonly Type m_selectedType;
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
                {
                    continue;
                }

                AdvancedDropdownItem categoryItem = new AdvancedDropdownItem($"<color=#a2d4a3>{domainCategoryType.Key}</color>");
                rootItem.AddChild(categoryItem);

                foreach (Type t in domainCategoryType.Value)
                {
                    if (!t.IsPublic)
                    {
                        continue;
                    }

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
            onItemSelected?.Invoke(item);

            // Disable after the invocation is done
            AdvDropdownElementLineStyle.richText = false;
            AdvDropdownElementHeaderStyle.richText = false;
        }

        public TypeSelectorDropdown(AdvancedDropdownState state) : base(state) { }
        public TypeSelectorDropdown(AdvancedDropdownState state, Type selected) : base(state)
        {
            m_selectedType = selected;
        }
        public TypeSelectorDropdown(AdvancedDropdownState state, string selectedAssemblyQualifiedName) : base(state)
        {
            if (string.IsNullOrWhiteSpace(selectedAssemblyQualifiedName))
            {
                return;
            }

            //m_selectedType = TypeListProvider.GetDomainTypesByPredicate((t) => t.AssemblyQualifiedName == selectedAssemblyQualifiedName).First();
            m_selectedType = Type.GetType(selectedAssemblyQualifiedName);
        }
    }

    [CustomPropertyDrawer(typeof(SerializableType), true)]
    public class SerializableTypeEditor : PropertyDrawer
    {
        private PropertyRectContext mainCtx = new PropertyRectContext(2f);
        private static TypeSelectorDropdown typeSelector;

        private float GetDropdownHeightOfType(Type t)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            // No element
            if (t == null)
            {
                return height;
            }

            Type[] args = t.GetGenericArguments();
            byte argsCount = (byte)Mathf.Clamp(args.Length, byte.MinValue, byte.MaxValue);
            // Cannot draw
            if (argsCount == 0xFF)
            {
                return height;
            }
            // Get/Recurse all types
            foreach (Type genericT in args)
            {
                // Open type
                if (genericT.IsGenericParameter)
                {
                    height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
                }
                // Closed, get height of type indiviually
                else
                {
                    height += GetDropdownHeightOfType(genericT);
                }
            }

            return height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializableType target = (SerializableType)property.GetTarget().value;
            float singleHeight = GetDropdownHeightOfType(target.Type);

            return singleHeight;
        }

        private static void SetTypeInType(ref Type check, Type target, Type setValue)
        {
            // Non-generic type, it is way easier to set the type
            if (check == null || !check.IsGenericType)
            {
                if (check == target)
                {
                    check = setValue;
                    return;
                }
            }

            // Iterate the type generics tree
            Type[] genericArgs = check.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                // Target matches, set value and bail out
                Debug.Log($"Check for equ : {genericArgs[i]} == {target}");
                if (genericArgs[i] == target)
                {
                    // We cannot set the type like this
                    //check.GenericTypeArguments[i] = setValue;
                    // So there's two options left : 
                    // A : Create a generic type with placeholder types for the remaining arguments (System.Object is what i am thinking now)
                    // B : Pass a sliceable type datatype, which has the ability to have resizable GenericTypeArguments
                    // Feel like the most probable option here is A for open types, just create them with placeholder types
                    // !!!! TODO : Also properly create a correct, constrainted dropdown for the given constrainted type.
                    if (check.IsGenericTypeDefinition)
                    {
                        // Repeat object or the very base type if the generic is constrainted
                        Type[] createTypeArgs = new Type[genericArgs.Length];
                        // Default Class Type : typeof(object) | Struct Type : typeof(int)
                        for (int j = 0; j < createTypeArgs.Length; j++)
                        {
                            // Allows any, no constraints
                            //if (constraints.Length <= 0)
                            //{
                            //    createTypeArgs[j] = typeof(object);
                            //    continue;
                            //}

                            // Time complexity is now O(n^whatever) because type operations
                            // and reflection in general is disgusting in performance.
                            // Just have to make it fast enough to be bearable.
                            // --
                            Type targetGenericType = TypeListProvider.FirstDomainTypeByPredicate((Type tCheck) =>
                            {
                                // Disallow non-public
                                if (!tCheck.IsPublic)
                                {
                                    return false;
                                }

                                // Get the first class that provides the constraints
                                foreach (Type arg in genericArgs)
                                {
                                    if (!Additionals.GenericArgumentAllowsType(arg, tCheck))
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            });

                            if (targetGenericType == null)
                            {
                                throw new TypeAccessException("[SerializableTypeEditor::SetTypeInType] Searched a valid constraintable type, but none was found.\nPlease create an appopriate class before assigning constrainted values to it.");
                            }

                            createTypeArgs[j] = targetGenericType;
                            Debug.Log($"Found 'targetGenericType' : {targetGenericType}");
                        }

                        Debug.Log($"Creating generic type, args are : ");
                        foreach (var a in createTypeArgs.Indexed())
                        {
                            Debug.Log($"{a.Key} : {a.Value}");
                        }
                        check = check.MakeGenericType(createTypeArgs);
                    }

                    Debug.Log($"Assigning args : {check.GenericTypeArguments[i]} to {setValue}");
                    check.GenericTypeArguments[i] = setValue;
                    return;
                }

                // Check generic recursively
                if (genericArgs[i].IsGenericType)
                {
                    SetTypeInType(ref check, target, setValue);
                }
            }
        }

        private Type setRootType;
        private void DrawTypeDropdown(SerializedProperty arrayProperty, Type editType, Rect position, GUIContent label)
        {
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

            EditorGUI.LabelField(dropdownLabelRect, label);
            bool openWindow = EditorGUI.DropdownButton(
                dropdownSelfRect,
                new GUIContent(editType?.FullName ?? "<null>", label.tooltip),
                FocusType.Keyboard
            );

            if (openWindow)
            {
                typeSelector = new TypeSelectorDropdown(new AdvancedDropdownState(), editType?.FullName);
                typeSelector.Show(dropdownSelfRect);

                // Copy the 'SerializedObject' + 'SerializedProperty'
                SerializedObject mainSo = new SerializedObject(arrayProperty.serializedObject.targetObjects);
                SerializedProperty arrayPropertyCopy = mainSo.FindProperty(arrayProperty.propertyPath);
                typeSelector.onItemSelected = (AdvancedDropdownItem item) =>
                {
                    if (!(item is TypeSelectorDropdown.Item typeItem))
                    {
                        return;
                    }

                    Type selectedType = Type.GetType(typeItem.assemblyQualifiedName, false);

                    // Find and change 'editType' on the current type context
                    SetTypeInType(ref setRootType, editType, selectedType);

                    // Write the bytes of type
                    // Creating a blank memory stream is a resizable one
                    using MemoryStream ms = new MemoryStream();
                    using BinaryWriter writer = new BinaryWriter(ms);

                    SerializableType.Write(writer, setRootType);
                    // And this creates the array
                    byte[] value = ms.ToArray();
                    // Write it to the target SerializedProperty
                    // Since the 'AdvancedDropdown' kind of suspends the execution this won't dispose hopefully
                    arrayPropertyCopy.arraySize = value.Length;
                    for (int i = 0; i < arrayPropertyCopy.arraySize; i++)
                    {
                        arrayPropertyCopy.GetArrayElementAtIndex(i).intValue = value[i];
                    }

                    mainSo.ApplyModifiedProperties();

                    arrayPropertyCopy.Dispose();
                    mainSo.Dispose();
                };
            }
        }

        private const float IndentHorizontal = 15f;
        private static Rect IndentedRect(Rect r)
        {
            return new Rect(r) { x = r.x + IndentHorizontal, width = r.width - IndentHorizontal };
        }
        private void DrawTypesRecursive(SerializedProperty arrayProperty, Type rootType, Rect position, GUIContent label)
        {
            // Draw the root type
            DrawTypeDropdown(arrayProperty, rootType, position, label);

            if (rootType == null)
            {
                return;
            }

            Type[] args = rootType.GetGenericArguments();

            // Get/Recurse all types
            foreach (Type genericT in args)
            {
                // Open type
                if (genericT.IsGenericParameter)
                {
                    DrawTypeDropdown(arrayProperty, genericT, IndentedRect(position), new GUIContent(label) { text = "Argument" });
                }
                // Closed, draw type recursively for it
                else
                {
                    DrawTypesRecursive(arrayProperty, genericT, IndentedRect(position), new GUIContent(label) { text = "Argument" });
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            // Target (read-only)
            SerializableType target = (SerializableType)property.GetTarget().value;
            // List of datas contained
            SerializedProperty dataArray = property.FindPropertyRelative("m_Data");

            // Depending on the 'target.Type', draw an inspector
            // If a value is changed change the entire 'dataArray'.
            // The generic drawing has to be done recursively.

            // Draw a list of dropdown selectors for the generic type(s) + root type
            setRootType = target.Type;
            DrawTypesRecursive(dataArray, target.Type, position, label);

            EditorGUI.EndProperty();
        }
    }
}
