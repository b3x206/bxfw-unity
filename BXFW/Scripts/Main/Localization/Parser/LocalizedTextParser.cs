using System;
using System.Text;
using System.Collections.Generic;

using UnityEngine;

namespace BXFW.Data
{
    /// <summary>
    /// <br><b>Use this parser only to convert the <see cref="TextAsset"/>s to the <see cref="LocalizedTextListAsset"/>.</b></br>
    /// <br>This is just left for the <c>ConvertLocalizedTextDataToAsset</c> converter.</br>
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// ; Here's how the data type looks like
    /// ; Lines starting with ';' are completely ignored.
    /// ; NOTE : This only applies if lines start with ';'. You can't put ';' after the start to comment out parts of the locale file.
    /// ; Save your locale file as utf8, this file is ansi so it uses ansi replacements (that don't break utf8).
    /// 
    /// ; Pragma definitions have a key and a value
    /// ; And these definitions can take anything
    /// #pragma SomethingWithValue "Anything really"
    /// 
    /// TEXT1_ID => en="Text Content", tr="Yazi icerik"
    /// TEXT2_ID => en="Other Text Content", tr="Diger yazi icerigi"
    /// TEXT3_ID => en="More Text Content", tr="Daha fazla yazi icerigi"
    /// ]]>
    /// </example>
    public static class LocalizedTextParser
    {
        /// <summary>
        /// Exception that occurs when the parsing fails.
        /// </summary>
        public class ParseException : Exception
        {
            public ParseException() : base()
            { }

            public ParseException(string msg) : base(msg)
            { }

            public ParseException(string msg, Exception innerException) : base(msg, innerException)
            { }
        }
        
        // inside 'SurroundChar' tokens
        private const char NewLineChar = '\n';
        private const char SpaceChar = ' ';
        private const char TabChar = '\t';
        private const char VerticalTabChar = '\v';
        private const char EscapeChar = '\\';
        // tokens
        private const char CommentChar = ';';
        private const char LocaleDefSeperateChar = ',';
        private const char SurroundChar = '"';
        private const char LocaleDefChar = '=';
        private const char PragmaDefChar = '#';
        private const string PragmaDefString = "pragma";
        private const string TextIDDefChar = "=>";

