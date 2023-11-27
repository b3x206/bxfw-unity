using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to disable gui on fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyViewAttribute : PropertyAttribute
    { }
}
