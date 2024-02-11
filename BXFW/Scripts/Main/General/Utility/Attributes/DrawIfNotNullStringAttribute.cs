using System;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Draws the attribute target field depending whether if the string field is null or not.
    /// <br>This is only meant to be targeting strings (<i>checkNullFieldName</i> property should be a name of a <see cref="string"/> field/property), 
    /// if the target field type is not <see cref="string"/> this will not work, use the <see cref="DrawIfNotNullAttribute"/> instead.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class DrawIfNotNullStringAttribute : ConditionalDrawAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of the field assigned into.
        /// </summary>
        public readonly string nullableFieldName;
#endif
        /// <summary>
        /// Whether to also consider whitespace string as null. (<see cref="string.IsNullOrWhiteSpace(string)"/> is used instead of <see cref="string.IsNullOrEmpty(string)"/>)
        /// </summary>
        public bool ConsiderWhitespaceNull { get; set; } = false;

        public DrawIfNotNullStringAttribute(string targetStringFieldName)
        {
#if UNITY_EDITOR
            nullableFieldName = targetStringFieldName;
#endif
        }

        protected override DrawCondition DoGetDrawCondition(FieldInfo targetField, object parentValue, out string errorString)
        {
            errorString = string.Empty;
#if UNITY_EDITOR
            // Try getting the FieldInfo
            FieldInfo targetFieldInfo = targetField.DeclaringType.GetField(nullableFieldName, TargetFlags);
            if (targetFieldInfo != null)
            {
                if (targetFieldInfo.FieldType != typeof(string))
                {
                    errorString = $"Attribute target field type isn't System.String, it's instead {targetFieldInfo.FieldType.GetTypeDefinitionString()}";
                    return DrawCondition.Error;
                }

                string castedTargetPropertyValue = targetFieldInfo.GetValue(parentValue) as string;
                bool nullComparisonResult = ConsiderWhitespaceNull ? string.IsNullOrWhiteSpace(castedTargetPropertyValue) : string.IsNullOrEmpty(castedTargetPropertyValue);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Try getting the PropertyInfo
            PropertyInfo targetPropertyInfo = targetField.DeclaringType.GetProperty(nullableFieldName, TargetFlags);
            if (targetPropertyInfo != null && targetPropertyInfo.CanRead)
            {
                if (targetFieldInfo.FieldType != typeof(string))
                {
                    errorString = $"Attribute target property type isn't System.String, it's instead {targetFieldInfo.FieldType.GetTypeDefinitionString()}";
                    return DrawCondition.Error;
                }

                string castedTargetPropertyValue = targetPropertyInfo.GetValue(parentValue) as string;
                bool nullComparisonResult = ConsiderWhitespaceNull ? string.IsNullOrWhiteSpace(castedTargetPropertyValue) : string.IsNullOrEmpty(castedTargetPropertyValue);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Both property + value failed
            errorString = "Attribute has incorrect named target field/property";
#endif
            return DrawCondition.Error;
        }
    }
}
