using System;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw a field conditionally on whether the given field's value equal other value given.
    /// <br>Expects object field names that is in the same class/data container. (but properties that take things and are safe to call in editor should work too)</br>
    /// <br/>
    /// <br>Can be used with <see cref="Enum"/>s and other objects that can be embed as attribute metadata.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class DrawIfObjectEqualsAttribute : ConditionalDrawAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of the field to check equality of it's value into <see cref="objectValue"/>.
        /// </summary>
        public readonly string objectFieldName;
        /// <summary>
        /// Value of the object to compare against.
        /// </summary>
        public readonly object objectValue;
        /// <summary>
        /// Type of the given 'objectValue' type.
        /// </summary>
        private readonly Type objectType;
#endif
        /// <summary>
        /// Constructs an 'DrawIfEnumEqualsAttribute'.
        /// </summary>
        /// <param name="objectFieldName">Name of the target field with the enum type.</param>
        /// <param name="objectValue">Value of the target field that can be embed as an attribute.</param>
        public DrawIfObjectEqualsAttribute(string objectFieldName, object objectValue)
        {
#if UNITY_EDITOR
            this.objectFieldName = objectFieldName;

            // Note : Attributes are metadata, so throwing an exception is mostly pointless.
            objectType = objectValue?.GetType();
            this.objectValue = objectValue;
#endif
        }

        protected override DrawCondition DoGetDrawCondition(FieldInfo targetField, object parentValue, out string errorString)
        {
            errorString = string.Empty;
#if UNITY_EDITOR
            if (objectType == null)
            {
                errorString = "Given objectValue is null. Please use [DrawIfNotNull] attribute instead.";
                return DrawCondition.Error;
            }

            // Try getting the FieldInfo
            FieldInfo targetEnumFieldInfo = targetField.DeclaringType.GetField(objectFieldName, TargetFlags);
            if (targetEnumFieldInfo != null)
            {
                // Check if the types match
                if (targetEnumFieldInfo.FieldType != objectType)
                {
                    errorString = $"Target field has incorrect type \"{targetEnumFieldInfo.FieldType.GetTypeDefinitionString()}\", expected type was \"{objectType.GetTypeDefinitionString()}\"";
                    return DrawCondition.Error;
                }

                return TypeUtility.TypedEqualityComparerResult(objectType, targetEnumFieldInfo.GetValue(parentValue), objectValue) ? DrawCondition.True : DrawCondition.False;
            }

            // Try getting the PropertyInfo
            PropertyInfo targetEnumPropertyInfo = targetField.DeclaringType.GetProperty(objectFieldName, TargetFlags);
            if (targetEnumPropertyInfo != null && targetEnumPropertyInfo.CanRead)
            {
                // Check if the types match
                if (targetEnumPropertyInfo.PropertyType != objectType)
                {
                    errorString = $"Target field has incorrect type \"{targetEnumPropertyInfo.PropertyType.GetTypeDefinitionString()}\", expected type was \"{objectType.GetTypeDefinitionString()}\"";
                    return DrawCondition.Error;
                }

                return TypeUtility.TypedEqualityComparerResult(objectType, targetEnumPropertyInfo.GetValue(parentValue), objectValue) ? DrawCondition.True : DrawCondition.False;
            }

            // Both property + value failed
            errorString = "Attribute has incorrect target";
#endif
            return DrawCondition.Error;
        }
    }
}
