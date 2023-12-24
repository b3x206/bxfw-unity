using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A type constraint used to be able to add interface constraints to <see cref="UnityEngine.Object"/> related fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ObjectFieldInterfaceConstraintAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Type interfaceType1;
        public Type interfaceType2;
        public Type interfaceType3;
        public Type interfaceType4;
#endif

        /// <summary>
        /// Creates a 4 type constraint field.
        /// </summary>
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2, Type constraint3, Type constraint4)
        {
#if UNITY_EDITOR
            interfaceType1 = constraint1;
            interfaceType2 = constraint2;
            interfaceType3 = constraint3;
            interfaceType4 = constraint4;
#endif
        }
        /// <summary>
        /// Creates a 3 type constraint field.
        /// </summary>
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2, Type constraint3)
            : this(constraint1, constraint2, constraint3, null)
        { }
        /// <summary>
        /// Creates a 2 type constraint field.
        /// </summary>
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2)
            : this(constraint1, constraint2, null, null)
        { }
        /// <summary>
        /// Creates a 1 type constraint field.
        /// </summary>
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1)
            : this(constraint1, null, null, null)
        { }
    }
}
