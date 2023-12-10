using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Contains a minimum/maximum value range.
    /// <br><see cref="Min"/> cannot be larger than <see cref="Max"/> and the other way around.</br>
    /// <br/>
    /// <br>This struct supports <see cref="ClampAttribute"/>.</br>
    /// </summary>
    [Serializable]
    public struct MinMaxValue : IEquatable<MinMaxValue>, IEquatable<MinMaxValueInt>
    {
        // - Variable+Property
        [SerializeField] private float m_Min;
        [SerializeField] private float m_Max;

        public float Min
        {
            get { return m_Min; }
            set
            {
                m_Min = Math.Clamp(value, float.NegativeInfinity, Max);
            }
        }
        public float Max
        {
            get { return m_Max; }
            set
            {
                m_Max = Math.Clamp(value, Min, float.PositiveInfinity);
            }
        }

        // - Ctors
        /// <summary>
        /// Constructs the MinMaxValue.
        /// <br>Checks the given parameters for appopriateness. If the values are misplaced, they are swapped.</br>
        /// </summary>
        public MinMaxValue(float min, float max)
        {
            // Check if the rules apply for those values
            if (min > max)
            {
                (min, max) = (max, min);
            }

            m_Min = min;
            m_Max = max;
        }

        // - Constants
        public static readonly MinMaxValue Zero = new MinMaxValue(0f, 0f);

        // - Methods
        /// <summary>
        /// Returns a random value between <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        public float GetRandomBetween()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
        /// <summary>
        /// Clamps given value between <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="value">Target value to call <see cref="Mathf.Clamp(float, float, float)"/> on.</param>
        /// <returns>Clamped value.</returns>
        public float ClampBetween(float value)
        {
            return Math.Clamp(value, Min, Max);
        }
        /// <summary>
        /// 1D size difference as <see cref="Max"/> - <see cref="Min"/>
        /// </summary>
        public float Size()
        {
            return Max - Min;
        }

        // - Operators
        public static implicit operator Vector2(MinMaxValue value)
        {
            return new Vector2(value.Min, value.Max);
        }
        public static implicit operator MinMaxValue(Vector2 value)
        {
            return new MinMaxValue(value.x, value.y);
        }

        /// <summary>
        /// Multiplies the <see cref="MinMaxValue"/> by <paramref name="rhs"/>.
        /// <br>If 'rhs' is a negative number and <see cref="Min"/> is more than <see cref="Max"/>, the values will swap.</br>
        /// </summary>
        public static MinMaxValue operator *(MinMaxValue lhs, float rhs)
        {
            lhs.m_Min *= rhs;
            lhs.m_Max *= rhs;

            if (lhs.m_Min > lhs.m_Max)
            {
                // Sign flipped
                if (rhs < 0)
                {
                    (lhs.m_Max, lhs.m_Min) = (lhs.m_Min, lhs.m_Max);
                }
                // No sign flipping, clamp min
                else
                {
                    lhs.m_Min = lhs.m_Max;
                }
            }

            return lhs;
        }
        /// <summary>
        /// Divides the <see cref="MinMaxValue"/> by <paramref name="rhs"/>.
        /// <br>If 'rhs' is a negative number and <see cref="Min"/> is more than <see cref="Max"/>, the values will swap.</br>
        /// </summary>
        public static MinMaxValue operator /(MinMaxValue lhs, float rhs)
        {
            lhs.m_Min /= rhs;
            lhs.m_Max /= rhs;

            if (lhs.m_Min > lhs.m_Max)
            {
                // Sign flipped
                if (rhs < 0)
                {
                    (lhs.m_Max, lhs.m_Min) = (lhs.m_Min, lhs.m_Max);
                }
                // No sign flipping, clamp min
                else
                {
                    lhs.m_Min = lhs.m_Max;
                }
            }

            return lhs;
        }

        public static bool operator ==(MinMaxValue lhs, MinMaxValue rhs)
        {
            // Epsilon equals
            float diffMin = lhs.Min - rhs.Min;
            float diffMax = lhs.Max - rhs.Max;

            // mul twice to force flip sign to positive
            // epsilon is multiplied by 4 for possible 4x epsilons difference
            return (diffMin * diffMin) + (diffMax * diffMax) < float.Epsilon * 4f;
        }
        public static bool operator !=(MinMaxValue lhs, MinMaxValue rhs)
        {
            return !(lhs == rhs);
        }
        public bool Equals(MinMaxValue v)
        {
            return Min == v.Min && Max == v.Max;
        }
        public bool Equals(MinMaxValueInt v)
        {
            return Min == v.Min && Max == v.Max;
        }

        public override bool Equals(object obj)
        {
            if (obj is MinMaxValue v)
            {
                return Equals(v);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 927388948;
            unchecked
            {
                hash = (hash * -1521134295) + Min.GetHashCode();
                hash = (hash * -1521134295) + Max.GetHashCode();
            }

            return hash;
        }
        public override string ToString()
        {
            return string.Format("min={0:G}, max={1:G}", Min, Max);
        }
    }

    /// <summary>
    /// Same as <see cref="MinMaxValue"/> range, but integers.
    /// <br/>
    /// <br>This struct supports <see cref="ClampAttribute"/>.</br>
    /// </summary>
    [Serializable]
    public struct MinMaxValueInt : IEquatable<MinMaxValueInt>, IEquatable<MinMaxValue>
    {
        // - Variables+Property
        [SerializeField] private int m_Min;
        [SerializeField] private int m_Max;

        public int Min
        {
            get { return m_Min; }
            set
            {
                m_Min = Mathf.Clamp(value, int.MinValue, Max);
            }
        }
        public int Max
        {
            get { return m_Max; }
            set
            {
                m_Max = Mathf.Clamp(value, Min, int.MaxValue);
            }
        }

        // - Ctors
        /// <summary>
        /// Constructs the MinMaxValueInt.
        /// <br>Checks the values for appopriateness. If the values are misplaced, they are swapped.</br>
        /// </summary>
        public MinMaxValueInt(int min, int max)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }

            m_Min = min;
            m_Max = max;
        }

        // - Constants
        public static readonly MinMaxValueInt Zero = new MinMaxValueInt(0, 0);

        // - Methods
        /// <summary>
        /// Returns a random value between min and max.
        /// </summary>
        public int GetRandomBetween()
        {
            return UnityEngine.Random.Range(Min, Max);
        }
        /// <summary>
        /// Clamps given value between <see cref="Min"/> and <see cref="Max"/>.
        /// </summary>
        /// <param name="value">Target value to call <see cref="Mathf.Clamp(int, int, int)"/> on.</param>
        /// <returns>Clamped value.</returns>
        public int ClampBetween(int value)
        {
            return Math.Clamp(value, Min, Max);
        }
        /// <summary>
        /// 1D size difference as <see cref="Max"/> - <see cref="Min"/>
        /// </summary>
        public int Size()
        {
            return Max - Min;
        }

        // - Operators
        public static implicit operator Vector2(MinMaxValueInt value)
        {
            return new Vector2Int(value.Min, value.Max);
        }
        public static implicit operator Vector2Int(MinMaxValueInt value)
        {
            return new Vector2Int(value.Min, value.Max);
        }
        public static implicit operator MinMaxValueInt(Vector2Int value)
        {
            return new MinMaxValueInt(value.x, value.y);
        }
        public static explicit operator MinMaxValue(MinMaxValueInt value)
        {
            return new MinMaxValue(value.Min, value.Max);
        }

        /// <summary>
        /// Multiplies the <see cref="MinMaxValue"/> by <paramref name="rhs"/>.
        /// <br>If 'rhs' is a negative number and <see cref="Min"/> is more than <see cref="Max"/>, the values will swap.</br>
        /// </summary>
        public static MinMaxValueInt operator *(MinMaxValueInt lhs, int rhs)
        {
            lhs.m_Min *= rhs;
            lhs.m_Max *= rhs;

            if (lhs.m_Min > lhs.m_Max)
            {
                // Sign flipped
                if (rhs < 0)
                {
                    (lhs.m_Max, lhs.m_Min) = (lhs.m_Min, lhs.m_Max);
                }
                // No sign flipping, clamp min
                else
                {
                    lhs.m_Min = lhs.m_Max;
                }
            }

            return lhs;
        }
        /// <summary>
        /// Divides the <see cref="MinMaxValue"/> by <paramref name="rhs"/>.
        /// <br>If 'rhs' is a negative number and <see cref="Min"/> is more than <see cref="Max"/>, the values will swap.</br>
        /// </summary>
        public static MinMaxValueInt operator /(MinMaxValueInt lhs, int rhs)
        {
            lhs.m_Min /= rhs;
            lhs.m_Max /= rhs;

            if (lhs.m_Min > lhs.m_Max)
            {
                // Sign flipped
                if (rhs < 0)
                {
                    (lhs.m_Max, lhs.m_Min) = (lhs.m_Min, lhs.m_Max);
                }
                // No sign flipping, clamp min
                else
                {
                    lhs.m_Min = lhs.m_Max;
                }
            }

            return lhs;
        }

        public static bool operator ==(MinMaxValueInt lhs, MinMaxValueInt rhs)
        {
            // int is int, doesn't require epsilon shenanigans
            return lhs.Equals(rhs);
        }
        public static bool operator !=(MinMaxValueInt lhs, MinMaxValueInt rhs)
        {
            return !(lhs == rhs);
        }
        public bool Equals(MinMaxValueInt v)
        {
            return Min == v.Min && Max == v.Max;
        }
        public bool Equals(MinMaxValue v)
        {
            return Min == v.Min && Max == v.Max;
        }
        public override bool Equals(object obj)
        {
            if (obj is MinMaxValueInt v)
            {
                return Equals(v);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 927388948;
            unchecked
            {
                hash = (hash * -1521134295) + Min.GetHashCode();
                hash = (hash * -1521134295) + Max.GetHashCode();
            }

            return hash;
        }
        public override string ToString()
        {
            return string.Format("min={0}, max={1}", Min, Max);
        }
    }
}
