using BXFW.Data;
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
        public bool useSingletonLocaleData = false;
        /// <summary>
        /// The cached singleton value (as accessing the <see cref="ScriptableObjectSingleton{T}.Instance"/> causes a <see cref="Resources.LoadAll{T}(string)"/> call)
        /// </summary>
        private LocalizedTextListAsset m_previousSingletonLocaleData;
        /// <summary>
        /// The locale data collection that this localized text contains.
        /// </summary>
        public LocalizedTextListAsset LocaleData
        {
            get
            {
                if (useSingletonLocaleData)
                {
                    if (m_previousSingletonLocaleData == null)
                    {
                        m_previousSingletonLocaleData = LocalizedTextListAsset.Instance;
                    }
                    if (m_LocaleData != m_previousSingletonLocaleData)
                    {
                        m_LocaleData = m_previousSingletonLocaleData;
                    }
                }

                return m_LocaleData;
            }
            set
            {
                m_LocaleData = value;
            }
        }
        [SerializeField, DrawIf(nameof(useSingletonLocaleData), ConditionInverted = true)]
        private LocalizedTextListAsset m_LocaleData;
        public string textID;
        /// <summary>
        /// Locale to spoof.
        /// <br>Only works in editor.</br>
        /// </summary>
        [SerializeField]
        internal string spoofLocale;

        /// <summary>
        /// The currently selected data.
        /// <br>Depending on the <see cref="textID"/>, this selects something.</br>
        /// </summary>
        public LocalizedTextData CurrentSelectedData => LocaleData != null ? LocaleData.FirstOrDefault(data => data.TextID == textID) : null;

        /// <summary>
        /// Locale file pragma definition to replace tmp chars that doesn't exist.
        /// (Removes diacritics from string)
        /// </summary>
        public const string ReplaceTMPCharsPragma = "ReplaceTMPInvalidChars";

        private void OnValidate()
        {
            TryGetComponent(out target);

            // This accesses the singleton on the objects to apply the text id.
            // Which causes a warning output in the console (but why unity?)
            if (!useSingletonLocaleData)
            {
                Apply(false, false);
            }
        }
        private void Start()
        {
            Apply();
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

            if (LocaleData == null)
            {
                if (logErrors)
                {
                    Debug.LogWarning(string.Format("[LocalizedText::Apply] Text on {0} doesn't have a 'localeData' field assigned.", this.GetPath()));
                }

                return;
            }

            // Compare TextID as ordinally, we just need an exact string match.
            LocalizedTextData data = LocaleData.SingleOrDefault(d => d.TextID.Equals(textID, StringComparison.Ordinal));
            bool replaceInvalidChars = false;
            {
                // unity doesn't compile 'out string v'
                string v = string.Empty;
                // will throw an exception if the pragma value is invalid.
                if (LocaleData.pragmaDefinitons.TryGetValue(ReplaceTMPCharsPragma, out v))
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
                    setData = Additionals.RemoveInvalidChars(setData, (char c) => target.font.HasCharacter(c));
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
            return LocaleData.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
