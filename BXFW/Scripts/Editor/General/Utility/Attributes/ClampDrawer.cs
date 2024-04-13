using System;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System.Reflection;
using System.Globalization;

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

        private const BindingFlags TargetFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static double GetAttributeMin(ClampAttribute attribute, FieldInfo attributeTargetField, object parentValue)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }
            if (attributeTargetField == null)
            {
                throw new ArgumentNullException(nameof(attributeTargetField));
            }

            if (string.IsNullOrWhiteSpace(attribute.minFieldName))
            {
                return attribute.min;
            }

            // Get either the field or the property
            // Try getting the FieldInfo
            FieldInfo targetNumberFieldInfo = attributeTargetField.DeclaringType.GetField(attribute.minFieldName, TargetFlags);
            if (targetNumberFieldInfo != null)
            {
                return Convert.ToDouble(targetNumberFieldInfo.GetValue(parentValue));
            }

            // Try getting the PropertyInfo
            PropertyInfo targetNumberPropertyInfo = attributeTargetField.DeclaringType.GetProperty(attribute.minFieldName, TargetFlags);
            if (targetNumberPropertyInfo != null && targetNumberPropertyInfo.CanRead && targetNumberPropertyInfo.GetIndexParameters().Length <= 0)
            {
                return Convert.ToDouble(targetNumberPropertyInfo.GetValue(parentValue));
            }

            return double.NegativeInfinity;
        }
        public static double GetAttributeMax(ClampAttribute attribute, FieldInfo attributeTargetField, object parentValue)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }
            if (attributeTargetField == null)
            {
                throw new ArgumentNullException(nameof(attributeTargetField));
            }

            if (string.IsNullOrWhiteSpace(attribute.maxFieldName))
            {
                return attribute.max;
            }

            // Get either the field or the property
            // Try getting the FieldInfo
            FieldInfo targetNumberFieldInfo = attributeTargetField.DeclaringType.GetField(attribute.maxFieldName, TargetFlags);
            if (targetNumberFieldInfo != null)
            {
                return Convert.ToDouble(targetNumberFieldInfo.GetValue(parentValue));
            }

            // Try getting the PropertyInfo
            PropertyInfo targetNumberPropertyInfo = attributeTargetField.DeclaringType.GetProperty(attribute.maxFieldName, TargetFlags);
            if (targetNumberPropertyInfo != null && targetNumberPropertyInfo.CanRead && targetNumberPropertyInfo.GetIndexParameters().Length <= 0)
            {
                return Convert.ToDouble(targetNumberPropertyInfo.GetValue(parentValue));
            }

            return double.PositiveInfinity;
        }

        private PropertyDrawer targetTypeCustomDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= Padding;
            position.y += Padding / 2f;

            bool previousShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            object propertyParent = property.GetParentOfTargetField().value;
            // will throw InvalidCastException if target is invalid
            double attributeMin = GetAttributeMin(Attribute, fieldInfo, propertyParent);
            double attributeMax = GetAttributeMax(Attribute, fieldInfo, propertyParent);

            bool tooltipNull = label.tooltip == null;
            if (tooltipNull || label.tooltip == string.Empty)
            {
                if (tooltipNull)
                {
                    label.tooltip = string.Empty;
                }

                if (attributeMin > float.MinValue)
                {
                    label.tooltip += $"min:{attributeMin.ToString("0.0#####", CultureInfo.InvariantCulture.NumberFormat)}";
                }
                if (attributeMax < float.MaxValue)
                {
                    label.tooltip += $"\nmax:{attributeMax.ToString("0.0#####", CultureInfo.InvariantCulture.NumberFormat)}";
                }
            }

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                // Can't just cast float to double because reasons
                if (property.type.Contains("float", StringComparison.Ordinal))
                {
                    property.floatValue = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), (float)attributeMin, (float)attributeMax);
                }
                else // Assume it's a double
                {
                    property.doubleValue = Math.Clamp(EditorGUI.DoubleField(position, label, property.doubleValue), attributeMin, attributeMax);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();
                property.longValue = Math.Clamp(EditorGUI.LongField(position, label, property.intValue), (long)attributeMin, (long)attributeMax);
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
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either long, double or MinMaxValue. (lower bit numbers allowed)", MessageType.Warning);
            }

            EditorGUI.showMixedValue = previousShowMixedValue;

            EditorGUI.EndProperty();
        }
    }
}
