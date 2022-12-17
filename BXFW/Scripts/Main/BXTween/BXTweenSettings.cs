using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using UnityEngine;

namespace BXFW.Tweening
{
    /// <summary>
    /// BXTweenSettings.
    /// <br>For <see cref="BXTweenProperty{T}"/>'s, we use editor scripts on demand to access and set.</br>
    /// </summary>
    [Serializable]
    public class BXTweenSettings : ScriptableObjectSingleton<BXTweenSettings>
    {
        // -- BXTweenStrings Settings
        // :: General
        public bool enableBXTween = true;
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