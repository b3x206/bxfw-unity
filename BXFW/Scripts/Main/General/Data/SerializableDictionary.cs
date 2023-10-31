using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace BXFW
{
    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> that can be serialized by unity.
    /// Uses the same constraints as the <see cref="Dictionary{TKey, TValue}"/> on code, 
    /// but on editor a <see cref="UnityEditor.PropertyDrawer"/> is needed (TODO)
    /// <br/>
    /// <br>NOTE : Array types such as <c><typeparamref name="TKey"/>[]</c> or <c><typeparamref name="TValue"/>[]</c> are NOT serializable 
    /// in <typeparamref name="TKey"/> or <typeparamref name="TValue"/> (by unity). Wrap them with array container class.</br>
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        // Save the base class Dictionary to serialized lists
        public void OnBeforeSerialize()
        {
            // The 'keys' and 'values' are already serialized, just add them to the actual lists that unity will serialize.
            // -- These used to only account for removing + adding keys and values
            // - Check for array sequence changes also (the SequenceEqual's)

            // - Check for removal + adding
            // If a key is removed
            if (keys.Count != values.Count)
            {
                // Removing or adding keys, set to defualt value
                values.Resize(keys.Count, default);
            }

            // Directly adding to dictionary (from c#, not from editor)
            // --Only useful if we directly add to the dictionary, then serialize
            if (!Enumerable.SequenceEqual(keys, Keys) || !Enumerable.SequenceEqual(values, Values) || keys.Count < Keys.Count) // If the actual dictionary is more up to date.
            {
                keys.Clear();
                values.Clear();

                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }
        }

        // Load base class Dictionary from the serialized lists
        public void OnAfterDeserialize()
        {
            // Clear the base dictionary in case of garbage data
            Clear();

            if (keys.Count != values.Count)
            {
                // Resize the values if the keys are more or less
                values.Resize(keys.Count, default);

                // Unity moment
                if (keys.Count != values.Count)
                {
                    throw new IndexOutOfRangeException(string.Format(@"[SerializableDictionary] There are {0} keys and {1} values after deserialization.
Make sure that both key and value types are serializable.", keys.Count, values.Count));
                }
            }

            // Append the serialized values into the base dictionary class
            for (int i = 0; i < keys.Count; i++)
            {
                if (Keys.Contains(keys[i]))
                {
                    // NOTE : 
                    // Ignore for now, don't update the dictionary.
                    // There is no elegant solution to the 'duplicate' issue.
                    // Just make sure that the dev is notified about the issue.

                    if (Application.isPlaying)
                    {
                        Debug.LogWarning(string.Format("[SerializableDictionary] Note : Key {0} is already contained in the dictionary. Please make sure your keys are all unique.", keys[i]));
                    }

                    continue;
                }

                Add(keys[i], values[i]);
            }
        }

        // Convert to-from dictionary
        /// <summary>
        /// Creates an empty SerializableDictionary.
        /// </summary>
        public SerializableDictionary() : base()
        { }
        /// <summary>
        /// Creates a dictionary with capacity reserved.
        /// </summary>
        public SerializableDictionary(int capacity) : base(capacity)
        { }
        /// <summary>
        /// Creates a dictionary from another dictionary.
        /// </summary>
        public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict)
        { }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> comparer) : base(values, comparer)
        { }
    }

    /// <summary>
    /// Base used for the <see cref="SerializableDictionary2{TKey, TValue}"/>.
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

        protected static bool TypeIsNullable(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t), "[SerializableDictionary::TypeIsNullable] Given argument was null.");
            }

            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }
    }
    
    // If 'SerializableDictionary{TKey, TValue}' was so good, why there isn't a 'SerializableDictionary2{TKey, TValue}'
    // Well, you asked. And i delivered. The all new SerializableDictionary2!
    // Features
    // * Probably will break your previous serialization lol
    // * Has a cool editor that isn't definitely broken because ReorderableList adding doesn't work unless you use dark magic.
    // TODO list:
    // * Some Unit testing (to check if the dictionary2 is screwed or if it works)
    // * Migrate all scripts to this version
    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> that can be serialized by unity.
    /// Uses the same constraints as the <see cref="Dictionary{TKey, TValue}"/> on both editor and code.
    /// <br/>
    /// <br>NOTE : Array types such as <c><typeparamref name="TKey"/>[]</c> or <c><typeparamref name="TValue"/>[]</c> are NOT serializable
    /// in this dictionary (by unity). Wrap them with array container class because double lists don't get serialized unless you trick the serializer.</br>
    /// </summary>
    [Serializable]
    public class SerializableDictionary2<TKey, TValue> : SerializableDictionaryBase, IDictionary<TKey, TValue>
    {
        // I know this is most likely how you not dictionary but unity does not serialize anything array like otherwise.
        // Because reorderable list, we have to use an incompatible data type
        // So we create a key/value pair contained in the class

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
            if (TypeIsNullable(typeof(TKey)) && Comparer.Equals(key, default))
            {
                throw new ArgumentNullException("[SerializableDictionary::SetKey] Given boxed key value is null.");
            }

            m_Keys[index] = key;
        }

        public void Add(TKey key, TValue value)
        {
            if (TypeIsNullable(typeof(TKey)) && Comparer.Equals(key, default))
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
        public SerializableDictionary2()
        { }
        /// <summary>
        /// Creates a dictionary with capacity reserved.
        /// </summary>
        public SerializableDictionary2(int capacity)
        {
            m_Keys.Capacity = capacity;
            m_Values.Capacity = capacity;
        }
        /// <summary>
        /// Creates a dictionary from another dictionary.
        /// </summary>
        public SerializableDictionary2(IDictionary<TKey, TValue> dict)
        {
            // Copy values to the pairs
            m_Keys = new List<TKey>(dict.Keys);
            m_Values = new List<TValue>(dict.Values);
        }
        public SerializableDictionary2(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
            : this(dict)
        {
            m_Comparer = comparer;
        }
        /// <summary>
        /// Creates a dictionary from a collection.
        /// </summary>
        public SerializableDictionary2(IEnumerable<KeyValuePair<TKey, TValue>> values)
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
        public SerializableDictionary2(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> comparer)
            : this(values)
        {
            m_Comparer = comparer;
        }
    }
}
