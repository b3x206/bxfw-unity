using System;
using System.Reflection;
using System.Collections.Generic;

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

#if UNITY_EDITOR
        /// <summary>
        /// An utility method used to get the typed <see cref="EqualityComparer{T}.Default"/>.
        /// </summary>
        /// <param name="t">
        /// Type of the both <paramref name="x"/> and <paramref name="y"/>.
        /// An <see cref="ArgumentException"/> will be thrown (by the reflection utility) on invocation if the types mismatch.
        /// </param>
        /// <param name="x">First object to compare. This method returns whether if this value is equal to <paramref name="y"/>.</param>
        /// <param name="y">Other way around. Method returns if this is equal to <paramref name="x"/>.</param>
        private bool GetTypedEqualityComparerResult(Type t, object x, object y)
        {
            // Because apparently there's no typeless EqualityComparer?
            // EqualityComparer is used because of the IEquatable check and other things
            // ----- No Typeless EqualityComparer? -----
            Type typedComparerType = typeof(EqualityComparer<>).MakeGenericType(t);
            object typedComparer = typedComparerType.GetProperty(nameof(EqualityComparer<object>.Default), BindingFlags.Public | BindingFlags.Static).GetValue(null);
            MethodInfo typedComparerEqualsMethod = typedComparerType.GetMethod(nameof(EqualityComparer<object>.Equals), 0, new Type[] { t, t });
            return (bool)typedComparerEqualsMethod.Invoke(typedComparer, new object[] { x, y });
        }
#endif
        protected override DrawCondition DoGetDrawCondition(FieldInfo targetField, object parentValue, out string errorString)
        {
            errorString = string.Empty;
#if UNITY_EDITOR
            // Try getting the FieldInfo
            FieldInfo targetFieldInfo = targetField.DeclaringType.GetField(nullableFieldName, TargetFlags);
            if (targetFieldInfo != null)
            {
                bool nullComparisonResult = GetTypedEqualityComparerResult(targetFieldInfo.FieldType, targetFieldInfo.GetValue(parentValue), null);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Try getting the PropertyInfo
            PropertyInfo targetPropertyInfo = targetField.DeclaringType.GetProperty(nullableFieldName, TargetFlags);
            if (targetPropertyInfo != null && targetPropertyInfo.CanRead)
            {
                bool nullComparisonResult = GetTypedEqualityComparerResult(targetPropertyInfo.PropertyType, targetPropertyInfo.GetValue(parentValue), null);
                return nullComparisonResult ? DrawCondition.False : DrawCondition.True;
            }

            // Both property + value failed
            errorString = "Attribute has incorrect target";
#endif
            return DrawCondition.Error;
        }
    }
}
