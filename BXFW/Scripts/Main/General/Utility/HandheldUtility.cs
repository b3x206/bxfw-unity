#if UNITY_ANDROID || UNITY_ANDROID_API || UNITY_IOS
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Handheld specific utilities.
    /// <br><b>WARNING : </b> This class only compiles when the '#if UNITY_ANDROID || UNITY_IOS' definition is true.</br>
    /// </summary>
    public static class HandheldUtility
    {
        // -- Common utility
        // These 2 methods still target android but it also works with most handheld related stuff.
        /// <summary>
        /// Returns the keyboard height ratio to the screen display height.
        /// </summary>
        public static float GetKeyboardHeightRatio(bool includeInput)
        {
            // Use 'Screen.height' instead of 'Display.main.screenHeight'
            // because in new android days the phones can scale application windows freely
            return Mathf.Clamp01((float)GetKeyboardHeight(includeInput) / Screen.height);
        }
        /// <summary>
        /// Returns the keyboard height in display pixels.
        /// </summary>
        public static int GetKeyboardHeight(bool includeInput)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // In android you have to do dumb stuff in order to get keyboard height
            // This 'may not be necessary' in more updated versions of unity, but here we are.
            using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
                AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
                AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");

                if (view == null || dialog == null)
                {
                    return 0;
                }

                int decorHeight = 0;
                // The input box that appears on top when the keyboard is visible
                if (includeInput)
                {
                    AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");

                    if (decorView != null)
                        decorHeight = decorView.Call<int>("getHeight");
                }
                // Get actual height
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
                }
            }
#else
            int height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
            return height >= Display.main.systemHeight ? 0 : height;
#endif
        }

#if UNITY_ANDROID
        /// <summary>
        /// Android specific handheld methods.
        /// <br>This will be only compiled when the '#if UNITY_ANDROID' definition is true. Cover using code with that definition.</br>
        /// </summary>
        public static class Android
        {
            /// <summary>
            /// The vibration JavaObject that's controlled. Set on construction.
            /// </summary>
            private static readonly AndroidJavaObject vibrateDelegate;

            // Trick Unity into giving the App vibration permission when it builds.
            // This check will always be false, but the compiler doesn't know that.
            static Android()
            {
                if (Application.isEditor)
                {
                    // Note : Handheld class is only available when compiling for mobile platforms.
                    Handheld.Vibrate();
                    return;
                }

                if (vibrateDelegate == null)
                {
                    vibrateDelegate = new AndroidJavaClass("com.unity3d.player.UnityPlayer") // Get the Unity Player.
                    .GetStatic<AndroidJavaObject>("currentActivity")                         // Get the Current Activity from the Unity Player.
                    .Call<AndroidJavaObject>("getSystemService", "vibrator");                // Then get the Vibration Service from the Current Activity.
                }

                // This is realistically never printed as this class is android exclusive.
                // Debug.LogError("[AndroidUtils::(static ctor)] Couldn't construct. Not android.");
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
#endif
    }
}
#endif
