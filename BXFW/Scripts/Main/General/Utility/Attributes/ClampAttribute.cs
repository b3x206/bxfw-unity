using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw clamped integers and floats in fields.
    /// <br>Supports <see cref="MinMaxValue"/> and it's integer counter part.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ClampAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly double min;
        public readonly double max;

        public readonly string minFieldName;
        public readonly string maxFieldName;
#endif
        /// <summary>
        /// Creates a clamping context with constants for min and max clamps.
        /// </summary>
        public ClampAttribute(double min, double max)
        {
#if UNITY_EDITOR
            this.min = min;
            this.max = max;
#endif
        }
        /// <summary>
        /// Creates a clamping context with a target field for the max clamp.
        /// </summary>
        public ClampAttribute(double min, string maxFieldName)
        {
#if UNITY_EDITOR
            this.min = min;
            max = double.PositiveInfinity;

            this.maxFieldName = maxFieldName;
#endif
        }
        /// <summary>
        /// Creates a clamping context with a target field for the min clamp.
        /// </summary>
        public ClampAttribute(string minFieldName, double max)
        {
#if UNITY_EDITOR
            min = double.NegativeInfinity;
            this.max = max;

            this.minFieldName = minFieldName;
#endif
        }
        /// <summary>
        /// Creates a clamping context with a target field for both the min and max clamp.
        /// </summary>
        public ClampAttribute(string minFieldName, string maxFieldName)
        {
#if UNITY_EDITOR
            min = double.NegativeInfinity;
            max = double.PositiveInfinity;

            this.minFieldName = minFieldName;
            this.maxFieldName = maxFieldName;
#endif
        }
    }
}
