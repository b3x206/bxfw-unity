using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw <see cref="Sprite"/> fields with a big preview texture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpriteAreaAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly float spriteBoxRectHeight = 44f;
#endif
        public SpriteAreaAttribute()
        { }

        public SpriteAreaAttribute(float spriteHeight)
        {
#if UNITY_EDITOR
            spriteBoxRectHeight = spriteHeight;
#endif
        }
    }
}
