using System;
using UnityEngine;

namespace BXFW
{
    // Written by Bryan Keiren (http://www.bryankeiren.com), modified by me lol
    // --
    // Since i'm lazy and don't want to bother with reflection (i probably will end up doing that anyways lol)
    // Since the newer unity serializer does accept generic types this works fine with those too,
    // but i may just serialize the type into some sort of binary for a full copy.
    // --
    // Changes :
    // 1 : Neat editor
    // 2 : Fix warnings
    // 3 : Remove some not needed values

    /// <summary>
    /// A <see cref="System.Type"/> datatype that can be serialized.
    /// </summary>
    [Serializable]
    public class SerializableSystemType : IEquatable<SerializableSystemType>
    {
        [SerializeField]
        private string m_AssemblyQualifiedName;
        /// <summary>
        /// The qualified assembly name for the type.
        /// <br>This is the actual name used to get the type.</br>
        /// </summary>
        public string AssemblyQualifiedName
        {
            get { return m_AssemblyQualifiedName; }
        }

        /// <summary>
        /// The internal cached type.
        /// </summary>
        private Type m_Type;
        /// <summary>
        /// The previous <see cref="m_AssemblyQualifiedName"/> checked last time the <see cref="Type"/> was accessed.
        /// </summary>
        private string m_PreviousQualifiedName;
        /// <summary>
        /// The System.Type that this serialized type corresponds to.
        /// </summary>
        public Type Type
        {
            get
            {
                if (m_Type == null || m_AssemblyQualifiedName != m_PreviousQualifiedName)
                {
                    m_Type = GetQualifiedSystemType();
                    m_PreviousQualifiedName = m_AssemblyQualifiedName;
                }
                return m_Type;
            }
            set
            {
                m_Type = value;
                // Change the values
                GetValuesFromType(m_Type);
            }
        }
        /// <summary>
        /// Caches the <see cref="Type"/>.
        /// </summary>
        private Type GetQualifiedSystemType()
        {
            return Type.GetType(m_AssemblyQualifiedName);
        }

        /// <summary>
        /// Creates a type from given <paramref name="type"/>.
        /// </summary>
        public SerializableSystemType(Type type)
        {
            GetValuesFromType(type);
        }

        private void GetValuesFromType(Type type)
        {
            m_Type = type;
            m_AssemblyQualifiedName = type?.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SerializableSystemType typeObj))
            {
                return false;
            }

            return Equals(typeObj);
        }

        public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // The 'ReferenceEquals' would ensure equal null/non-null values, so just ensure that not just only one is null.
            if (a is null || b is null)
            {
                return false;
            }

            // Return the 'IEquatable' thing
            return a.Equals(b);
        }
        public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }

        public bool Equals(SerializableSystemType value)
        {
            return value.Type.Equals(Type);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_AssemblyQualifiedName, Type);
        }
    }
}