        /// <summary>
        /// Utility function for creating a <see cref="Dictionary{TKey, TValue}"/> from <paramref name="listKeys"/> and <paramref name="listValues"/>.
        /// </summary>
        private static Dictionary<TKey, TValue> CreateDictFromLists<TKey, TValue>(List<TKey> listKeys, List<TValue> listValues)
        {
            if (listKeys.Count < listValues.Count)
            {
                throw new ArgumentException(string.Format("[LocalizedAssetParser::CreateDictFromLists] Key list has less elements than value list. It should be equal or less. K:{0} < V:{1}", listKeys.Count, listValues.Count));
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
        /// Reads string until it's no longer whitespace and returns the index of non-whitespace char.
        /// <br>Returns -1 if all of the string is whitespace.</br>
        /// </summary>
        private static int IndexOfNonWhitespace(this string target)
        {
            int index = 0;
            foreach (char c in target)
            {
                if (char.IsWhiteSpace(c))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            return index != target.Length ? index : -1;
        }

        /// <summary>
        /// Parses the text into a list of localized text.
        /// </summary>
        /// <param name="parseString">The string to parse.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parseString"/> is null.</exception>
        /// <exception cref="ParseException">Thrown when any parse related error occurs.</exception>
        public static List<LocalizedTextData> Parse(string parseString, out Dictionary<string, string> globalPragmaSettings)
        {
            if (string.IsNullOrWhiteSpace(parseString))
            {
                throw new ArgumentNullException("[LocalizedAssetParser::Parse] Error while parsing : argument named 'parseString' passed is null.");
            }

            // Split lines and prepare list of text code representations.
            string[] parseStringSplit = parseString.Split(NewLineChar);
            var currentAsset = new List<LocalizedTextData>();
            globalPragmaSettings = new Dictionary<string, string>();

            // Iterate all lines
            for (int i = 0; i < parseStringSplit.Length; i++)
            {
                string line = parseStringSplit[i];

                // Ignore blank lines.
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // -- Pragma + comment lines
                // NOTE : Only comment lines starting with ';' is ignored.
                {
                    int trimCount = line.IndexOfNonWhitespace();    // index does not contain the rest of the line
                    string trimmedLine = line.Substring(trimCount); // because of this, this does contain rest of line

                    if (trimmedLine.StartsWith(CommentChar.ToString()))
                    {
                        continue;
                    }

                    // Check if pragma line
                    if (trimmedLine.StartsWith(PragmaDefChar.ToString()))
                    {
                        int indexOfPragmaDef = line.IndexOf(PragmaDefString);
                        if (indexOfPragmaDef != -1)
                        {
                            // Push pragma and continue
                            string splitString = line.Substring(indexOfPragmaDef + PragmaDefString.Length + 1);
                            string[] pragmaKeyValue = splitString.Split(new char[] { SpaceChar, VerticalTabChar, TabChar }, StringSplitOptions.RemoveEmptyEntries);
                            
                            if (pragmaKeyValue.Length != 2)
                            {
                                throw new ParseException(string.Format(
                                    "[LocalizationDictionaryParser::Parse] Error on '{0}:{1}' : Invalid pragma seperation. Expected length was 2, got '{2}'.", 
                                    i + 1, indexOfPragmaDef + PragmaDefString.Length, pragmaKeyValue.Length
                                ));
                            }

                            globalPragmaSettings.Add(pragmaKeyValue[0], pragmaKeyValue[1]);
                        }
                        else
                        {
                            throw new ParseException(string.Format("[LocalizationDictionaryParser::Parse] Error on '{0}:{1}' : Invalid pragma definition / char.", i + 1, trimCount + 1));
                        }

                        continue;
                    }
                }

                // -- Main Parsing
                // Split string BEFORE '=>' char sequence (get id)
                // Omit '=>'
                string textID = line.Substring(0, line.IndexOf(TextIDDefChar)).Trim();
                // Split string AFTER '=>' char sequence (omit char sequence)
                string idLocaleDefs = line.Substring(line.IndexOf(TextIDDefChar) + TextIDDefChar.Length + 1).Trim();

                // Definitions split ([0]='en="bla bla"' [1]='tr="bla bla"')
                string[] localeDefsSplit = idLocaleDefs.Split(LocaleDefSeperateChar, StringSplitOptions.RemoveEmptyEntries);

                // Parsed data (pass length for optimization)
                var listLocaleLang = new List<string>(localeDefsSplit.Length); // Parsed language definitions     : 'en, tr, de, ...'
                var listLocaleData = new List<string>(localeDefsSplit.Length); // Parsed language definition data : '"bla bla", "seyler", ...'

                foreach (string localeKeyValueString in localeDefsSplit)
                {
                    // No need to check this as split is now set to remove empty entries.
                    //if (string.IsNullOrWhiteSpace(localeKeyValueString))
                    //    continue;

                    // There should be exactly 2 values (on substring)
                    // Use consistent index of & trim from space
                    // Key   = [0 : index of locale def character]
                    string localeKey = localeKeyValueString.Substring(0, localeKeyValueString.IndexOf(LocaleDefChar)).Trim();
                    // Value = [index of locale def character : end]
                    string localeValue = localeKeyValueString.Substring(localeKeyValueString.IndexOf(LocaleDefChar) + 1).Trim();

                    listLocaleLang.Add(localeKey); // Add the locale definition

                    // -- Add listLocaleData -- //
                    // Parse the text covered in "" marks. (note: only parse strings in quotation marks)
                    // This is because we parse assuming that the escape characters are written in code as is.
                    // Basically if we parse a plain text file '\' has no meaning.

                    if (!localeValue.Contains(SurroundChar))
                    {
                        throw new ParseException(string.Format("[LocalizedAssetParser::Parse] Parsed line {0} with key \"{1}\" is not valid. [Not surrounded in \"'s properly]. Line Parsed : \n{2}", i + 1, textID, line));
                    }

                    // -- Reading the localeData contained in quotation --
                    // We can ignore comment char + pragma char for now.
                    // Parse the string contained in quotations
                    StringBuilder localeValueParsed = new StringBuilder(localeValue.Length);
                    // Handle these chars better (For now this will do).
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
                                continue;
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
                            //throw new ParseException("[LocalizationDictionaryParser::Parse] Invalid escape character {0}. (in line {1})");
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
                        Debug.LogWarning(string.Format("[LocalizedAssetParser::Parse] Warning while parsing : No locale / data was found while searching (parse will continue). String parsed :\n{0}", line));
                        continue;
                    }

                    currentAsset.Add(new LocalizedTextData(textID, CreateDictFromLists(listLocaleLang, listLocaleData)));
                }
            }

            // Return created list.
            return currentAsset;
        }
        /// <inheritdoc cref="Parse(string, out Dictionary{string, string})"/>
        public static List<LocalizedTextData> Parse(string parseString)
        {
            return Parse(parseString, out Dictionary<string, string> _);
        }
    }
}
