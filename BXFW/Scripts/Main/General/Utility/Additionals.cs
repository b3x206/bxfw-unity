using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// Serialization
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BXFW
{
    /// <summary>
    /// Transform Axis.
    /// <br>(Used in few helper methods.)</br>
    /// </summary>
    public enum TransformAxis
    {
        None = 0,

        XAxis,
        YAxis,
        ZAxis,

        XYAxis,
        YZAxis,
        XZAxis,

        // All Axis
        XYZAxis
    }

    /// <summary>
    /// Quaternion axis.
    /// Isn't used for anything useful.
    /// </summary>
    public enum QuatAxis
    {
        // P(4,0)
        None = 0,

        // P(4,1)
        XAxis = 1,
        YAxis = 2,
        ZAxis = 3,
        WAxis = 4,

        // P(4,2)
        XYAxis = 5,
        XZAxis = 6,
        XWAxis = 7,
        YZAxis = 8,
        YWAxis = 9,
        ZWAxis = 10,

        // P(4,3)
        XYZAxis = 11,
        XYWAxis = 12,
        YZWAxis = 13,
        XZWAxis = 14,

        // P(4,4)
        XYZWAxis = 15
    }

    /// <summary>
    /// Transform axis, used in 2D space.
    /// <br>NOTE : This is an axis value for position.
    /// For rotation, please use the <see cref="TransformAxis"/> (or <see cref="QuatAxis"/>).</br>
    /// </summary>
    [Flags]
    public enum TransformAxis2D
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,

        XYAxis = XAxis | YAxis
    }

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
                return string.Format("/{0}", target.name);

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
            float explosionDistance = (explosionDir.magnitude / explosionRadius);

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

            rb.AddForce(Mathf.Lerp(0, explosionForce, (1 - explosionDistance)) * explosionDir, mode);
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
            return IsTypeNameInteger(typeName) ||
                IsTypeNameFloat(typeName);
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
        /// Get an iterator of the base types of <paramref name="type"/>.
        /// <br>Returns a blank iterator if no base type.</br>
        /// </summary>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type.BaseType == null)
                return type.GetInterfaces();

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

        #region Obsolete
        // !!--- This entire section is planned to be removed ---!! //
        // !!--- It will be removed in the beta release       ---!! //

        // -- Mesh + Transform
        /// <summary>
        /// Converts the vertices of <paramref name="mesh"/> into world space using <paramref name="matrixSpace"/>.
        /// <br>Values are assigned into the 'vertsArray', the 'vertsArray' will be overwritten by <see cref="Mesh.GetVertices(List{Vector3})"/>.</br>
        /// </summary>
        [Obsolete("Use 'MeshUtility' instead", false)]
        public static void VerticesToMatrixSpaceNoAlloc(Mesh mesh, Matrix4x4 matrixSpace, List<Vector3> vertsArray)
        {
            MeshUtility.VerticesToMatrixSpaceNoAlloc(mesh, matrixSpace, vertsArray);
        }
        /// <summary>
        /// Converts the vertices of <paramref name="mesh"/> into world space using <paramref name="matrixSpace"/>.
        /// <br>Allocates a new <see cref="List{T}"/> every time it's called.</br>
        /// </summary>
        [Obsolete("Use 'MeshUtility' instead", false)]
        public static List<Vector3> VerticesToMatrixSpace(Mesh mesh, Matrix4x4 matrixSpace)
        {
            return MeshUtility.VerticesToMatrixSpace(mesh, matrixSpace);
        }

        // -- Math
        /// <inheritdoc cref="MathUtility.Map"/>
        [Obsolete("Use 'MathUtility' instead.", false)]
        public static float Map(float from, float to, float from2, float to2, float value)
        {
            return MathUtility.Map(from, to, from2, to2, value);
        }

        #region Serializing

        #region PlayerPrefs
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetBool(string SaveKey, bool Value)
        {
            PlayerPrefsUtility.SetBool(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead\n(This method has different behaviour, as it will only consider values higher or equal to 1 as true. The new method checks for inequality to zero.)", false)]
        public static bool GetBool(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogWarning(string.Format("[Additionals::GetBool] The key is null. It will return false. Key={0}", SaveKey));
                return false;
            }
            else
            {
                return PlayerPrefs.GetInt(SaveKey) >= 1;
            }
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetLong(string SaveKey, long Value)
        {
            PlayerPrefsUtility.SetLong(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static long GetLong(string SaveKey)
        {
            return PlayerPrefsUtility.GetLong(SaveKey);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetDouble(string SaveKey, double Value)
        {
            PlayerPrefsUtility.SetDouble(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static double GetDouble(string SaveKey)
        {
            return PlayerPrefsUtility.GetDouble(SaveKey);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetVector2(string SaveKey, Vector2 Value)
        {
            PlayerPrefsUtility.SetVector2(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static Vector2 GetVector2(string SaveKey)
        {
            return PlayerPrefsUtility.GetVector2(SaveKey);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetVector3(string SaveKey, Vector3 Value)
        {
            PlayerPrefsUtility.SetVector3(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static Vector3 GetVector3(string SaveKey)
        {
            return PlayerPrefsUtility.GetVector3(SaveKey);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetColor(string SaveKey, Color Value)
        {
            PlayerPrefsUtility.SetColor(SaveKey, Value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static Color GetColor(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::GetColor] Couldn't get the savekey because it is null. Key={0}", SaveKey));
                return default;
            }

            return new Color(PlayerPrefs.GetFloat(string.Format("{0}_R", SaveKey)), PlayerPrefs.GetFloat(string.Format("{0}_G", SaveKey)),
                PlayerPrefs.GetFloat(string.Format("{0}_B", SaveKey)), PlayerPrefs.GetFloat(string.Format("{0}_A", SaveKey)));
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static void SetEnum<T>(string SaveKey, T value)
#if CSHARP_7_3_OR_NEWER
            where T : Enum
#endif
        {
            PlayerPrefsUtility.SetEnum(SaveKey, value);
        }
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static T GetEnum<T>(string SaveKey)
#if CSHARP_7_3_OR_NEWER
            where T : Enum
#endif
        {
            return PlayerPrefsUtility.GetEnum<T>(SaveKey);
        }
        /// <summary>
        /// Use this method to control whether your save key was serialized as type <typeparamref name="T"/>.
        /// </summary>
        [Obsolete("Use the 'PlayerPrefsUtility' class instead", false)]
        public static bool HasPlayerPrefsKey<T>(string SaveKey)
        {
            return PlayerPrefsUtility.HasKey<T>(SaveKey);
        }
        #endregion

        #region Binary Serializer
        /// <summary>
        /// This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile.
        /// </summary>
        public sealed class VersionDeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
                {
                    assemblyName = Assembly.GetExecutingAssembly().FullName;

                    // The following line of code returns the type. 
                    Type typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

                    return typeToDeserialize;
                }

                return null;
            }
        }

        /// <summary>
        /// Saves the object as binary.
        /// <br>NOTE : These binary serialization functions are unsafe, refrain from using them as much as possible.</br>
        /// </summary>
        /// <typeparam name="T">Type of object. Can be anything as long as it has the <see cref="SerializableAttribute"/>.</typeparam>
        /// <param name="serializableObject">The object itself.</param>
        /// <param name="filePath">The file path to save.</param>
        /// <param name="OverWrite">Should we overwrite our save?</param>
        [Obsolete("Binary serialization is unsafe", true)]
        public static void BSave<T>(T serializableObject, string filePath, bool OverWrite = false)
        {
            // Make sure the generic is serializable.
            if (typeof(T).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::BSave] Given type '{0}' does not have the [System.Serializable] attribute.", typeof(T).Name));
            }

            try
            {
                if (OverWrite)
                {
                    File.Delete(filePath);
                }
                else if (File.Exists(filePath))
                {
                    Debug.Log(string.Format("[Additionals::BSave] File '{0}' already exists, creating new file name.", filePath));

                    /// File path for <see cref="Directory.GetFiles(string)"/>
                    string modifiedFilePath = null;
                    // Cutting everything after the last slash
                    // NOTE : Use backward slash for winbloat : '\'
#if UNITY_EDITOR_WIN
                    char filePathSeperatorChar = '\\';
#elif UNITY_EDITOR
                    // Non-windows editor?
                    char filePathSeperatorChar = '/';
#else
                    // If windows, use the line seperator as '\\'.
                    char filePathSeperatorChar = Application.platform == RuntimePlatform.WindowsPlayer ? '\\' : '/';
#endif

                    int IndexOfmodifiedPath = filePath.LastIndexOf(filePathSeperatorChar);
                    if (IndexOfmodifiedPath > 0)
                    {
                        // To remove everything after the last '\'
                        modifiedFilePath = filePath.Remove(IndexOfmodifiedPath);
                    }

                    // Parsing the file directory to parts for overriding.
                    string[] existing = Directory.GetFiles(modifiedFilePath, "*.bytes", SearchOption.TopDirectoryOnly);
                    string[] splitLast_Existing = existing[existing.Length - 1].Split(filePathSeperatorChar);
                    string LastName = splitLast_Existing[splitLast_Existing.Length - 1];

                    string[] splitLast_Extension = LastName.Split('.');

                    // Cutting the extension
                    string FileExtension = string.Format(".{0}", splitLast_Extension[splitLast_Extension.Length - 1]);
                    string FileName = null;
                    // If the size is larger than 2, we assume the stupid user has put dots inside the file name. 
                    // Generate FileName string 
                    if (splitLast_Extension.Length > 2)
                    {
                        // This for loop 'might' be flawed because of the length 
                        for (int i = 0; i < splitLast_Extension.Length - 1; i++)
                        {
                            // Split ignores the dots
                            FileName += string.Format("{0}.", splitLast_Extension[i]);
                        }
                    }
                    else
                    {
                        FileName = splitLast_Extension[0];
                    }

                    // Cutting everything after the last slash
                    int IndexOfFilePath = filePath.LastIndexOf('\\');
                    if (IndexOfFilePath > 0)
                    {
                        // To remove everything after the last '\' |
                        // Incremented 1 as we need the last '\'   |
                        filePath = filePath.Remove(IndexOfFilePath + 1);
                        // Generate new filePath.
                        filePath += string.Format("{0}{1}{2}", FileName, existing.Length, FileExtension);
                    }
                }

                using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    BinaryFormatter bformatter = new BinaryFormatter
                    {
                        Binder = new VersionDeserializationBinder()
                    };

                    stream.Position = 0;
                    bformatter.Serialize(stream, serializableObject);
                }
            }
            catch (Exception e)
            {
                // This can be generalized into 'SerializationException'
                throw new SerializationException(string.Format("[Additionals::Load] An error occured while deserializing.\n->{0}\n->{1}", e.Message, e.StackTrace));
            }
        }
        /// <summary>
        /// Loads binary saved data from path.
        /// <br>NOTE : These binary serialization functions are unsafe, refrain from using them as much as possible.</br>
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="filePath">File path to load from.</param>
        /// <returns>Data that is loaded. NOTE : Please don't invoke/parse/do anything with this data.</returns>
        [Obsolete("Binary serialization is unsafe", true)]
        public static ExpectT BLoad<ExpectT>(string filePath)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::BLoad] Given type '{0}' does not have the [System.Serializable] attribute.", typeof(ExpectT).Name));
            }

            ExpectT DSerObj;

            using (Stream stream = File.OpenRead(filePath))
            {
                BinaryFormatter bformatter = new BinaryFormatter
                {
                    Binder = new VersionDeserializationBinder()
                };

                stream.Position = 0;
                // You should use json instead anyway, anyone can inject custom data that will cause an issue.
                DSerObj = (ExpectT)bformatter.Deserialize(stream);
            }

            return DSerObj;
        }
        /// <summary>
        /// Loads binary saved data from text.
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="fileContents">The content of the folder to make data from. Make sure the file is utf8.</param>
        /// <returns></returns>
        [Obsolete("Binary serialization is unsafe", true)]
        public static ExpectT BLoad<ExpectT>(char[] fileContents)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::BLoad] Given type '{0}' does not have the [System.Serializable] attribute.", typeof(ExpectT).Name));
            }

            byte[] fileContentData = new byte[fileContents.Length];
            for (int i = 0; i < fileContents.Length; i++)
            {
                fileContentData[i] = Convert.ToByte(fileContents[i]);
            }

            return BLoad<ExpectT>(fileContentData);
        }
        /// <summary>
        /// Loads binary saved data from bytes.
        /// </summary>
        /// <typeparam name="ExpectT"></typeparam>
        /// <param name="fileContents">The content of the folder to make data from.</param>
        /// <returns></returns>
        [Obsolete("Binary serialization is unsafe", true)]
        public static ExpectT BLoad<ExpectT>(byte[] fileContents)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::BLoad] Given type '{0}' does not have the [System.Serializable] attribute.", typeof(ExpectT).Name));
            }

            ExpectT DSerObj;

            using (MemoryStream ms = new MemoryStream(fileContents))
            {
                BinaryFormatter bformatter = new BinaryFormatter
                {
                    Binder = new VersionDeserializationBinder()
                };

                ms.Position = 0;
                DSerObj = (ExpectT)bformatter.Deserialize(ms);
            }

            return DSerObj;
        }

        /// <summary>
        /// Returns a byte array from an object.
        /// </summary>
        /// <param name="obj">Object that has the <see cref="SerializableAttribute"/>.</param>
        /// <returns>Object as serializd byte array.</returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException("[Additionals::ObjectToByteArray] The given object is null.");
            }

            // Require attribute.
            if (obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                throw new ArgumentException(string.Format("[Additionals::ObjectToByteArray] Given type '{0}' does not have the [System.Serializable] attribute.", obj));
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        #endregion

        #endregion

        #endregion
    }
}
