using System;
using System.Reflection;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw a field conditionally.
    /// <br>Only expects absolute bool field names! (but properties that take things and are safe to call in editor should work too)</br>
    /// <br>If you want a 'NoDraw' attribute, try using <see cref="UnityEngine.HideInInspector"/> attribute.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class DrawIfAttribute : ConditionalDrawAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of the field assigned into.
        /// </summary>
        public readonly string boolFieldName;
#endif

        public DrawIfAttribute(string boolFieldName)
        {
#if UNITY_EDITOR
            this.boolFieldName = boolFieldName;
#endif
        }

        protected override DrawCondition DoGetDrawCondition(FieldInfo targetField, object parentValue, out string errorString)
        {
            errorString = string.Empty;
#if UNITY_EDITOR
            // Try getting the FieldInfo
            FieldInfo targetBoolFieldInfo = targetField.DeclaringType.GetField(boolFieldName, TargetFlags);
            if (targetBoolFieldInfo != null)
            {
                return (bool)targetBoolFieldInfo.GetValue(parentValue) ? DrawCondition.True : DrawCondition.False;
            }

            // Try getting the PropertyInfo
            PropertyInfo targetBoolPropertyInfo = targetField.DeclaringType.GetProperty(boolFieldName, TargetFlags);
            if (targetBoolPropertyInfo != null && targetBoolPropertyInfo.CanRead)
            {
                return (bool)targetBoolPropertyInfo.GetValue(parentValue) ? DrawCondition.True : DrawCondition.False;
            }

            // Both property + value failed
            errorString = "Attribute has incorrect target";
#endif
            return DrawCondition.Error;
        }
    }
}
