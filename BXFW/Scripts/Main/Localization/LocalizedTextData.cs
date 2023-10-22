using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace BXFW.Data
{
    /// <summary>
    /// Data for the localized text.
    /// <br/>
    /// <br>To create new of this class, use the following step:</br>
    /// <br><c><see langword="new"/> <see cref="LocalizedTextData"/>(localized_text_id, 
    /// <see langword="new"/> Dictionary&lt;<see cref="string"/>, <see cref="string"/>&gt; { { language_id, text_content } })</c></br>
    /// <br/>
    /// </summary>
    [Serializable]
    public class LocalizedTextData : IEnumerable<KeyValuePair<string, string>>, IEquatable<LocalizedTextData>
    {
        /// <summary>
        /// Default locale used for the data.
        /// <br>This is set to "en" by default.</br>
        /// </summary>
        public static string DefaultLocale = "en";
        private static string m_CurrentISOLocaleName = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        /// <summary>
        /// Current TwoLetterISOLanguageName of this current locale of your system.
        /// <br>This does not change during runtime (unless intervened with using <see cref="RefreshCurrentLocaleName"/>).
        /// The app needs to be restarted or <see cref="RefreshCurrentLocaleName"/> needs to be called to correct this value.</br>
        /// </summary>
        public static string CurrentISOLocaleName => m_CurrentISOLocaleName;
        /// <summary>
        /// Refreshes the current locale to the current culture value.
        /// <br>Depending on the OS, this may or may not have an effect.</br>
        /// </summary>
        public static void RefreshCurrentLocaleName()
        {
            m_CurrentISOLocaleName = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// A text ID, used for finding a <see cref="LocalizedTextData"/> inside an array.
        /// </summary>
        public string TextID;
        /// <summary>
        /// Contains the definitions that start with #pragma.
        /// <br>An optional extension way of defining variables.</br>
        /// </summary>
        public readonly SerializableDictionary<string, string> PragmaDefinitions = new SerializableDictionary<string, string>();
        /// <summary>
        /// A dictionary that contains the localization two letter iso identifiers and it's respective values.
        /// </summary>
        [SerializeField, FormerlySerializedAs("LocalizedValues")]
        private SerializableDictionary<string, string> m_LocaleDatas = new SerializableDictionary<string, string>();
        /// <summary>
        /// The current localization data but as a IDictionary : <br/>
        /// <inheritdoc cref="m_LocaleDatas"/>
        /// </summary>
        public IDictionary<string, string> LocaleDatas
        {
            get
            {
                return m_LocaleDatas;
            }
        }
        /// <summary>
        /// Returns the total size (approximation, takes the length of the dictionary strings) of the <see cref="LocalizedTextData"/> data.
        /// </summary>
        public int StringSize
        {
            get
            {
                int sizeLocaleData = 0;
                int sizePragma = 0;
                
                foreach (var pragma in PragmaDefinitions)
                {
                    sizePragma += pragma.Key.Length + pragma.Value.Length;
                }
                foreach (var localeDef in m_LocaleDatas)
                {
                    sizeLocaleData += localeDef.Key.Length + localeDef.Value.Length;
                }

                return sizeLocaleData + sizePragma;
            }
        }

        public string this[string key]
        {
            get
            {
                if (!m_LocaleDatas.TryGetValue(key, out string value))
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
            return m_LocaleDatas.ContainsKey(key);
        }

        /// <summary>
        /// Returns the current locale string. If it doesn't exist it fallbacks to <see cref="DefaultLocale"/>.
        /// <br>If the default locale doesn't also exist, it fallbacks to the first value.</br>
        /// <br>If no locales were registered, this function just returns null.</br>
        /// </summary>
        public string GetCurrentLocaleString()
        {
            if (m_LocaleDatas.Count <= 0)
            {
                // since no locales exist at this state, return empty
                return string.Empty;
            }

            var locale = CurrentISOLocaleName;
            if (ContainsLocale(locale))
                return this[locale];

            if (ContainsLocale(DefaultLocale))
                return this[DefaultLocale];

            // Return the first in values
            Debug.LogWarning(string.Format("[LocalizedTextData::GetCurrentLocaleString] No fallback locale found with iso code '{0}'. Returning first element.", DefaultLocale));
            return m_LocaleDatas.Values.First();
        }
        /// <summary>
        /// Sets a value for the current locale for this data.
        /// <br>Creates a key in <see cref="LocaleDatas"/> if the value for current locale does not exist.</br>
        /// </summary>
        public void SetCurrentLocaleString(string value)
        {
            if (!ContainsLocale(value))
            {
                m_LocaleDatas.Add(CurrentISOLocaleName, value);
                return;
            }

            m_LocaleDatas[CurrentISOLocaleName] = value;
        }
        /// <summary>
        /// <c><see langword="get"/> : </c> <see cref="GetCurrentLocaleString"/>
        /// <br/>-&gt; <inheritdoc cref="GetCurrentLocaleString"/>
        /// <br/>
        /// <br><c><see langword="set"/> : </c> <see cref="SetCurrentLocaleString(string)"/></br>
        /// <br/>-&gt; <inheritdoc cref="SetCurrentLocaleString(string)"/>
        /// </summary>
        public string CurrentLocaleString
        {
            get { return GetCurrentLocaleString(); }
            set { SetCurrentLocaleString(value); }
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
        /// <summary>
        /// Creates the full fat <see cref="LocalizedTextData"/> with every variable settable as is.
        /// <br>This is mostly used for <see cref="LocalizedText"/>s on the scene that import a 'LocalizedTextAsset', which require an ID appended into it.</br>
        /// </summary>
        /// <param name="textID">ID appended to this <see cref="LocalizedTextData"/>. This is not needed unless you are thinking of filtering out a list of LocalizedTextData.</param>
        /// <param name="values">The values added to the data. (Key=Two[or three] Letter ISO identifier for the language, Value=The value to use when matched)</param>
        /// <param name="pragmaDefs">Pragmatic additional definitions for other classes to use. Basically a settings dictionary. Only used in <see cref="LocalizedText"/> for the time being.</param>
        public LocalizedTextData(string textID, SerializableDictionary<string, string> values, SerializableDictionary<string, string> pragmaDefs)
        {
            TextID            = textID;
            m_LocaleDatas     = values;
            PragmaDefinitions = pragmaDefs;
        }

        /// <summary>
        /// Returns the current locale string.
        /// </summary>
        public static explicit operator string(LocalizedTextData text)
        {
            return text.GetCurrentLocaleString();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return m_LocaleDatas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LocaleDatas.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        /// <summary>
        /// Returns debug information with the text id and the current locale string.
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
            if (other is null)
                return false;

            // Pragma settings can be different, idc.
            return TextID.Equals(other.TextID, StringComparison.Ordinal) && 
                m_LocaleDatas.SequenceEqual(other.m_LocaleDatas);
        }

        public override int GetHashCode()
        {
            int hashCode = -2018306565;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(TextID);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Dictionary<string, string>>.Default.GetHashCode(m_LocaleDatas);
            return hashCode;
        }
    }
}
