using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to assert a sorted array drawer. Can be only applied to array values that have <see cref="IComparable{T}"/>, otherwise it will draw a warning box.
    /// <br/>
    /// <br>For non-numerical types that have <see cref="IComparable{T}"/>, the array values will be switched. (<see cref="Array.Sort(Array)"/> will be called)</br>
    /// <br>For numerical types the first and the last value can be changed freely while other values will be clamped between it's previous and next.</br>
    /// <br>Other types that don't have <see cref="IComparable{T}"/> or the attribute parent is not an array, the attribute will display a warning.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SortedArrayAttribute : PropertyAttribute
    {
        /// <summary>
        /// If this is true, the array will be asserted to be sorted in reverse instead.
        /// </summary>
        public bool Reverse { get; set; }

        public SortedArrayAttribute()
        { }
    }
}
