using System;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Draws the attribute depending whether if the field is null or not.
    /// <br><see cref="IEquatable{T}"/> values recommended.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class DrawIfNotNullAttribute : ConditionalDrawAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of the field assigned into.
        /// </summary>
        public readonly string nullableFieldName;
#endif

        public DrawIfNotNullAttribute(string checkNullFieldName)
        {
#if UNITY_EDITOR
            nullableFieldName = checkNullFieldName;
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
                bool nullComparisonResult = TypeUtility.TypedEqualityComparerResult(targetFieldInfo.FieldType, targetFieldInfo.GetValue(parentValue), null);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Try getting the PropertyInfo
            PropertyInfo targetPropertyInfo = targetField.DeclaringType.GetProperty(nullableFieldName, TargetFlags);
            if (targetPropertyInfo != null && targetPropertyInfo.CanRead)
            {
                bool nullComparisonResult = TypeUtility.TypedEqualityComparerResult(targetPropertyInfo.PropertyType, targetPropertyInfo.GetValue(parentValue), null);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Both property + value failed
            errorString = "Attribute has incorrect target";
#endif
            return DrawCondition.Error;
        }
    }
}
