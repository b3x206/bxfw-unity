using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// An attribute used in string fields to disallow a set of characters.
    /// <br>Expects a <see cref="string"/>, and disallows all characters in the <see cref="string"/>.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DisallowCharsAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// The current list of disallowed characters.
        /// </summary>
        public readonly string disallowText;
#endif
        // These optional parameters won't compile if the given values won't exist
        /// <summary>
        /// Whether to assume that the disallowText is regex?
        /// </summary>
        public bool isRegex;
        /// <summary>
        /// If the disallow text is regex, the options to use.
        /// </summary>
        public RegexOptions regexOpts;

        public DisallowCharsAttribute(string disallow)
        {
#if UNITY_EDITOR
            disallowText = disallow;
#endif
        }
    }
}
