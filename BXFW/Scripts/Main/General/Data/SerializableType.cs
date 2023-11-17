using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A <see cref="System.Type"/> datatype that can be serialized.
    /// </summary>
    [Serializable]
    public class SerializableType : IEquatable<SerializableType>
    {
        /// <summary>
        /// Data contained about the type.
        /// </summary>
        [SerializeField]
        private byte[] m_Data = new byte[0];
        /// <summary>
        /// Previously contained runtime data about this type.
        /// </summary>
        private byte[] m_PreviousData = new byte[0];
        /// <summary>
        /// Returns whether if the contained data is null (more optimized).
        /// </summary>
        public bool IsNull => m_Data == null || m_Data.Length <= 0 || m_Data[0] == 0xFF;
        /// <summary>
        /// Cached type value.
        /// </summary>
        private Type m_Type;
        /// <summary>
        /// The given type value.
        /// <br>This can be whether open or closed. The serialized type data can contain partial 
        /// generic arguments about given types, but this data will always be open or closed until all arguments have a type.</br>
        /// </summary>
        public Type Type
        {
            get
            {
                if (IsNull)
                {
                    return null;
                }

                // Change the returned type if the data sequences of runtime isn't equal.
                // Though in a normal usage case (in code for example) this is not really required
                // But it won't be constrainted to just Play mode or editor just to avoid weird behaviour.
                if (m_Type == null || !Enumerable.SequenceEqual(m_Data, m_PreviousData))
                {
                    using MemoryStream ms = new MemoryStream(m_Data);
                    using BinaryReader reader = new BinaryReader(ms);
                    m_Type = Read(reader);

                    // Check eq thing
                    m_PreviousData = new byte[m_Data.Length];
                    Array.Copy(m_Data, m_PreviousData, m_Data.Length);
                }

                return m_Type;
            }
            set
            {
                m_Type = value;

                using MemoryStream ms = new MemoryStream(m_Data);
                using BinaryWriter writer = new BinaryWriter(ms);
                Write(writer, m_Type);
            }
        }
        /// <summary>
        /// Returns whether if the type generics are not assigned.
        /// </summary>
        public bool IsOpenType
        {
            get
            {
                if (Type == null)
                {
                    return false;
                }

                return Type.ContainsGenericParameters && Type.GenericTypeArguments.Length != Type.GetGenericArguments().Length;
            }
        }

        /// <summary>
        /// Reads the given data from the stream <paramref name="readStream"/>.
        /// </summary>
        /// <returns>The read data. This can be null.</returns>
        /// <exception cref="TypeAccessException"/>
        public static Type Read(BinaryReader readStream)
        {
            // Size is invalid, a type string is required to exist
            if (readStream.BaseStream.Length <= 1)
            {
                return null;
            }

            // Get type parameter count
            byte validParamCount = readStream.ReadByte();
            if (validParamCount == 0xFF)
            {
                return null;
            }

            // Get initial type name
            string typeName = readStream.ReadString();
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new TypeAccessException($"Can't find type: '{typeName}'");
            }

            // Get generic type definitions
            // Don't make a closed type until all generic args are provided.
            // Because of this, read until the valid param count is skipped.
            if (type.IsGenericTypeDefinition && /*paramCount > 0 && */
                type.GetGenericArguments().Length == validParamCount)
            {
                Type[] genericArgs = new Type[validParamCount];
                bool isValidClosedType = true;
                for (int i = 0; i < validParamCount; i++)
                {
                    Type argument = Read(readStream);

                    if (argument == null || !isValidClosedType)
                    {
                        isValidClosedType = false;
                        break;
                    }

                    genericArgs[i] = argument;
                }

                if (isValidClosedType)
                {
                    type = type.MakeGenericType(genericArgs);
                }
            }
            else
            {
                for (int i = 0; i < validParamCount; i++)
                {
                    // Skip while discarding the given type.
                    _ = Read(readStream);
                }
            }

            return type;
        }
        /// <summary>
        /// Writes the type value into <paramref name="writeStream"/>.
        /// </summary>
        public static void Write(BinaryWriter writeStream, Type type)
        {
            // Don't write invalid types
            if (type == null || type.IsGenericParameter)
            {
                writeStream.Write((byte)0xFF);
                return;
            }

            if (type.IsGenericType)
            {
                Type openType = type.GetGenericTypeDefinition();
                Type[] args = type.GetGenericArguments();
                // 8 bytes : Actual valid generic length
                // We can just write that as reading + processing order doesn't really matter lol.
                byte validArgsLength = (byte)args.Count(gType => !gType.IsGenericParameter);
                writeStream.Write(validArgsLength);
                writeStream.Write(openType.AssemblyQualifiedName);
                for (int i = 0; i < validArgsLength; i++)
                {
                    Write(writeStream, args[i]);
                }

                return;
            }

            writeStream.Write((byte)0);
            writeStream.Write(type.AssemblyQualifiedName);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SerializableType typeObj))
            {
                return false;
            }

            return Equals(typeObj);
        }
        public bool Equals(SerializableType value)
        {
            return value.Type.Equals(Type);
        }
        public static bool operator ==(SerializableType a, SerializableType b)
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
        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_Data);
        }
    }
}
