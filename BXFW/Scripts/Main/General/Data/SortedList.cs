using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Class used to specify the base for the '<see cref="SortedList{T}"/>'.
    /// <br>Does nothing special other than providing a class match for <c>SortedListFieldEditor</c> and some sanity check requirements.</br>
    /// </summary>
    public abstract class SortedListBase
    {
        /// <summary>
        /// Returns the count of the array.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Type of an element in the list.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Checks if the array is sorted.
        /// <br>Under normal conditions, this doesn't need to be called.</br>
        /// <br>
        /// This method is for unity editor / reflection or indirectly changing IComparable order based stuff,
        /// where the values of the array can be changed without awareness to the sorted-ness.
        /// </br>
        /// </summary>
        public abstract bool IsSorted();

        /// <summary>
        /// Forces the array to be sort.
        /// <br>Under normal conditions, this doesn't need to be called.</br>
        /// <br>
        /// This method is for unity editor / reflection or indirectly changing IComparable order based stuff, 
        /// where the values of the array can be changed without awareness to the sorted-ness.
        /// </br>
        /// </summary>
        public abstract void Sort();
    }

    /// <summary>
    /// A serializable sorted list that is sorted by the value's comparability to itself.
    /// <br>(<see cref="IComparable{T}"/> types recommended)</br>
    /// </summary>
    [Serializable]
    public class SortedList<T> : SortedListBase, ICollection<T>, IEquatable<IEnumerable<T>>
    {
        /// <summary>
        /// Internal list used. (<c>Serialized</c>)
        /// </summary>
        [SerializeField]
        private List<T> m_list;
        /// <summary>
        /// The comparer used for comparing the <typeparamref name="T"/> type objects.
        /// </summary>
        private IComparer<T> m_comparer = Comparer<T>.Default;
        /// <summary>
        /// The current comparer for this sorted list.
        /// </summary>
        public IComparer<T> Comparer
        {
            get => m_comparer;
            set => m_comparer = value;
        }

        /// <summary>
        /// Finds the index of value mostly lower but the closest to <paramref name="value"/>.
        /// <br>If it's an exact match, the index will be the same.</br>
        /// </summary>
        private int FindClosestIndexBinarySearch(T value, int lower = -1, int upper = -1)
        {
            // Check list (dumb oversight, the Add function throws if no elements lol)
            if (m_list.Count <= 0)
            {
                return 0;
            }

            // Get optional parameters
            if (lower <= -1)
                lower = 0;
            if (upper <= -1)
                upper = m_list.Count;

            int center = (upper + lower) / 2;                     // Center of array values
            T elem = m_list[center];                              // Element
            int comparisonDiff = m_comparer.Compare(value, elem); // Comparison sign

            // Value was either found or no value (determine the closest with another comparison)
            if (lower == center || Mathf.Abs(lower - upper) <= 1)
            {
                // lower == center, can return 'lower' if the values are equal
                return comparisonDiff <= 0 ? lower : upper;
            }

            // Value is on lower
            if (comparisonDiff < 0)
            {
                return FindClosestIndexBinarySearch(value, lower, center);
            }
            // Value is in the upper
            else if (comparisonDiff > 0)
            {
                return FindClosestIndexBinarySearch(value, center, upper);
            }
            // Value equal
            else
            {
                return center;
            }
        }

        #region Ctor + Interface
        public SortedList()
        {
            m_list = new List<T>();
        }
        public SortedList(int capacity)
        {
            m_list = new List<T>(capacity);
        }
        public SortedList(IEnumerable<T> collection)
        {
            m_list = new List<T>(collection);
            m_list.Sort(m_comparer);
        }
        public SortedList(IEnumerable<T> collection, IComparer<T> comparer)
        {
            m_list = new List<T>(collection);
            m_comparer = comparer;
            m_list.Sort(m_comparer);
        }
        public SortedList(IComparer<T> comparer)
        {
            m_comparer = comparer;
        }

        /// <summary>
        /// Pretend that this list is a list.
        /// </summary>
        public static explicit operator List<T>(SortedList<T> list)
        {
            return list.m_list;
        }

        public override bool IsSorted()
        {
            for (int i = 0; i < m_list.Count - 1; i++)
            {
                T current = m_list[i], next = m_list[i + 1];

                // Current is larger than the next value, which is incorrect.
                if (m_comparer.Compare(current, next) > 0)
                    return false;
            }

            return true;
        }
        public override void Sort()
        {
            m_list.Sort();
        }

        public bool Equals(IEnumerable<T> other)
        {
            return Enumerable.SequenceEqual(m_list, other);
        }
        #endregion

        #region ICollection
        /// <summary>
        /// <b>get : </b>
        /// Returns a value in the array.
        /// <br>If the <see cref="IComparable{T}"/> comparability value is changed, the element may not be in the same index.</br>
        /// <br>When the comparability is changed, please either call <see cref="Sort"/> or don't change <see cref="IComparable{T}"/> values like that.</br>
        /// <br/>
        /// <b>set : </b>
        /// Changes a value in an index.
        /// <br>This also switches the values accordingly if something was switched.</br>
        /// </summary>
        public T this[int index]
        {
            get 
            {
                if (!IsSorted())
                    Sort();

                return m_list[index];
            }
            set
            {
                // FIXME : Maybe remove this because it's a bad idea?
                // -------
                // Previous index value for the value that is being set.
                int prevIndex = index;

                // Find it linearly as binary search only works correctly with Insert applications
                // And the moving towards to target from index usually crashes the application
                for (int i = 0; i < Count; i++)
                {
                    int compareToPrevValue = i > 0 ? m_comparer.Compare(value, m_list[i - 1]) : 1;
                    int compareToNextValue = i < Count - 1 ? m_comparer.Compare(value, m_list[i + 1]) : -1;

                    if (i == Count - 1 || (compareToPrevValue >= 0 && compareToNextValue <= 0))
                    {
                        index = i;
                        break;
                    }
                }

                // Set values into appopriate indices
                if (prevIndex == index)
                {
                    m_list[index] = value;
                    return;
                }

                T valueOnPrevIndex = m_list[prevIndex];
                m_list[index] = value;
                m_list[prevIndex] = valueOnPrevIndex;
                // Can alternatively also be this :
                // (m_list[index], m_list[prevIndex]) = (value, m_list[index]);
            }
        }

        public override int Count => m_list.Count;
        public override Type ElementType => typeof(T);
        public bool IsReadOnly => false;
        public void Add(T item)
        {
            // Insert to array
            int closestIndex = FindClosestIndexBinarySearch(item);
            m_list.Insert(closestIndex, item);
        }
        public void AddRange(IEnumerable<T> elements)
        {
            // Allocate
            if (elements is ICollection collection1)
            {
                m_list.Capacity += collection1.Count;
            }
            if (elements is ICollection<T> collection2)
            {
                m_list.Capacity += collection2.Count;
            }

            // Add
            foreach (T elem in elements)
            {
                Add(elem);
            }
        }
        public void Clear()
        {
            m_list.Clear();
        }
        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }
        public int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }
        public int LastIndexOf(T item)
        {
            return m_list.LastIndexOf(item);
        }
        public bool Remove(T item)
        {
            return m_list.Remove(item);
        }
        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        #endregion

        #region List Extensions
        public void RemoveRange(int startIndex, int count)
        {
            m_list.RemoveRange(startIndex, count);
        }
        public int RemoveAll(Predicate<T> match)
        {
            return m_list.RemoveAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return m_list.FindIndex(match);
        }
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return m_list.FindIndex(startIndex, match);
        }
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return m_list.FindIndex(startIndex, count, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return m_list.FindLastIndex(match);
        }
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return m_list.FindLastIndex(startIndex, match);
        }
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return m_list.FindLastIndex(startIndex, count, match);
        }
        #endregion
    }
}
