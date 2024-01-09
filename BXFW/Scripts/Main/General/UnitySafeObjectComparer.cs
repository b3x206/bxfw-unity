using System.Collections.Generic;

namespace BXFW
{
    /// <summary>
    /// Allows for safely comparing <see cref="UnityEngine.Object"/>'s to anything.
    /// <br>Also works as a default <see cref="object"/> comparer. (for non-typesafe boxed objects)</br>
    /// </summary>
    public class UnitySafeObjectComparer : IEqualityComparer<object>
    {
        private static UnitySafeObjectComparer m_Default = new UnitySafeObjectComparer();
        /// <summary>
        /// Default comparer. This is just a static instance with assurance that it will never be null.
        /// </summary>
        public static UnitySafeObjectComparer Default
        {
            get
            {
                m_Default ??= new UnitySafeObjectComparer();
                return m_Default;
            }
        }

        /// <summary>
        /// Checks whether if two c# objects are equal, with respectiveness to the fake null unity <see cref="UnityEngine.Object"/>'s.
        /// </summary>
        /// <param name="x">Left-hand side object to compare.</param>
        /// <param name="y">Right-hand side object to compare.</param>
        /// <returns>Whether if two objects are equal.</returns>
        public new bool Equals(object x, object y)
        {
#if UNITY_5_5_OR_NEWER
            // Yes, we have to type test it 4 times. Feel free to fix this.
            // This will work with y as not UnityEngine.Object or y as null because the comparsion operator handles these.
            UnityEngine.Object lhsUnityObject = x as UnityEngine.Object;
            UnityEngine.Object rhsUnityObject = y as UnityEngine.Object;
            // Use unity object comparison
            // This is terrible because unity, once upon a time, made the wise decision of
            // fake nulling objects just for a nicer error output. great.
            if (x is UnityEngine.Object || y is UnityEngine.Object)
            {
                return lhsUnityObject == rhsUnityObject;
            }
#endif
            // Use the default object comparison
            // This, if the Equals implementing class object instance, is type testing the equal object, should work fine.
            return EqualityComparer<object>.Default.Equals(x, y);
            // return object.Equals(x, y);
        }

        public int GetHashCode(object obj)
        {
#if UNITY_5_5_OR_NEWER
            if (obj is UnityEngine.Object unityObject)
            {
                if (unityObject == null)
                {
                    return 0;
                }
            }
#endif
            return obj.GetHashCode();
        }
    }
}
