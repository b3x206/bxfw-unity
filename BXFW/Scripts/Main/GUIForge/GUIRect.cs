using System;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW.GUIForge
{
    /// <summary>
    /// This is a Rect struct that can contain relative size.
    /// <br>It is behaviourally same to <see cref="Rect"/>.</br>
    /// </summary>
    [Serializable]
    public struct GUIRect : IEquatable<Rect>, IEquatable<GUIRect>
    {
        public static readonly GUIRect zero = new GUIRect(0f, 0f, 0f, 0f);

        public GUIRect(float x, float y, float relativeW, float relativeH, Rect relative) 
            : this(x, y, relativeW, relativeH)
        {
            isRelative = true;
            relativeRect = relative;
        }
        public GUIRect(Vector2 pos, Vector2 size, Rect relative)
            : this(pos, size)
        {
            isRelative = true;
            relativeRect = relative;
        }
        public GUIRect(Rect copy, Rect relative)
            : this(copy)
        {
            isRelative = true;
            relativeRect = relative;
        }

        public GUIRect(float x, float y, float w, float h)
        {
            mXMin = x;
            mYMin = y;
            mWidth = w;
            mHeight = h;

            isRelative = false;
            relativeRect = Rect.zero;
        }
        public GUIRect(Vector2 pos, Vector2 size)
            : this(pos.x, pos.y, size.x, size.y)  { }
        public GUIRect(Rect copy)
            : this(copy.x, copy.y, copy.width, copy.height)
        { }
        public GUIRect(GUIRect copy)
            : this(copy.x, copy.y, copy.width, copy.height)
        {
            isRelative = copy.isRelative;
            relativeRect = copy.relativeRect;
        }

        /// <summary>
        /// Whether if the rect is relative.
        /// </summary>
        public bool isRelative;
        /// <summary>
        /// Which rect this is relative to.
        /// </summary>
        public Rect relativeRect;

        /// <summary>
        /// The 100% value of the relativeness.
        /// <br>(basically if <see cref="isRelative"/> <see langword="true"/> then width and height go between 0~<see cref="PERCENT"/>)</br>
        /// </summary>
        public const int PERCENT = 1;

        private float mXMin;   // X top left corner
        private float mYMin;   // Y top left corner
        private float mWidth;  // Width of the rect
        private float mHeight; // Height of the rect

        /// <summary>
        /// X position of the rect.
        /// <br>Is equal t</br>
        /// </summary>
        public float x
        {
            get 
            {
                if (isRelative)
                    return relativeRect.x - mXMin;

                return mXMin; 
            }
            set 
            { 
                mXMin = value; 
            }
        }
        /// <summary>
        /// Y position of the rect.
        /// </summary>
        public float y
        {
            get
            {
                if (isRelative)
                    return relativeRect.y - mYMin;

                return mYMin;
            }
            set
            {
                mYMin = value;
            }
        }
        /// <summary>
        /// Position of the rect.
        /// </summary>
        public Vector2 position
        {
            get { return new Vector2(x, y); }
            set
            {
                x = value.x;
                y = value.y;
            }
        }

        public float absWidth => isRelative ? (mWidth * relativeRect.width) / PERCENT : mWidth;
        public float absHeight => isRelative ? (mHeight * relativeRect.height) / PERCENT : mHeight;
        public float width
        {
            get
            {
                return isRelative ? (mWidth / PERCENT) * relativeRect.width : mWidth;
            }
            set
            {
                mWidth = Mathf.Clamp(value, 0, PERCENT);
            }
        }
        public float height
        {
            get
            {
                return isRelative ? (mHeight / PERCENT) * relativeRect.height : mHeight;
            }
            set
            {
                mHeight = Mathf.Clamp(value, 0, PERCENT);
            }
        }
        public Vector2 size
        {
            get { return new Vector2(width, height); }
            set { width = value.x; height = value.y; }
        }
        /// <summary>
        /// Center of the rectangular structure object's reference that you are accessing from, also known as a rect.
        /// </summary>
        public Vector2 center
        {
            get { return new Vector2(x + (mWidth / 2f), y + (mHeight / 2f)); }
            set
            {
                x = value.x + (mWidth / 2f);
                y = value.y + (mHeight / 2f);
            }
        }
        public Vector2 min
        {
            get { return new Vector2(mXMin, mYMin); }
            set { mXMin = value.x; mYMin = value.y; }
        }
        public Vector2 max
        {
            get { return new Vector2(mXMin + absWidth, mYMin + absHeight); }
            set
            {
                mXMin = value.x - mXMin;
                mYMin = value.y - mYMin;
            }
        }
        
        /// <summary>
        /// Returns whether if the <paramref name="point"/> is contained.
        /// </summary>
        public bool Contains(Vector2 point)
        {
            return point.x >= min.x && point.x < max.x && 
                point.y >= min.y && point.y < max.y;
        }

        public static bool operator ==(GUIRect lhs, GUIRect rhs)
        {
            // structs are non-nullable
            return lhs.Equals(rhs);
        }
        public static bool operator !=(GUIRect lhs, GUIRect rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator Rect(GUIRect r)
        {
            return new Rect(r);
        }
        public static implicit operator GUIRect(Rect r)
        {
            return new GUIRect(r);
        }

        public override bool Equals(object other)
        {
            if (other is GUIRect r)
            {
                return Equals(r);
            }

            return false;
        }
        public bool Equals(GUIRect other)
        {
            return x == other.x && y == other.y && width == other.width && height == other.height;
        }
        public bool Equals(Rect other)
        {
            return mXMin == other.x && mYMin == other.y && width == other.width && height == other.height;
        }

        public override int GetHashCode()
        {
            int hashCode = 525882332;
            hashCode = (hashCode * -1521134295) + isRelative.GetHashCode();
            hashCode = (hashCode * -1521134295) + relativeRect.GetHashCode();
            hashCode = (hashCode * -1521134295) + mXMin.GetHashCode();
            hashCode = (hashCode * -1521134295) + mYMin.GetHashCode();
            hashCode = (hashCode * -1521134295) + mWidth.GetHashCode();
            hashCode = (hashCode * -1521134295) + mHeight.GetHashCode();

            return hashCode;
        }
        public override string ToString()
        {
            return $"min : {min} | size : {size}";
        }
    }
}