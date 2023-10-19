using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

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
}
