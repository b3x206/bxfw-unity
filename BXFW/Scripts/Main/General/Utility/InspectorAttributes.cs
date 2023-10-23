using System;
using System.Text.RegularExpressions;
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
    /// A color list containing colors for the inspector line. Default color is <see cref="White"/>.
    /// <br/>
    /// <br><see cref="White"/> = r:1 g:1 b:1</br>
    /// <br><see cref="Red"/>   = r:1 g:0 b:0</br>
    /// <br><see cref="Green"/> = r:0 g:1 b:0</br>
    /// <br><see cref="Blue"/>  = r:0 g:0 b:1</br>
    /// <br/>
    /// <br><see cref="Gray"/>    = r:0.5 g:0.5 b:0.5</br>
    /// <br><see cref="Magenta"/> = r:1 g:0 b:1</br>
    /// <br><see cref="Yellow"/>  = r:1 g:0.92 b:0.016</br>
    /// <br><see cref="Cyan"/>    = r:0 g:1 b:1</br>
    /// </summary>
    public enum LineColor
    {
        White, Red, Green, Blue,
        Gray, Magenta, Yellow, Cyan,
    }

    /// <summary>
    /// Attribute to draw a line using <see cref="EditorAdditionals.DrawUILine(Color, int, int)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InspectorLineAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Color Color { get; private set; }
        public int LineThickness { get; private set; }
        public int LinePadding { get; private set; }
#endif
        public InspectorLineAttribute(LineColor color, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            switch (color)
            {
                case LineColor.White:
                    Color = Color.white;
                    break;
                case LineColor.Red:
                    Color = Color.red;
                    break;
                case LineColor.Green:
                    Color = Color.green;
                    break;
                case LineColor.Blue:
                    Color = Color.blue;
                    break;
                case LineColor.Gray:
                    Color = Color.gray;
                    break;
                case LineColor.Magenta:
                    Color = Color.magenta;
                    break;
                case LineColor.Yellow:
                    Color = Color.yellow;
                    break;
                case LineColor.Cyan:
                    Color = Color.cyan;
                    break;
            }
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

        public InspectorLineAttribute(float r, float g, float b, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            Color = new Color(r, g, b);
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
    /// Attribute to draw a field conditionally.
    /// <br>Only expects absolute bool field names! (but properties that take things and are safe to call in editor should work too)</br>
    /// <br>If you want a 'NoDraw' attribute, try using <see cref="HideInInspector"/> attribute.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InspectorConditionalDrawAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of the field assigned into.
        /// </summary>
        public readonly string boolFieldName;
#endif
        /// <summary>
        /// A bool to whether to invert the draw condition field or not.
        /// </summary>
        public bool ConditionInverted { get; set; } = false;

        public InspectorConditionalDrawAttribute(string boolFieldName)
        {
#if UNITY_EDITOR
            this.boolFieldName = boolFieldName;
#endif
        }
    }

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

    /// <summary>
    /// A <see cref="GameObject.tag"/> selector for the all available tags, to be applied into <see cref="string"/> fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TagSelectorAttribute : PropertyAttribute { }

    /// <summary>
    /// An attribute used in string fields to disallow a set of characters.
    /// <br>Expects a <see cref="string"/>, and disallows all characters in the <see cref="string"/>.</br>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EditDisallowCharsAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// The current list of disallowed characters.
        /// </summary>
        public readonly string disallowText;
        /// <summary>
        /// Whether to assume that the disallowText is regex?
        /// </summary>
        public bool isRegex;
        /// <summary>
        /// If the disallow text is regex, the options to use.
        /// </summary>
        public RegexOptions regexOpts;
#endif
        public EditDisallowCharsAttribute(string disallow)
        {
#if UNITY_EDITOR
            disallowText = disallow;
#endif
        }
    }

    /// <summary>
    /// A type constraint used to add interface constraints to <see cref="UnityEngine.Object"/> related fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ObjectFieldInterfaceConstraintAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Type interfaceType1;
        public Type interfaceType2;
        public Type interfaceType3;
        public Type interfaceType4;
#endif

        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2, Type constraint3, Type constraint4)
        {
#if UNITY_EDITOR
            interfaceType1 = constraint1;
            interfaceType2 = constraint2;
            interfaceType3 = constraint3;
            interfaceType4 = constraint4;
#endif
        }
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2, Type constraint3)
            : this(constraint1, constraint2, constraint3, null)
        { }
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1, Type constraint2)
            : this(constraint1, constraint2, null, null)
        { }
        public ObjectFieldInterfaceConstraintAttribute(Type constraint1)
            : this(constraint1, null, null, null)
        { }
    }
}
