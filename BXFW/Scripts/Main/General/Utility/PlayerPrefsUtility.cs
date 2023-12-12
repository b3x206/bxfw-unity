using System;
using System.Text;
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
        // TODO : Convert all string.Format's to StringBuilder based stuff
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
                // This should only happen in a case of erroreneous parameters
                // (Unity allows this so that's why i am not throwing an exception)
                Debug.LogError(string.Format("[PlayerPrefsUtility::Set{0}] Couldn't set the savekey because it is null. Key={1}", savePrefix, key));
                return;
            }

            uint lower32 = (uint)(value & uint.MaxValue); // The lower bytes (0 to 2**32)
            uint upper32 = (uint)(value >> 32);           // This does not depend on endianness, i guess? (put upper bytes where the lower bytes would be)
            // String.Format is unoptimized and causes 5kb garbage on achievement trackers on some certain game that i am working.
            StringBuilder keySb = new StringBuilder(key.Length + 4 + savePrefix.Length);

            keySb.Append(key).Append("_l32").Append(savePrefix);
            PlayerPrefs.SetInt(keySb.ToString(), (int)lower32);
            keySb.Replace("_l32", "_u32", key.Length, 4);
            PlayerPrefs.SetInt(keySb.ToString(), (int)upper32);
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

            StringBuilder keySb = new StringBuilder(key.Length + 4 + savePrefix.Length);

            keySb.Append(key).Append("_l32").Append(savePrefix);
            uint lower32 = (uint)PlayerPrefs.GetInt(keySb.ToString());
            keySb.Replace("_l32", "_u32", key.Length, 4);
            uint upper32 = (uint)PlayerPrefs.GetInt(keySb.ToString());

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
            {
                return defaultValue;
            }

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
            {
                return defaultValue;
            }

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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            PlayerPrefs.SetFloat(keySb.ToString(), value.x);
            keySb[keySb.Length - 1] = 'Y';
            PlayerPrefs.SetFloat(keySb.ToString(), value.y);
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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            float xValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.x);
            keySb[keySb.Length - 1] = 'Y';
            float yValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.y);

            return new Vector2(xValue, yValue);
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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            PlayerPrefs.SetFloat(keySb.ToString(), value.x);
            keySb[keySb.Length - 1] = 'Y';
            PlayerPrefs.SetFloat(keySb.ToString(), value.y);
            keySb[keySb.Length - 1] = 'Z';
            PlayerPrefs.SetFloat(keySb.ToString(), value.z);
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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            float xValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.x);
            keySb[keySb.Length - 1] = 'Y';
            float yValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.y);
            keySb[keySb.Length - 1] = 'Z';
            float zValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.z);

            return new Vector3(xValue, yValue, zValue);
        }
        /// <summary>
        /// Returns the values set by <see cref="SetVector3(string, Vector3)"/>
        /// </summary>
        public static Vector3 GetVector3(string key)
        {
            return GetVector3(key, Vector3.zero);
        }

        /// <summary>
        /// Sets the four float values of <paramref name="value"/> with <see cref="PlayerPrefs"/>.
        /// </summary>
        public static void SetVector4(string key, Vector4 value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetVector4] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            PlayerPrefs.SetFloat(keySb.ToString(), value.x);
            keySb[keySb.Length - 1] = 'Y';
            PlayerPrefs.SetFloat(keySb.ToString(), value.y);
            keySb[keySb.Length - 1] = 'Z';
            PlayerPrefs.SetFloat(keySb.ToString(), value.z);
            keySb[keySb.Length - 1] = 'W';
            PlayerPrefs.SetFloat(keySb.ToString(), value.w);
        }
        /// <inheritdoc cref="GetVector3(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static Vector4 GetVector4(string key, Vector4 defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::GetVector4] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_X");
            float xValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.x);
            keySb[keySb.Length - 1] = 'Y';
            float yValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.y);
            keySb[keySb.Length - 1] = 'Z';
            float zValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.z);
            keySb[keySb.Length - 1] = 'W';
            float wValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.w);

            return new Vector4(xValue, yValue, zValue, wValue);
        }
        /// <summary>
        /// Returns the values set by <see cref="SetVector4(string, Vector4)"/>
        /// </summary>
        public static Vector4 GetVector4(string key)
        {
            return GetVector4(key, Vector4.zero);
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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_R");
            PlayerPrefs.SetFloat(keySb.ToString(), value.r);
            keySb[keySb.Length - 1] = 'G';
            PlayerPrefs.SetFloat(keySb.ToString(), value.g);
            keySb[keySb.Length - 1] = 'B';
            PlayerPrefs.SetFloat(keySb.ToString(), value.b);
            keySb[keySb.Length - 1] = 'A';
            PlayerPrefs.SetFloat(keySb.ToString(), value.a);
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

            StringBuilder keySb = new StringBuilder(key + 2);

            keySb.Append(key).Append("_R");
            float rValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.r);
            keySb[keySb.Length - 1] = 'G';
            float gValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.g);
            keySb[keySb.Length - 1] = 'B';
            float bValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.b);
            keySb[keySb.Length - 1] = 'A';
            float aValue = PlayerPrefs.GetFloat(keySb.ToString(), defaultValue.a);

            return new Color(rValue, gValue, bValue, aValue);
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
        public static void SetEnum<T>(string key, T value) where T : Enum
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetEnum] Couldn't set the savekey because it is null. Key={0}", key));
                return;
            }

            string enumTypeName = typeof(T).Name;
            StringBuilder keySb = new StringBuilder(key.Length + 6 + enumTypeName.Length);

            // Allow for setting Long enums as well, always set enum as long
            SetLong(keySb.Append(key).Append("_ENUM:").Append(typeof(T).Name).ToString(), Convert.ToInt64(value));
        }
        /// <inheritdoc cref="GetEnum{T}(string)"/>
        /// <param name="defaultValue">The default value to fallback into if the given <paramref name="key"/> doesn't exist in PlayerPrefs.</param>
        public static T GetEnum<T>(string key, T defaultValue) where T : Enum
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError(string.Format("[PlayerPrefsUtility::SetEnum] Couldn't get the savekey because it is null. Key={0}", key));
                return default;
            }

            string enumTypeName = typeof(T).Name;
            StringBuilder keySb = new StringBuilder(key.Length + 6 + enumTypeName.Length);
            keySb.Append(key).Append("_ENUM:").Append(typeof(T).Name);

            // If the given enum is a shorter value, accept the data loss. The classes of integers implement 'IConvertable'
            // So use the convertable method that takes the 'T's underlying int type as converter
            // As enums can only be exactly converted to that given type, and nothing else. But their underlying integer value is IConvertable
            object intValue = Convert.ChangeType(GetLong(keySb.ToString(), Convert.ToInt64(defaultValue)), typeof(T).GetEnumUnderlyingType());
            return (T)intValue;
        }
        /// <summary>
        /// Returns a value set by <see cref="SetEnum{T}(string, T)"/>.
        /// </summary>
        public static T GetEnum<T>(string key) where T : Enum
        {
            return GetEnum<T>(key, default);
        }

        /// <summary>
        /// Use this method to control whether your save key was serialized as type <typeparamref name="T"/>.
        /// </summary>
        public static bool HasKey<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            // Just when i said that the 'PlayerPrefsUtility' couldn't suck more
            StringBuilder keySb = new StringBuilder(key.Length + 16);
            keySb.Clear();

            // type system abuse
            Type tType = typeof(T);
            if (tType == typeof(Vector2))
            {
                keySb.Append(key).Append("_X");

                bool hasX = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasX)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'Y';
                bool hasY = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasY)
                {
                    return false;
                }

                return true;
            }
            if (tType == typeof(Vector3))
            {
                keySb.Append(key).Append("_X");

                bool hasX = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasX)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'Y';
                bool hasY = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasY)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'Z';
                bool hasZ = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasZ)
                {
                    return false;
                }

                return true;
            }
            if (tType == typeof(Vector4))
            {
                keySb.Append(key).Append("_X");

                bool hasX = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasX)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'Y';
                bool hasY = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasY)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'Z';
                bool hasZ = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasZ)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'W';
                bool hasW = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasW)
                {
                    return false;
                }

                return true;
            }
            if (tType == typeof(Color))
            {
                keySb.Append(key).Append("_R");

                bool hasR = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasR)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'G';
                bool hasG = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasG)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'B';
                bool hasB = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasB)
                {
                    return false;
                }

                keySb[keySb.Length - 1] = 'A';
                bool hasA = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasA)
                {
                    return false;
                }

                return true;
            }
            if (tType == typeof(bool))
            {
                return PlayerPrefs.HasKey(key);
            }
            if (tType == typeof(long))
            {
                keySb.Append(key).Append("_l32Long");
                bool hasL32 = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasL32)
                {
                    return false;
                }

                keySb.Replace("_l32", "_u32", key.Length, 4);
                bool hasU32 = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasU32)
                {
                    return false;
                }

                return true;
            }
            if (tType == typeof(double))
            {
                keySb.Append(key).Append("_l32Double");
                bool hasL32 = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasL32)
                {
                    return false;
                }

                keySb.Replace("_l32", "_u32", key.Length, 4);
                bool hasU32 = PlayerPrefs.HasKey(keySb.ToString());
                if (!hasU32)
                {
                    return false;
                }

                return true;
            }
            if (tType.IsEnum)
            {
                return PlayerPrefs.HasKey(keySb.Append(key).Append("_ENUM:").Append(typeof(T).Name).ToString());
            }

            return PlayerPrefs.HasKey(key);
        }
    }
}
