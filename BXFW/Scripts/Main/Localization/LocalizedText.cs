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
                    Debug.LogError($"[LocalizedText::Awake] Text on {this.GetPath()} doesn't have target.");

                return;
            }

            if (localeData != null)
            {
                if (localeData.text != null)
                    localeTextData ??= LocalizedAssetParser.Parse(localeData.text);
                else
                {
                    if (logErrors)
                        Debug.LogWarning($"[LocalizedText::Awake] Text on {this.GetPath()} doesn't have a 'localeData' field with text on it assigned.");
                    return;
                }
            }
            else
            {
                if (logErrors)
                    Debug.LogWarning($"[LocalizedText::Awake] Text on {this.GetPath()} doesn't have a 'localeData' field assigned.");
                return;
            }

            var data = localeTextData.SingleOrDefault(d => d.TextID == textID);

            if (data == null)
            {
                if (logErrors)
                    Debug.LogError($"[LocalizedText::Awake] Text on {this.GetPath()} has invalid id '{textID}'.");
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
                Debug.LogError($"[LocalizedText::Awake] Text on {this.GetPath()} doesn't have target.");
                return;
            }

            if (localeData != null)
                localeTextData ??= LocalizedAssetParser.Parse(localeData.text);
            else
            {
                Debug.LogWarning($"[LocalizedText::Awake] Text on {this.GetPath()} doesn't have a 'localeData' field assigned.");
                return;
            }

            var data = localeTextData.SingleOrDefault(d => d.TextID == textID);

            if (data == null)
            {
                Debug.LogError($"[LocalizedText::Awake] Text on {this.GetPath()} has invalid id '{textID}'.");
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