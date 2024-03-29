﻿using System;
using BXFW.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace BXFW.Data
{
    /// <summary>
    /// Data for a localized text. Uses the current system locale and can take multiple locale values.
    /// </summary>
    /// <remarks>
    /// <br>New way of creating inline in code :</br>
    /// <c>
    /// <br><see langword="new"/> <see cref="LocalizedTextData"/>(localizedTextID)</br>
    /// <br>{</br>
    /// <br>&#10240;&#10240;&#10240;&#10240;{ "2char_lang_id", "text content"  }</br>
    /// <br>}</br>
    /// </c>
    /// <br/>
    /// <br>Old way of creating inline in code :</br>
    /// <br>To create new of this class, use the following step:</br>
    /// <br><c><see langword="new"/> <see cref="LocalizedTextData"/>(localizedTextID, 
    /// <see langword="new"/> Dictionary&lt;<see cref="string"/>, <see cref="string"/>&gt; { { "2char_lang_id", "text content for that id" } })</c></br>
    /// <br/>
    /// </remarks>
    [Serializable]
    public class LocalizedTextData : ICollection<KeyValuePair<string, string>>, IEquatable<LocalizedTextData>
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
        /// <br/>
        /// <br>Note : This does not change the current scene or anything, it just changes a variable.</br>
        /// <br>To change the current scene's localized texts, you can do <see cref="UnityEngine.Object.FindObjectOfType{T}()"/> with <see cref="LocalizedText"/> and call <see cref="LocalizedText.Apply(bool, bool)"/>.</br>
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
        /// A dictionary that contains the localization two letter iso identifiers and it's respective values.
        /// </summary>
        [SerializeField, FormerlySerializedAs("LocalizedValues")]
        private SerializableDictionary<string, string> m_LocaleDatas = new SerializableDictionary<string, string>();
        /// <summary>
        /// The current localization data but as a IDictionary : <br/>
        /// <inheritdoc cref="m_LocaleDatas"/>
        /// </summary>
        public IDictionary<string, string> LocaleDatas => m_LocaleDatas;

        /// <summary>
        /// Returns the total size (approximation, takes the length of the dictionary strings) of the <see cref="LocalizedTextData"/> data.
        /// </summary>
        public int StringSize
        {
            get
            {
                int sizeLocaleData = 0;
                foreach (KeyValuePair<string, string> localeDef in m_LocaleDatas)
                {
                    sizeLocaleData += localeDef.Key.Length + localeDef.Value.Length;
                }

                return sizeLocaleData;
            }
        }

        /// <summary>
        /// Get the corresponding value for the given two letter country id <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Two letter ISO identifier for the country ID.</param>
        /// <returns>The corresponding locale value. If no locale exists for <paramref name="key"/>, debug value like <c>"{key} (no-locale) | {TextID}"</c> is returned.</returns>
        public string this[string key]
        {
            get
            {
                if (!m_LocaleDatas.TryGetValue(key, out string value))
                {
                    return string.Format("{0} (no-locale) | {1}", key, TextID); // OnError, just return the problematic TextID.
                }

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
        /// <br>If no locales were registered, this function just returns <see cref="string.Empty"/>.</br>
        /// </summary>
        public string GetCurrentLocaleString()
        {
            if (m_LocaleDatas.Count <= 0)
            {
                // since no locales exist at this state, return empty
                return string.Empty;
            }

            string locale = CurrentISOLocaleName;
            if (ContainsLocale(locale))
            {
                return this[locale];
            }

            if (ContainsLocale(DefaultLocale))
            {
                return this[DefaultLocale];
            }

            // Return the first in values
            Debug.LogWarning(string.Format("[LocalizedTextData::GetCurrentLocaleString] No fallback locale found with iso code '{0}'. Returning first element.", DefaultLocale));
            return m_LocaleDatas.Values.FirstOrDefault(string.Empty);
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

        /// <summary>
        /// Count of the current locales.
        /// </summary>
        public int Count => m_LocaleDatas.Count;

        public bool IsReadOnly => m_LocaleDatas.IsReadOnly;

        /// <summary>
        /// Adds a locale value.
        /// </summary>
        /// <param name="item">Key + Value to add. This is recommended to have a Key as a valid <see cref="System.Globalization.CultureInfo.TwoLetterISOLanguageName"/>.</param>
        /// <exception cref="ArgumentException"></exception>
        public void Add(KeyValuePair<string, string> item)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                throw new ArgumentException("[LocalizedTextData::Add] Given 'item.Key' is invalid. 'item.Key' cannot be null or whitespace.", nameof(item.Key));
            }

            m_LocaleDatas.Add(item);
        }
        /// <summary>
        /// Adds a locale value.
        /// </summary>
        /// <param name="localeKey">Key to add. This is recommended to be a valid <see cref="System.Globalization.CultureInfo.TwoLetterISOLanguageName"/>.</param>
        /// <param name="localeValue">Value to correspond to. This can be anything.</param>
        /// <exception cref="ArgumentException"></exception>
        public void Add(string localeKey, string localeValue)
        {
            if (string.IsNullOrWhiteSpace(localeKey))
            {
                throw new ArgumentException("[LocalizedTextData::Add] Given 'localeKey' is invalid. 'localeKey' cannot be null or whitespace.", nameof(localeKey));
            }

            m_LocaleDatas.Add(localeKey, localeValue);
        }
        /// <summary>
        /// Clears the localized text data list.
        /// </summary>
        public void Clear()
        {
            m_LocaleDatas.Clear();
        }
        /// <summary>
        /// Whether if this LocalizedTextData contains the given <paramref name="item"/>.
        /// <br>This method, while it returns a valid value, is not really meant to be used.</br>
        /// </summary>
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            return m_LocaleDatas.Contains(item);
        }
        /// <summary>
        /// Copies the internal dictionary to given <paramref name="array"/>.
        /// </summary>
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            m_LocaleDatas.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Remove an <paramref name="item"/> that matches.
        /// <br>This method, while it returns a valid value, is not really meant to be used.</br>
        /// </summary>
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            return m_LocaleDatas.Remove(item);
        }
        /// <summary>
        /// Removes a locale with <paramref name="key"/> from this data.
        /// </summary>
        /// <returns>Whether if the <paramref name="key"/> existed and data was removed.</returns>
        public bool RemoveKey(string key)
        {
            return m_LocaleDatas.Remove(key);
        }

        // -- Operator / Class
        /// <summary>
        /// Creates an completely empty data.
        /// </summary>
        public LocalizedTextData()
        { }
        /// <summary>
        /// Creates a <see cref="LocalizedTextData"/> with <see cref="TextID"/> assigned to given <paramref name="textID"/>.
        /// </summary>
        public LocalizedTextData(string textID)
        {
            TextID = textID;
        }
        /// <summary>
        /// Creates a LocalizedTextData using a <see cref="IDictionary{TKey, TValue}"/>.
        /// <br>(without id, use for code based locale)</br>
        /// </summary>
        public LocalizedTextData(IDictionary<string, string> values) 
            : this(new SerializableDictionary<string, string>(values))
        { }
        /// <summary>
        /// Creates the 'LocalizedTextData' with the <see cref="IDictionary{TKey, TValue}"/> type instead of a <see cref="SerializableDictionary{TKey, TValue}"/> type.
        /// </summary>
        /// <param name="textID">ID of the text.</param>
        /// <param name="values">Values of the dictionary. (Key=Two letter iso lang, Value=Corresponding string)</param>
        public LocalizedTextData(string textID, IDictionary<string, string> values) 
            : this(textID, new SerializableDictionary<string, string>(values))
        { }

        /// <summary>
        /// Creates an new <see cref="LocalizedTextData"/> object. (without an id, use for inline localization)
        /// </summary>
        /// <param name="values">Dictionary Data => Locale = Key | Content = Value</param>
        public LocalizedTextData(SerializableDictionary<string, string> values) 
            : this(string.Empty, values)
        { }
        /// <summary>
        /// Creates the full <see cref="LocalizedTextData"/> with every variable settable as is.
        /// <br>This is mostly used for <see cref="LocalizedText"/>s on the scene that import a 'LocalizedTextAsset', which require an ID appended into it.</br>
        /// </summary>
        /// <param name="textID">ID appended to this <see cref="LocalizedTextData"/>. This is not needed unless you are thinking of filtering out a list of LocalizedTextData.</param>
        /// <param name="values">The values added to the data. (Key=Two[or three] Letter ISO identifier for the language, Value=The value to use when matched)</param>
        public LocalizedTextData(string textID, SerializableDictionary<string, string> values)
        {
            TextID            = textID;
            m_LocaleDatas     = values;
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
            if (lhs is null)
            {
                return rhs is null;
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
            {
                return false;
            }

            // Pragma settings can be different, idc.
            return TextID.Equals(other.TextID, StringComparison.Ordinal) && 
                m_LocaleDatas.SequenceEqual(other.m_LocaleDatas);
        }

        public override int GetHashCode()
        {
            int hashCode = -2018306565;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(TextID);
            hashCode = (hashCode * -1521134295) + EqualityComparer<SerializableDictionary<string, string>>.Default.GetHashCode(m_LocaleDatas);
            return hashCode;
        }
    }
}
