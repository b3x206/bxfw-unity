using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW.Collections
{
    /// <summary>
    /// Base used for the <see cref="SerializableDictionary{TKey, TValue}"/>.
    /// <br>Used to match the editor for the dictionary with a custom reorderable list.</br>
    /// </summary>
    [Serializable]
    public abstract class SerializableDictionaryBase
    {
        public abstract int Count { get; }

        /// <summary>
        /// A sanity check used to ensure that the keys are unique.
        /// </summary>
        public abstract bool KeysAreUnique();

        /// <summary>
        /// Returns whether if the dummy pair is valid.
        /// </summary>
        internal abstract bool DummyPairIsValid();

        /// <summary>
        /// Adds the dummy pair.
        /// </summary>
        internal abstract void AddDummyPair();
    }

    /// If 'SerializableDictionary{TKey, TValue}' was so good, why there isn't a 'SerializableDictionary2{TKey, TValue}'
    /// Well, you asked. And i delivered. The all new SerializableDictionary2!
    /// Features
    /// * Probably will break your previous serialization lol
    /// * Has a cool editor that isn't definitely broken because ReorderableList adding doesn't work unless you use dark magic.
    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> that can be serialized by unity.
    /// Uses (mostly) the same constraints as the <see cref="Dictionary{TKey, TValue}"/> on both editor and code.
    /// <br/>
    /// <br>NOTE : Array of array types such as <c><typeparamref name="TKey"/>[]</c> or <c><typeparamref name="TValue"/>[]</c> are NOT serializable
    /// in this dictionary (by unity). Wrap them with array container class because double lists don't get serialized unless you trick the serializer.</br>
    /// <br>NOTE 2 : If this dictionary's values were changed from the Debug inspector menu, this dictionary MAY contain duplicate keys.</br>
    /// </summary>
    /// <remarks>
    /// * Unlike the <see cref="Dictionary{TKey, TValue}"/>, this does not use hashsets.
    /// (because of this it is slower than an usual dictionary, you should only use this if serialization is required or 
    /// cast back to <see cref="Dictionary{TKey, TValue}"/> if performance and editor serializability is required)
    /// <br>* Ordering of the elements is ordered in the same order of elements that are added. It is not undefined behaviour.</br>
    /// </remarks>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase, IDictionary<TKey, TValue>
    {
        // Time to literally deserialize all localization keys (fml)
        // Pair time B) [this is because ReorderableList craps itself when you try to edit 2 SerializedProperties in a same one]
        // While i did manage to achieve such a feat, it usually throws 12831283 exceptions before working so yeah.

        // -- here's an idiot rambling : 
        // without DSA this is what i would have done as a dictionary (it work doe, adding complexity best case (N!^N!)!1!)
        // However, doing a 'HashSet' based key list will disallow the unity serialization so do it "normally".
        // unity can't serialize any data type more complex than an generic array :)
        // --
        // basically this dictionary is as o(n^2) as it gets
        // --

        /// <summary>
        /// A pair datatype, this is used to make the pairing of data more easier and also make the editor no longer cursed.
        /// </summary>
        [Serializable]
        public sealed class Pair : IEquatable<Pair>
        {
            /// <summary>
            /// Key of this pair.
            /// </summary>
            public TKey key;

            /// <summary>
            /// Value of this pair.
            /// </summary>
            public TValue value;

            /// <summary>
            /// Creates a pair with empty values.
            /// </summary>
            public Pair()
            { }

            /// <summary>
            /// Creates a pair with given values.
            /// </summary>
            public Pair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public static explicit operator KeyValuePair<TKey, TValue>(Pair pair)
            {
                return new KeyValuePair<TKey, TValue>(pair.key, pair.value);
            }

            public bool Equals(Pair other)
            {
                if (other is null)
                {
                    return false;
                }

                return EqualityComparer<TKey>.Default.Equals(key, other.key) && EqualityComparer<TValue>.Default.Equals(value, other.value);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(key, value);
            }
        }

        /// <summary>
        /// A lighter class to substitute for a key collection.
        /// </summary>
        public abstract class GenericCollection<TCollection> : ICollection<TCollection>, IEnumerable<TCollection>, IEnumerable, IReadOnlyCollection<TCollection>
        {
            protected SerializableDictionary<TKey, TValue> m_parentDictionary;

            public GenericCollection(SerializableDictionary<TKey, TValue> parentDictionary)
            {
                m_parentDictionary = parentDictionary;
            }

            public int Count => m_parentDictionary.m_Pairs.Count;
            public bool IsReadOnly => true;

            public abstract bool Contains(TCollection item);
            public abstract void CopyTo(TCollection[] array, int arrayIndex);
            public abstract IEnumerator<TCollection> GetEnumerator();

            void ICollection<TCollection>.Add(TCollection item)
            {
                throw new NotImplementedException("[SerializableDictionary::GenericCollection::Add] This method will always throw NotImplementedException.");
            }
            void ICollection<TCollection>.Clear()
            {
                throw new NotImplementedException("[SerializableDictionary::GenericCollection::Clear] This method will always throw NotImplementedException.");
            }
            bool ICollection<TCollection>.Remove(TCollection item)
            {
                throw new NotImplementedException("[SerializableDictionary::GenericCollection::Remove] This method will always throw NotImplementedException.");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// A collection of keys in this dictionary.
        /// </summary>
        public sealed class KeyCollection : GenericCollection<TKey>
        {
            public KeyCollection(SerializableDictionary<TKey, TValue> parentDictionary) : base(parentDictionary)
            { }

            public override bool Contains(TKey item)
            {
                for (int i = 0; i < m_parentDictionary.Count; i++)
                {
                    if (m_parentDictionary.m_Comparer.Equals(m_parentDictionary.m_Pairs[i].key, item))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override void CopyTo(TKey[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array), "[SerializableDictionary::KeyCollection::CopyTo] Given argument was null.");
                }
                if ((m_parentDictionary.Count + arrayIndex) > array.Length)
                {
                    throw new ArgumentException("[SerializableDictionary::KeyCollection::CopyTo] Given array size is less than the collection size + offset.", nameof(array));
                }

                for (int i = arrayIndex; i < m_parentDictionary.Count; i++)
                {
                    array[i] = m_parentDictionary.m_Pairs[i].key;
                }
            }

            public override IEnumerator<TKey> GetEnumerator()
            {
                for (int i = 0; i < m_parentDictionary.Count; i++)
                {
                    yield return m_parentDictionary.m_Pairs[i].key;
                }
            }
        }

        /// <summary>
        /// A collection of values in this dictionary.
        /// <br>The values are synchronized to the base <see cref="SerializableDictionary{TKey, TValue}"/>, there is no need to refresh reference.</br>
        /// </summary>
        public sealed class ValueCollection : GenericCollection<TValue>
        {
            public ValueCollection(SerializableDictionary<TKey, TValue> parentDictionary) : base(parentDictionary)
            { }

            public override bool Contains(TValue item)
            {
                return Contains(item, EqualityComparer<TValue>.Default);
            }

            public bool Contains(TValue item, IEqualityComparer<TValue> comparer)
            {
                for (int i = 0; i < m_parentDictionary.Count; i++)
                {
                    if (comparer.Equals(m_parentDictionary.m_Pairs[i].value, item))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array), "[SerializableDictionary::KeyCollection::CopyTo] Given argument was null.");
                }
                if ((m_parentDictionary.Count + arrayIndex) > array.Length)
                {
                    throw new ArgumentException("[SerializableDictionary::KeyCollection::CopyTo] Given array size is less than the collection size + offset.", nameof(array));
                }

                for (int i = arrayIndex; i < m_parentDictionary.Count; i++)
                {
                    array[i] = m_parentDictionary.m_Pairs[i].value;
                }
            }

            public override IEnumerator<TValue> GetEnumerator()
            {
                for (int i = 0; i < m_parentDictionary.Count; i++)
                {
                    yield return m_parentDictionary.m_Pairs[i].value;
                }
            }
        }

        /// <summary>
        /// List of the pairs contained inside this dictionary.
        /// </summary>
        [SerializeField]
        private List<Pair> m_Pairs = new List<Pair>();

        /// <summary>
        /// A pair that contains a dummy value.
        /// <br>This value, while only used in the editor, has to be serialized to avoid errors on built players.</br>
        /// </summary>
        [SerializeField]
        private Pair m_DummyPair;

        private KeyCollection m_CachedKeysCollection;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        public KeyCollection Keys
        {
            get
            {
                m_CachedKeysCollection ??= new KeyCollection(this);
                return m_CachedKeysCollection;
            }
        }

        private ValueCollection m_CachedValuesCollection;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        public ValueCollection Values
        {
            get
            {
                m_CachedValuesCollection ??= new ValueCollection(this);
                return m_CachedValuesCollection;
            }
        }

        public override int Count => m_Pairs.Count;
        public bool IsReadOnly => false;

        private readonly IEqualityComparer<TKey> m_Comparer = EqualityComparer<TKey>.Default;
        /// <summary>
        /// The equality comparer that this dictionary uses.
        /// </summary>
        public IEqualityComparer<TKey> Comparer => m_Comparer;

        /// <summary>
        /// Returns the index of given <paramref name="key"/>.
        /// <br>Returns -1 if <paramref name="key"/> does not exist in <see cref="m_Keys"/>.</br>
        /// <br>Uses the current dictionary's <see cref="IEqualityComparer{TKey}"/></br>
        /// <br>This is used to avoid Linq's IndexOf with <see cref="IEqualityComparer{T}"/> as it allocates garbage.</br>
        /// </summary>
        /// <param name="key">Key to search for.</param>
        private int IndexOfKey(TKey key)
        {
            for (int i = 0; i < m_Pairs.Count; i++)
            {
                if (Comparer.Equals(m_Pairs[i].key, key))
                {
                    return i;
                }
            }

            return -1;
        }

        public override bool KeysAreUnique()
        {
            HashSet<TKey> currentKeys = new HashSet<TKey>(m_Pairs.Count);
            for (int i = 0; i < m_Pairs.Count; i++)
            {
                if (!currentKeys.Add(m_Pairs[i].key))
                {
                    return false;
                }
            }

            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = IndexOfKey(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException("[SerializableDictionary::this[]::get] Given key value was not found.");
                }

                // Key/Values are matched according to their indices
                return m_Pairs[index].value;
            }
            set
            {
                int index = IndexOfKey(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException("[SerializableDictionary::this[]::set] Given key value was not found.");
                }

                m_Pairs[index].value = value;
            }
        }
        /// <summary>
        /// Returns the <see langword="default"/> value if the given <paramref name="key"/> doesn't exist.
        /// </summary>
        public TValue GetValueOrDefault(TKey key)
        {
            return GetValueOrDefault(key, default);
        }
        /// <inheritdoc cref="GetValueOrDefault(TKey)"/>
        /// <param name="defaultValue">The value to default into if the given <paramref name="key"/> doesn't exist.</param>
        public TValue GetValueOrDefault(TKey key, TValue defaultValue)
        {
            int index = IndexOfKey(key);
            if (index < 0)
            {
                return defaultValue;
            }

            return m_Pairs[index].value;
        }

        internal override bool DummyPairIsValid()
        {
            return !ContainsKey(m_DummyPair.key);
        }
        internal override void AddDummyPair()
        {
            Add(m_DummyPair.key, m_DummyPair.value);
            m_DummyPair = new Pair();
        }

        public void Add(TKey key, TValue value)
        {
            if (typeof(TKey).IsNullable() && Comparer.Equals(key, default))
            {
                throw new ArgumentNullException(nameof(key), "[SerializableDictionary::Add] Given key was null.");
            }
            if (ContainsKey(key))
            {
                throw new ArgumentException("[SerializableDictionary::Add] An element with the same key already exists in the dictionary.", nameof(key));
            }

            m_Pairs.Add(new Pair(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < m_Pairs.Count; i++)
            {
                if (Comparer.Equals(m_Pairs[i].key, key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Remove(TKey key)
        {
            // Get index of key
            int index = IndexOfKey(key);

            if (index < 0)
            {
                return false;
            }

            m_Pairs.RemoveAt(index);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!ContainsKey(key))
            {
                value = default;
                return false;
            }

            value = this[key];
            return true;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            m_Pairs.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item, EqualityComparer<TValue>.Default);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item, IEqualityComparer<TValue> comparer)
        {
            if (!TryGetValue(item.Key, out TValue value))
            {
                return false;
            }

            return comparer.Equals(item.Value, value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "[SerializableDictionary::CopyTo] Argument was null.");
            }

            if (array.Length < (arrayIndex + Count))
            {
                throw new ArgumentException("[SerializableDictionary::CopyTo] Failed to copy into given array. Array length is smaller than dictionary or index is out of bounds", nameof(array));
            }

            for (int i = arrayIndex; i < Count; i++)
            {
                array[i] = (KeyValuePair<TKey, TValue>)m_Pairs[i];
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item, EqualityComparer<TValue>.Default);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item, IEqualityComparer<TValue> comparer)
        {
            int indexOfKey = -1;
            for (int i = 0; i < m_Pairs.Count; i++)
            {
                if (Comparer.Equals(item.Key, m_Pairs[i].key))
                {
                    indexOfKey = i;
                    break;
                }
            }

            if (indexOfKey < 0)
            {
                return false;
            }

            if (!comparer.Equals(item.Value, m_Pairs[indexOfKey].value))
            {
                return false;
            }

            m_Pairs.RemoveAt(indexOfKey);
            return true;
        }

        public void TrimExcess()
        {
            m_Pairs.TrimExcess();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (KeyValuePair<TKey, TValue>)m_Pairs[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Convert to-from dictionary
        /// <summary>
        /// Creates an empty SerializableDictionary.
        /// </summary>
        public SerializableDictionary()
        { }
        /// <summary>
        /// Creates a dictionary with capacity reserved.
        /// </summary>
        public SerializableDictionary(int capacity)
        {
            m_Pairs.Capacity = capacity;
        }
        /// <summary>
        /// Creates a dictionary from another dictionary.
        /// </summary>
        public SerializableDictionary(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
        {
            // assignation of a different comparer means that you most likely want a check according to 'comparer'
            // so only accept 'IDictionary'ies for this case.
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict), "[SerializableDictionary::ctor] Given argument was null.");
            }
            // Assign optionally
            if (comparer != null)
            {
                m_Comparer = comparer;
            }

            // Copy values to the pairs
            m_Pairs.Capacity = dict.Count;

            // Initial keys add loop
            HashSet<TKey> keysSet = new HashSet<TKey>(m_Pairs.Capacity, Comparer);
            foreach (TKey dictKey in dict.Keys)
            {
                // o(n^2) moment
                if (!keysSet.Add(dictKey))
                {
                    throw new ArgumentException("[SerializableDictionary::ctor] Given 'dict' contains duplicate keys.", nameof(dict));
                }

                m_Pairs.Add(new Pair() { key = dictKey });
            }
            // The thing is, the foreach iteration is index matched according to the 'IDictionary' spec.
            foreach (KeyValuePair<int, TValue> indexedValuePair in dict.Values.Indexed())
            {
                m_Pairs[indexedValuePair.Key].value = indexedValuePair.Value;
            }
        }
        public SerializableDictionary(IDictionary<TKey, TValue> dict) : this(dict, null)
        { }
        /// <summary>
        /// Creates a dictionary from another dictionary (without any checks on <paramref name="dict"/>).
        /// <br>If you want the <paramref name="dict"/> to have it's keys checked anyways, cast it to <see cref="IDictionary{TKey, TValue}"/>.</br>
        /// </summary>
        public SerializableDictionary(Dictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict), "[SerializableDictionary::ctor] Given argument was null.");
            }

            m_Pairs.Capacity = dict.Count;

            // Since the 'dict' should be a complete class that enforces that all keys are different, just create without checking.
            foreach (KeyValuePair<TKey, TValue> pair in dict)
            {
                m_Pairs.Add(new Pair(pair.Key, pair.Value));
            }
        }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> comparer)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values), "[SerializableDictionary::ctor] Given argument was null.");
            }
            if (comparer != null)
            {
                m_Comparer = comparer;
            }

            m_Pairs.Capacity = values.Count();

            HashSet<TKey> keysSet = new HashSet<TKey>(m_Pairs.Capacity, Comparer);
            foreach (KeyValuePair<TKey, TValue> pair in values)
            {
                if (!keysSet.Add(pair.Key))
                {
                    throw new ArgumentException("[SerializableDictionary::ctor] Given 'values' collection contains duplicate keys.", nameof(values));
                }

                Add(pair);
            }
        }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values) : this(values, null)
        { }
    }
}
