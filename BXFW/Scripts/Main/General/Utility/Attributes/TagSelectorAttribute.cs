using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A <see cref="GameObject.tag"/> selector for the all available tags, to be applied into <see cref="string"/> fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TagSelectorAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Whether to show no tag / null string as an option.
        /// <br>When selected, the no tag option will set the string as <see cref="string.Empty"/>.</br>
        /// </summary>
        public readonly bool showEmptyOption = false;
#endif

        public TagSelectorAttribute()
        { }

        /// <param name="showEmptyOption">
        /// Whether to show no tag as an option.
        /// <br>When selected, the no tag option will set the string as <see cref="string.Empty"/>.</br>
        /// </param>
        public TagSelectorAttribute(bool showEmptyOption)
        {
#if UNITY_EDITOR
            this.showEmptyOption = showEmptyOption;
#endif
        }
    }
}
