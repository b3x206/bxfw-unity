using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw <see cref="Sprite"/> fields as a big preview.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BigSpriteFieldAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly float spriteBoxRectHeight = 44f;
#endif
        public BigSpriteFieldAttribute()
        { }

        public BigSpriteFieldAttribute(float spriteHeight)
        {
#if UNITY_EDITOR
            spriteBoxRectHeight = spriteHeight;
#endif
        }
    }

    /// <summary>
    /// Attribute to draw a line using <see cref="EditorAdditionals.DrawUILine(Color, int, int)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InspectorLineAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Color LineColor { get; private set; }
        public int LineThickness { get; private set; }
        public int LinePadding { get; private set; }
#endif
        public InspectorLineAttribute(float r, float g, float b, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            LineColor = new Color(r, g, b);
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Return the draw height.
        /// </summary>
        internal float GetYPosHeightOffset()
        {
            return LineThickness + LinePadding;
        }
#endif
    }

    /// <summary>
    /// Attribute to disable gui on fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyViewAttribute : PropertyAttribute { }

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
        /// If this is true, the array will be asserted to be sorted in reverse.
        /// </summary>
        public bool Reverse { get; set; }
        public SortedArrayAttribute()
        { }
    }

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

    /// <summary>
    /// Attribute to draw clamped UnityEngine vector of most type (except for custom classes/structs) in fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ClampVectorAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly double minX, minY, minZ, minW;
        public readonly double maxX, maxY, maxZ, maxW;
#endif

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
        public ClampVectorAttribute(
            double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ
        ) : this(minX, minY, minZ, 0f, maxX, maxY, maxZ, 0f)
        { }
        public ClampVectorAttribute(
            double minX, double minY,
            double maxX, double maxY
        ) : this(minX, minY, 0f, 0f, maxX, maxY, 0f, 0f)
        { }
        public ClampVectorAttribute(double min, double max) :
            this(min, min, min, min, max, max, max, max)
        { }
    }
}
