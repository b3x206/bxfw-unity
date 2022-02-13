using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using BXFW;
using System.Text;
using UnityEngine.Assertions;

namespace BXFW.Data
{
    [Serializable]
    public class LocalizedText
    {
        /// TODO : Put <see cref="DefaultLocale"/> to a different place.
        public static string DefaultLocale = "en";
        public string TextID;
        private Dictionary<string, string> LocalizedValues = new Dictionary<string, string>();
        public IDictionary<string, string> Data
        {
            get
            {
                return LocalizedValues;
            }
        }

        public string this[string key]
        {
            get
            {
                if (!LocalizedValues.TryGetValue(key, out string value))
                    return $"{key} (no-locale) | {TextID}"; // OnError, just return the problematic TextID.

                return value;
            }
        }

        // -- Operator / Class
        public LocalizedText() 
        { }
        /// <summary>
        /// Creates an new <see cref="LocalizedText"/> object.
        /// </summary>
        /// <param name="textID">ID of the localized text.</param>
        /// <param name="values">Dictionary Data => Locale = Key | Content = Value</param>
        public LocalizedText(string textID, Dictionary<string, string> values)
        {
            TextID = textID;
            LocalizedValues = values;
        }
        public static implicit operator string(LocalizedText text)
        {
            // Whether if we want this to be treated as an string.
            return text[DefaultLocale];
        }
    }

    /// <summary>
    /// Parses localized text asset.
    /// </summary>
    /// Here's how the data type looks like
    /// <example>
    /// TEXT_ID => en="Text Content" tr="Yazý içerik"
    /// TEXT2_ID => en="Other Text Content" tr="Diðer yazý içeriði"
    /// TEXT3_ID => en="More Text Content" tr="Daha fazla yazý içeriði"
    /// </example>
    public static class LocalizedAssetParser
    {
        public const char NewLineChar = '\n';

        public const char EscapeChar = '\\';
        public const char SurroundChar = '"';
        public const char LocaleDefChar = '=';
        public const string TextIDChar = "=>";

        private static Dictionary<TKey, TValue> CreateDictFromLists<TKey, TValue>(List<TKey> listKeys, List<TValue> listValues)
        {
            // Make sure the assertion is correct.
            Debug.Log($"{listKeys.Count} == {listValues.Count}");
            Assert.IsTrue(listKeys.Count == listValues.Count);

            var dict = new Dictionary<TKey, TValue>(listKeys.Count);

            for (int i = 0; i < listKeys.Count; i++)
            {
                dict.Add(listKeys[i], listValues[i]);
            }

            return dict;
        }
        /// <summary>
        /// Converts the strings with quotations to have escape characters in it.
        /// </summary>
        private static string ConvertQuotationString(this string target)
        {
            var newStr = string.Empty;

            foreach (var c in target)
            {
                // TODO : Make a list of 'EscapableChars' (maybe again=
                if (c == SurroundChar)
                {
                    // Add this instead.
                    newStr += "\\\"";
                    continue;
                }

                newStr += c;
            }

            return newStr;
        }

        public static List<LocalizedText> Parse(string parse)
        {
            var parseLn = parse.Split(NewLineChar);
            var currentAsset = new List<LocalizedText>();

            foreach (var line in parseLn)
            {
                // Get identification of the text registries (TEXT_ID)
                var TextID = line.Substring(0, line.IndexOf(TextIDChar)).Trim();
                var LocaleDefsSplit = line.Substring(line.IndexOf(TextIDChar), line.Length - line.IndexOf(TextIDChar)).Trim().Split(LocaleDefChar);

                var listLocaleLang = new List<string>();
                var listLocaleData = new List<string>();

                foreach (var localeKeyValue in LocaleDefsSplit)
                {
                    // Save the key first
                    if (listLocaleLang.Count == listLocaleData.Count)
                    {
                        // We only care about the 'KeyValue' contains, the values can be the same.
                        if (!listLocaleData.Contains(localeKeyValue))
                        {
                            listLocaleLang.Add(localeKeyValue.Trim());
                        }
                        else
                        {
                            Debug.LogError($"[LocalizedAssetParser::Parse] Error while parsing : Locale {localeKeyValue} already exists on ID {TextID}. String parsed :\n{line}");
                            listLocaleLang = null;
                            listLocaleData = null;
                            break;
                        }
                    }
                    else
                    {
                        // Parse the text covered in "" marks.
                        // This is because we parse assuming that the escape characters are written in code as is.
                        // Basically if we parse a plain text file '\' has no meaning.
                        // Maybe TODO : Make a escape fixer method for strings.
                        var localeKeyValueConverted = localeKeyValue.ConvertQuotationString(); 

                        var currDataStr = string.Empty;
                        var currDataStrIsEscapeChar = false;

                        foreach (var currDataChar in localeKeyValueConverted)
                        {
                            // Surrounding character (either denotes the start or the end)
                            if (currDataChar == SurroundChar)
                            {
                                // String is null. We 'most likely' still have text to parse
                                if (string.IsNullOrEmpty(currDataStr))
                                    continue;

                                // Ending of string if we are not in an escape char.
                                if (currDataStrIsEscapeChar)
                                {
                                    currDataStr += currDataChar;
                                    currDataStrIsEscapeChar = false;
                                    continue;
                                }

                                // End the string, both conditions are not satisfied.
                                break;
                            }

                            // Escape char (not the SurrondChar)
                            if (currDataChar == EscapeChar)
                            {
                                if (currDataStrIsEscapeChar)
                                {
                                    // Escape char is written as '\\'.
                                    currDataStr += currDataChar;
                                    currDataStrIsEscapeChar = false;
                                }

                                currDataStrIsEscapeChar = true;
                                continue;
                            }

                            if (currDataStrIsEscapeChar)
                            {
                                // Ignore the 'Invalidly Escaped' character.
                                currDataStrIsEscapeChar = false;
                            }

                            currDataStr += currDataChar;
                        }

                        // If the string is still null, accept as is.
                        listLocaleLang.Add(currDataStr);
                    }

                    if (listLocaleLang != null && listLocaleData != null)
                    {
                        if (listLocaleLang.Count != 0 || listLocaleData.Count != 0)
                        {
                            Debug.LogError($"[LocalizedAssetParser::Parse] Error while parsing : No locale's was found while searching. String parsed :\n{line}]");
                            continue;
                        }

                        currentAsset.Add(new LocalizedText(TextID, CreateDictFromLists(listLocaleLang, listLocaleData)));
                    }
                }
            }

            return currentAsset;
        }

        /// <summary>
        /// Saves the list.
        /// </summary>
        /// <returns>The resulting string to be written.</returns>
        public static string Save(List<LocalizedText> assets)
        {
            if (assets == null)
                throw new ArgumentNullException($"[LocalizedAssetParser::Save] Error while saving : argument named {nameof(assets)} passed is null.");
            if (assets.Count <= 0)
                return string.Empty;

            // NOTE : This is still an inefficient way of making this.
            // But it will do for now.
            StringBuilder sb = new StringBuilder();

            foreach (var text in assets)
            {
                // Append the textID
                sb.Append(text.TextID);
                sb.Append($" {TextIDChar} ");

                foreach (var textWLocale in text.Data)
                {
                    // Append the dictionary data
                    // Translates (roughly, the value is subject to change) to | TEXT_ID => localeValue = "whatever here is there."
                    sb.Append($"{textWLocale.Key} {LocaleDefChar} {SurroundChar}{textWLocale.Value}{SurroundChar} ");
                }

                // Create a new line
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}