using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using UnityEngine;

namespace BXFW.Tweening
{
    [Serializable]
    public class BXTweenSettings
    {
        /// <summary>
        /// Hash helper class for getting hash of general strings.
        /// </summary>
        public static class HashHelper
        {
            [Serializable]
            public class HashScriptableObject : ScriptableObject
            {
                // Only contains a string inside resources that contains the hash
                public const string DefaultHash = "NO_HASH_DEFINED";
                public string Hash = DefaultHash;
            }
            public enum OnHashMatchFailAction
            {
                Ignore = 0,
                Crash = 1,
                Custom = 2
            }

            /// <summary>
            /// Gets the <see cref="HashScriptableObject"/> using <see cref="InstanceJsonString"/>.
            /// </summary>
            public static HashScriptableObject GetTweenSettingsHashObject()
            {
                var hObj = ScriptableObject.CreateInstance<HashScriptableObject>();
                hObj.Hash = HashData(InstanceJsonString);

                return hObj;
            }

            public static string HashData(string data)
            {
                var textToBytes = Encoding.UTF8.GetBytes(data);
                var sha256 = new SHA256Managed();

                var hashValue = sha256.ComputeHash(textToBytes);

                return GetHexFromHash(hashValue);
            }
            private static string GetHexFromHash(byte[] hash)
            {
                var hexString = string.Empty;

                foreach (var b in hash)
                    hexString += b.ToString("x2");

                return hexString;
            }
        }

        /// <summary>Path that contains the <see cref="Application.streamingAssetsPath"/> for <see cref="BXTweenSettings"/>.</summary>
        public static readonly string SettingsObjectDirectory = string.Format("{0}/BXTween", Application.streamingAssetsPath);
        /// <summary>Name of the .json file.</summary>
        public const string SettingsObjectName = "BXTweenSettings";

        public const string HashResourceName = "BXTweenSettingsHashRes";
        private static BXTweenSettings instance;
        public static string InstanceJsonString;
        /// <summary>
        /// Get whether if <see cref="instance"/> is modified after build.
        /// </summary>
        public static bool IsModified
        {
            get
            {
#if !UNITY_EDITOR
                if (instance != null)
                {
                    /// Get hash from resources.
                    /// Note that this isn't checked on <see cref="GetBXTweenSettings"/> as that can be called on Monobehaviour constructors.
                    var CurrentHash = BXTweenSettingsHashHelper.HashData(InstanceJsonString);
                    var hash = Resources.Load<HashHelper.HashScriptableObject>(HashResourceName);

                    return hash.Hash == CurrentHash;
                }
#else
                // In editor, manipulation of any sort is allowed.
                return false;
#endif
            }
        }

        /// <summary>
        /// Get the <see cref="instance"/> with fancier refreshing.
        /// </summary>
        public static BXTweenSettings GetBXTweenSettings()
        {
            if (instance != null)
                return instance;

            var settingsResPath = string.Format("{0}{1}", SettingsObjectDirectory, SettingsObjectName);
#if UNITY_EDITOR
            var settingsAbsPath = string.Format("{0}{1}/{2}", Directory.GetCurrentDirectory(), SettingsObjectDirectory, SettingsObjectName);
#else
            // Exclude 'Directory.GetCurrentDirectory()' as it's not necessary on builds?
            var settingsAbsPath = string.Format("{0}{1}", SettingsObjectDirectory, SettingsObjectName);
#endif
            /// In android, we need to create an <see cref="UnityEngine.Networking.UnityWebRequest"/>.
#if !UNITY_ANDROID || UNITY_EDITOR
            // Just load the file.
            InstanceJsonString = File.ReadAllText(settingsAbsPath);
#else // Not Unity editor but android
            // UNITY WHY (oh it's android moment not unity but WHY YOU CAN'T CALL RESOURCE.LOAD ON MONOBEHAVIOUR CONSTRUCTOR AAAAAAAAAAAAAA)
            UnityWebRequest InstanceJsonRequest = UnityWebRequest.Get(settingsAbsPath);
            // Note that if the application crashes here it's my responsibility.
            // Since we are loading the file synchronously i need to do this (yeah its bad)
            while (!InstanceJsonRequest.isDone && InstanceJsonRequest.result == UnityWebRequest.Result.InProgress);

            if (InstanceJsonRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(BXTweenStrings.Err_BXTwSettingsNoResource);
                return new BXTweenSettings();
            }

            InstanceJsonString = InstanceJsonRequest.downloadHandler.text;
#endif  
            instance = JsonUtility.FromJson<BXTweenSettings>(InstanceJsonString);

#if UNITY_EDITOR
            if (instance == null)
            {
                // Generate settings
                // (Have a static instance to our settings)
                instance = new BXTweenSettings();
                if (!Directory.Exists(SettingsObjectDirectory))
                {
                    Directory.CreateDirectory(SettingsObjectDirectory);
                }

                // Write json text.
                var fileDir = settingsAbsPath;
                File.WriteAllText(fileDir, JsonUtility.ToJson(instance, true)); // << This does create the file.

                // Refresh assets
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log(BXTweenStrings.GetLog_BXTwSettingsOnCreate(settingsResPath));

                // Return settings
                return instance;
            }
#else
            if (instance == null)
            {
                Debug.LogError(BXTweenStrings.Err_BXTwSettingsNoResource);
                return new BXTweenSettings();
            }
#endif
            return instance;
        }

        // -- BXTweenStrings Settings
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
    }
}