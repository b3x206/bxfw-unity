using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using BXFW;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Implements the basic interface for <see cref="ChanceValue{T}"/>.
    /// <br>Contains the non-generic values.</br>
    /// </summary>
    public interface IChanceValue : IEquatable<IChanceValue>
    {
        /// <summary>
        /// Selectability chance of the data.
        /// </summary>
        public float Chance { get; set; }
    }
    /// <summary>
    /// Contains the base values and is used for match with <c>CustomPropertyDrawerAttribute</c>.
    /// </summary>
    public abstract class ChanceValuesListBase
    {
        /// <summary>
        /// The upper limit of all chances to use.
        /// </summary>
        public const float ChanceUpperLimit = 1f;

        /// <summary>
        /// List of the all of the chance values.
        /// <br>Instead of changing the chance directly from this value, use the <see cref="SetChance(int, float)"/> method instead.</br>
        /// </summary>
        public abstract IReadOnlyList<IChanceValue> ChanceDatas { get; }

        /// <summary>
        /// Fills the missing sum of chances if a data was removed.
        /// <br>Can be used as a sanity check.</br>
        /// </summary>
        public abstract void FillMissingChanceSum();
        /// <summary>
        /// Used to set the chance of an value.
        /// </summary>
        public abstract void SetChance(int index, float chance);
        /// <summary>
        /// Used to add an element to the array.
        /// <br>If the chance value actually has value other than 0, a space will be reserved for <paramref name="element"/>.</br>
        /// </summary>
        public abstract void Add(IChanceValue element);
        /// <summary>
        /// Used to remove an element from the array.
        /// </summary>
        /// <returns>Whether if anything was removed.</returns>
        public abstract bool Remove(IChanceValue element);
        /// <summary>
        /// Used to remove an element at <paramref name="index"/> from the array.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        public abstract void RemoveAt(int index);
    }

    /// <summary>
    /// Data block for the chance.
    /// </summary>
    [Serializable]
    public sealed class ChanceValue<T> : IChanceValue, IEquatable<ChanceValue<T>>
    {
        // -- Interface
        [SerializeField, Range(0f, ChanceValuesListBase.ChanceUpperLimit)]
        private float m_Chance;
        public float Chance
        {
            get { return m_Chance; }
            set
            {
                if (float.IsNaN(value))
                {
                    Debug.LogWarning($"[ChanceData<{typeof(T)}>] Tried to set value to NaN.");
                    return;
                }

                m_Chance = Mathf.Clamp(value, 0f, 100f);
            }
        }

        // -- Generic
        /// <summary>
        /// Value of this chanced item.
        /// </summary>
        public T Value;

        public override bool Equals(object obj)
        {
            if (!(obj is ChanceValue<T> other))
                return false;

            return Equals(other);
        }
        public bool Equals(ChanceValue<T> other)
        {
            return !(other is null) &&
                   Chance == other.Chance &&
                   EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
        public bool Equals(IChanceValue data)
        {
            return (data is ChanceValue<T> cData) && Equals(cData);
        }

        public static bool operator ==(ChanceValue<T> left, ChanceValue<T> right)
        {
            return EqualityComparer<ChanceValue<T>>.Default.Equals(left, right);
        }
        public static bool operator !=(ChanceValue<T> left, ChanceValue<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked // no int checks
            {
                int hashCode = 1403209422;
                hashCode = hashCode * -1521134295 + Chance.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
                return hashCode;
            }
        }
        public override string ToString()
        {
            return $"[ChanceData] Chance:{Chance} | Data:{Value}";
        }
    }

    /// <summary>
    /// A list of values that can be randomly selected, with the chances distributed
    /// by all of the other element's chances for making the chance selection accurate.
    /// </summary>
    [Serializable]
    public class ChanceValuesList<T> : ChanceValuesListBase, ICollection<ChanceValue<T>>, IEnumerable<ChanceValue<T>>
    {
        /// <summary>
        /// Get a list of the chance data.
        /// <br>While the chances of the elements in the list could be changed, this most likely shouldn't change the chances inside other datas.</br>
        /// </summary>
        public override IReadOnlyList<IChanceValue> ChanceDatas
        {
            get
            {
                return m_list;
            }
        }
        [SerializeField]
        private List<ChanceValue<T>> m_list = new List<ChanceValue<T>>();
        /// <summary>
        /// The equality comparer used with this chance data.
        /// </summary>
        public IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        /// <summary>
        /// Returns a randomly selected random value, using the data.
        /// </summary>
        public T Get()
        {
            // could do 'this' but this probs creates an List copy with ref values?
            return GetRandomInternal(m_list);
        }
        /// <summary>
        /// A tempoary list for doing no allocations at <see cref="Get(Predicate{T})"/> with predicate.
        /// </summary>
        private readonly List<ChanceValue<T>> filterList = new List<ChanceValue<T>>();
        /// <summary>
        /// Returns a random element from arrays.
        /// <br>NOTE : This mutates the chances, but equally distributes them.</br>
        /// <br>It completely ignores the filtered out chances.</br>
        /// </summary>
        /// <param name="filter">Return <see langword="true"/> to add to the 'GetRand' list.</param>
        public T Get(Predicate<T> filter)
        {
            filterList.Clear();
            filterList.Capacity = m_list.Count;
            filterList.AddRange(m_list.Where(cd => filter(cd.Value)));

            if (filterList.Count <= 0)
                throw new InvalidOperationException("[ChanceGroupList::GetData] No data in filtered list. Please make sure the filter predicate is valid.");

            // don't adjust the chances of the 'ChanceData's (because the classes are refs),
            // instead get a random value between zero and the existing sum of this array.
            float randUpperLimit = filterList.Sum((ChanceValue<T> c) => c.Chance);

            // Do the same thing as 'GetRand'
            return GetRandomInternal(filterList, randUpperLimit);
        }

        /// <summary>
        /// <c>[Internal method]</c> Get random value from a chance list of given <paramref name="list"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no array elements.</exception>
        protected static T GetRandomInternal(in List<ChanceValue<T>> list, float randUpperLimit = ChanceUpperLimit)
        {
            if (list == null)
                throw new NullReferenceException("[ChanceDataList::GetRand] Array passed is null.");

            if (list.Count < 1)
            {
                if (list.Count <= 0)
                    throw new InvalidOperationException("[ChanceDataList::GetRand] Array has no elements!");

                return list[0].Value;  // Always return the first object
            }

            // The given random value
            float randValue = UnityEngine.Random.Range(0f, randUpperLimit);
            // Current range offset
            float currentValueOffset = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                ChanceValue<T> chData = list[i];
                // check if the (randomValue - currentOffset) is in the chance data range.
                // With this each chance data gets uniform randomness distribution
                // (or not depending on what type of float rng unity is using)
                bool dataInRange = (randValue - currentValueOffset) < chData.Chance;
                // Move offset by this data's chance (as all data's chance sum is assumed to be 'randUpperLimit')
                currentValueOffset += chData.Chance;

                if (dataInRange)
                {
                    return chData.Value;
                }
            }

            // Throw an exception here?
#if UNITY_EDITOR || DEBUG
            Debug.LogError($"[ChanceDataList::GetRand(DEBUG)] Should not reach here. Check if the sum of chances are larger than or equal to {ChanceUpperLimit}. Returning a fallback data.");
#endif
            return list[0].Value;
        }

        /// <summary>
        /// Creates an empty list of chance list.
        /// </summary>
        public ChanceValuesList() { }
        /// <summary>
        /// Creates an empty list with a comparer attached.
        /// </summary>
        public ChanceValuesList(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer), "[ChanceValuesList::ctor] Given argument was null.");

            this.comparer = comparer;
        }
        /// <summary>
        /// Creates a chance list equally distributed.
        /// </summary>
        public ChanceValuesList(IEnumerable<T> collection)
        {
            int collectionSize = collection.Count();
            float chancePerElement = ChanceUpperLimit / collectionSize;
            filterList.Capacity = collectionSize;
            foreach (T elem in collection)
            {
                filterList.Add(new ChanceValue<T> { Chance = chancePerElement, Value = elem });
            }
        }
        /// <summary>
        /// Creates a chance list with a custom comparer.
        /// </summary>
        public ChanceValuesList(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(collection)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer), "[ChanceValuesList::ctor] Given argument was null.");

            this.comparer = comparer;
        }

        /// <summary>
        /// Returns the chance as the <see cref="KeyValuePair{TKey, TValue}.Key"/> 
        /// and the data as <see cref="KeyValuePair{TKey, TValue}.Value"/>.
        /// <br>This is a read-only reprensation.</br>
        /// </summary>
        public KeyValuePair<float, T> this[int index]
        {
            get
            {
                return new KeyValuePair<float, T>(m_list[index].Chance, m_list[index].Value);
            }
        }
        /// <summary>
        /// Reserves a chance amount for the given <paramref name="item"/>.
        /// <br>This should be called with the item to be added for the element.</br>
        /// </summary>
        protected void ReserveChanceForItem(ChanceValue<T> item)
        {
            // If the item has chance, lower all others
            for (int i = 0; i < m_list.Count; i++)
            {
                ChanceValue<T> data = m_list[i];
                data.Chance -= item.Chance / m_list.Count;
            }
        }
        public override void FillMissingChanceSum()
        {
            float sum = m_list.Sum((data) => data.Chance);

            // Sum is already at the upper limit
            if (sum + float.Epsilon >= ChanceUpperLimit)
                return;

            for (int i = 0; i < m_list.Count; i++)
            {
                m_list[i].Chance += sum / m_list.Count;
            }
        }
        /// <summary>
        /// Sets a chance of element in given <paramref name="index"/>.
        /// </summary>
        public override void SetChance(int index, float chance)
        {
            // This should throw 'IndexOutOfRangeException' anyway
            m_list[index].Chance = chance;

            float sum = m_list.Sum((data) => data.Chance);
            float chanceDelta = ChanceUpperLimit - sum;

            for (int i = 0; i < m_list.Count; i++)
            {
                // Only don't set the already set chance.
                if (i == index)
                    continue;

                // Set all to evenly distribute the sum
                m_list[i].Chance += chanceDelta / (m_list.Count - 1);
            }
        }
        /// <inheritdoc cref="SetChance(int, float)"/>
        public void SetChance(T value, float chance)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "[ChanceDataList::SetChance] Given argument was null.");

            int indexOfData = m_list.IndexOf((chanceData) => comparer.Equals(chanceData.Value, value));

            if (indexOfData == -1)
                return;

            SetChance(indexOfData, chance);
        }

        #region ICollection
        public int Count => m_list.Count;
        public bool IsReadOnly => false;

        public void Add(ChanceValue<T> item)
        {
            // Assert item to be not null
            if (item == null)
                throw new ArgumentNullException(nameof(item), "[ChanceDataList::Add] Given argument was null.");

            ReserveChanceForItem(item);
            m_list.Add(item);
        }
        public void Clear()
        {
            m_list.Clear();
        }
        public bool Contains(ChanceValue<T> item)
        {
            return m_list.Contains(item);
        }
        public void CopyTo(ChanceValue<T>[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }
        public IEnumerator<ChanceValue<T>> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        public int IndexOf(ChanceValue<T> item)
        {
            return m_list.IndexOf(item);
        }
        public void Insert(int index, ChanceValue<T> item)
        {
            ReserveChanceForItem(item);
            m_list.Insert(index, item);
        }
        public bool Remove(ChanceValue<T> item)
        {
            bool result = m_list.Remove(item);

            if (result)
            {
                FillMissingChanceSum();
            }

            return result;
        }
        public override void Add(IChanceValue element)
        {
            Add(element as ChanceValue<T>);
        }
        public override bool Remove(IChanceValue element)
        {
            return Remove(element as ChanceValue<T>);
        }
        public override void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
            FillMissingChanceSum();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        #endregion
    }
}
