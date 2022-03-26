using System.IO;
using UnityEngine;

namespace BXFW.Tweening
{
    [System.Serializable]
    public class BXTweenSettings : ScriptableObject
    {
        // -- Resources Create
        public const string BXTweenSettingsObjectDirectory = "Assets/Resources/";
        public const string BXTweenSettingsObjectName = "BXTweenSettings";
        public static BXTweenSettings GetBXTweenSettings()
        {
            var settingsPath = string.Format("{0}{1}", BXTweenSettingsObjectDirectory, BXTweenSettingsObjectName);
            var settings = Resources.Load<BXTweenSettings>(BXTweenSettingsObjectName);

            if (settings == null)
            {
                // Generate settings
                var dSettings = CreateInstance<BXTweenSettings>();
#if UNITY_EDITOR
                if (!Directory.Exists(BXTweenSettingsObjectDirectory))
                {
                    Directory.CreateDirectory(BXTweenSettingsObjectDirectory);
                }

                UnityEditor.AssetDatabase.CreateAsset(dSettings, string.Format("{0}.asset", settingsPath));
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log(BXTweenStrings.GetLog_BXTwSettingsOnCreate(settingsPath));
#else
                // Something went wrong on the editor, return a default instance and call it a day.
                Debug.LogError(BXTweenStrings.Err_BXTwSettingsNoResource);
#endif
                // Return settings
                return dSettings;
            }

            return settings;
        }

        // -- CTweenStrings Settings
        [Header(":: General")]
        public EaseType DefaultEaseType = EaseType.QuadInOut;
        public RepeatType DefaultRepeatType = RepeatType.PingPong;

        [Header(":: Debug")]
        public bool diagnosticMode = false;
        [Header(":: CTweenStrings")]
        public Color LogColor = new Color(.68f, .61f, .43f);
        public Color LogDiagColor = new Color(1f, .54f, 0f);
        public Color WarnColor = new Color(1f, .8f, 0f);
        public Color ErrColor = new Color(.52f, .2f, .9f);
    }
}