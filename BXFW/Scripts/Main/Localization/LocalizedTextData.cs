using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// Data for the localized text.
    /// <br/>
    /// <br>To create new of this class, use the following step:</br>
    /// <br><c><see langword="new"/> <see cref="LocalizedTextData"/>(localized_text_id, 
    /// <see langword="new"/> Dictionary&lt;<see cref="string"/>, <see cref="string"/>&gt; { { language_id, text_content } })</c></br>
    /// <br/>
    /// <br>And so on.. The constructor isn't 'concise' enough and this is 'experimental'.</br>
    /// </summary>
    [Serializable]
    public class LocalizedTextData : IEnumerable<KeyValuePair<string, string>>, IEquatable<LocalizedTextData>
    {
        /// TODO : Put <see cref="DefaultLocale"/> to a different place.
        /// TODO 2 : <see cref="UnityEditor.CustomPropertyDrawer"/> for this
        public static readonly string DefaultLocale = "en";
        public static readonly string ISOCurrentLocale = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        public string TextID;

        /// <summary>
        /// Contains the definitions that start with #pragma.
        /// <br>An optional extension way of defining variables.</br>
        /// </summary>
        public readonly SerializableDictionary<string, string> PragmaDefinitions = new SerializableDictionary<string, string>();
        // TODO : Use the BXFW.SerializableDictionary class (no need for ISerializationCallbackReceiver)
        [SerializeField] private SerializableDictionary<string, string> LocalizedValues = new SerializableDictionary<string, string>();
        public IDictionary<string, string> Data
        {
            get
            {
                return LocalizedValues;
            }
        }
        /// <summary>
        /// Returns the total size (approximation, takes the length of the dictionary strings) of the <see cref="LocalizedTextData"/> data.
        /// </summary>
        public int StringSize
        {
            get
            {
                int sizeValues = 0;
                int sizeKeys = 0;

                foreach (var value in LocalizedValues.Values)
                {
                    sizeValues += value.Length;
                }
                foreach (var key in LocalizedValues.Keys)
                {
                    sizeKeys += key.Length;
                }

                return sizeValues + sizeKeys;
            }
        }

        public string this[string key]
        {
            get
            {
                if (!LocalizedValues.TryGetValue(key, out string value))
                    return string.Format("{0} (no-locale) | {1}", key, TextID); // OnError, just return the problematic TextID.

                return value;
            }
        }
        /// <summary>
        /// Returns <see langword="true"/> if the language <paramref name="key"/> is contained in the values.
        /// </summary>
        /// <param name="key">2-letter iso identifier for the language.</param>
        public bool ContainsLocale(string key)
        {
            return LocalizedValues.ContainsKey(key);
        }

        /// <summary>
        /// Returns the current locale string. If it doesn't exist it fallbacks to <see cref="DefaultLocale"/>.
        /// <br>If the default locale doesn't also exist, it fallbacks to the first value.</br>
        /// </summary>
        public string GetCurrentLocaleString()
        {
            if (LocalizedValues.Count == 0)
                throw new NullReferenceException("[LocalizedTextData::GetCurrentLocaleString] No locale strings registered!");

            var locale = ISOCurrentLocale;
            if (ContainsLocale(locale))
                return this[locale];

            if (ContainsLocale(DefaultLocale))
                return this[DefaultLocale];

            // Return the first in values
            Debug.LogWarning(string.Format("[LocalizedTextData::GetCurrentLocaleString] No fallback locale found with iso code '{0}'. Returning first element.", DefaultLocale));
            return LocalizedValues.Values.ToArray()[0];
        }
        /// <summary>
        /// Sets a value for the current locale for this data.
        /// <br>Creates a key in <see cref="Data"/> if the value for current locale does not exist.</br>
        /// </summary>
        public void SetCurrentLocaleString(string value)
        {
            if (!ContainsLocale(value))
            {
                LocalizedValues.Add(ISOCurrentLocale, value);
                return;
            }

            LocalizedValues[ISOCurrentLocale] = value;
        }

        // -- Operator / Class
        /// <summary>
        /// Creates an completely empty data.
        /// </summary>
        public LocalizedTextData()
        { }
        /// <summary>
        /// Creates a LocalizedTextData using a <see cref="Dictionary{TKey, TValue}"/>.
        /// <br>(without id, use for code based locale)</br>
        /// </summary>
        public LocalizedTextData(Dictionary<string, string> values) 
            : this(new SerializableDictionary<string, string>(values))
        { }
        /// <summary>
        /// Creates the 'LocalizedTextData' with the <see cref="Dictionary{TKey, TValue}"/> type instead of a <see cref="SerializableDictionary{TKey, TValue}"/> type.
        /// </summary>
        /// <param name="TextID">ID of the text.</param>
        /// <param name="values">Values of the dictionary. (Key=Two letter iso lang, Value=Corresponding string)</param>
        public LocalizedTextData(string TextID, Dictionary<string, string> values) 
            : this(TextID, new SerializableDictionary<string, string>(values))
        { }
        /// <summary>
        /// Creates the 'LocalizedTextData' with the <see cref="Dictionary{TKey, TValue}"/> type instead of a <see cref="SerializableDictionary{TKey, TValue}"/> type.
        /// </summary>
        /// <param name="TextID">ID of the text.</param>
        /// <param name="values">Values of the dictionary. (Key=Two letter iso lang, Value=Corresponding string)</param>
        /// <param name="pragmaDefs">Pragma definitions for the data.</param>
        public LocalizedTextData(string TextID, Dictionary<string, string> values, Dictionary<string, string> pragmaDefs) 
            : this(TextID, new SerializableDictionary<string, string>(values), new SerializableDictionary<string, string>(pragmaDefs))
        { }

        /// <summary>
        /// Creates an new <see cref="LocalizedTextData"/> object. (without an id, use for inline localization)
        /// </summary>
        /// <param name="values">Dictionary Data => Locale = Key | Content = Value</param>
        public LocalizedTextData(SerializableDictionary<string, string> values) 
            : this(string.Empty, values)
        { }
        /// <summary>
        /// Creates an new <see cref="LocalizedTextData"/> object without pragma options. (pragma options are completely optional)
        /// </summary>
        /// <param name="TextID">ID of the localized text.</param>
        /// <param name="values">Values of the dictionary. (Key=Two letter iso lang, Value=Corresponding string)</param>
        public LocalizedTextData(string TextID, SerializableDictionary<string, string> values) 
            : this(TextID, values, null)
        { }
        public LocalizedTextData(string textID, SerializableDictionary<string, string> values, SerializableDictionary<string, string> pragmaDefs)
        {
            TextID              = textID;
            LocalizedValues     = values;
            PragmaDefinitions   = pragmaDefs;
        }

        public static explicit operator string(LocalizedTextData text)
        {
            return text.GetCurrentLocaleString();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return LocalizedValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LocalizedValues.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        /// <summary>
        /// Returns a debug display with the text id and the current locale string.
        /// </summary>
        public override string ToString()
        {
            return $"ID={TextID}, CurrentLocale={GetCurrentLocaleString()}";
        }

        public static bool operator ==(LocalizedTextData lhs, LocalizedTextData rhs)
        {
            bool lNull = lhs is null;
            bool rNull = rhs is null;

            if (lNull)
            {
                return rNull;
            }

            return lhs.Equals(rhs);
        }
        public static bool operator !=(LocalizedTextData lhs, LocalizedTextData rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(LocalizedTextData other)
        {
            if (other == null)
                return false;

            return LocalizedValues == other.LocalizedValues && TextID == other.TextID;
        }

        public override int GetHashCode()
        {
            int hashCode = -2018306565;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(TextID);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Dictionary<string, string>>.Default.GetHashCode(LocalizedValues);
            return hashCode;
        }
    }
}
