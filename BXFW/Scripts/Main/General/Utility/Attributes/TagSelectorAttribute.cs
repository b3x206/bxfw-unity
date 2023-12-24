using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A <see cref="GameObject.tag"/> selector for the all available tags, to be applied into <see cref="string"/> fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TagSelectorAttribute : PropertyAttribute
    { }
}
