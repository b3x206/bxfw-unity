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
#endif

        public ClampAttribute(double min, double max)
        {
#if UNITY_EDITOR
            this.min = min;
            this.max = max;
#endif
        }
    }
}
