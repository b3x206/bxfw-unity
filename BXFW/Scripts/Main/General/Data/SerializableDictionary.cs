using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Serializable dictionary.
    /// <br/>
    /// <br>NOTE : Array types such as <c><typeparamref name="TKey"/>[]</c> or <c><typeparamref name="TValue"/>[]</c> are NOT serializable 
    /// in <typeparamref name="TKey"/> or <typeparamref name="TValue"/> (by unity). Wrap them with array container class.</br>
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        where TValue : new()
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        // Save the dictionary to lists
        public void OnBeforeSerialize()
        {
            // The 'keys' and 'values' are already serialized, just add them 

            // If a key is removed
            if (keys.Count != values.Count)
            {
                // Removing or adding keys, set to defualt value
                values.Resize(keys.Count);
            }

            // Directly adding to dictionary (from c#, not from editor)
            // --Only useful if we directly add to the dictionary, then serialize
            if (keys.Count < Keys.Count) // If the actual dictionary is more up to date.
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

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
            {
                values.Resize(keys.Count);

                // Unity moment
                if (keys.Count != values.Count)
                {
                    throw new IndexOutOfRangeException(string.Format(@"[SerializableDictionary] There are {0} keys and {1} values after deserialization.
Make sure that both key and value types are serializable.", keys.Count, values.Count));
                }
            }

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
    }
}