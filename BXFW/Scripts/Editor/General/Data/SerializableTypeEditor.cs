using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System.Linq;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(SerializableType), true)]
    public class SerializableTypeEditor : PropertyDrawer
    {
        private PropertyRectContext mainCtx = new PropertyRectContext();
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

        /// <summary>
        /// Returns the first valid created closed type.
        /// </summary>
        private static Type GetFirstValidClosedType(Type openType)
        {
            // TODO : Get normal types first, then get the weird ones.
            if (openType == null)
            {
                return null;
            }
            // Don't throw an argument exception for this one.
            if (!openType.IsGenericTypeDefinition)
            {
                return openType;
            }

            Type closedType;
            Type[] genericArguments = openType.GetGenericArguments();
            // If none of the generic parameters have constraints return the 'System.Object'
            if (genericArguments.All(p => p.GenericParameterAttributes == System.Reflection.GenericParameterAttributes.None && p.GenericTypeArguments.Length <= 0))
            {
                // Create a 'System.Object' from openType
                closedType = openType.MakeGenericType(Enumerable.Repeat(typeof(object), genericArguments.Length).ToArray());
                return closedType;
            }

            Type[] createArguments = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type argument = genericArguments[i];

                Type firstMatchingType = TypeListProvider.FirstDomainTypeByPredicate((Type t) => TypeUtility.GenericArgumentAllowsType(argument, openType));
                if (firstMatchingType == null)
                {
                    throw new Exception($"[SerializableTypeEditor::GetFirstValidClosedType] There is no valid constraint compliant type for open type ({openType})'s generic {argument}.");
                }

                createArguments[i] = firstMatchingType;
            }

            closedType = openType.MakeGenericType(createArguments);
            return closedType;
        }

        private static void SetTypeInType(ref Type check, Type target, int targetTypeIndex, Type setValue)
        {
            // FIXME / TODO : This method's impl is kinda faulty
            // Get the target type and the depth + generic type index
            // Otherwise it sets it incorrectly

            // Non-generic type, it is way easier to set the type
            if (check == target)
            {
                check = setValue;
                return;
            }

            // Iterate the type generics tree
            Type[] genericArgs = check.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                // Target matches, set value and bail out
                if (i == targetTypeIndex && genericArgs[i] == target)
                {
                    // Open type if any of the set values are null.
                    if (setValue == null)
                    {
                        check = check.GetGenericTypeDefinition();
                        return;
                    }

                    // We cannot set the type like this
                    //check.GenericTypeArguments[i] = setValue;
                    // So there's two options left : 
                    // A : Create a generic type with placeholder types for the remaining arguments
                    // B : Pass a sliceable type datatype, which has the ability to have resizable GenericTypeArguments
                    // A will suffice for now. This method will trust any class given to it as the dropdown is meant to set this normally.
                    // --
                    // Setting GenericTypeArguments directly like that won't work due to the gathered array being a copy
                    // So retain valid preassigned values and assign new ones

                    // We now can start making values
                    // Make the 'setValue' a closed type if applicable
                    // This is required to make the type valid
                    setValue = GetFirstValidClosedType(setValue);
                    Type[] makeGenericTypes = new Type[genericArgs.Length];
                    for (int j = 0; j < genericArgs.Length; j++)
                    {
                        Type assignedType = genericArgs[j];

                        // Type in 'i' is to be changed to 'setValue'
                        if (i == j)
                        {
                            makeGenericTypes[j] = setValue;
                            continue;
                        }
                        // Retain valid type or set to new given valid type
                        makeGenericTypes[j] = !assignedType.IsGenericParameter ? assignedType : setValue;
                    }

                    // Debug.Log($"set check {check} to {check.GetGenericTypeDefinition().MakeGenericType(makeGenericTypes)}");
                    check = check.GetGenericTypeDefinition().MakeGenericType(makeGenericTypes);
                    return;
                }

                // Check generic recursively
                if (genericArgs[i].IsGenericType)
                {
                    // !! : If the given 'setValue' is an open type, make it closed with the first types.
                    setValue = GetFirstValidClosedType(setValue);
                    // Cannot recurse type this way, because the 'genericArgs[i]' is most likely not a ref.
                    SetTypeInType(ref genericArgs[i], target, targetTypeIndex, setValue);
                    // This most likely sets the 'genericArgs'
                    check = check.GetGenericTypeDefinition().MakeGenericType(genericArgs);
                }
            }
        }

        private Type setRootType;
        private void DrawTypeDropdown(SerializedProperty arrayProperty, Type editConstraintGeneric, Type editType, int typeIndex, Rect position, GUIContent label)
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
                new GUIContent(
                    editType?.GetTypeDefinitionString(true, false) ?? "<null>",
                    string.IsNullOrEmpty(label.tooltip) ? editType?.AssemblyQualifiedName : label.tooltip
                ),
                FocusType.Keyboard
            );

            if (openWindow)
            {
                typeSelector = new TypeSelectorDropdown(editType, (Type t) =>
                {
                    // Disallow non-public
                    if (!t.IsPublic)
                    {
                        return false;
                    }

                    // A open generic argument type, only allow assignable types
                    if (editConstraintGeneric?.IsGenericParameter ?? false)
                    {
                        return TypeUtility.GenericArgumentAllowsType(editConstraintGeneric, t);
                    }

                    return true;
                });
                typeSelector.Show(dropdownSelfRect);

                // Copy the 'SerializedObject' + 'SerializedProperty'
                SerializedObject mainSo = new SerializedObject(arrayProperty.serializedObject.targetObjects);
                SerializedProperty arrayPropertyCopy = mainSo.FindProperty(arrayProperty.propertyPath);
                typeSelector.OnElementSelectedEvent += (SearchDropdownElement element) =>
                {
                    if (!(element is TypeSelectorDropdown.Item typeItem))
                    {
                        return;
                    }

                    Type selectedType = Type.GetType(typeItem.assemblyQualifiedName, false);

                    // Find and change 'editType' on the current type context
                    SetTypeInType(ref setRootType, editType, typeIndex, selectedType);

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
        private void DrawTypesRecursive(SerializedProperty arrayProperty, Type genericConstraintType, Type rootType, int typeIndex, Rect position, GUIContent label)
        {
            // Draw the root type
            DrawTypeDropdown(arrayProperty, genericConstraintType, rootType, typeIndex, position, label);

            if (rootType == null)
            {
                return;
            }

            if (!rootType.IsGenericType)
            {
                return;
            }

            Type[] args = rootType.GetGenericArguments();
            Type openRootType = rootType.GetGenericTypeDefinition();
            Type[] openArgs = openRootType.GetGenericArguments();

            // Get/Recurse all types
            for (int i = 0; i < args.Length; i++)
            {
                Type genericT = args[i];
                Type openGenericT = openArgs[i];

                // Open type
                if (genericT.IsGenericParameter)
                {
                    DrawTypeDropdown(arrayProperty, openGenericT, genericT, i, IndentedRect(position), new GUIContent(label) { text = "Argument" });
                }
                // Closed, draw type recursively for it
                else
                {
                    DrawTypesRecursive(arrayProperty, openGenericT, genericT, i, IndentedRect(position), new GUIContent(label) { text = "Argument" });
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
            DrawTypesRecursive(dataArray, null, target.Type, 0, position, label);

            EditorGUI.EndProperty();
        }
    }
}
