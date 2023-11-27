using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private ClampAttribute Attribute => attribute as ClampAttribute;
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.Float &&
                // Supported by self types
                property.type != typeof(MinMaxValue).Name && property.type != typeof(MinMaxValueInt).Name)
            {
                addHeight += WarningBoxHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + Padding;
            }

            return addHeight;
        }

        private PropertyDrawer targetTypeCustomDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= Padding;
            position.y += Padding / 2f;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                // Can't just cast float to double because reasons
                if (property.type.Contains("float", StringComparison.Ordinal))
                {
                    float v = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), (float)Attribute.min, (float)Attribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set clamped float");
                        property.doubleValue = v;
                    }
                }
                else // Assume it's a double
                {
                    double v = Math.Clamp(EditorGUI.DoubleField(position, label, property.doubleValue), Attribute.min, Attribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set clamped double");
                        property.doubleValue = v;
                    }
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();
                long v = Math.Clamp(EditorGUI.LongField(position, label, property.intValue), (long)Attribute.min, (long)Attribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped int");
                    property.longValue = v;
                }
            }
            // Check if property is a valid type
            // Currently supported (by the PropertyDrawer) are
            // > MinMaxValue, MinMaxValueInt
            else if (property.type == typeof(MinMaxValue).Name || property.type == typeof(MinMaxValueInt).Name)
            {
                targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);
                targetTypeCustomDrawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either int or float.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
