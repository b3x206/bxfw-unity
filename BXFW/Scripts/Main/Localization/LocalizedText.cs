using BXFW.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Localizes a text (with current locale).
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [Header(":: Reference")]
        public TMP_Text target;

        [Header(":: Settings")]
        public TextAsset localeData;
        public string textID;
#if UNITY_EDITOR
        /// <summary>
        /// Locale to spoof.
        /// <br>Only works in editor.</br>
        /// </summary>
        [SerializeField] internal string spoofLocale;
#endif

        [NonSerialized] private List<LocalizedTextData> localeTextData;
        internal IList<LocalizedTextData> TextData
        {
            get
            {
                if (localeTextData != null)
                {
                    if (localeTextData.Count <= 0)
                    {
                        localeTextData = LocalizedAssetParser.Parse(localeData.text);
                    }
                }
                else
                {
                    localeTextData = LocalizedAssetParser.Parse(localeData.text);
                }

                return localeTextData;
            }
        }

        private void OnValidate()
        {
            TryGetComponent(out target);

            Apply(false, false);
        }
        private void Awake()
        {
            Apply();
        }

        public void Refresh()
        {
            localeTextData = LocalizedAssetParser.Parse(localeData.text);
        }

        public void Apply(bool setText = true, bool logErrors = true)
        {
            if (target == null)
            {
                if (logErrors)
                    Debug.LogError(string.Format("[LocalizedText::Awake] Text on {0} doesn't have target.", this.GetPath()));

                return;
            }

            if (localeData != null)
            {
                if (localeData.text != null)
                    localeTextData ??= LocalizedAssetParser.Parse(localeData.text);
                else
                {
                    if (logErrors)
                        Debug.LogWarning(string.Format("[LocalizedText::Awake] Text on {0} doesn't have a 'localeData' field with text on it assigned.", this.GetPath()));
                    return;
                }
            }
            else
            {
                if (logErrors)
                    Debug.LogWarning(string.Format("[LocalizedText::Awake] Text on {0} doesn't have a 'localeData' field assigned.", this.GetPath()));
                return;
            }

            LocalizedTextData data = localeTextData.SingleOrDefault(d => d.TextID == textID);

            if (data == null)
            {
                if (logErrors)
                    Debug.LogError(string.Format("[LocalizedText::Awake] Text on {0} has invalid id '{1}'.", this.GetPath(), textID));
                return;
            }

            if (setText)
            {
                string setData = string.Empty;

#if UNITY_EDITOR
                if (data.ContainsLocale(spoofLocale))
                    setData = data[spoofLocale];
                else
                    setData = data.GetCurrentLocaleString();
#else
                setData = data.GetCurrentLocaleString();
#endif
                target.SetText(setData);
            }
        }

        public void ApplyFormatted(params object[] formatArgs)
        {
            if (target == null)
            {
                Debug.LogError(string.Format("[LocalizedText::Awake] Text on {0} doesn't have target.", this.GetPath()));
                return;
            }

            if (localeData != null)
                localeTextData ??= LocalizedAssetParser.Parse(localeData.text);
            else
            {
                Debug.LogWarning(string.Format("[LocalizedText::Awake] Text on {0} doesn't have a 'localeData' field assigned.", this.GetPath()));
                return;
            }

            LocalizedTextData data = localeTextData.SingleOrDefault(d => d.TextID == textID);

            if (data == null)
            {
                Debug.LogError(string.Format("[LocalizedText::Awake] Text on {0} has invalid id '{1}'.", this.GetPath(), textID));
                return;
            }

            string setData = string.Empty;

#if UNITY_EDITOR
            if (data.ContainsLocale(spoofLocale))
                setData = data[spoofLocale];
            else
                setData = data.GetCurrentLocaleString();
#else
            setData = data.GetCurrentLocaleString();
#endif

            setData = string.Format(setData, formatArgs);
            target.SetText(setData);
        }
    }
}