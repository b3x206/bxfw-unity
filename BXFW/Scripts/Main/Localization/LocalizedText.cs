﻿using BXFW.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Shows a statically localized text (with current locale).
    /// </summary>
    public class LocalizedText : MonoBehaviour, IEnumerable<LocalizedTextData>
    {
        [Header(":: Reference")]
        public TMP_Text target;

        [Header(":: Settings")]
        public LocalizedTextListAsset localeData;
        public string textID;
        /// <summary>
        /// Locale to spoof.
        /// <br>Only works in editor.</br>
        /// </summary>
        [SerializeField] internal string spoofLocale;
        internal IList<LocalizedTextData> TextData => localeData;

        /// <summary>
        /// The currently selected data.
        /// <br>Depending on the <see cref="textID"/>, this selects something.</br>
        /// </summary>
        public LocalizedTextData CurrentSelectedData => TextData.FirstOrDefault(data => data.TextID == textID);

        /// <summary>
        /// Locale file pragma definition to replace tmp chars that doesn't exist.
        /// (Removes diacritics from string)
        /// </summary>
        public const string ReplaceTMPCharsPragma = "ReplaceTMPInvalidChars";

        private void OnValidate()
        {
            TryGetComponent(out target);

            Apply(false, false);
        }
        private void Start()
        {
            Apply();
        }

        /// <summary>
        /// Utility method to remove diacritics (sans some non-ascii non-diacritic characters) from the string.
        /// </summary>
        /// <param name="str">Target string.</param>
        /// <param name="predMustKeep">
        /// Return true if the given character parameter 
        /// should be kept as is (from source string <paramref name="str"/>).
        /// </param>
        public string RemoveDiacritics(string str, Func<char, bool> predMustKeep = null)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            string normalizedStr = str.Normalize(NormalizationForm.FormD);

            for (int i = 0, j = 0; i < normalizedStr.Length; i++, j++)
            {
                char cNorm = normalizedStr[i];
                
                // nonspacing mark : the diacritic itself, just as a seperate char
                if (CharUnicodeInfo.GetUnicodeCategory(cNorm) != UnicodeCategory.NonSpacingMark)
                {
                    // Access 'cDefault' here to avoid IndexOutOfRangeException becuase incrementing-decrementing
                    // the base string index happens after this if control (basically we need the safe 'j' index)
                    char cDefault = str[j];

                    // Keep if the predicate is successful
                    bool? pred = predMustKeep?.Invoke(cDefault);
                    if (pred ?? false)
                    {
                        sb.Append(cDefault);
                        continue;
                    }
                    else if (pred != null)
                    {
                        // Predicate failed but exists, convert directly to ascii counterpart of it assuming the character is latin.
                        // TODO : but how?
                        switch (cNorm)
                        {
                            default:
                                // This warning is redundant for diacriticified chars that fail the predicate test.
                                // so just control the cNorm as that is successfully seperated from it's diacritics.
                                
                                // i also decided to not print the warning.
                                // Debug.LogWarning(string.Format("[LocalizedText::RemoveDiacritics] Failed to convert fail match char '{0}' into ascii. Appending normalized as is.", cDefault));
                                break;

                            // This is actually diacriticifying (the thing that is called a 'diacritic' is the points on top the letters
                            // This is more like 'ascii-ifying' the text.
                            // Only latin case is this. this will do for now.
                            case 'ı':
                                cNorm = 'i';
                                break;
                        }
                    }

                    sb.Append(cNorm);
                }
                else
                {
                    // nonspacing marks doesn't exist on the actual string.
                    j--;
                }
            }

            return sb.ToString();
        }

        private void ApplyInternal(bool setText, bool logErrors, params object[] fmt)
        {
            if (target == null)
            {
                if (logErrors)
                {
                    Debug.LogError(string.Format("[LocalizedText::Apply] Text on {0} doesn't have target.", this.GetPath()));
                }

                return;
            }

            if (localeData == null)
            {
                if (logErrors)
                {
                    Debug.LogWarning(string.Format("[LocalizedText::Apply] Text on {0} doesn't have a 'localeData' field assigned.", this.GetPath()));
                }

                return;
            }

            // Compare TextID as ordinally, we just need an exact string match.
            LocalizedTextData data = localeData.SingleOrDefault(d => d.TextID.Equals(textID, StringComparison.Ordinal));
            bool replaceInvalidChars = false;
            {
                // unity doesn't compile 'out string v'
                string v = string.Empty;
                // will throw an exception if the pragma value is invalid.
                if (localeData.pragmaDefinitons.TryGetValue(ReplaceTMPCharsPragma, out v))
                {
                    replaceInvalidChars = bool.Parse(v);
                }
            }

            if (data == null)
            {
                if (logErrors)
                {
                    Debug.LogError(string.Format("[LocalizedText::Apply] Text on {0} has invalid id '{1}'.", this.GetPath(), textID));
                }

                return;
            }

            if (setText)
            {
                string setData = string.Empty;
#if UNITY_EDITOR
                if (data.ContainsLocale(spoofLocale))
                {
                    setData = data[spoofLocale];
                }
                else
                {
                    setData = data.GetCurrentLocaleString();
                }
#else
                setData = data.GetCurrentLocaleString();
#endif
                if (replaceInvalidChars)
                {
                    setData = RemoveDiacritics(setData, (char c) => target.font.HasCharacter(c));
                }

                if (fmt.Length > 0)
                { 
                    setData = string.Format(setData, fmt);
                }

                target.SetText(setData);
            }
        }

        public void Apply(bool setText = true, bool logErrors = true)
        {
            ApplyInternal(setText, logErrors);
        }
        public void ApplyFormatted(params object[] formatArgs)
        {
            ApplyInternal(true, true, formatArgs);
        }

        public IEnumerator<LocalizedTextData> GetEnumerator()
        {
            return localeData.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
