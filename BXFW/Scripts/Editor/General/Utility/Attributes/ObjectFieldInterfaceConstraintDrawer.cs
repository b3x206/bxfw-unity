using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ObjectFieldInterfaceConstraintAttribute))]
    public class ObjectFieldInterfaceConstraintDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private ObjectFieldInterfaceConstraintAttribute Attribute => attribute as ObjectFieldInterfaceConstraintAttribute;
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
                var objectValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
                EditorGUI.showMixedValue = showMixed;

                if (EditorGUI.EndChangeCheck())
                {
                    if (objectValue == null)
                    {
                        property.objectReferenceValue = null;
                        EditorGUI.EndProperty();
                        return;
                    }

                    bool hasType1 = Attribute.interfaceType1 == null,
                         hasType2 = Attribute.interfaceType2 == null,
                         hasType3 = Attribute.interfaceType3 == null,
                         hasType4 = Attribute.interfaceType4 == null;

                    // Find component with attributes if the object is a 'GetComponent'able one
                    Type objectValueType = objectValue.GetType();
                    MethodInfo miTryGetComponent = objectValueType.GetMethod(nameof(Component.TryGetComponent), 0, new Type[] { typeof(Type), typeof(Component).MakeByRefType() });
                    if (miTryGetComponent != null)
                    {
                        // This value is never written into, only the 'params' is directly written to.
                        // Since the object[] is a pointer array, we can just cast null to component (epic safe code)
                        object[] tryGetComponentParams = new object[] { Attribute.interfaceType1, (Component)null };

                        // This is not code, this is a travesty.
                        if (!hasType1 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType2;
                        if (!hasType2 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType3;
                        if (!hasType3 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType4;
                        if (!hasType4 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                    }
                    else
                    {
                        // Since i'm stupid and attributes don't support anything other than basic data types on ctor.
                        // This is the way i did it. If there's a better way please tell.
                        Type[] interfaces = objectValueType.GetInterfaces();

                        foreach (Type interfaceType in interfaces)
                        {
                            if (!hasType1)
                            {
                                hasType1 = interfaceType == Attribute.interfaceType1;
                            }

                            if (!hasType2)
                            {
                                hasType2 = interfaceType == Attribute.interfaceType2;
                            }

                            if (!hasType3)
                            {
                                hasType3 = interfaceType == Attribute.interfaceType3;
                            }

                            if (!hasType4)
                            {
                                hasType4 = interfaceType == Attribute.interfaceType4;
                            }
                        }
                        if (hasType1 && hasType2 && hasType3 && hasType4)
                        {
                            property.objectReferenceValue = objectValue;
                        }
                    }
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
