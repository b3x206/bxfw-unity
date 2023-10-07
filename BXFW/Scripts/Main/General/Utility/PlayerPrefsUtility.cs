using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Allows <see cref="PlayerPrefs"/> to serialize more advanced data types.
    /// <br/>
    /// <br>To check whether if there's a <see cref="PlayerPrefs"/> set from this class, use the <see cref="HasKey"/> of this class!</br>
    /// <br>It is also not suggested to use <see cref="PlayerPrefs"/> for more complex projects
    /// (use a custom data type serializer / <see cref="JsonUtility"/>+System.IO [?]), this class just provides convenience.</br>
    /// </summary>
    public static class PlayerPrefsUtility
    {
        /// <summary>
        /// Sets a <see cref="PlayerPrefs.SetInt(string, int)"/> interpreted as bool.
        /// <br><see langword="false"/> values are set as 0, <see langword="true"/> values are set as 1.</br>
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetBool] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
        /// <inheritdoc cref="GetBool(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static bool GetBool(string key, bool defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning(string.Format("[PlayerPrefsUtility::GetBool] The key is null. It will return false. Key={0}", key));
                return false;
            }
            else
            {
                return PlayerPrefs.GetInt(key, defaultValue ? 0 : 1) != 0;
            }
        }
        /// <summary>
        /// Returns a <see cref="PlayerPrefs.GetInt(string)"/> interpreted as a boolean. Non-zero values are true.
        /// <br>If the <paramref name="key"/> does not exist this will return <see langword="false"/></br>
        /// </summary>
        public static bool GetBool(string key)
        {
            return GetBool(key, false);
        }

        /// <summary>
        /// Internal method to set a generic long value.
        /// <br>
        /// The difference of this method is that this method calls two <see cref="PlayerPrefs.SetInt(string, int)"/>'s 
        /// and seperates the given long <paramref name="value"/> as lower32 and upper32<br/>
        /// <b>instead</b> of just calling <see cref="PlayerPrefs.SetString(string, string)"/> with the <paramref name="value"/>.ToString() as string.
        /// </br>
        /// </summary>
        private static void SetLongInternal(string key, long value, string savePrefix)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::Set{0}] Couldn't set the savekey because it is null. Key={1}", savePrefix, key));
                return;
            }

            uint lower32 = (uint)(value & uint.MaxValue); // The lower bytes (0 to 2**32)
            uint upper32 = (uint)(value >> 32);           // This does not depend on endianness, i guess? (put upper bytes where the lower bytes would be)
            PlayerPrefs.SetInt(string.Format("{0}_l32{1}", key, savePrefix), (int)lower32);
            PlayerPrefs.SetInt(string.Format("{0}_u32{1}", key, savePrefix), (int)upper32);
        }
        /// <summary>
        /// This method returns the generic long as two <see cref="PlayerPrefs.GetInt(string)"/> serialized integers bitshifted to it's proper values.
        /// </summary>
        private static long GetLongInternal(string key, string savePrefix)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning(string.Format("[PlayerPrefsUtility::Get{0}] The key is null. It will return 0. Key={1}", savePrefix, key));
                return 0;
            }

            uint lower32 = (uint)PlayerPrefs.GetInt(string.Format("{0}_l32{1}", key, savePrefix)), upper32 = (uint)PlayerPrefs.GetInt(string.Format("{0}_u32{1}", key, savePrefix));
            long result = lower32 | ((long)upper32 << 32);
            return result;
        }
        /// <summary>
        /// Sets a long integer to the PlayerPrefs.
        /// <br>This internally calls two <see cref="PlayerPrefs.SetInt(string, int)"/>'s.</br>
        /// </summary>
        public static void SetLong(string key, long value)
        {
            SetLongInternal(key, value, "Long");
        }
        /// <inheritdoc cref="GetLong(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static long GetLong(string key, long defaultValue)
        {
            if (!HasKey<long>(key))
                return defaultValue;

            return GetLong(key);
        }
        /// <summary>
        /// Returns the long value set by the <see cref="SetLong(string, long)"/>.
        /// </summary>
        public static long GetLong(string key)
        {
            return GetLongInternal(key, "Long");
        }
        /// <summary>
        /// Sets a double value into PlayerPrefs.
        /// <br>This internally calls two <see cref="PlayerPrefs.SetInt(string, int)"/>'s.</br>
        /// </summary>
        public static void SetDouble(string key, double value)
        {
            // apparently c# has reinterpret cast, bruh (but, only for 32 and 64 bit values)
            SetLongInternal(key, BitConverter.DoubleToInt64Bits(value), "Double");
        }
        /// <summary>
        /// Returns the double value set by the <see cref="SetDouble(string, double)"/>.
        /// <br>The internal value type of set double is two integers and it's bits are converted to the target double type.</br>
        /// </summary>
        public static double GetDouble(string key)
        {
            return BitConverter.Int64BitsToDouble(GetLongInternal(key, "Double"));
        }
        /// <inheritdoc cref="GetDouble(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static double GetDouble(string key, double defaultValue)
        {
            if (!HasKey<double>(key))
                return defaultValue;

            return GetDouble(key);
        }

        /// <summary>
        /// Sets the two float values of <paramref name="value"/> with <see cref="PlayerPrefs"/>.
        /// </summary>
        public static void SetVector2(string key, Vector2 value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetVector2] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_X", key), value.x);
            PlayerPrefs.SetFloat(string.Format("{0}_Y", key), value.y);
        }
        /// <summary>
        /// Returns the values set by <see cref="SetVector2(string, Vector2)"/>
        /// </summary>
        public static Vector2 GetVector2(string key)
        {
            return GetVector2(key, Vector2.zero);
        }
        /// <inheritdoc cref="GetVector2(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static Vector2 GetVector2(string key, Vector2 defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::GetVector2] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }

            return new Vector2(
                PlayerPrefs.GetFloat(string.Format("{0}_X", key), defaultValue.x),
                PlayerPrefs.GetFloat(string.Format("{0}_Y", key), defaultValue.y)
            );
        }
        /// <summary>
        /// Sets the three float values of <paramref name="value"/> with <see cref="PlayerPrefs"/>.
        /// </summary>
        public static void SetVector3(string key, Vector3 value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetVector3] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_X", key), value.x);
            PlayerPrefs.SetFloat(string.Format("{0}_Y", key), value.y);
            PlayerPrefs.SetFloat(string.Format("{0}_Z", key), value.z);
        }
        /// <inheritdoc cref="GetVector3(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static Vector3 GetVector3(string key, Vector3 defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::GetVector3] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }

            return new Vector3(
                PlayerPrefs.GetFloat(string.Format("{0}_X", key), defaultValue.x),
                PlayerPrefs.GetFloat(string.Format("{0}_Y", key), defaultValue.y),
                PlayerPrefs.GetFloat(string.Format("{0}_Z", key), defaultValue.z)
            );
        }
        /// <summary>
        /// Returns the values set by <see cref="SetVector2(string, Vector2)"/>
        /// </summary>
        public static Vector3 GetVector3(string key)
        {
            return GetVector3(key, Vector3.zero);
        }
        /// <summary>
        /// Sets the four float values of <paramref name="value"/> with <see cref="PlayerPrefs"/>.
        /// </summary>
        public static void SetColor(string key, Color value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetColor] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_R", key), value.r);
            PlayerPrefs.SetFloat(string.Format("{0}_G", key), value.g);
            PlayerPrefs.SetFloat(string.Format("{0}_B", key), value.b);
            PlayerPrefs.SetFloat(string.Format("{0}_A", key), value.a);
        }
        /// <inheritdoc cref="GetColor(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static Color GetColor(string key, Color defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::GetColor] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }

            return new Color(
                PlayerPrefs.GetFloat(string.Format("{0}_R", key), defaultValue.r),
                PlayerPrefs.GetFloat(string.Format("{0}_G", key), defaultValue.g),
                PlayerPrefs.GetFloat(string.Format("{0}_B", key), defaultValue.b),
                PlayerPrefs.GetFloat(string.Format("{0}_A", key), defaultValue.a)
            );
        }
        /// <summary>
        /// Returns the values set by <see cref="SetColor(string, Color)"/>
        /// </summary>
        public static Color GetColor(string key)
        {
            return GetColor(key, Color.black);
        }

        /// <summary>
        /// Sets an enum value to <see cref="PlayerPrefs"/>.
        /// <br>The type is enforced by the string prefixing.</br>
        /// </summary>
        public static void SetEnum<T>(string key, T value)
