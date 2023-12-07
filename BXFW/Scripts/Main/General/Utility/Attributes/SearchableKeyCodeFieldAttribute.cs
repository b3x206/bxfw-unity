using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// When used on <see cref="KeyCode"/> fields, shows a <c>SearchDropdown</c> instead of the old enum selector.
    /// <br>This allows for more convenient input selection.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SearchableKeyCodeFieldAttribute : PropertyAttribute
    { }
}
