using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A type constraint used to be able to add type constraints (of anything inheritable) to <see cref="UnityEngine.Object"/> related fields.
    /// <br>Open type constraints are not supported.</br> <!-- for now -->
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ObjectFieldTypeConstraintAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly Type constraintType1;
        public readonly Type constraintType2;
        public readonly Type constraintType3;
        public readonly Type constraintType4;
#endif

        /// <summary>
        /// Creates a 4 type constraint field.
        /// </summary>
        public ObjectFieldTypeConstraintAttribute(Type constraint1, Type constraint2, Type constraint3, Type constraint4)
        {
#if UNITY_EDITOR
            constraintType1 = constraint1;
            constraintType2 = constraint2;
            constraintType3 = constraint3;
            constraintType4 = constraint4;
#endif
        }
        /// <summary>
        /// Creates a 3 type constraint field.
        /// </summary>
        public ObjectFieldTypeConstraintAttribute(Type constraint1, Type constraint2, Type constraint3)
            : this(constraint1, constraint2, constraint3, null)
        { }
        /// <summary>
        /// Creates a 2 type constraint field.
        /// </summary>
        public ObjectFieldTypeConstraintAttribute(Type constraint1, Type constraint2)
            : this(constraint1, constraint2, null, null)
        { }
        /// <summary>
        /// Creates a 1 type constraint field.
        /// </summary>
        public ObjectFieldTypeConstraintAttribute(Type constraint1)
            : this(constraint1, null, null, null)
        { }
    }
}
