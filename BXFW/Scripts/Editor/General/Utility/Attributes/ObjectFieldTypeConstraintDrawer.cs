using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Handles the logic and draws the drawer on the fields that have <see cref="ObjectFieldTypeConstraintAttribute"/> applied to.
    /// </summary>
    [CustomPropertyDrawer(typeof(ObjectFieldTypeConstraintAttribute))]
    public class ObjectFieldTypeConstraintDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private ObjectFieldTypeConstraintAttribute Attribute => attribute as ObjectFieldTypeConstraintAttribute;
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                addHeight += EditorGUIUtility.singleLineHeight + Padding;
            }
            else
            {
                addHeight += WarningBoxHeight;
            }

            return addHeight;
        }

        private void SetPropertyObjectReferenceValue(SerializedProperty property, UnityEngine.Object objectValue)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (objectValue == null)
            {
                property.objectReferenceValue = null;
                return;
            }

            Type objectValueType = objectValue.GetType();
            UnityEngine.Object assignValue = objectValue;

            bool hasType1 = Attribute.constraintType1 != null,
                hasType2 = Attribute.constraintType2 != null,
                hasType3 = Attribute.constraintType3 != null,
                hasType4 = Attribute.constraintType4 != null;

            // Initial type checking
            // This still sucks but whatever, if it works it's fine..
            // If only unity's field assigner supported multiple types, but this is a very weird use case so whatever..
            // (this just brings covariance and contravariance to the editor, with the given types.. type test them in the code)
            if ((hasType1 && Attribute.constraintType1.IsAssignableFrom(objectValueType)) ||
                (hasType2 && Attribute.constraintType2.IsAssignableFrom(objectValueType)) ||
                (hasType3 && Attribute.constraintType3.IsAssignableFrom(objectValueType)) ||
                (hasType4 && Attribute.constraintType4.IsAssignableFrom(objectValueType)))
            {
                assignValue = objectValue;
            }

            // Check if a component that is type constrainted to this is gettable.
            MethodInfo miTryGetComponent = objectValueType.GetMethod(nameof(GameObject.TryGetComponent), 0, new Type[] { typeof(Type), typeof(Component).MakeByRefType() });
            if (miTryGetComponent != null)
            {
                // Get component if the types inherit from 'Component'
                // This invalidates the 'hasType' booleans so do this last.
                hasType1 = hasType1 && typeof(Component).IsAssignableFrom(Attribute.constraintType1);
                hasType2 = hasType2 && typeof(Component).IsAssignableFrom(Attribute.constraintType2);
                hasType3 = hasType3 && typeof(Component).IsAssignableFrom(Attribute.constraintType3);
                hasType4 = hasType4 && typeof(Component).IsAssignableFrom(Attribute.constraintType4);

                // This value is never written into, only the 'params' is directly written to, which is weird
                // (at first) since you never consider System.Object as pointers in c#
                object[] tryGetComponentParams = new object[] { Attribute.constraintType1, null };
                if (hasType1 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                {
                    assignValue = (Component)tryGetComponentParams[1];
                }
                tryGetComponentParams[0] = Attribute.constraintType2;
                if (hasType2 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                {
                    assignValue = (Component)tryGetComponentParams[1];
                }
                tryGetComponentParams[0] = Attribute.constraintType3;
                if (hasType3 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                {
                    assignValue = (Component)tryGetComponentParams[1];
                }
                tryGetComponentParams[0] = Attribute.constraintType4;
                if (hasType4 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                {
                    assignValue = (Component)tryGetComponentParams[1];
                }
            }

            property.objectReferenceValue = assignValue;
            property.serializedObject.ApplyModifiedProperties(); // force repainting / updating whatever was assigned to the field
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= Padding;
            position.y += Padding / 2f;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginChangeCheck();

                bool showMixed = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = property.hasMultipleDifferentValues; // ObjectField without 'SerializedProperty' doesn't handle this

                // If the fieldInfo is a type of array or if this attribute is applied to arrays the 'fieldInfo.FieldType' is an array
                // This is not wanted, but whatever.. here's the fix. so done with the PropertyDrawer shenanigans
                Type objectFieldType = fieldInfo.FieldType;
                if (objectFieldType.IsArray)
                {
                    objectFieldType = objectFieldType.GetElementType();
                }
                else if (objectFieldType.IsGenericType && typeof(IList<>).IsAssignableFromOpenGeneric(objectFieldType.GetGenericTypeDefinition()))
                {
                    objectFieldType = objectFieldType.GetGenericArguments().First();
                }

                if (string.IsNullOrEmpty(label.tooltip))
                {
                    label.tooltip = "[ObjectFieldTypeConstraint]";

                    // Add the type constraints as tooltip(s)
                    if (Attribute.constraintType1 != null)
                    {
                        label.tooltip += $"\n{Attribute.constraintType1.GetTypeDefinitionString(true)}";
                    }
                    if (Attribute.constraintType2 != null)
                    {
                        label.tooltip += $"\n{Attribute.constraintType2.GetTypeDefinitionString(true)}";
                    }
                    if (Attribute.constraintType3 != null)
                    {
                        label.tooltip += $"\n{Attribute.constraintType3.GetTypeDefinitionString(true)}";
                    }
                    if (Attribute.constraintType4 != null)
                    {
                        label.tooltip += $"\n{Attribute.constraintType4.GetTypeDefinitionString(true)}";
                    }
                }

                var objectValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, objectFieldType, true);
                EditorGUI.showMixedValue = showMixed;

                if (EditorGUI.EndChangeCheck())
                {
                    SetPropertyObjectReferenceValue(property, objectValue);
                }
            }
            else
            {
                EditorGUI.HelpBox(position, $"Given type isn't valid for property {label.text}. Please use on UnityEngine.Object deriving as type fields.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
