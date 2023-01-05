using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace BXFW.Data
{
    /// <summary>
    /// Parses localized text asset.
    /// <br>Note : This is 'Not' recommended to be used in this state, use an actual localization library.</br>
    /// <br>This is more of an 'experimental' thing.</br>
    /// </summary>
    /// Here's how the data type looks like
    /// <example>
    /// ; For some reason git does not correctly commit the turkish characters, save your locale file as utf8, this is probs ansi.
    /// TEXT_ID => en="Text Content", tr="Yazi icerik"
    /// TEXT2_ID => en="Other Text Content", tr="Diger yazi icerigi"
    /// TEXT3_ID => en="More Text Content", tr="Daha fazla yazi icerigi"
    /// </example>
    public static class LocalizedAssetParser
    {
        public const char NewLineChar = '\n';
        public const char TabChar = '\t';
        public const char EscapeChar = '\\';

        public const char CommentChar = ';';
        public const char LocaleDefSeperateChar = ',';
        public const char SurroundChar = '"';
        public const char LocaleDefChar = '=';
        public const string TextIDDefChar = "=>";

        /// <summary>
        /// Utility function for creating a <see cref="Dictionary{TKey, TValue}"/> from <paramref name="listKeys"/> and <paramref name="listValues"/>.
        /// </summary>
        private static Dictionary<TKey, TValue> CreateDictFromLists<TKey, TValue>(List<TKey> listKeys, List<TValue> listValues)
        {
            if (listKeys.Count < listValues.Count)
            {
                Debug.LogError(string.Format("[LocalizedAssetParser::CreateDictFromLists] Key list has less elements than value list. It should be equal or less. K:{0} < V:{1}", listKeys.Count, listValues.Count));
                return null;
            }

            var dict = new Dictionary<TKey, TValue>(listKeys.Count);

            for (int i = 0; i < listKeys.Count; i++)
            {
                // This method ignores no values in corresponding key. (by assigning a default value)
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
        /// Converts the strings with quotations to have escape characters in it. (for the parse saving file)
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
                if (c == NewLineChar)
                {
                    newStr.Append("\\n");
                    continue;
                }
                if (c == TabChar)
                {
                    newStr.Append("\\t");
                    continue;
                }

                newStr.Append(c);
            }

            return newStr.ToString();
        }

        /// <summary>
        /// Parses the text into a list of localized text.
        /// </summary>
        /// <param name="parseString">The string to parse.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parseString"/> is null.</exception>
        public static List<LocalizedTextData> Parse(string parseString)
        {
            if (string.IsNullOrWhiteSpace(parseString))
            {
                throw new ArgumentNullException("[LocalizedAssetParser::Parse] Error while parsing : argument named 'parseString' passed is null.");
            }

            // Split lines and prepare list of text code representations.
            var parseStringSplit = parseString.Split(NewLineChar);
            var currentAsset = new List<LocalizedTextData>();

            // Iterate all lines
            foreach (string line in parseStringSplit)
            {
                // Ignore blank lines.
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Ignore comment lines
                // NOTE : Only comment lines starting with ';' is ignored.
                if (line.TrimStart().StartsWith(CommentChar.ToString()))
                    continue;

                // Use consistent 'IndexOf' method : Basically only catch the first index of the def char substring.

                // Split string BEFORE '=>' char sequence (get id)
                // Omit '=>'
                string TextID = line.Substring(0, line.IndexOf(TextIDDefChar)).Trim();

                // Split string AFTER '=>' char sequence (omit char sequence)
                string LocaleDefStringLine = line.Substring(line.IndexOf(TextIDDefChar) + TextIDDefChar.Length + 1).Trim(/*' '*/);
                // Definitions split ([0]='en="bla bla"' [1]='tr="bla bla"')
                string[] LocaleDefsSplit = LocaleDefStringLine.Split(LocaleDefSeperateChar);

                // Parsed data (pass length for optimization)
                var listLocaleLang = new List<string>(LocaleDefsSplit.Length); // Parsed language definitions     : 'en, tr, de, ...'
                var listLocaleData = new List<string>(LocaleDefsSplit.Length); // Parsed language definition data : '"bla bla", "seyler", ...'

                foreach (string localeKeyValueString in LocaleDefsSplit)
                {
                    if (string.IsNullOrWhiteSpace(localeKeyValueString))
                    {
                        // log here in some sort of debug mode?
                        // not really necessary as whitespace is ignored.
                        continue;
                    }

                    // There should be exactly 2 values (on substring)
                    // Use consistent index of & trim from space

                    // Key : 0 -> length of key
                    string localeKey = localeKeyValueString.Substring(0, localeKeyValueString.IndexOf(LocaleDefChar)).Trim();
                    // Value: index of the first assign param -> end of string
                    string localeValue = localeKeyValueString.Substring(localeKeyValueString.IndexOf(LocaleDefChar) + 1).Trim();

                    // Use this to debug substring related incidents
                    // Debug.Log($"to parse : localeKey={localeKey} | localeValue={localeValue}");

                    listLocaleLang.Add(localeKey); // Add the locale definition

                    // -- Add listLocaleData -- //
                    // Parse the text covered in "" marks. (note: only parse strings in quotation marks)
                    // This is because we parse assuming that the escape characters are written in code as is.
                    // Basically if we parse a plain text file '\' has no meaning.

                    // Get conversion of the locale key (convert into escaped chars)
                    // This is not needed as we are parsing
                    // string localeKey = localeValue.ConvertQuotationString();

                    if (!localeValue.Contains(SurroundChar))
                    {
                        Debug.LogError(string.Format("[LocalizedAssetParser::Parse] Parsed line with key \"{0}\" is not valid. [Not surrounded in \"'s properly]. Line Parsed : \n{1}", TextID, line));
                        break;
                    }

                    // Parse the string.
                    StringBuilder localeValueParsed = new StringBuilder(localeValue.Length);
                    // TODO : Handle these chars better (For now this will do).
                    var localeValueParseBegin = false;
                    var currDataStrIsEscapeChar = false;

                    foreach (char dataChar in localeValue)
                    {
                        #region Escape Char
                        // Ignore this mess for now

                        // Escape char (not the SurrondChar)
                        if (dataChar == EscapeChar)
                        {
                            if (currDataStrIsEscapeChar)
                            {
                                // Escape char is written as '\\'.
                                localeValueParsed.Append(dataChar);
                                currDataStrIsEscapeChar = false;
                            }

                            currDataStrIsEscapeChar = true;
                            continue;
                        }
                        // Surrounding character (either denotes the start or the end)
                        if (dataChar == SurroundChar)
                        {
                            // Ending of string if we are not in an escape char.
                            if (currDataStrIsEscapeChar)
                            {
                                localeValueParsed.Append(dataChar);
                                currDataStrIsEscapeChar = false;
                                continue;
                            }

                            // String is null. We 'most likely' still have text to parse
                            if (localeValueParsed.Length <= 0 && !localeValueParseBegin)
                            {
                                // This means that we encountered an escapeless Surround character.
                                localeValueParseBegin = true;
                                continue;
                            }

                            // End the string, both conditions are not satisfied. (probably end of the char)
                            break;
                        }

                        // (not so) Invalidly escaped char (some stuff is done here, for invalid escape; ifs are defined here too)
                        // Basically this is spaghetti
                        if (currDataStrIsEscapeChar)
                        {
                            // If the current character is 'n' (or t, we ignore t and other chars for now), this is not invalidly escaped at all
                            // TODO : Make a delegate dict (with escape chars) for behaviour on escape chars
                            if (dataChar == 'n')
                            {
                                // don't call AppendLine, in windows it appends CRLF even though all files/datas are LF
                                localeValueParsed.Append(NewLineChar);
                                currDataStrIsEscapeChar = false;
                                continue;
                            }
                            if (dataChar == 't')
                            {
                                localeValueParsed.Append(TabChar);
                                currDataStrIsEscapeChar = false;
                                continue;
                            }

                            // This is done anyways because an escape sequence was already inserted.
                            // Ignore the 'Invalidly Escaped' character.
                            currDataStrIsEscapeChar = false;
                        }
                        #endregion

                        localeValueParsed.Append(dataChar);
                    }

                    // Add the parsed data
                    listLocaleData.Add(localeValueParsed.ToString());
                }

                // Lists are complete for this line.
                if (listLocaleLang != null && listLocaleData != null)
                {
                    if (listLocaleLang.Count == 0 || listLocaleData.Count == 0)
                    {
                        Debug.LogWarning(string.Format("[LocalizedAssetParser::Parse] Warning while parsing : No locale / data was found while searching (keeping search). String parsed :\n{0}", line));
                        continue;
                    }

                    currentAsset.Add(new LocalizedTextData(TextID, CreateDictFromLists(listLocaleLang, listLocaleData)));
                }
            }

            // Return created list.
            return currentAsset;
        }

        /// <summary>
        /// Saves the list.
        /// </summary>
        /// <returns>The resulting string to be written.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static string Save(List<LocalizedTextData> assets)
        {
            if (assets == null)
                throw new ArgumentNullException("[LocalizedAssetParser::Save] Error while saving : argument named 'assets' passed is null.");
            if (assets.Count <= 0)
            {
                Debug.LogWarning("[LocalizedAssetParser::Save] Given save list is empty.");
                return string.Empty;
            }

            // Save using the string builder.
            // Approximate capacity of the 'StringBuilder' is the length of the dictionary strings.
            int stringAllocSize = 0;
            assets.ForEach((LocalizedTextData t) => { stringAllocSize += t.StringSize; });
            StringBuilder sb = new StringBuilder(stringAllocSize);

            foreach (LocalizedTextData text in assets)
            {
                // Append the textID + TextIDChar define
                // It looks like : TEXT_ID => 
                sb.Append(string.Format("{0} {1} ", text.TextID, TextIDDefChar));

                for (int i = 0; i < text.Data.Count; i++)
                {
                    var textWLocaleKey = text.Data.Keys.ToArray()[i];
                    var textWLocaleValue = text.Data.Values.ToArray()[i];

                    // Append the dictionary data. 
                    // It looks like : localeValue="whatever here is there.", (comma added if other elements exist)
                    sb.Append(
                        // yes, inferred string (whatever the '$' thing before "" is called) is indeed nicer to work with
                        string.Format("{0}{1}{2}{3}{2}{4} ",
                            textWLocaleKey, LocaleDefChar, SurroundChar,        // Definition Char
                            textWLocaleValue.ConvertQuotationString(),          // Content
                            i != assets.Count - 1 ? LocaleDefSeperateChar : ' ' // Seperation comma (parse)
                        )
                    );
                }

                // Create a new line
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}