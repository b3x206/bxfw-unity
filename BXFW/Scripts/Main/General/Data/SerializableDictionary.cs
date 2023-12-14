using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace BXFW
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
        /// Returns the key boxed as an <see cref="object"/>.
        /// </summary>
        public abstract object GetKey(int index);
        /// <summary>
        /// Sets the boxed key object <paramref name="value"/>.
        /// </summary>
        public abstract void SetKey(int index, object value);
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
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase, IDictionary<TKey, TValue>
    {
        // ?? : Unless this works fine, refactor this to use a 'Pair' data type.
        // -- here's an idiot rambling : 
        // This is what you do when you don't learn DSA, you do this rubbish. (it work doe, adding complexity best case (N!^N!)!1!)
        // However, doing a 'HashSet' based key list will disallow the unity serialization so do it "normally".
        // unity can't serialize any data type more complex than an generic array :)
        // --
        [SerializeField, FormerlySerializedAs("keys")]
        private List<TKey> m_Keys = new List<TKey>();
        public ICollection<TKey> Keys => m_Keys;

        [SerializeField, FormerlySerializedAs("values")]
        private List<TValue> m_Values = new List<TValue>();
        public ICollection<TValue> Values => m_Values;

        public override int Count => m_Keys.Count;
        public bool IsReadOnly => false;

        private IEqualityComparer<TKey> m_Comparer = EqualityComparer<TKey>.Default;
        /// <summary>
        /// The equality comparer that this dictionary uses.
        /// </summary>
        public IEqualityComparer<TKey> Comparer => m_Comparer;

        public override bool KeysAreUnique()
        {
            // HashSet's don't serialize, but it's a fast and performant unique ensuring data type
            HashSet<TKey> uniqueKeys = new HashSet<TKey>(m_Keys);
            return uniqueKeys.Count == m_Keys.Count;
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = m_Keys.IndexOf(key, m_Comparer);
                if (index < 0)
                {
                    throw new KeyNotFoundException("[SerializableDictionary::this[]::get] Given key value was not found.");
                }

                // Key/Values are matched according to their indices
                return m_Values[index];
            }
            set
            {
                int index = m_Keys.IndexOf(key, m_Comparer);
                if (index < 0)
                {
                    throw new KeyNotFoundException("[SerializableDictionary::this[]::set] Given key value was not found.");
                }

                m_Values[index] = value;
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
            int index = m_Keys.IndexOf(key, m_Comparer);
            if (index < 0)
            {
                return defaultValue;
            }

            return m_Values[index];
        }
        public override object GetKey(int index)
        {
            return m_Keys[index];
        }
        public override void SetKey(int index, object value)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentException($"[SerializableDictionary::SetKey] Given index {index} is out of bounds.");
            }
            // Unbox type
            if (!(value is TKey key))
            {
                throw new ArgumentException("[SerializableDictionary::SetKey] Given boxed key value is not 'TKey'.");
            }
            // Check if unboxed value is valid
            if (typeof(TKey).IsNullable() && Comparer.Equals(key, default))
            {
                throw new ArgumentNullException("[SerializableDictionary::SetKey] Given boxed key value is null.");
            }

            m_Keys[index] = key;
        }

        public void Add(TKey key, TValue value)
        {
            if (typeof(TKey).IsNullable() && Comparer.Equals(key, default))
            {
                throw new ArgumentNullException("[SerializableDictionary::Add] Given key was null.", nameof(key));
            }
            if (m_Keys.Contains(key, Comparer))
            {
                throw new ArgumentException("[SerializableDictionary::Add] An element with the same key already exists in the dictionary.", nameof(key));
            }

            // Assert the size of the keys and values
            if (m_Values.Count != m_Keys.Count)
            {
                // Sizes don't match, resize and throw exception?
                // Or just silently resize values to be equal to 'm_Keys'
                m_Values.Resize(m_Keys.Count, default);
            }

            m_Keys.Add(key);
            m_Values.Add(value);
        }

        public bool ContainsKey(TKey key)
        {
            return m_Keys.Contains(key, Comparer);
        }

        public bool Remove(TKey key)
        {
            // Get index of key
            int index = m_Keys.IndexOf(key);

            if (index < 0)
            {
                return false;
            }

            // Set the size of the keys and values
            if (m_Values.Count != m_Keys.Count)
            {
                m_Values.Resize(m_Keys.Count, default);
            }

            // Remove values at
            m_Keys.RemoveAt(index);
            m_Values.RemoveAt(index);

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
            m_Keys.Clear();
            m_Values.Clear();
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

            if (array.Length < arrayIndex + Count)
            {
                throw new ArgumentException("[SerializableDictionary::CopyTo] Failed to copy into given array. Array length is smaller than dictionary or index is out of bounds", nameof(array));
            }

            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<TKey, TValue>(m_Keys[i], m_Values[i]);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item, EqualityComparer<TValue>.Default);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item, IEqualityComparer<TValue> comparer)
        {
            if (!TryGetValue(item.Key, out TValue value))
            {
                return false;
            }

            if (!comparer.Equals(item.Value, value))
            {
                return false;
            }

            return Remove(item.Key);
        }

        public void TrimExcess()
        {
            m_Keys.TrimExcess();
            m_Values.TrimExcess();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(m_Keys[i], m_Values[i]);
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
            m_Keys.Capacity = capacity;
            m_Values.Capacity = capacity;
        }
        /// <summary>
        /// Creates a dictionary from another dictionary.
        /// </summary>
        public SerializableDictionary(IDictionary<TKey, TValue> dict)
        {
            // Copy values to the pairs
            m_Keys = new List<TKey>(dict.Keys);
            m_Values = new List<TValue>(dict.Values);
        }
        public SerializableDictionary(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
            : this(dict)
        {
            m_Comparer = comparer;
        }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            m_Keys.Capacity = m_Values.Capacity = values.Count();

            foreach (KeyValuePair<TKey, TValue> pair in values)
            {
                Add(pair);
            }
        }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> comparer)
            : this(values)
        {
            m_Comparer = comparer;
        }
    }
}
