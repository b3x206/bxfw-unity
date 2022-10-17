﻿using UnityEngine;
using BXFW.Tweening.Events;

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace BXFW.Tweening
{
    /// <summary>
    /// Coroutine based tweening system.
    /// Contains all the base methods.
    /// </summary>
    public static class BXTween
    {
        public static BXTweenCore Current;
        public static List<ITweenCTX> CurrentRunningTweens = new List<ITweenCTX>();

        private static BXTweenSettings currentSettings;
        public static BXTweenSettings CurrentSettings
        {
            get
            {
                // Get singleton
                if (currentSettings == null)
                    currentSettings = BXTweenSettings.Instance;

                // Still null? Create new settings.
                if (currentSettings == null)
                {
#if UNITY_EDITOR
                    // We are still null, create instance at given const resources directory.
                    // Maybe we can add a EditorPref for creation directory?
                    currentSettings = BXTweenSettings.CreateEditorInstance(BXTweenStrings.SettingsResourceCreatePath, BXTweenStrings.SettingsResourceCreateName);
                    // Current editor is diagnostic by default, for the creation
                    // This will be here for debug (we can't check current settings whether if it's diagnostic mode because we just created it)
                    Debug.Log(BXTweenStrings.DLog_BXTwSettingsCreatedNew($"Assets/Resources/{BXTweenStrings.SettingsResourceCreatePath} | File : {BXTweenStrings.SettingsResourceCreateName}"));
#else
                    // maybe throw exception? making it more obvious that something has went wrong on compilation-generation process?
                    Debug.LogError(BXTweenStrings.Err_BXTwSettingsNoResource);
                    // Create a tempoary resource using default settings.
                    currentSettings = ScriptableObject.CreateInstance<BXTweenSettings>();
#endif
                }

                return currentSettings;
            }
        }

        private static readonly MethodInfo[] BXTweenMethods = typeof(BXTween).GetMethods();

        #region Utility
        /// <summary>
        /// Returns a <see cref="BXTween"/>.To() method from type if it exists.
        /// <br>The gathered methods <see cref="BXTweenMethods"/> are cached.</br>
        /// </summary>
        /// <param name="t">Type to check. See the <see cref="BXTween"/> class 
        /// (or the non-existent documentation) for more info on tweenable types.</param>
        public static MethodInfo GetTweenMethodFromType(Type t)
        {
            var info = BXTweenMethods.SingleOrDefault(x => x.Name == nameof(To) && x.GetParameters()[0].ParameterType == t);

            return info;
        }
        /// <summary>
        /// Returns a <see cref="BXTween"/>.To() method from type if it exists.
        /// </summary>
        /// <typeparam name="T">Type to check. See the <see cref="BXTween"/> class 
        /// (or the non-existent documentation) for more info on tweenable types.</typeparam>
        public static MethodInfo GetTweenMethodFromType<T>()
        {
            return GetTweenMethodFromType(typeof(T));
        }

        /// <summary>
        /// Utility for checking if the type is tweenable.
        /// </summary>
        /// <param name="t">Type to check.</param>
        /// <returns>Bool = If it's tweenable | MethodInfo = For usage if tweenable</returns>
        public static bool IsTweenableType(Type t)
        {
            // Get method if it's tweenable (dynamic using reflection)
            return GetTweenMethodFromType(t) != null;
        }
        /// <summary>
        /// Utility for checking if the type is tweenable.
        /// </summary>
        /// <param name="t">Type to check.</param>
        /// <returns>Bool = If it's tweenable | MethodInfo = For usage if tweenable</returns>
        public static bool IsTweenableType<T>()
        {
            return IsTweenableType(typeof(T));
        }
        /// <summary>
        /// Check the status of <see cref="BXTweenCore"/> <see cref="Current"/> variable.
        /// <br>NOTE : This method tries to re-launch the tween core if it fails. 
        /// It only returns <see langword="false"/> if it fails <b>twice</b>.</br>
        /// </summary>
        /// <returns>Whether if the tweening 'engine' is ok.</returns>
        public static bool CheckStatus()
        {
            // Editor checks
            if (Current == null)
            {
                if (!Application.isPlaying && Application.isEditor)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(BXTweenStrings.Warn_EditorBXTwCoreNotInit);
                    BXTweenCore.EditorInitilazeBXTw();
#endif
                }
                else // Not editor or playing
                {
                    // Re-initilaze on editor.
                    Debug.LogWarning(BXTweenStrings.Warn_BXTwCoreNotInit);
                    BXTweenCore.InitilazeBXTw();
                }
            }
            // It still is null, print error and return false.
            if (Current == null)
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCoreFailInit);
                return false;
            }

            return true;
        }
        #endregion

        #region Context Creation (To Methods)
        /// <summary>
        /// Create a tween manually. Note that you have to pass a tweenable type. (check using <see cref="IsTweenableType{T}"/>)
        /// <br>(More Info : Tweenable type is the ones that exist in <see cref="BXTween"/> class (except for the generic To).)</br>
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<T> GenericTo<T>(T StartValue, T TargetValue, float Duration, BXTweenSetMethod<T> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // Call helper method
            var tweenMethod = GetTweenMethodFromType<T>();

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.DLog_BXTwCallGenericTo);
            }

            // Check Tweenable
            if (tweenMethod != null)
            {
                /// We get method <see cref="BXTween.To"/> returned from this class.
                return (BXTweenCTX<T>)tweenMethod.Invoke(null, new object[] { StartValue, TargetValue, Duration, Setter, TargetObject, StartTween });
            }
            else
            {
                Debug.LogError(BXTweenStrings.GetErr_NonTweenableType(typeof(T).ToString()));
                return null;
            }
        }

        // These 'To' methods probably doesn't need boilerplate lowering as they are short,
        // and using a generic method to get the context is slower than usual (calls reflection + linq)
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters. This is the object that the tween runs on.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<int> To(int StartValue, int TargetValue, float Duration, BXTweenSetMethod<int> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<int>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<int> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters. This is the object that the tween runs on.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<float> To(float StartValue, float TargetValue, float Duration, BXTweenSetMethod<float> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<float>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<float> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<Color> To(Color StartValue, Color TargetValue, float Duration, BXTweenSetMethod<Color> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Color>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Color> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<Vector2> To(Vector2 StartValue, Vector2 TargetValue, float Duration, BXTweenSetMethod<Vector2> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Vector2>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Vector2> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<Vector3> To(Vector3 StartValue, Vector3 TargetValue, float Duration, BXTweenSetMethod<Vector3> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Vector3>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Vector3> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<Quaternion> To(Quaternion StartValue, Quaternion TargetValue, float Duration, BXTweenSetMethod<Quaternion> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Quaternion>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Quaternion> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<Matrix4x4> To(Matrix4x4 StartValue, Matrix4x4 TargetValue, float Duration, BXTweenSetMethod<Matrix4x4> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Matrix4x4>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Matrix4x4> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        #endregion
    }
}