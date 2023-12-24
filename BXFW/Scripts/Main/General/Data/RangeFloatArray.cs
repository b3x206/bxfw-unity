using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BXFW
{
    /// <summary>
    /// Acts like a regular float array, but the values can't be equal and they are clamped between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [Serializable]
    public class RangeFloatArray : IList<float>, IEquatable<RangeFloatArray>, IEquatable<IEnumerable<float>>
    {
        [SerializeField, FormerlySerializedAs("array")]
        private List<float> m_values = new List<float>();
        [SerializeField, FormerlySerializedAs("min")]
        private float m_Min = 0f;
        [SerializeField, FormerlySerializedAs("max")]
        private float m_Max = 1f;
        /// <summary>
        /// The minimum difference between the minimum and maximum values.
        /// </summary>
        private const float MinMaxOffset = .0001f;

        /// <summary>
        /// Minimum value of the elements in the array defined.
        /// <br>Changing this will also change array elements.</br>
        /// </summary>
        public float Min
        {
            get { return m_Min; }
            set 
            { 
                m_Min = Mathf.Clamp(value, float.MinValue, Max - (MinMaxOffset + Mathf.Epsilon));
            
                // Check array
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] < m_Min)
                    {
                        this[i] = m_Min;
                    }
                }
            }
        }
        /// <summary>
        /// Maximum value of the elements in the array that is defined.
        /// </summary>
        public float Max
        {
            get { return m_Max; }
            set
            { 
                m_Max = Mathf.Clamp(value, Min + (MinMaxOffset + Mathf.Epsilon), float.MaxValue);

                // Check array
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] > m_Max)
                    {
                        this[i] = m_Max;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a 'RangeFloatArray' with no settings set.
        /// <br>Default settings set <see cref="Min"/> to 0 and <see cref="Max"/> to 1.</br>
        /// </summary>
        public RangeFloatArray() { }
        /// <summary>
        /// Creates a 'RangeFloatArray' without any elements.
        /// </summary>
        public RangeFloatArray(float min, float max) 
        {
            Min = min;
            Max = max;
        }
        /// <summary>
        /// Constructor for the 'RangeFloatArray'.
        /// <br>Removes duplicate values from <paramref name="collection"/> and sets min/max depending on the collection's size.</br>
        /// </summary>
        /// <param name="collection"></param>
        public RangeFloatArray(IEnumerable<float> collection) : this(0f, 0f, collection) { }
        /// <summary>
        /// Constructor for the 'RangeFloatArray'.
        /// <br>Removes duplicate values from <paramref name="collection"/>.</br>
        /// </summary>
        public RangeFloatArray(float min, float max, IEnumerable<float> collection)
        {
            // If the array is an all-set-to-same-value one, set values with constant interval => range.max - range.min / array.size.
            // If there's more than 1 value at the same value, just move those forward by => (smallest value larger than same value) / (same value count)
            m_values = new List<float>(collection.Distinct());
            // Get array min/max
            float minValue = m_values.MinOrDefault(0f), maxValue = m_values.MaxOrDefault(1f);
            if (Mathf.Approximately(minValue, maxValue))
            {
                // Seperate values as defaults
                min = 0f;
                max = 1f;
            }

            Min = min;
            Max = max;
        }

        // -- Equality comparers
        public bool Equals(RangeFloatArray other)
        {
            return m_values.SequenceEqual(other.m_values) && m_Min == other.m_Min && m_Max == other.m_Max;
        }
        public bool Equals(IEnumerable<float> other)
        {
            return m_values.SequenceEqual(other);
        }

        #region IList (with clamps)
        /// <summary>
        /// Sets the index to a value, then calls <see cref="List{T}.Sort"/> for <see cref="m_values"/>.
        /// </summary>
        public float this[int index]
        {
            get { return m_values[index]; }
            set 
            {
                m_values[index] = value;
                m_values.Sort();
            }
        }

        public int Count => m_values.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Adds to the list as clamped <paramref name="item"/>.
        /// </summary>
        public void Add(float item)
        {
            float value = item;
            // Insert value to make the array sorted
            int i;
            for (i = 0; i < Count; i++)
            {
                if (value > m_values[i])
                {
                    continue;
                }

                break;
            }

            m_values.Insert(i, value);
        }

        public void Clear()
        {
            m_values.Clear();
        }

        public bool Contains(float item)
        {
            return m_values.Contains(item);
        }

        public void CopyTo(float[] array, int arrayIndex)
        {
            array.CopyTo(array, arrayIndex);
        }

        public IEnumerator<float> GetEnumerator()
        {
            return m_values.GetEnumerator();
        }

        public int IndexOf(float item)
        {
            return m_values.IndexOf(item);
        }

        /// <summary>
        /// Inserts at index, but may cause the array to be unsorted!
        /// </summary>
        public void Insert(int index, float item)
        {
            m_values.Insert(index, item);
            m_values.Sort();
        }

        public bool Remove(float item)
        {
            return m_values.Remove(item);
        }

        public void RemoveAt(int index)
        {
            m_values.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_values.GetEnumerator();
        }
        #endregion
    }
}
