using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BXFW.SceneManagement
{
    /// <summary>
    /// A <see cref="GUID"/> that can be serialized and used in the game.
    /// <br>However, the data has to be gathered from editor apis.</br>
    /// </summary>
    [Serializable]
    public struct SerializableGUID : 
        // Serializable interfaces
        IEquatable<SerializableGUID>
#if UNITY_EDITOR
        // Editor interfaces
        , IEquatable<GUID>
#endif
    {
        [SerializeField] private uint m_Value0;
        [SerializeField] private uint m_Value1;
        [SerializeField] private uint m_Value2;
        [SerializeField] private uint m_Value3;

        public override int GetHashCode()
        {
            unchecked
            {
                int v = (int)m_Value0;
                v = (v * 397) ^ (int)m_Value1;
                v = (v * 397) ^ (int)m_Value2;
                return (v * 397) ^ (int)m_Value3;
            }
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        /// <summary>
        /// Returns the hex representation of this GUID.
        /// <br>Not prefixed with an '0x'.</br>
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0:X8}{1:X8}{2:X8}{3:X8}", m_Value0, m_Value1, m_Value2, m_Value3);
        }
        /// <summary>
        /// Returns the number with a format string.
        /// </summary>
        public string ToString(string fmtParam)
        {
            return string.Format("{0}{1}{2}{3}", m_Value0.ToString(fmtParam), m_Value1.ToString(fmtParam), m_Value2.ToString(fmtParam), m_Value3.ToString(fmtParam));
        }

        public bool Equals(SerializableGUID other)
        {
            return m_Value0 == other.m_Value0 && m_Value1 == other.m_Value1 && m_Value2 == other.m_Value2 && m_Value3 == other.m_Value3;
        }
        public static bool operator ==(SerializableGUID left, SerializableGUID right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(SerializableGUID left, SerializableGUID right)
        {
            return !(left == right);
        }

        public bool Empty()
        {
            return m_Value0 == 0 && m_Value1 == 0 && m_Value2 == 0 && m_Value3 == 0;
        }

#if UNITY_EDITOR
        public bool Equals(GUID uOther)
        {
            return this == new SerializableGUID(uOther);
        }
        public static bool operator ==(SerializableGUID left, GUID uRight)
        {
            return left.Equals(uRight);
        }
        public static bool operator !=(SerializableGUID left, GUID uRight)
        {
            return !(left == uRight);
        }

        public SerializableGUID(GUID uGUID)
        {
            // Field names are the same as they are on this.
            m_Value0 = (uint)typeof(GUID).GetField(nameof(m_Value0), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(uGUID);
            m_Value1 = (uint)typeof(GUID).GetField(nameof(m_Value1), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(uGUID);
            m_Value2 = (uint)typeof(GUID).GetField(nameof(m_Value2), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(uGUID);
            m_Value3 = (uint)typeof(GUID).GetField(nameof(m_Value3), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(uGUID);
        }
        public static implicit operator GUID(SerializableGUID sGUID)
        {
            object v = new GUID(); // ... this should be object because structs are passed by copy
            // and these functions need reference ...
            typeof(GUID).GetField(nameof(m_Value0), BindingFlags.NonPublic | BindingFlags.Instance).SetValue(v, sGUID.m_Value0);
            typeof(GUID).GetField(nameof(m_Value1), BindingFlags.NonPublic | BindingFlags.Instance).SetValue(v, sGUID.m_Value1);
            typeof(GUID).GetField(nameof(m_Value2), BindingFlags.NonPublic | BindingFlags.Instance).SetValue(v, sGUID.m_Value2);
            typeof(GUID).GetField(nameof(m_Value3), BindingFlags.NonPublic | BindingFlags.Instance).SetValue(v, sGUID.m_Value3);
            
            return (GUID)v;
        }
#endif
    }
}
