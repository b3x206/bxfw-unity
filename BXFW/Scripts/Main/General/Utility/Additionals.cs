using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// Serialization
using System.Runtime.Serialization.Formatters.Binary;

namespace BXFW
{
    /// <summary>
    /// The additionals class.
    /// <br>Contains additionals that doesn't exactly fit somewhere, or it is too small that it doesn't have it's own class.</br>
    /// </summary>
    public static class Additionals
    {
        // -- GameObject / Transform
        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this Component target)
        {
            return target.transform.GetPath();
        }
        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this GameObject target)
        {
            return target.transform.GetPath();
        }
        /// <summary>
        /// Get the path of the gameobject.
        /// <br>This is useful for <see cref="Debug.Log"/>-ging with better troubleshooting.</br>
        /// </summary>
        public static string GetPath(this Transform target)
        {
            if (target.parent == null)
            {
                return string.Format("/{0}", target.name);
            }

            return string.Format("{0}/{1}", target.parent.GetPath(), target.name);
        }
        /// <summary>
        /// Scales a transform <paramref name="target"/> around <paramref name="pivot"/>, using the given <paramref name="scale"/> and <paramref name="pivotSpace"/>.
        /// <br>Note : May put your transform into a floating point number heaven. (<see cref="float.PositiveInfinity"/> or <see cref="float.NaN"/>)</br>
        /// <br>Use with caution as there's no clamping present. </br>
        /// </summary>
        /// <param name="pivotSpace">
        /// Space of the pivot. 
        /// <br>When this is set to 'World', the <paramref name="pivot"/> is untouched.</br>
        /// <br>
        /// When this is set to 'Self', the <paramref name="pivot"/> is converted to a transform point using <see cref="Transform.InverseTransformPoint(Vector3)"/>.
        /// This gets affected by stuff like rotation of <paramref name="target"/> and stuff etc.
        /// </br>
        /// <br>To use a consistent point as pivot, use the given <c>transform's position + constant pivot point you want</c> with the parameter <paramref name="pivot"/>.</br>
        /// </param>
        public static void ScaleAroundPivot(this Transform target, Vector3 pivot, Vector3 scale, Space pivotSpace = Space.World)
        {
            // Necessary points
            Vector3 targetPoint = target.localPosition;
            Vector3 pivotPoint;
            switch (pivotSpace)
            {
                default:
                case Space.World:
                    pivotPoint = pivot;
                    break;
                case Space.Self:
                    pivotPoint = target.InverseTransformPoint(pivot);
                    break;
            }
            Vector3 diff = targetPoint - pivotPoint;

            // Scale relative to previous one (TODO : Make relativity calculation more accurate, this will do for uniform scales)
            float relativeScale = scale.x / target.transform.localScale.x;
            // damn operator precedence, please put paranthesis correctly before going insane for an hour
            // Move the relative scale amount from pivot point (as we assume the pivot is the given 'pivotPoint')
            Vector3 finalPos = pivotPoint + (diff * relativeScale);

            target.localScale = scale;
            target.localPosition = finalPos;
        }

        // -- Rigidbody
        /// <summary>
        /// Clamps rigidbody velocity.
        /// </summary>
        public static void ClampVelocity(this Rigidbody rb, float MaxSpeed)
        {
            if (rb == null)
            {
                Debug.LogError("[Additionals::ClampVelocity] The referenced rigidbody is null.");
                return;
            }

            // Trying to Limit Speed
            if (rb.velocity.magnitude > MaxSpeed)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxSpeed);
            }
        }
        /// <summary>
        /// Adds an explosion to rigidbody.
        /// </summary>
        public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0f, ForceMode2D mode = ForceMode2D.Force)
        {
            Vector2 explosionDir = rb.position - explosionPosition;
            float explosionDistance = explosionDir.magnitude / explosionRadius;

            // Normalize without computing magnitude again
            if (upwardsModifier == 0)
            {
                explosionDir /= explosionDistance;
            }
            else
            {
                // If you pass a non-zero value for the upwardsModifier parameter, the direction
                // will be modified by subtracting that value from the Y component of the centre point.
                explosionDir.y += upwardsModifier;
                explosionDir.Normalize();
            }

            rb.AddForce(Mathf.Lerp(0f, explosionForce, (1f - explosionDistance)) * explosionDir, mode);
        }

        // --- SpriteRenderer + Camera
        /// <summary>
        /// Resizes an sprite renderer to the size of the <b>orthographic</b> camera fit.
        /// </summary>
        /// <param name="relativeCam">Orthographic camera to resize.</param>
        /// <param name="sr">Sprite renderer to resize.</param>
        /// <param name="axis">Axis to resize.</param>
        public static void ResizeSpriteToScreen(this Camera relativeCam, SpriteRenderer sr, TransformAxis2D axis = TransformAxis2D.XYAxis)
        {
            if (sr == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }

            sr.transform.localScale = new Vector3(1, 1, 1);

            float width = sr.sprite.bounds.size.x;
            float height = sr.sprite.bounds.size.y;

            float worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            Vector3 scale;
            switch (axis)
            {
                default:
                case TransformAxis2D.XYAxis:
                    scale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.XAxis:
                    scale = new Vector3(worldScreenWidth / width, sr.transform.localScale.y, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.YAxis:
                    scale = new Vector3(sr.transform.localScale.x, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
            }

            if (sr.drawMode != SpriteDrawMode.Tiled)
            {
                sr.transform.localScale = scale;
            }
            else
            {
                // If the drawing is tiled, resize to screen using the tile properties.
                sr.size = scale;
            }
        }
        /// <summary>
        /// Resizes an mesh renderer to the size of the <b>orthographic</b> camera fit.
        /// </summary>
        /// <param name="relativeCam">Orthographic camera to resize.</param>
        /// <param name="sr">Sprite renderer to resize.</param>
        /// <param name="axis">Axis to resize.</param>
        public static void ResizeMeshToScreen(this Camera relativeCam, MeshFilter sr, TransformAxis2D axis = TransformAxis2D.XYAxis)
        {
            if (sr == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }
            if (!relativeCam.orthographic)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] Camera relative to resize isn't ortographic. The resizing won't be correct.");
            }

            sr.transform.localScale = new Vector3(1, 1, 1);

            float width = sr.mesh.bounds.size.x;
            float height = sr.mesh.bounds.size.y;

            float worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            switch (axis)
            {
                // Do nothing if none.
                case TransformAxis2D.None:
                    Debug.LogWarning("[Additionals::ResizeSpriteToScreen] TransformAxis2D to resize was none.");
                    return;

                default:
                case TransformAxis2D.XYAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.XAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, sr.transform.localScale.y, sr.transform.localScale.z);
                    break;
                case TransformAxis2D.YAxis:
                    sr.transform.localScale =
                        new Vector3(sr.transform.localScale.x, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
            }
        }

        // -- Random
        /// <summary>
        /// Returns a random boolean.
        /// </summary>
        public static bool RandomBool()
        {
            // Using floats here is faster and more random.
            // (for some reason, maybe the System.Convert.ToBoolean method takes more time than float comparison?)
            return UnityEngine.Random.Range(0f, 1f) > .5f;
        }

        // -- FileSystem
        /// <summary>
        /// Copies the given directory.
        /// </summary>
        /// <param name="sourceDirName">The given directory to copy. If this does not exist then <see cref="DirectoryNotFoundException"/> is thrown.</param>
        /// <param name="destDirName">
        /// The destination directory to copy into. This is created if it doesn't exist, 
        /// and it shouldn't be the same as <paramref name="sourceDirName"/>.
        /// </param>
        /// <param name="copySubDirs">Whether to also copy the subdirectories on the <paramref name="sourceDirName"/> into <paramref name="destDirName"/>. (recursive method)</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format("[Additionals::DirectoryCopy] Source directory does not exist or could not be found: {0}", sourceDirName));
            }

            if (sourceDirName.Equals(destDirName))
            {
                Debug.LogWarning("[Additionals::DirectoryCopy] The directory you are trying to copy is the same as the destination directory.");
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, copySubDirs);
                }
            }
        }

        // -- Type
        /// <summary>
        /// Returns whether if the <paramref name="typeName"/> has an integral name.
        /// <br>Useful to compare <see cref="UnityEditor.SerializedProperty.type"/> to check.</br>
        /// </summary>
        public static bool IsTypeNameInteger(string typeName)
        {
            return typeName == typeof(byte).Name ||
                typeName == typeof(sbyte).Name ||
                typeName == typeof(short).Name ||
                typeName == typeof(ushort).Name ||
                typeName == typeof(int).Name ||
                typeName == typeof(uint).Name ||
                typeName == typeof(long).Name ||
                typeName == typeof(ulong).Name ||
                typeName == typeof(IntPtr).Name ||
                typeName == typeof(UIntPtr).Name ||
                // SerializedProperty.type
                typeName == "int" ||
                typeName == "long";
        }
        /// <summary>
        /// Returns whether if the type name is a floating point number type.
        /// <br>Compares <paramref name="typeName"/> to <see cref="float"/>, <see cref="double"/> or <see cref="decimal"/>.</br>
        /// </summary>
        public static bool IsTypeNameFloat(string typeName)
        {
            return typeName == typeof(float).Name ||
                typeName == typeof(double).Name ||
                typeName == typeof(decimal).Name ||
                // SerializedProperty.type
                typeName == "float" ||
                typeName == "double";
        }
        /// <summary>
        /// Returns whether if the <paramref name="typeName"/> has a numerical name.
        /// <br>The difference between <see cref="IsTypeNameInteger(string)"/> is that the type name is also compared against <see cref="float"/> and <see cref="double"/>.</br>
        /// <br>Useful to compare <see cref="UnityEditor.SerializedProperty.type"/> to check.</br>
        /// </summary>
        public static bool IsTypeNameNumerical(string typeName)
        {
            return IsTypeNameInteger(typeName) || IsTypeNameFloat(typeName);
        }

        /// <summary>
        /// Returns whether if <paramref name="type"/> is a built-in c# integer type.
        /// </summary>
        public static bool IsTypeInteger(Type type)
        {
            return IsTypeNameInteger(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is a built-in c# integer type.
        /// </summary>
        public static bool IsTypeInteger<T>()
        {
            return IsTypeInteger(typeof(T));
        }
        /// <summary>
        /// Returns whether if <paramref name="type"/> is a built-in c# floating point number type.
        /// </summary>
        public static bool IsTypeFloat(Type type)
        {
            return IsTypeNameFloat(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is a built-in c# floating point number type.
        /// </summary>
        public static bool IsTypeFloat<T>()
        {
            return IsTypeFloat(typeof(T));
        }
        /// <summary>
        /// Returns whether if <paramref name="type"/> is an numerical type.
        /// <br>Checks type name against <see cref="float"/> and <see cref="double"/> also, unlike <see cref="IsTypeInteger(Type)"/></br>
        /// </summary>
        public static bool IsTypeNumerical(Type type)
        {
            return IsTypeNameNumerical(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is an numerical type.
        /// <br>Checks type name against <see cref="float"/> and <see cref="double"/> also, unlike <see cref="IsTypeInteger{T}()"/></br>
        /// </summary>
        public static bool IsTypeNumerical<T>()
        {
            return IsTypeNumerical(typeof(T));
        }

        /// <summary>
        /// Returns whether if the type is a nullable one.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsTypeNullable(this Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t), "[SerializableDictionary::TypeIsNullable] Given argument was null.");
            }

            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }
        /// <summary>
        /// Returns whether if the given type is assignable from generic type <paramref name="openGenericType"/>.
        /// <br>Can be used/tested against <b>open generic</b> types and <paramref name="target"/>'s base types are checked recursively.</br>
        /// </summary>
        public static bool IsAssignableFromOpenGeneric(this Type target, Type openGenericType)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "[Additionals::IsAssignableFromOpenGeneric] Given argument was null.");
            }
            if (openGenericType == null)
            {
                return false;
            }

            // target      => given type
            // genericType => Generic<> (open type)

            // Can be assigned using interface (can be checked only once, GetInterfaces returns all interfaces)
            if (target.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == openGenericType))
            {
                return true;
            }

            Type iterTarget = target;
            do
            {
                // Can be assigned directly (with open type)
                if (iterTarget.IsGenericType && iterTarget.GetGenericTypeDefinition() == openGenericType)
                {
                    return true;
                }

                iterTarget = iterTarget.BaseType;
            }
            while (iterTarget != null);

            // Reached end of base type
            return false;
        }
        /// <summary>
        /// Returns a list of base generic types inside given <paramref name="type"/>,
        /// mapped accordingly to the dictionary of it's base inheriting generic types.
        /// <br>The keys of the given dictionary is open generic types and the values are the keys generic arguments.</br>
        /// </summary>
        public static Dictionary<Type, Type[]> GetBaseGenericTypeArguments(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "[Additionals::GetBaseGenericTypeArguments] Given argument was null.");
            }    

            Dictionary<Type, Type[]> baseTypePairs = new Dictionary<Type, Type[]>(4);

            Type iterTarget = type;
            do
            {
                if (iterTarget.IsGenericType)
                {
                    // Set dictionary not null if element was added.
                    baseTypePairs.Add(iterTarget.GetGenericTypeDefinition(), iterTarget.GetGenericArguments());
                }

                iterTarget = iterTarget.BaseType;
            }
            while (iterTarget != null);

            return baseTypePairs;
        }

        /// <summary>
        /// Get an iterator of the base types of <paramref name="type"/>.
        /// <br>Returns a blank iterator if no base type.</br>
        /// </summary>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type.BaseType == null)
            {
                return type.GetInterfaces();
            }

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(type.BaseType.GetBaseTypes());
        }
        /// <summary>Get types that has the <paramref name="attributeType"/> attribute from <see cref="Assembly"/> <paramref name="attbAsm"/>.</summary>
        /// <returns>The types with the attribute <paramref name="attributeType"/>.</returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType, Assembly attbAsm = null)
        {
            if (attbAsm == null)
            {
                attbAsm = attributeType.Assembly;
            }

            foreach (Type type in attbAsm.GetTypes())
            {
                if (type.GetCustomAttributes(attributeType, true).Length > 0)
                {
                    yield return type;
                }
            }
        }
        /// <summary>
        /// Gets types that inherit from 'T'.
        /// </summary>
        public static IEnumerable<Type> GetEnumerableOfType<T>() where T : class
        {
            return Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where((Type myType) => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
        }

        /// <summary>
        /// Returns a byte array from an object.
        /// </summary>
        /// <param name="obj">Object that has <see cref="SerializableAttribute"/>.</param>
        /// <returns>Object as serializd byte array.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static byte[] ObjectToByteArray(object obj)
        {
            // Can't convert null object.
            if (obj is null)
            {
                throw new ArgumentNullException("[Additionals::ObjectToByteArray] The given object is null.");
            }
            // Require SerializableAttribute
            if (obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::ObjectToByteArray] Given object '{0}' with type '{1}' does not have the [System.Serializable] attribute.", obj, obj.GetType()));
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