#if CSHARP_7_3_OR_NEWER
            where T : Enum
#endif
        {
#if !CSHARP_7_3_OR_NEWER
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException(string.Format("[PlayerPrefsUtility::SetEnum] Error while setting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));
#endif
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetEnum] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            PlayerPrefs.SetInt(string.Format("{0}_ENUM:{1}", key, typeof(T).Name), Convert.ToInt32(value));
        }
        /// <inheritdoc cref="GetEnum{T}(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static T GetEnum<T>(string key, T defaultValue)
#if CSHARP_7_3_OR_NEWER
            where T : Enum
#endif
        {
#if !CSHARP_7_3_OR_NEWER
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException(string.Format("[PlayerPrefsUtility::GetEnum] Error while getting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));
#endif

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetEnum] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }
            string prefixKey = string.Format("{0}_ENUM:{1}", key, typeof(T).Name);

            if (!PlayerPrefs.HasKey(prefixKey))
            {
                return defaultValue;
            }

            return (T)(object)PlayerPrefs.GetInt(prefixKey);
        }
        /// <summary>
        /// Returns a value set by <see cref="SetEnum{T}(string, T)"/>.
        /// </summary>
        public static T GetEnum<T>(string key)
#if CSHARP_7_3_OR_NEWER
    where T : Enum
#endif
        {
            return GetEnum<T>(key, default);
        }

        /// <summary>
        /// Use this method to control whether your save key was serialized as type <typeparamref name="T"/>.
        /// </summary>
        public static bool HasKey<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            // type system abuse
            Type tType = typeof(T);
            if (tType == typeof(Vector2))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_X", key)) && PlayerPrefs.HasKey(string.Format("{0}_Y", key));
            }
            if (tType == typeof(Vector3))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_X", key)) && PlayerPrefs.HasKey(string.Format("{0}_Y", key)) && PlayerPrefs.HasKey(string.Format("{0}_Z", key));
            }
            if (tType == typeof(Color))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_R", key)) && PlayerPrefs.HasKey(string.Format("{0}_G", key))
                    && PlayerPrefs.HasKey(string.Format("{0}_B", key)) && PlayerPrefs.HasKey(string.Format("{0}_A", key));
            }
            if (tType == typeof(bool))
            {
                return PlayerPrefs.HasKey(key);
            }
            if (tType == typeof(long))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_l32Long", key)) && PlayerPrefs.HasKey(string.Format("{0}_u32Long", key));
            }
            if (tType == typeof(double))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_l32Double", key)) && PlayerPrefs.HasKey(string.Format("{0}_u32Double", key));
            }
            if (tType.IsEnum)
            {
                return PlayerPrefs.HasKey(string.Format("{0}_ENUM:{1}", key, typeof(T).Name));
            }

            return PlayerPrefs.HasKey(key);
        }
    }
}
