using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using BXFW;
using System.Text;
using UnityEngine.Assertions;
using System.Linq;

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
        public static explicit operator string(LocalizedText text)
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
    /// TEXT_ID => en="Text Content", tr="Yazý içerik"
    /// TEXT2_ID => en="Other Text Content", tr="Diðer yazý içeriði"
    /// TEXT3_ID => en="More Text Content", tr="Daha fazla yazý içeriði"
    /// </example>
    public static class LocalizedAssetParser
    {
        public const char NewLineChar = '\n';

        public const char LocaleDefSeperateChar = ',';
        public const char EscapeChar = '\\';
        public const char SurroundChar = '"';
        public const char LocaleDefChar = '=';
        public const string TextIDChar = "=>";

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}"/> from <paramref name="listKeys"/> & <paramref name="listValues"/>.
        /// </summary>
        private static Dictionary<TKey, TValue> CreateDictFromLists<TKey, TValue>(List<TKey> listKeys, List<TValue> listValues)
        {
            // Make sure the assertion is correct.
            Assert.IsTrue(listKeys.Count >= listValues.Count);

            var dict = new Dictionary<TKey, TValue>(listKeys.Count);

            for (int i = 0; i < listKeys.Count; i++)
            {
                var valueAdd = (TValue)default;
                if (i < listValues.Count)
                {
                    // If we have a corresponding value, add that.
                    valueAdd = listValues[i];
                }

                dict.Add(listKeys[i], valueAdd);
            }

            return dict;
        }
        /// <summary>
        /// Converts the strings with quotations to have escape characters in it. (for the parse file)
        /// </summary>
        private static string ConvertQuotationString(this string target)
        {
            var newStr = new StringBuilder(target.Length);

            foreach (var c in target)
            {
                // TODO : Make a list of 'EscapableChars' (maybe again)
                if (c == SurroundChar)
                {
                    // Add this instead.
                    newStr.Append("\\\"");
                    continue;
                }
                if (c == EscapeChar)
                {
                    newStr.Append("\\\\");
                    continue;
                }

                newStr.Append(c);
            }

            return newStr.ToString();
        }

        public static List<LocalizedText> Parse(string fromString)
        {
            var parseLn = fromString.Split(NewLineChar);
            var currentAsset = new List<LocalizedText>();

            foreach (var line in parseLn)
            {
                // Ignore blank lines.
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Get identification of the text registries (TEXT_ID)
                var TextID = line.Substring(0, line.IndexOf(TextIDChar)).Trim();
                var LocaleDefStringLine = line.Substring(line.LastIndexOf(TextIDChar) + TextIDChar.Length).Trim(' ');
                var LocaleDefsSplit = LocaleDefStringLine.Split(LocaleDefSeperateChar);

                var listLocaleLang = new List<string>();
                var listLocaleData = new List<string>();

                foreach (var localeKeyValueNonSplit in LocaleDefsSplit)
                {
                    if (string.IsNullOrWhiteSpace(localeKeyValueNonSplit))
                        continue;

                    var localeKeyValue = localeKeyValueNonSplit.Split(LocaleDefChar);

                    // Make a dictionary, using localeKeyValue
                    foreach (var keyValue in localeKeyValue)
                    {
                        bool isValue = keyValue.Contains(SurroundChar);

                        if (!isValue)
                        {
                            // We only care about the 'KeyValue' contains, the values can be the same.
                            if (!listLocaleLang.Contains(keyValue))
                            {
                                listLocaleLang.Add(keyValue.Trim());
                            }
                            else
                            {
                                Debug.LogError($"[LocalizedAssetParser::Parse] Error while parsing : Locale {localeKeyValue} already exists on ID {TextID}. Line parsed :\n{line}");
                                listLocaleLang = null;
                                listLocaleData = null;
                                break;
                            }
                        }
                        else // isValue
                        {
                            // -- Add listLocaleData -- //
                            // Parse the text covered in "" marks.
                            // This is because we parse assuming that the escape characters are written in code as is.
                            // Basically if we parse a plain text file '\' has no meaning.
                            // Maybe TODO : Make a escape fixer method for strings.
                            var localeKeyValueConverted = keyValue.ConvertQuotationString();

                            // Parse the string.
                            var currDataStr = string.Empty;
                            var encSurroundChar = false;
                            var currDataStrIsEscapeChar = false;

                            foreach (var currDataChar in localeKeyValueConverted)
                            {
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

                                // Surrounding character (either denotes the start or the end)
                                if (currDataChar == SurroundChar)
                                {
                                    // Ending of string if we are not in an escape char.
                                    if (currDataStrIsEscapeChar)
                                    {
                                        currDataStr += currDataChar;
                                        currDataStrIsEscapeChar = false;
                                        continue;
                                    }

                                    // String is null. We 'most likely' still have text to parse
                                    if (string.IsNullOrEmpty(currDataStr) && !encSurroundChar)
                                    {
                                        // This means that we encountered an escapeless Surround character.
                                        encSurroundChar = true;
                                        continue;
                                    }

                                    // End the string, both conditions are not satisfied.
                                    break;
                                }

                                if (currDataStrIsEscapeChar)
                                {
                                    // Ignore the 'Invalidly Escaped' character.
                                    currDataStrIsEscapeChar = false;
                                }

                                currDataStr += currDataChar;
                            }

                            // If the string is still null, accept as is.
                            // Add to the list of locales anyway.
                            listLocaleData.Add(currDataStr);
                        }
                    }

                    // Lists are complete for this line.
                    if (listLocaleLang != null && listLocaleData != null)
                    {
                        if (listLocaleLang.Count == 0 || listLocaleData.Count == 0)
                        {
                            Debug.LogError($"[LocalizedAssetParser::Parse] Error while parsing : No locale / data was found while searching. String parsed :\n{line}");
                            continue;
                        }

                        currentAsset.Add(new LocalizedText(TextID, CreateDictFromLists(listLocaleLang, listLocaleData)));
                    }


                    //    // -- Add listLocaleData -- //
                    //    // Parse the text covered in "" marks.
                    //    // This is because we parse assuming that the escape characters are written in code as is.
                    //    // Basically if we parse a plain text file '\' has no meaning.
                    //    // Maybe TODO : Make a escape fixer method for strings.
                    //    var localeKeyValueConverted = localeKeyValue[1].ConvertQuotationString();

                    //    // Parse the string.
                    //    var currDataStr = string.Empty;
                    //    var encSurroundChar = false;
                    //    var currDataStrIsEscapeChar = false;

                    //    foreach (var currDataChar in localeKeyValue[1])
                    //    {
                    //        // Escape char (not the SurrondChar)
                    //        if (currDataChar == EscapeChar)
                    //        {
                    //            if (currDataStrIsEscapeChar)
                    //            {
                    //                // Escape char is written as '\\'.
                    //                currDataStr += currDataChar;
                    //                currDataStrIsEscapeChar = false;
                    //            }

                    //            currDataStrIsEscapeChar = true;
                    //            continue;
                    //        }

                    //        // Surrounding character (either denotes the start or the end)
                    //        if (currDataChar == SurroundChar)
                    //        {
                    //            // Ending of string if we are not in an escape char.
                    //            if (currDataStrIsEscapeChar)
                    //            {
                    //                currDataStr += currDataChar;
                    //                currDataStrIsEscapeChar = false;
                    //                continue;
                    //            }

                    //            // String is null. We 'most likely' still have text to parse
                    //            if (string.IsNullOrEmpty(currDataStr) && !encSurroundChar)
                    //            {
                    //                // This means that we encountered an escapeless Surround character.
                    //                encSurroundChar = true;
                    //                continue;
                    //            }

                    //            // End the string, both conditions are not satisfied.
                    //            break;
                    //        }

                    //        if (currDataStrIsEscapeChar)
                    //        {
                    //            // Ignore the 'Invalidly Escaped' character.
                    //            currDataStrIsEscapeChar = false;
                    //        }

                    //        currDataStr += currDataChar;
                    //    }

                    //    // If the string is still null, accept as is.
                    //    // Add to the list of locales anyway.
                    //    listLocaleLang.Add(currDataStr);
                    //}
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

                for (int i = 0; i < text.Data.Count; i++)
                {
                    var textWLocaleKey = text.Data.Keys.ToArray()[i];
                    var textWLocaleValue = text.Data.Values.ToArray()[i];

                    // Append the dictionary data
                    // Translates (roughly, the value is subject to change) to | TEXT_ID => localeValue = "whatever here is there.", (comma added if other elements exist)
                    sb.Append($"{textWLocaleKey} {LocaleDefChar} {SurroundChar}{textWLocaleValue.ConvertQuotationString()}{SurroundChar}{(i != assets.Count - 1 ? LocaleDefSeperateChar : ' ')} ");
                }

                // Create a new line
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}