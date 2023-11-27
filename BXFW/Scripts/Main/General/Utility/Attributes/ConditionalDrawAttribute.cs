using System;
using System.Reflection;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// An attribute that allows conditional drawing of the field that it's applied into.
    /// <br>Only works in <see cref="AttributeTargets.Field"/> or anything that unity serializes.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public abstract class ConditionalDrawAttribute : PropertyAttribute
    {
        /// <summary>
        /// Target flags shorthand to be able to check the serialized non-private and private fields.
        /// </summary>
        protected const BindingFlags TargetFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// A bool to whether to invert the draw condition field or not.
        /// </summary>
        public bool ConditionInverted { get; set; } = false;

        /// <summary>
        /// State defined for drawing.
        /// <br><see cref="False"/> =&gt; No drawing allowed</br>
        /// <br><see cref="True"/> =&gt; Drawing allowed</br>
        /// <br><see cref="Error"/> =&gt; Invalid object/arguments.</br>
        /// </summary>
        public enum DrawCondition
        {
            False, True, Error
        }

        /// <summary>
        /// Return the condition of this attribute.
        /// <br>This is the internal call to be overriden, it is called by <see cref="GetDrawCondition(FieldInfo, object, out string)"/>.</br>
        /// </summary>
        protected abstract DrawCondition DoGetDrawCondition(FieldInfo targetField, object parentValue, out string errorString);
        /// <summary>
        /// Return the condition of this attribute.
        /// </summary>
        public DrawCondition GetDrawCondition(FieldInfo targetField, object parentValue, out string errorString)
        {
            DrawCondition result = DoGetDrawCondition(targetField, parentValue, out errorString);

            if (ConditionInverted)
            {
                switch (result)
                {
                    case DrawCondition.False:
                        result = DrawCondition.True;
                        break;
                    case DrawCondition.True:
                        result = DrawCondition.False;
                        break;

                    default:
                        break;
                }
            }

            return result;
        }
    }
}
