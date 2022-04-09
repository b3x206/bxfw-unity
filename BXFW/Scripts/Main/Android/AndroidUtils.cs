using System.Collections;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Android specific utilities.
    /// </summary>
    public static class AndroidUtils
    {
        /// <summary>
        /// The vibration JavaObject that's controlled. Set on construction.
        /// </summary>
        private static readonly AndroidJavaObject vibrateDelegate;

        // Trick Unity into giving the App vibration permission when it builds.
        // This check will always be false, but the compiler doesn't know that.
        static AndroidUtils()
        {
            if (Application.isEditor)
            {
                Handheld.Vibrate();
                return;
            }

#if UNITY_ANDROID
            if (vibrateDelegate == null)
            {
                vibrateDelegate = new AndroidJavaClass("com.unity3d.player.UnityPlayer") // Get the Unity Player.
                .GetStatic<AndroidJavaObject>("currentActivity")                         // Get the Current Activity from the Unity Player.
                .Call<AndroidJavaObject>("getSystemService", "vibrator");                // Then get the Vibration Service from the Current Activity.
            }
#else
            Debug.LogError("[AndroidUtils::(static ctor)] Couldn't construct. Not android.");
#endif
        }
        /// <summary>
        /// Vibrates device.
        /// </summary>
        /// <param name="Milliseconds">How many milliseconds will the device vibrate?</param>
        public static void Vibrate(long Milliseconds)
        {
            vibrateDelegate?.Call("vibrate", Milliseconds);
#if UNITY_EDITOR
            Debug.Log(string.Format("[AndroidUtils::Vibrate(ms={0})] Called device vibrate.", Milliseconds));
#endif
        }
        /// <summary>
        /// Vibrates with more customization.
        /// </summary>
        /// <param name="Pattern">Pattern syntax = long[] l = { Start Delay, Ms Inbetween vibrations, Last(?) };</param>
        /// <param name="Repeat">How many times the vibration will repeat?</param>
        public static void Vibrate(long[] Pattern, int Repeat)
        {
            vibrateDelegate?.Call("vibrate", Pattern, Repeat);
#if UNITY_EDITOR
            Debug.Log(string.Format("[AndroidUtils::Vibrate(pattern={0}, repeat={1})] Called device vibrate with pattern.", Pattern, Repeat));
#endif
        }
    }
}