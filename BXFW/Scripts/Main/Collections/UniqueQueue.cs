﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace BXFW.Collections
{
    /// <summary>
    /// A <see cref="Queue{T}"/> that only holds unique items.
    /// <br>Memory usage is higher because it uses a <see cref="HashSet{T}"/> to check whether the Enqueue'd element is unique or not.</br>
    /// </summary>
    public class UniqueQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
    {
        /// <summary>
        /// Internal queue used.
        /// </summary>
        private readonly Queue<T> m_queue;
        /// <summary>
        /// Internal hashset used for uniqueness comparison.
        /// </summary>
        private readonly HashSet<T> m_hashSet;

        /// <summary>
        /// Size of the Queue.
        /// </summary>
        public int Count => m_queue.Count;

        /// <summary>
        /// In the default impl of <see cref="Queue{T}"/>, this returns false.
        /// </summary>
        bool ICollection.IsSynchronized => false;
        /// <summary>
        /// In the default impl of <see cref="Queue{T}"/>, this returns the instance itself.
        /// </summary>
        object ICollection.SyncRoot => this;

        /// <summary>
        /// Creates an empty queue.
        /// </summary>
        public UniqueQueue()
        {
            m_hashSet = new HashSet<T>();
            m_queue = new Queue<T>();
        }
        /// <summary>
        /// <inheritdoc cref="UniqueQueue{T}.UniqueQueue(int, IEqualityComparer{T})"/>
        /// </summary>
        /// <param name="capacity">Capacity of the queue to create.</param>
        public UniqueQueue(int capacity)
        {
            m_hashSet = new HashSet<T>(capacity);
            m_queue = new Queue<T>(capacity);
        }
        /// <summary>
        /// <inheritdoc cref="UniqueQueue{T}.UniqueQueue(IEnumerable{T}, IEqualityComparer{T})"/>
        /// </summary>
        /// <param name="collection">List of the values to add into the queue. Non-unique values are trimmed.</param>
        public UniqueQueue(IEnumerable<T> collection)
        {
            m_hashSet = new HashSet<T>(collection);
            m_queue = new Queue<T>(m_hashSet);
        }
        /// <summary>
        /// Creates an empty queue with capacity.
        /// </summary>
        /// <param name="capacity">Capacity of the queue to create.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> for uniqueness check.</param>
        public UniqueQueue(int capacity, IEqualityComparer<T> comparer)
        {
            m_hashSet = new HashSet<T>(capacity, comparer);
            m_queue = new Queue<T>(capacity);
        }
        /// <summary>
        /// Creates a queue with only unique values from <paramref name="collection"/> in it.
        /// </summary>
        /// <param name="collection">List of the values to add into the queue. Non-unique values are trimmed.</param>
        /// <param name="comparer">The equality comparer to use.</param>
        public UniqueQueue(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            m_hashSet = new HashSet<T>(collection, comparer); // Create the hashSet with unique values first
            m_queue = new Queue<T>(m_hashSet);                // Use the hashSet to create the queue.
        }

        /// <summary>
        /// Dequeues the first element as an out parameter named <paramref name="result"/>.
        /// </summary>
        /// <returns><see langword="true"/> if a value to dequeue exists and dequeueing was successful.</returns>
        public bool TryDequeue(out T result)
        {
            bool dqResult = m_queue.TryDequeue(out result);
            if (dqResult)
            {
                m_hashSet.Remove(result);
            }
            return dqResult;
        }
        /// <summary>
        /// Dequeues the first value in the queue and removes it.
        /// </summary>
        public T Dequeue()
        {
            T value = m_queue.Dequeue();
            m_hashSet.Remove(value);
            return value;
        }
        /// <summary>
        /// Enqueues a value. If it's a duplicate or a failed enqueueing it returns <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if enqueueing was successful.</returns>
        public bool TryEnqueue(T item)
        {
            bool result = m_hashSet.Add(item);
            if (!result)
            {
                return false;
            }

            m_queue.Enqueue(item);
            return true;
        }
        /// <summary>
        /// Enqueues an value. If it's a duplicate value it will throw <see cref="ArgumentException"/>.
        /// </summary>
        public void Enqueue(T item)
        {
            if (!TryEnqueue(item))
            {
                throw new ArgumentException(string.Format("[UniqueQueue::Enqueue] Item '{0}' already exists in the queue.", item));
            }
        }
        /// <summary>
        /// Returns the first element without removing it.
        /// </summary>
        public bool TryPeek(out T result)
        {
            return m_queue.TryPeek(out result);
        }
        /// <summary>
        /// Returns the first element in the queue without removing it.
        /// </summary>
        public T Peek()
        {
            return m_queue.Peek();
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            m_queue.Clear();
            m_hashSet.Clear();
        }
        /// <summary>
        /// Returns whether if the queue contains the element.
        /// </summary>
        public bool Contains(T item)
        {
            return m_hashSet.Contains(item);
        }
        /// <summary>
        /// Converts the <see cref="UniqueQueue{T}"/> into a typed array.
        /// </summary>
        public T[] ToArray()
        {
            return m_queue.ToArray();
        }
        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="Queue{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            m_queue.TrimExcess();
        }
        /// <summary>
        /// Copies the queue into an array.
        /// </summary>
        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }
        /// <summary>
        /// Copies the queue into an array, but it's a typed array now. Yay.
        /// </summary>
        public void CopyTo(T[] array, int index)
        {
            m_queue.CopyTo(array, index);
        }
        /// <summary>
        /// Returns the <see langword="foreach"/> iteration provider, to get elements sequentially.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return m_queue.GetEnumerator();
        }
        /// <summary>
        /// Returns the same enumerator.
        /// <br>What was i thinking while writing this one's summary?</br>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_queue.GetEnumerator();
        }
    }
}
