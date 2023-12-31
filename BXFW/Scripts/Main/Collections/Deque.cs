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
        // TODO : This is a mostly bad attempt on doing a Double ended queue
        // Maybe steal something MIT licensed from github? (with proper attributions)
        // TODO 2 : Test this crap
        // TODO 3 : Add the 'AddFirstRange' and 'AddLastRange' methods
        // --
        // Because i am very lazy, i just use 'List<T>'.
        // --
        // **** How Deque works? [some sort of explainer?] ****
        // Basically, imagine two arrays
        // One of these arrays are the 'm_tailCollection' which is the starts of the Queue
        // The other one is the 'm_headCollection' which is the ends of the Queue
        // -- [] => array def or index def | () => cell of <T> --
        //  m_tailCollection       -  m_headCollection
        // [ (1) (2) (3) (4) (5) ] - [ (1) (2) (3) (4) (5) ]
        //   [0] [1] [2] [3] [4]   -   [0] [1] [2] [3] [4]
        // --
        // The m_tailCollection is always assumed to be reverse iterated.
        // While the 'm_headCollection' is forward iterated and the values dequeued will be just set null.
        // --
        //  m_tailCollection (treated reverse) - m_headCollection
        // [ (5) (4) (3) (2) (1) ]             - [ (1) (2) (3) (4) (5) ]
        //   [4] [3] [2] [1] [0]               -   [0] [1] [2] [3] [4]
        // --
        // In this case, the 'm_tailIndex = 4' and the 'm_headIndex' is also 4.
        // So, how does 'running out of tail' or 'running out of head' behaves?
        // Well, if any of the indices are negative, we just add to the other array
        // (this means tail operation will add to head if the head has some free space.)
        // And i guess that's pretty much it.
        // --
        // Oh and also both the head and tail collection can be differently sized (m_tailIndex / m_headIndex). K thx bye.
        // -- 
        // But why 2 arrays?
        // This is because enqueueing without 2 arrays would have been o(N) (we would have to list.Insert(0, someItem))
        // I want both insertions to be o(1), but this maybe will create memory fragmentation which could be worse, idk.

        /// <summary>
        /// Collection of tail queue items to check for.
        /// <br>This array is in reverse, the last element is the first element on the tail.</br>
        /// </summary>
        private List<T> m_tailCollection;
        /// <summary>
        /// Collection of head queue items to check for.
        /// </summary>
        private List<T> m_headCollection;

        /// <summary>
        /// Ensures array capacities.
        /// </summary>
        private void EnsureArraysCapacity(int capacity)
        {
            // Capacity is evenly split
            m_tailCollection.Capacity = (capacity / 2) + 1;
            m_headCollection.Capacity = (capacity / 2) + 1;
        }
        /// <inheritdoc cref="Capacity"/>
        private int m_capacity = 0;
        /// <summary>
        /// Capacity of this deque.
        /// <br> <see langword="get"/> : </br>
        /// <br> Gets the capacity (noexcept)</br>
        /// <br> <see langword="set"/> : </br>
        /// <br> Sets the given capacity.</br>
        /// <br> <see cref="ArgumentException"/> : Occurs when the assigned capacity is less than 0.</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public int Capacity
        {
            get
            {
                return m_capacity;
            }
            set
            {
                if (m_capacity < 0)
                {
                    throw new ArgumentException("[Deque::(set)Capacity] Given capacity is negative and lower than zero.", nameof(value));
                }

                m_capacity = Math.Max(Count, value);
                EnsureArraysCapacity(m_capacity);
            }
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
        public int Count => m_headIndex + m_tailIndex;

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
            if (m_headIndex < 0)
            {
                // !! TODO : Ensure that the 'm_tailIndex' and 'm_headIndex' is correct.
                // always add 1 while getting the other index as 0 would be ignored if we directly negated the value
                // but the first -1 should specify 0 in this case to not skip 0.
                m_tailCollection[-(m_headIndex + 1)] = item;
            }
            else
            {
                m_headCollection.Add(item);
            }

            m_headIndex++;
        }
        /// <summary>
        /// Enqueues an element to the start (tail), which in traditional queueing setups this is just <see cref="Queue{T}.Enqueue(T)"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddLast(T item)
        {
            if (m_tailIndex < 0)
            {
                m_headCollection[-(m_tailIndex + 1)] = item;
            }
            else
            {
                m_tailCollection.Add(item);
            }

            m_tailIndex++;
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

            if (m_headIndex < 0)
            {
                return m_tailCollection[-(m_headIndex + 1)];
            }

            // Non-negative, can directly peek first
            return m_headCollection[m_headIndex];
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

            if (m_tailIndex < 0)
            {
                // Get the negated position
                return m_headCollection[-(m_tailIndex + 1)];
            }

            // Non-negative, can directly peek last
            return m_tailCollection[m_tailIndex];
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

            T element;
            int removalIndex = m_headIndex;
            // RemoveAt reshuffles the list so it's still O(N)
            // Yeah this is what happens when you no leetcode, DSA and just sit on your lazy bum
            // (note 2 : the only author of BXFW talking to himself)
            if (m_headIndex < 0)
            {
                removalIndex = -(m_headIndex + 1);
                element = m_tailCollection[removalIndex];
                m_tailCollection.RemoveAt(removalIndex);
                m_headIndex--;

                return element;
            }

            // Non-negative, can directly peek first
            // But for a case of an element existing in 'm_headCollection', 'm_headIndex' is usually the end
            // So it can be removed on O(1)
            element = m_headCollection[removalIndex];
            m_headCollection.RemoveAt(removalIndex);
            m_headIndex--;
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

            T element;
            int removalIndex = m_tailIndex;
            if (m_tailIndex < 0)
            {
                // Get the negated position
                element = m_headCollection[-(m_tailIndex + 1)];
                m_headCollection.RemoveAt(removalIndex);
                m_tailIndex--;
                return element;
            }

            // Non-negative, can directly peek last
            element = m_tailCollection[removalIndex];
            m_tailCollection.RemoveAt(removalIndex);
            m_tailIndex--;
            return element;
        }
        /// <summary>
        /// Reverses this double-ended queue.
        /// </summary>
        public void Reverse()
        {
            (m_tailIndex, m_headIndex) = (m_headIndex, m_tailIndex);

            m_tailCollection.Reverse();
            m_headCollection.Reverse();
            (m_tailCollection, m_headCollection) = (m_headCollection, m_tailCollection);
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
            m_tailCollection.Clear();
            m_headCollection.Clear();
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

            for (int i = m_tailIndex; i < m_headIndex; i++)
            {
                if (comparer.Equals(m_tailCollection[i], item))
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
            m_tailCollection.TrimExcess();
            m_headCollection.TrimExcess();

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
            for (int i = m_tailIndex; i >= 0; i--)
            {
                yield return m_tailCollection[i];
            }
            for (int i = 0; i < m_headIndex; i++)
            {
                yield return m_headCollection[i];
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
            foreach (KeyValuePair<int, T> indexedValue in collection.Indexed())
            {
                bool addIntoHead = indexedValue.Key > (collectionSize / 2);

                if (addIntoHead)
                {
                    AddFirst(indexedValue.Value);
                }
                else
                {
                    AddLast(indexedValue.Value);
                }
            }
        }
    }
}
