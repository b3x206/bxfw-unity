using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// A base class for the MinMaxValue classes and it's likes.
    /// <br>Handles the primary drawing, only logic the inheriting classes has to handle is the abstract overrides.</br>
    /// </summary>
    /// <typeparam name="TMinMax">Type of the primary MinMaxValue class/struct.</typeparam>
    /// <typeparam name="TMinMaxField">The value type adjusted in the <typeparamref name="TMinMax"/>.</typeparam>
    public abstract class MinMaxEditor<TMinMax, TMinMaxField> : PropertyDrawer
        where TMinMaxField : struct, IComparable<TMinMaxField>, IEquatable<TMinMaxField>
    {
        protected const float Padding = 2f;

        /// <summary>
        /// Name of the minimum property name.
        /// </summary>
        protected virtual string MinPropertyName => "m_Min";
        /// <summary>
        /// Name of the maximum property.
        /// </summary>
        protected virtual string MaxPropertyName => "m_Max";

        // -- Access
        /// <summary>
        /// Get the values from an <typeparamref name="TMinMax"/>. (deconstruct the class/struct)
        /// </summary>
        protected abstract void GetValues(TMinMax minMax, out TMinMaxField minValue, out TMinMaxField maxValue);
        /// <summary>
        /// Get the value from an <typeparamref name="TMinMax"/>. (which field is unknown)
        /// </summary>
        protected abstract TMinMaxField GetPropertyValue(SerializedProperty property);
        /// <summary>
        /// Set the given <paramref name="property"/>'s value to <paramref name="fieldTypeValue"/>. (set field value)
        /// </summary>
        protected abstract void SetPropertyValue(SerializedProperty property, TMinMaxField fieldTypeValue);

        // -- Operations
        /// <summary>
        /// Create a new <typeparamref name="TMinMax"/> value in this method (basically the constructor).
        /// </summary>
        protected abstract TMinMax CreateMinMax(TMinMaxField minValue, TMinMaxField maxValue);
        /// <summary>
        /// Draw the min max value field in this function. (EditorGUI function for drawing)
        /// </summary>
        protected abstract TMinMax ValueFieldDrawer(Rect position, GUIContent label, TMinMax value);
        /// <summary>
        /// Does the clamping for <typeparamref name="TMinMaxField"/>. (the Clamp attribute)
        /// </summary>
        protected abstract TMinMaxField Clamp(TMinMaxField value, TMinMaxField min, TMinMaxField max);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + Padding;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.y += Padding / 2f;
            position.height -= Padding;

            bool showMixed = EditorGUI.showMixedValue;

            // Do this becuase this is still technically not a property field GUI drawer
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            using SerializedProperty minProperty = property.FindPropertyRelative(MinPropertyName);
            using SerializedProperty maxProperty = property.FindPropertyRelative(MaxPropertyName);

            EditorGUI.BeginChangeCheck();

            TMinMax setValue = ValueFieldDrawer(position, label, CreateMinMax(GetPropertyValue(minProperty), GetPropertyValue(maxProperty)));
            GetValues(setValue, out TMinMaxField min, out TMinMaxField max);

            if (EditorGUI.EndChangeCheck())
            {
                // note to self : Multi editing only happens with the same types, which in that case would have the same attribute on the same object.
                FieldInfo targetPropertyFieldInfo = property.GetTarget().fieldInfo;

                // TODO : Figure out a way to ensure that the object is clamped correctly for the initial clamp.
                // Check supported attributes (for the first object)
                ClampAttribute clamp = targetPropertyFieldInfo.GetCustomAttribute<ClampAttribute>();
                if (clamp != null)
                {
                    object propertyParent = property.GetParentOfTargetField().value;
                    TMinMaxField clampMin = (TMinMaxField)Convert.ChangeType(ClampDrawer.GetAttributeMin(clamp, targetPropertyFieldInfo, propertyParent), typeof(TMinMaxField));
                    TMinMaxField clampMax = (TMinMaxField)Convert.ChangeType(ClampDrawer.GetAttributeMax(clamp, targetPropertyFieldInfo, propertyParent), typeof(TMinMaxField));

                    min = Clamp(min, clampMin, clampMax);
                    max = Clamp(max, clampMin, clampMax);
                }
                // Set limiter
                if (min.CompareTo(max) > 0)
                {
                    max = min;
                }

                // Set limited values
                SetPropertyValue(minProperty, min);
                SetPropertyValue(maxProperty, max);
            }

            EditorGUI.showMixedValue = showMixed;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(MinMaxValue))]
    public class MinMaxValueEditor : MinMaxEditor<MinMaxValue, float>
    {
        protected override float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
        protected override MinMaxValue CreateMinMax(float minValue, float maxValue) => new MinMaxValue(minValue, maxValue);
        protected override float GetPropertyValue(SerializedProperty property) => property.floatValue;
        protected override void SetPropertyValue(SerializedProperty property, float fieldTypeValue) => property.floatValue = fieldTypeValue;
        protected override void GetValues(MinMaxValue minMax, out float minValue, out float maxValue)
        {
            minValue = minMax.Min;
            maxValue = minMax.Max;
        }
        protected override MinMaxValue ValueFieldDrawer(Rect position, GUIContent label, MinMaxValue value) => EditorGUI.Vector2Field(position, label, value);
    }
    /// <summary>
    /// Same as the <see cref="MinMaxValueEditor"/>, but integers.
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxValueInt))]
    public class MinMaxValueIntEditor : MinMaxEditor<MinMaxValueInt, int>
    {
        protected override int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
        protected override MinMaxValueInt CreateMinMax(int minValue, int maxValue) => new MinMaxValueInt(minValue, maxValue);
        protected override int GetPropertyValue(SerializedProperty property) => property.intValue;
        protected override void SetPropertyValue(SerializedProperty property, int fieldTypeValue) => property.intValue = fieldTypeValue;
        protected override void GetValues(MinMaxValueInt minMax, out int minValue, out int maxValue)
        {
            minValue = minMax.Min;
            maxValue = minMax.Max;
        }
        protected override MinMaxValueInt ValueFieldDrawer(Rect position, GUIContent label, MinMaxValueInt value) => EditorGUI.Vector2IntField(position, label, value);
    }
}
