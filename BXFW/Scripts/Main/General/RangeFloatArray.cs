using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Acts like an regular float array, but the values can't be equal and they are clamped between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [Serializable]
    public class RangeFloatArray : IList<float>, IEquatable<RangeFloatArray>, IEquatable<IEnumerable<float>>
    {
        [SerializeField] private List<float> array = new List<float>();
        [SerializeField] private float min = 0f;
        [SerializeField] private float max = 1f;
        /// <summary>
        /// The offset applied if the element already exists.
        /// </summary>
        private const float EXISTING_OFFSET = .033f;

        /// <summary>
        /// Minimum value of the elements in the array defined.
        /// </summary>
        public float Min
        {
            get { return min; }
            // Clamp between 'float.Min <> Mathf.Min(this.Max - offset, smallest value in array)';
            set { min = Mathf.Clamp(value, float.MinValue, Mathf.Min(Max - (EXISTING_OFFSET + Mathf.Epsilon), array.MinOrDefault(float.MaxValue))); }
        }
        /// <summary>
        /// Maximum value of the elements in the array that is defined.
        /// </summary>
        public float Max
        {
            get { return max; }
            // Clamp between 'Mathf.Max(this.Min + offset, largest value in array) <> float.Max';
            set { max = Mathf.Clamp(value, Mathf.Max(Min + (EXISTING_OFFSET + Mathf.Epsilon), array.MaxOrDefault(float.MinValue)), float.MaxValue); }
        }

        public RangeFloatArray() { }
        public RangeFloatArray(IEnumerable<float> collection) : this(0f, 0f, collection) { }
        /// <summary>
        /// Constructor for the 'RangeFloatArray'.
        /// <br>Removes duplicate values from <paramref name="collection"/>.</br>
        /// </summary>
        public RangeFloatArray(float min, float max, IEnumerable<float> collection)
        {
            // If the array is an all-set-to-same-value one, set values with constant interval => range.max - range.min / array.size.
            // If there's more than 1 value at the same value, just move those forward by => (smallest value larger than same value) / (same value count)
            array = new List<float>(collection);
            float minValue = array.MinOrDefault(0f), maxValue = array.MaxOrDefault(1f);
            if (Mathf.Approximately(minValue, maxValue))
            {
                // Seperate values as defaults
                min = 0f;
                max = 1f;
            }
            // Check existing + duplicates
            List<float> existingValues = new List<float>(array.Count);
            for (int i = 0; i < array.Count; i++)
            {
                float value = array[i];

                if (existingValues.IndexOf(value) != -1)
                {
                    // Remove existing duplicate?
                    // Or move it?
                    // For now just remove it
                    array.RemoveAt(i);
                    i--;
                    continue;
                }

                existingValues.Add(value);
            }

            // These calculate the values by the array anyways, so we can pass 0 0 or something else.
            Min = min;
            Max = max;
        }
        
        /// <summary>
        /// Clamps the value, if it already exists on <see cref="array"/>, it keeps offsetting by value.
        /// </summary>
        private float ClampOffsettable(float value)
        {
            return ClampOffsettableIgnoredIndex(value, -1);
        }
        /// <summary>
        /// Clamps the value, but can ignore an index of the array.
        /// </summary>
        private float ClampOffsettableIgnoredIndex(float value, int index)
        {
            // If the clamped value already exists, remove element?
            // Or just offset it with a changing sign value lol
            float clamped = Mathf.Clamp(value, Min, Max);
            float sign = 1f;
            while (array.IndexOf(clamped) != -1 && array.IndexOf(clamped) != index)
            {
                clamped = Mathf.Clamp(value + (sign * EXISTING_OFFSET), Min, Max);
                sign = -sign;

                // Make offset larger every 2 loops
                if (sign > 0f)
                {
                    sign++;
                }

                // If the offset is larger than the Max value, just give up
                // because that is a weird array.
                if (sign * EXISTING_OFFSET > Max)
                {
                    throw new InvalidOperationException(string.Format("[RangeFloatArray::ClampOffsettable] There is no existing value to offset into. Passed value is '{0}', array length is '{1}' (wtf)", value, array.Count));
                }
            }
            return clamped;
        }

        // -- Equality comparers
        public bool Equals(RangeFloatArray other)
        {
            return array == other.array && min == other.min && max == other.max;
        }

        public bool Equals(IEnumerable<float> other)
        {
            return array.SequenceEqual(other);
        }

        #region IList (with clamps)
        public float this[int index]
        {
            get { return array[index]; }
            set { array[index] = ClampOffsettableIgnoredIndex(value, index); }
        }

        public int Count => array.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Adds to the list as clamped <paramref name="item"/>.
        /// </summary>
        public void Add(float item)
        {
            array.Add(ClampOffsettable(item));
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(float item)
        {
            return array.Contains(item);
        }

        public void CopyTo(float[] array, int arrayIndex)
        {
            array.CopyTo(array, arrayIndex);
        }

        public IEnumerator<float> GetEnumerator()
        {
            return array.GetEnumerator();
        }

        public int IndexOf(float item)
        {
            return array.IndexOf(item);
        }

        public void Insert(int index, float item)
        {
            array.Insert(index, ClampOffsettable(item));
        }

        public bool Remove(float item)
        {
            return array.Remove(item);
        }

        public void RemoveAt(int index)
        {
            array.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
        #endregion
    }
}
