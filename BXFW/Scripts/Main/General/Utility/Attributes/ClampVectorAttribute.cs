using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw clamped UnityEngine vector of most type (except for custom classes/structs) in fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ClampVectorAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Minimum value of the vector component.
        /// <br>If both min and max is set to zero, the value will be locked at zero.</br>
        /// </summary>
        public readonly double minX, minY, minZ, minW;
        /// <summary>
        /// Maximum value of the vector component.
        /// <br>If both min and max is set to zero, the value will be locked at zero.</br>
        /// </summary>
        public readonly double maxX, maxY, maxZ, maxW;
#endif

        /// <summary>
        /// Allows for setting a 4 dimensional vector clamp. All components are only used in <see cref="Vector4"/>'s.
        /// </summary>
        public ClampVectorAttribute(
            double minX, double minY, double minZ, double minW,
            double maxX, double maxY, double maxZ, double maxW
        )
        {
#if UNITY_EDITOR
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.minW = minW;

            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
            this.maxW = maxW;
#endif
        }

        /// <summary>
        /// Allows for setting a 3 dimensional vector clamp. Targeted towards fields with <see cref="Vector3"/> and <see cref="Vector3Int"/>'s.
        /// </summary>
        public ClampVectorAttribute(
            double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ
        ) : this(minX, minY, minZ, 0f, maxX, maxY, maxZ, 0f)
        { }
        /// <summary>
        /// Allows for setting a 2 dimensional vector clamp. Targeted towards fields with <see cref="Vector2"/> and <see cref="Vector2Int"/>'s.
        /// </summary>
        public ClampVectorAttribute(
            double minX, double minY,
            double maxX, double maxY
        ) : this(minX, minY, 0f, 0f, maxX, maxY, 0f, 0f)
        { }
        /// <summary>
        /// Sets all vector dimension Min/Max components to it's corresponding <paramref name="min"/> and <paramref name="max"/>.
        /// <br>Works with all dimensional vectors.</br>
        /// </summary>
        public ClampVectorAttribute(double min, double max) :
            this(min, min, min, min, max, max, max, max)
        { }
    }
}
