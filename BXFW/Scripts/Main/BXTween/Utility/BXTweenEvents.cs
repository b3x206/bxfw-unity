using System;
using UnityEngine.Events;

namespace BXFW.Tweening.Events
{
    /// <summary>
    /// A blank delegate.
    /// </summary>
    public delegate void BXTweenMethod();
    /// <summary>
    /// Delegate with generic. Used for setter.
    /// </summary>
    /// <typeparam name="T">Type. Mostly struct types.</typeparam>
    /// Note that i might constraint 'T' only to struct, but idk.
    /// <param name="value">Set value.</param>
    public delegate void BXTweenSetMethod<in T>(T value);
    /// <summary>
    /// Tween easing method, for use with <see cref="BXTweenLerpMethod{T}"/> or any lerp method's time parameter.
    /// <br>Used in <see cref="BXTweenEase"/>.</br>
    /// </summary>
    /// <param name="time">Time value. Interpolate time linearly if possible.</param>
    /// <returns>Interpolation value (usually between 0-1)</returns>
    public delegate float BXTweenEaseSetMethod(float time, bool clamped = true);
    /// <summary>
    /// A pretty generic linear interpolation method.
    /// <br>Interpolates from the first parameter to the second parameter, in span of 'time' 
    /// (usually unclamped lerp, but normal, clamped values range between 0-1)</br>
    /// </summary>
    /// <typeparam name="T">Type of the lerped value. It can be a numeric / math type (or any type you like).</typeparam>
    /// <param name="from">Start value.</param>
    /// <param name="to">End value.</param>
    /// <param name="time">Time (0 => returns 'from', 1 => returns 'to')</param>
    public delegate T BXTweenLerpMethod<T>(T from, T to, float time);

    /// <summary>
    /// Unity event for <see cref="BXTweenProperty{T}"/> and <see cref="BXTweenCTX{T}"/>
    /// </summary>
    [Serializable]
    public sealed class BXTweenUnityEvent : UnityEvent<ITweenCTX> { }
}
