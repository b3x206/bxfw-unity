using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BXFW.Collections
{
    /// <summary>
    /// A implementation of a double-ended queue.
    /// <br>The 'First' prefixed methods will act as the saying, 'FIFO', which means that it will be the first element to be dequeue'd or be inserted.</br>
    /// <br><b>Warning : </b> This class is currently untested, please use with caution.</br>
    /// </summary>
    /// <typeparam name="T">Type of the children contained.</typeparam>
    public class Deque<T> : ICollection<T>
    {
        /// <summary>
        /// The current list collection of the deque.
        /// </summary>
        private readonly List<T> m_collection = new List<T>();

        /// <summary>
        /// Capacity of this deque.
        /// <br> <see langword="get"/> : </br>
        /// <br> Gets the capacity (noexcept)</br>
        /// <br> <see langword="set"/> : </br>
        /// <br> Sets the given capacity. Capacity cannot go lower than the <see cref="Count"/>.</br>
        /// <br> <see cref="ArgumentException"/> : Occurs when the assigned capacity is less than 0.</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public int Capacity
        {
            get => m_collection.Capacity;
            set => m_collection.Capacity = value;
        }

        /// <summary>
        /// Clamps with <paramref name="value"/> rollback between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        private static int WrapClamp(int value, int min, int max)
        {
            // People before math existed : 
            //if (value < min)
            //{
            //    int valueDelta = Math.Abs(min) - Math.Abs(value);
            //    return max - valueDelta;
            //}
            //if (value > max)
            //{
            //    int valueDelta = Math.Abs(max) - Math.Abs(value);
            //    return min + valueDelta;
            //}

            int maxMinDelta = max - min;
            value = maxMinDelta * (int)Math.Floor((double)(value / maxMinDelta));
            return value;
        }

        /// <summary>
        /// Index of the tail.
        /// <br>This value can be negative or positive. If this value is in the negative then the tail is inside <see cref="m_tailCollection"/>.</br>
        /// </summary>
        private int m_tailIndex = 0;
        /// <summary>
        /// Index of the head.
        /// </summary>
        private int m_headIndex = 0;

        /// <summary>
        /// Size of this queue.
        /// </summary>
        public int Count => m_headIndex - m_tailIndex;

        /// <summary>
        /// Always returns <see langword="false"/>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Enqueues an element to the Deque.
        /// <br>This is not meant to be used explicitly.</br>
        /// </summary>
        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }
        /// <summary>
        /// Just throws <see cref="NotImplementedException"/> with an error of "you should actually use the 'Pop' methods".
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException("[Deque::Remove] Use PopFirst or PopLast.");
        }

        /// <summary>
        /// Enqueues an element to the end (head), which in queueing setups this is inserting to the first-last element, making it the first element to be dequeued.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddFirst(T item)
        {
            if (m_tailIndex == m_headIndex)
            {
                m_collection.Insert(m_headIndex, item);
                m_tailIndex = WrapClamp(m_tailIndex + 1, 0, m_collection.Count - 1);
            }
            else
            {
                m_collection.Add(item);
            }

            m_headIndex = WrapClamp(m_headIndex + 1, 0, m_collection.Count - 1);
        }
        /// <summary>
        /// Enqueues an element to the start (tail), which in traditional queueing setups this is just <see cref="Queue{T}.Enqueue(T)"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddLast(T item)
        {
            if (m_tailIndex == m_headIndex)
            {
                // Reserve size for the queue?
                m_collection.Insert(m_tailIndex, item);
                m_headIndex = WrapClamp(m_headIndex - 1, 0, m_collection.Count - 1);
            }
            else
            {
                m_collection[m_tailIndex] = item;
            }

            m_tailIndex = WrapClamp(m_tailIndex - 1, 0, m_collection.Count - 1);
        }
        /// <summary>
        /// Peeks the first element to be <see cref="Queue{T}.Dequeue"/>'d.
        /// </summary>
        public T PeekFirst()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("[Deque::PeekFirst] PeekFirst called on Deque that does not contain any elements.");
            }

            return m_collection[m_headIndex];
        }
        /// <summary>
        /// Peeks the last element, that would have been traditionally added last on a normal <see cref="Queue{T}"/> setup.
        /// </summary>
        public T PeekLast()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("[Deque::PeekLast] PeekLast called on Deque that does not contain any elements.");
            }

            return m_collection[m_tailIndex];
        }
        /// <summary>
        /// Pops the first element to be <see cref="Queue{T}.Dequeue"/>'d.
        /// <br><see cref="InvalidOperationException"/> : Thrown when the queue has no elements.</br>
        /// </summary>
        /// <returns>The popped result.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T PopFirst()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("[Deque::PopFirst] PopFirst called on Deque that does not contain any elements.");
            }

            T element = m_collection[m_headIndex];
            m_headIndex = WrapClamp(m_headIndex - 1, 0, m_collection.Count - 1);
            return element;
        }
        /// <summary>
        /// Pops the last element enqueued.
        /// <br><see cref="InvalidOperationException"/> : Thrown when the queue has no elements.</br>
        /// </summary>
        /// <returns>The popped result.</returns>
        /// <exception cref="InvalidOperationException"/>
        public T PopLast()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("[Deque::PeekLast] PeekLast called on Deque that does not contain any elements.");
            }

            T element = m_collection[m_tailIndex];
            m_tailIndex = WrapClamp(m_tailIndex + 1, 0, m_collection.Count - 1);
            return element;
        }
        /// <summary>
        /// Reverses this double-ended queue.
        /// </summary>
        public void Reverse()
        {
            m_collection.Reverse();
            (m_tailIndex, m_headIndex) = (m_headIndex, m_tailIndex);
        }

        /// <summary>
        /// Tries peeking the first element to be <see cref="Queue{T}.Dequeue"/>'d.
        /// </summary>
        /// <param name="value">Value to peek.</param>
        /// <returns>Whether if peeking was successful and value is non-<see langword="null"/>.</returns>
        public bool TryPeekFirst(out T value)
        {
            value = default;

            if (Count <= 0)
            {
                return false;
            }

            // Now, this is bad coding B) 
            // Use a potentially exception throwing thing? YEES.
            value = PeekFirst();
            return true;
        }
        /// <summary>
        /// Tries peeking the last element, that would have been traditionally added last on a normal <see cref="Queue{T}"/> setup.
        /// </summary>
        /// <param name="value">Value to peek.</param>
        /// <returns>Whether if peeking was successful and value is non-<see langword="null"/>.</returns>
        public bool TryPeekLast(out T value)
        {
            value = default;

            if (Count <= 0)
            {
                return false;
            }

            value = PeekLast();
            return true;
        }

        /// <summary>
        /// Tries popping the first element (basically <see cref="Queue{T}.TryDequeue(out T)"/>)'d.
        /// </summary>
        /// <param name="value">Value to pop.</param>
        /// <returns>Whether if popping was successful and value is non-<see langword="null"/>.</returns>
        public bool TryPopFirst(out T value)
        {
            value = default;

            if (Count <= 0)
            {
                return false;
            }

            // Now, this is bad coding B) 
            // Use a potentially exception throwing thing? YEES.
            value = PopFirst();
            return true;
        }
        /// <summary>
        /// Tries popping the last element, that would have been traditionally added last on a normal <see cref="Queue{T}"/> setup.
        /// </summary>
        /// <param name="value">Value to pop.</param>
        /// <returns>Whether if popping was successful and value is non-<see langword="null"/>.</returns>
        public bool TryPopLast(out T value)
        {
            value = default;

            if (Count <= 0)
            {
                return false;
            }

            value = PopLast();
            return true;
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            m_tailIndex = 0;
            m_headIndex = 0;
            m_collection.Clear();
        }
        /// <summary>
        /// Returns whether if the deque contains <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Item to check whether if it's contained.</param>
        public bool Contains(T item)
        {
            return Contains(item, EqualityComparer<T>.Default);
        }
        /// <inheritdoc cref="Contains(T)"/>
        /// <exception cref="ArgumentNullException"/>
        public bool Contains(T item, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer), "[Deque::Contains] Given 'comparer' is null.");
            }

            for (int i = m_tailIndex; i <= m_headIndex; i++)
            {
                if (comparer.Equals(m_collection[i], item))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Copies collection to <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Array to copy into. This cannot be null.</param>
        /// <param name="arrayIndex">Index of array to start copying from.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "[Deque::CopyTo] Argument was null.");
            }

            if (array.Length < arrayIndex + Count)
            {
                throw new ArgumentException("[Deque::CopyTo] Failed to copy into given array. Array length is smaller than dictionary or index is out of bounds", nameof(array));
            }

            // do the copy slow way because proper iteration is not very fun.
            int i = arrayIndex;
            foreach (T element in this)
            {
                array[i] = element;
                i++;
            }
        }

        /// <summary>
        /// Trims the excess memory allocated.
        /// </summary>
        public void TrimExcess()
        {
            // TODO : A proper impl for this?
            // For now this just doesn't remove elements from the collections
            m_collection.TrimExcess();

            // --
            // Get values with actual size
            // T[] values = new T[Count];
            // Copy into values
            // Array.Copy(m_tailCollection, m_tailIndex, values, 0, values.Length);
            // Set (and let 'm_collection' be GC collected) m_collection
            // m_tailCollection = values;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = m_tailIndex; i < m_headIndex; i++)
            {
                yield return m_collection[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates an empty Deque.
        /// </summary>
        public Deque()
        { }
        /// <summary>
        /// Creates a Deque with capacity reserved.
        /// </summary>
        public Deque(int capacity)
        {
            Capacity = capacity;
        }
        /// <summary>
        /// Creates a Deque from a collection.
        /// </summary>
        public Deque(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "[Deque::ctor] Given argument was null.");
            }

            // Copy values to the pairs
            int collectionSize = collection.Count();
            Capacity = collectionSize;

            // Split the tail and values evenly.
            // TODO : Add range until index (IEnumerable iterate until index? (it's just called Take))
            foreach (T value in collection)
            {
                AddLast(value);
            }
        }
    }
}
