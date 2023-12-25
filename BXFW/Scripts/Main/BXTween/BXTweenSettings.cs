using System;
using UnityEngine;

namespace BXFW.Tweening
{
    /// <summary>
    /// BXTweenSettings which contains the settings for code created <see cref="BXTweenCTX{T}"/>'s.
    /// </summary>
    [Serializable]
    public class BXTweenSettings : ScriptableObjectSingleton<BXTweenSettings>
    {
        // -- BXTweenStrings Settings
        // :: General
        public bool enableBXTween = true;
        /// <summary>
        /// Global variable for whether to ignore the time scale.
        /// <br>Instead of switching this for tweening, just use <see cref="BXTweenCTX{T}.SetIgnoreTimeScale(bool)"/> with <see langword="true"/> parameter.</br>
        /// </summary>
        public bool ignoreTimeScale = false;
        public int maxTweens = 50;

        // :: Default
        public EaseType DefaultEaseType = EaseType.QuadInOut;
        public RepeatType DefaultRepeatType = RepeatType.PingPong;

        // :: Debug
        public bool diagnosticMode = false;
        
        // :: BXTweenStrings
        public Color LogColor = new Color(.68f, .61f, .43f);
        public Color LogDiagColor = new Color(1f, .54f, 0f);
        public Color WarnColor = new Color(1f, .8f, 0f);
        public Color ErrColor = new Color(.52f, .2f, .9f);

        /// <summary>
        /// Get values from other <see cref="BXTweenSettings"/>.
        /// </summary>
        public void FromSettings(BXTweenSettings from)
        {
            enableBXTween = from.enableBXTween;
            ignoreTimeScale = from.ignoreTimeScale;
            maxTweens = from.maxTweens;

            DefaultEaseType = from.DefaultEaseType;
            DefaultRepeatType = from.DefaultRepeatType;

            diagnosticMode = from.diagnosticMode;

            LogColor = from.LogColor;
            LogDiagColor = from.LogDiagColor;
            WarnColor = from.WarnColor;
            ErrColor = from.ErrColor;
        }
    }
}
