// Standard
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

// Serialization
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace BXFW
{
    /// <summary>
    /// The additionals class.
    /// INFO : This class depends on unity engine.
    /// </summary>
    public static class Additionals
    {
        #region Unity Functions

        #region Gizmo View
        /// <summary>Draws an arrow to the unity scene using <see cref="Gizmos"/> class.</summary>
        /// <param name="pos">Start position of the arrow.</param>
        /// <param name="direction">Direction point of the arrow.</param>
        /// <param name="arrowHeadLength">Head side rays length.</param>
        /// <param name="arrowHeadAngle">Head side rays angle.</param>
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;

            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        /// <inheritdoc cref="Additionals.DrawArrowGizmos(Vector3, Vector3, float, float)"/>
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;

            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        /// <summary>Draws an arrow to the unity scene using <see cref="Debug"/> class.</summary>
        /// <param name="pos">Start position of the arrow.</param>
        /// <param name="direction">Direction point of the arrow.</param>
        /// <param name="arrowHeadLength">Head side rays length.</param>
        /// <param name="arrowHeadAngle">Head side rays angle.</param>
        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;

            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        /// <inheritdoc cref="Additionals.DrawArrowDebug(Vector3, Vector3, float, float)"/>
        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;

            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
        #endregion

        /// <summary>Clamps rigidbody velocity.</summary>
        public static void ClampVelocity(this Rigidbody rb, float MaxSpeed)
        {
            if (rb is null)
            {
                Debug.LogError("<color=red>[Additionals::ClampVelocity] The referenced rigidbody is null.</color>");
                return;
            }

            // Trying to Limit Speed
            if (rb.velocity.magnitude > MaxSpeed)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxSpeed);
            }
        }
        /// <summary>Converts the vertices of this mesh filter to the world space.</summary>
        public static Vector3[] VerticesToWorldSpace(this MeshFilter filter, bool removeDuplicateVert = true)
        {
            if (filter == null)
            {
                Debug.LogWarning("[Additionals::VerticesToWorldSpace] The mesh filter reference is null.");
                return new Vector3[0];
            }
            if (filter.mesh == null)
            {
                Debug.LogWarning("[Additionals::VerticesToWorldSpace] The mesh filter mesh is null.");
                return new Vector3[0];
            }

            Matrix4x4 localToWorld = filter.transform.localToWorldMatrix;
            Vector3[] world_v = new Vector3[filter.mesh.vertices.Length];

            for (int i = 0; i < filter.mesh.vertices.Length; i++)
            {
                var setWorldVert = localToWorld.MultiplyPoint3x4(filter.mesh.vertices[i]);
                if (Array.IndexOf(world_v, setWorldVert) <= -1 && removeDuplicateVert)
                    continue;

                world_v[i] = setWorldVert;
            }

            return world_v;
        }
        /// <summary>Converts the vertices of this mesh filter to the world space.</summary>
        public static Vector3[] VerticesToWorldSpace(this BoxCollider coll)
        {
            if (coll == null)
            {
                Debug.LogWarning("[Additionals::VerticesToWorldSpace] The collider reference is null.");
                return new Vector3[0];
            }

            var vertices = new Vector3[8];
            var thisMatrix = coll.transform.localToWorldMatrix;
            var storedRotation = coll.transform.rotation;
            coll.transform.rotation = Quaternion.identity;

            var extents = coll.bounds.extents;
            vertices[0] = thisMatrix.MultiplyPoint3x4(extents);
            vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
            vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
            vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
            vertices[4] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
            vertices[5] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
            vertices[6] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
            vertices[7] = thisMatrix.MultiplyPoint3x4(-extents);

            coll.transform.rotation = storedRotation;
            return vertices;
        }
        public static float GetBiggestAxis(this Vector3 target)
        {
            if (target.x > target.y && target.x > target.z)
            {
                return target.x;
            }
            if (target.y > target.x && target.y > target.z)
            {
                return target.y;
            }
            if (target.z > target.y && target.z > target.x)
            {
                return target.z;
            }

            return target.x;
        }
        /// <summary>Returns the keyboard height ratio.</summary>
        public static float GetKeyboardHeightRatio(bool includeInput)
        {
            return Mathf.Clamp01((float)GetKeyboardHeight(includeInput) / Display.main.systemHeight);
        }
        /// <summary>Returns the keyboard height in display pixels. </summary>
        public static int GetKeyboardHeight(bool includeInput)
        {
#if UNITY_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            var view = unityPlayer.Call<AndroidJavaObject>("getView");
            var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");

            if (view == null || dialog == null)
                return 0;

            var decorHeight = 0;

            if (includeInput)
            {
                var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");

                if (decorView != null)
                    decorHeight = decorView.Call<int>("getHeight");
            }

            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);
                return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
            }
        }
#else
            var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
            return height >= Display.main.systemHeight ? 0 : height;
#endif
        }
        public static string GetGameObjectPath(this GameObject obj)
        {
            string path = $"/{obj.name}";
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = $"/{obj.name}{path}";
            }

            return path;
        }
        /// <summary>Sets or removes the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.</summary>
        public static Vector3 SetVectorUsingTransformAxis(this TransformAxis axisConstraint, Vector3 current, Vector3 setCurrent)
        {
            var v = current;
            switch (axisConstraint)
            {
                case TransformAxis.None:
                    return Vector3.zero;

                case TransformAxis.XAxis:
                    v.x = setCurrent.x;
                    break;
                case TransformAxis.YAxis:
                    v.y = setCurrent.y;
                    break;
                case TransformAxis.ZAxis:
                    v.z = setCurrent.z;
                    break;
                case TransformAxis.XYAxis:
                    v.x = setCurrent.x;
                    v.y = setCurrent.y;
                    break;
                case TransformAxis.YZAxis:
                    v.y = setCurrent.y;
                    v.z = setCurrent.z;
                    break;
                case TransformAxis.XZAxis:
                    v.x = setCurrent.x;
                    v.z = setCurrent.z;
                    break;

                default:
                case TransformAxis.XYZAxis:
                    return setCurrent;
            }

            return v;
        }
        /// <summary>Get the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.</summary>
        public static Vector3 GetVectorUsingTransformAxis(this TransformAxis axisConstraint, Vector3 current)
        {
            var v = current;
            switch (axisConstraint)
            {
                case TransformAxis.None:
                    return Vector3.zero;

                case TransformAxis.XAxis:
                    v.y = 0f;
                    v.z = 0f;
                    break;
                case TransformAxis.YAxis:
                    v.x = 0f;
                    v.z = 0f;
                    break;
                case TransformAxis.ZAxis:
                    v.x = 0f;
                    v.y = 0f;
                    break;
                case TransformAxis.XYAxis:
                    v.z = 0f;
                    break;
                case TransformAxis.YZAxis:
                    v.x = 0f;
                    break;
                case TransformAxis.XZAxis:
                    v.y = 0f;
                    break;

                default:
                case TransformAxis.XYZAxis:
                    return v;
            }

            return v;
        }
        /// <summary>Sets or removes the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.</summary>
        public static Vector2 SetVectorUsingTransformAxis(this TransformAxis2D axisConstraint, Vector2 current, Vector2 setCurrent)
        {
            var v = current;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    return Vector2.zero;

                case TransformAxis2D.XAxis:
                    v.x = setCurrent.x;
                    break;
                case TransformAxis2D.YAxis:
                    v.y = setCurrent.y;
                    break;
                default:
                case TransformAxis2D.XYAxis:
                    v.x = setCurrent.x;
                    v.y = setCurrent.y;
                    break;
            }

            return v;
        }
        /// <summary>Get the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.</summary>
        public static Vector2 GetVectorUsingTransformAxis(this TransformAxis2D axisConstraint, Vector2 current)
        {
            var v = current;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    return Vector2.zero;

                default:
                case TransformAxis2D.XYAxis:
                    break;

                case TransformAxis2D.XAxis:
                    v.y = 0f;
                    break;
                case TransformAxis2D.YAxis:
                    v.x = 0f;
                    break;
            }

            return v;
        }
        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        /// <summary>Resizes an sprite renderer to the size of the camera fit.</summary>
        /// <param name="relativeCam">Orthographic camera to resize.</param>
        /// <param name="sr">Sprite renderer to resize.</param>
        /// <param name="axis">Axis to resize.</param>
        public static void ResizeSpriteToScreen(this Camera relativeCam, SpriteRenderer sr, TransformAxis axis = TransformAxis.XYAxis)
        {
            if (sr == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }

            sr.transform.localScale = new Vector3(1, 1, 1);

            var width = sr.sprite.bounds.size.x;
            var height = sr.sprite.bounds.size.y;

            var worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            switch (axis)
            {
                default:
                case TransformAxis.XYAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
                case TransformAxis.XAxis:
                    sr.transform.localScale =
                        new Vector3(worldScreenWidth / width, sr.transform.localScale.y, sr.transform.localScale.z);
                    break;
                case TransformAxis.YAxis:
                    sr.transform.localScale =
                        new Vector3(sr.transform.localScale.x, worldScreenHeight / height, sr.transform.localScale.z);
                    break;
            }
        }
        /// <summary>Resizes an mesh renderer to the size of the camera fit.</summary>
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

            sr.transform.localScale = new Vector3(1, 1, 1);

            var width = sr.mesh.bounds.size.x;
            var height = sr.mesh.bounds.size.y;

            var worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

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
        /// <summary>Fixes euler rotation to standard code ranges instead of Unity Editor ranges.</summary>
        public static Vector3 FixEulerRotation(Vector3 eulerRot)
        {
            Vector3 TransformEulerFixed = new Vector3(
                eulerRot.x > 180f ? eulerRot.x - 360f : eulerRot.x,
                eulerRot.y > 180f ? eulerRot.y - 360f : eulerRot.y,
                eulerRot.z > 180f ? eulerRot.z - 360f : eulerRot.z
                );

            return TransformEulerFixed;
        }
        /// <summary>Gathers a gameobject from <paramref name="path"/>. (Path seperator is '/')</summary>
        public static GameObject GetGameObjectFromPath(string path)
        {
            // Okay, i know this method is useless, but it
            // explicitly specifies that the 'GameObject.Find' method also supports paths.
            if (string.IsNullOrEmpty(path))
                return null;

            return GameObject.Find(path);
        }
        #endregion

        #region Helper Functions
        // -- Random Utils
        /// <summary>Returns a random boolean.</summary>
        public static bool RandBool()
        {
            // Using floats here is faster.
            return UnityEngine.Random.Range(0f, 1f) > .5f;
        }

        // -- Other Utils (General)
        /// <summary>
        /// Copies the given directory.
        /// </summary>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException
                    ($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            if (sourceDirName.Equals(destDirName))
            {
                Debug.LogWarning("[ChangeLightmap] The directory you are trying to copy is the same as the destination directory.");
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
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        /// <summary>
        /// Converts a 2 dimensional array to an array of arrays.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="src">2-Dimensional array</param>
        /// <returns>Array of arrays of the same size the <paramref name="src"/>.</returns>
        public static TDest[][] Convert2DArray<TDest>(this TDest[,] src)
        {
            // Match input
            if (src == null)
                return null;

            // Get array dimensions
            var height = src.GetLength(0);
            var width = src.GetLength(1);

            // Create the new array
            var tgt = new TDest[height][];

            // Cast the array to the arrays of array.
            for (int i = 0; i < height; i++)
            {
                // Create new member on index 'i' with the size of the first dimension
                tgt[i] = new TDest[width];

                // Set source 'WTF'
                for (int j = 0; j < width; j++)
                    tgt[i][j] = src[i, j];
            }

            // Return it
            return tgt;
        }
        /// <summary>
        /// Converts a 3 dimensional array to an array of arrays (lol).
        /// </summary>
        /// <typeparam name="TSrc">Type of array.</typeparam>
        /// <typeparam name="TDest">Destination type <c>(? Undocumented)</c></typeparam>
        /// <param name="src">3-Dimensional array.</param>
        /// <returns>Array of arrays of the same size the <paramref name="src"/>.</returns>
        public static TDest[][][] Convert3DArray<TSrc, TDest>(this TSrc[,,] src, Func<TSrc, TDest> converter)
        {
            // Match input
            if (src == null)
                return null;
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));

            // Get array dimensions
            var iLen = src.GetLength(0);
            var jLen = src.GetLength(1);
            var kLen = src.GetLength(2);

            // Create the new array
            var tgt = new TDest[iLen][][];
            for (int i = 0; i < iLen; i++)
            {
                tgt[i] = new TDest[jLen][];
                for (int j = 0; j < jLen; j++)
                {
                    tgt[i][j] = new TDest[kLen];
                    for (int k = 0; k < kLen; k++)
                        tgt[i][j][k] = converter(src[i, j, k]);
                }
            }

            // Return it
            return tgt;
        }
        public static T GetRandomEnum<T>(T[] EnumToIgnore = null) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            List<T> ListValues = new List<T>();

            /* TODO : Check duplicate EnumToIgnore values. */
            if (EnumToIgnore.Length >= values.Length)
            {
                Debug.LogWarning($"[Additionals::GetRandomEnum] EnumToIgnore list is longer than array, returning null. Bool : {EnumToIgnore.Length} >= {values.Length}");
                return default;
            }

            for (int i = 0; i < values.Length; i++)
            { ListValues.Add((T)values.GetValue(i)); }

            if (EnumToIgnore != null)
            {
                foreach (T rmEnum in EnumToIgnore)
                { ListValues.Remove(rmEnum); }
            }

            return ListValues[UnityEngine.Random.Range(0, values.Length)];
        }
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(type.BaseType.GetBaseTypes());
        }
        /// <summary>Converts <see cref="Vector3"/> to negative values.</summary>
        public static Vector3 NegativeAbs(this Vector3 v)
        {
            return -v.Abs();
        }
        /// <summary>Converts <see cref="Vector3"/> to positive values.</summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        /// <summary>Get the index of chars.</summary>
        public static (int index, char value) IndexOfAnyChar(this string s, params char[] toFind)
        {
            // DONE: input parameters validation
            if (null == s)
                return (-1, default(char)); // or throw ArgumentNullException(nameof(s))
            else if (null == toFind || toFind.Length <= 0)
                return (-1, default(char)); // or throw ArgumentNullException(nameof(toFind))

            int bestIndex = -1;
            char bestChar = default;

            foreach (char c in toFind)
            {
                // for the long strings let's provide count for efficency
                int index = s.IndexOf(c, 0, bestIndex < 0 ? s.Length : bestIndex);

                if (index >= 0)
                {
                    bestIndex = index;
                    bestChar = c;
                }
            }

            return (bestIndex, bestChar);
        }
        /// <summary>Get types that has the <paramref name="AttributeType"/> attribute from <see cref="Assembly"/> <paramref name="AttributeAssem"/>.</summary>
        /// <returns>The types with the attribute <paramref name="AttributeType"/>.</returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Type AttributeType, Assembly AttributeAssem = null)
        {
            if (AttributeAssem == null)
            {
                AttributeAssem = Assembly.GetExecutingAssembly();
            }

            foreach (Type type in AttributeAssem.GetTypes())
            {
                if (type.GetCustomAttributes(AttributeType, true).Length > 0)
                {
                    // what
                    yield return type;
                }
            }
        }
        /// <summary>Replaces multiple chars in a string built by <paramref name="builder"/>.</summary>
        /// <param name="builder">The string to modify. Put in a string builder.</param>
        /// <param name="toReplace">Chars to replace.</param>
        /// <param name="replacement">New chars put after replacing.</param>
        public static void MultiReplace(this StringBuilder builder, char[] toReplace, char replacement)
        {
            for (int i = 0; i < builder.Length; ++i)
            {
                var currentCharacter = builder[i];
                // Check if there's a match with the chars to replace.
                if (toReplace.All((char c) => { return currentCharacter == c; }))
                {
                    builder[i] = replacement;
                }
            }
        }

        // -- Array Utils
        /// <summary>
        /// Similar to the python's <c>'enumerate()'</c> keyword for it's <c>for</c> loops.
        /// </summary>
        /// <typeparam name="T">Type of the actual object to enumerate.</typeparam>
        /// <param name="enumerable">The enumerated object.</param>
        /// <returns>Object + Index of <c><see cref="foreach"/></c>.</returns>
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> enumerable)
        {
            int i = -1;

            foreach (var obj in enumerable)
            {
                i++;
                yield return (i, obj);
            }
        }
        /// <summary>Resize array.</summary>
        /// <param name="newT">The instance of a new generic. This is added due to 'T' not being a 'new T()' able type.</param>
        public static void Resize<T>(this List<T> list, int sz, T newT)
        {
            int cur = list.Count;
            if (sz < cur)
            {
                list.RemoveRange(sz, cur - sz);
            }
            else if (sz > cur)
            {
                if (sz > list.Capacity) // this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                    list.Capacity = sz;

                list.AddRange(Enumerable.Repeat(newT, sz - cur));
                Debug.Log(list.Count);
            }
        }
        /// <summary>Resize array.</summary>
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }
        /// <summary>Resets array values to their default values.</summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="array">The array to reset it's values.</param>
        public static void ResetArray<T>(this T[] array)
        {
            T genDefValue = default(T);

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = genDefValue;
            }
        }
        #endregion

        #region Serializing

        #region PlayerPrefs
        public static void SetBool(string SaveKey, bool Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::SetBool] Couldn't set the savekey because it is null. {SaveKey}");
                return;
            }

            PlayerPrefs.SetInt(SaveKey, Value ? 1 : 0);
        }
        public static bool GetBool(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogWarning($"[Additionals::GetBool] The key is null. It will return false. {SaveKey}");
                return false;
            }
            else
            {
                return PlayerPrefs.GetInt(SaveKey) == 1;
            }
        }
        public static void SetVector2(string SaveKey, Vector2 Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::SetVector2] Couldn't set the savekey because it is null. {SaveKey}");
                return;
            }

            PlayerPrefs.SetFloat($"{SaveKey}_X", Value.x);
            PlayerPrefs.SetFloat($"{SaveKey}_Y", Value.y);
        }
        public static Vector2 GetVector2(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::GetVector2] Couldn't get the savekey because it is null. {SaveKey}");
                return default;
            }

            return new Vector2(PlayerPrefs.GetFloat($"{SaveKey}_X"),
                PlayerPrefs.GetFloat($"{SaveKey}_Y"));
        }
        public static void SetVector3(string SaveKey, Vector3 Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::SetVector3] Couldn't set the savekey because it is null. {SaveKey}");
                return;
            }

            PlayerPrefs.SetFloat($"{SaveKey}_X", Value.x);
            PlayerPrefs.SetFloat($"{SaveKey}_Y", Value.y);
            PlayerPrefs.SetFloat($"{SaveKey}_Z", Value.z);
        }
        public static Vector3 GetVector3(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::GetVector3] Couldn't get the savekey because it is null. {SaveKey}");
                return default;
            }

            return new Vector3(PlayerPrefs.GetFloat($"{SaveKey}_X"),
                PlayerPrefs.GetFloat($"{SaveKey}_Y"), PlayerPrefs.GetFloat($"{SaveKey}_Z"));
        }
        public static void SetColor(string SaveKey, Color Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::SetColor] Couldn't set the savekey because it is null. {SaveKey}");
                return;
            }

            PlayerPrefs.SetFloat($"{SaveKey}_R", Value.r);
            PlayerPrefs.SetFloat($"{SaveKey}_G", Value.g);
            PlayerPrefs.SetFloat($"{SaveKey}_B", Value.b);
            PlayerPrefs.SetFloat($"{SaveKey}_A", Value.a);
        }
        public static Color GetColor(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::GetColor] Couldn't get the savekey because it is null. {SaveKey}");
                return default;
            }

            return new Color(PlayerPrefs.GetFloat($"{SaveKey}_R"), PlayerPrefs.GetFloat($"{SaveKey}_G"),
                PlayerPrefs.GetFloat($"{SaveKey}_B"), PlayerPrefs.GetFloat($"{SaveKey}_A"));
        }
        public static void SetEnum<T>(string SaveKey, T Enum) where T : Enum
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::SetEnum] Couldn't set the savekey because it is null. {SaveKey}");
                return;
            }

            PlayerPrefs.SetInt($"{SaveKey}_ENUM:{typeof(T).Name}", Convert.ToInt32(Enum));
        }
        public static T GetEnum<T>(string SaveKey) where T : Enum
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError($"[Additionals::GetEnum] Couldn't get the savekey because it is null. {SaveKey}");
                return default;
            }

            return (T)(object)PlayerPrefs.GetInt($"{SaveKey}_ENUM:{typeof(T).Name}");
        }
        public static bool HasPlayerPrefsKey<T>(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey)) return false;

            var tType = typeof(T);
            if (tType == typeof(Vector2))
            {
                return PlayerPrefs.HasKey($"{SaveKey}_X") && PlayerPrefs.HasKey($"{SaveKey}_Y");
            }
            if (tType == typeof(Vector3))
            {
                return PlayerPrefs.HasKey($"{SaveKey}_X") && PlayerPrefs.HasKey($"{SaveKey}_Y") && PlayerPrefs.HasKey($"{SaveKey}_Z");
            }
            if (tType == typeof(Color))
            {
                return PlayerPrefs.HasKey($"{SaveKey}_R") && PlayerPrefs.HasKey($"{SaveKey}_G")
                    && PlayerPrefs.HasKey($"{SaveKey}_B") && PlayerPrefs.HasKey($"{SaveKey}_A");
            }
            if (tType == typeof(bool))
            {
                return PlayerPrefs.HasKey("SaveKey");
            }
            if (tType.IsEnum)
            {
                return PlayerPrefs.HasKey($"{SaveKey}_ENUM:{typeof(T).Name}");
            }

            Debug.LogWarning($"[Additionals::HasPlayerPrefsKey] The type {typeof(T).Name} is not supported by additionals playerprefs. Returning false.");
            return false;
        }
        #endregion

        #region Binary Serializer
        // === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
        // Do not change this
        public sealed class VersionDeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
                {
                    Type typeToDeserialize;

                    assemblyName = Assembly.GetExecutingAssembly().FullName;

                    // The following line of code returns the type. 
                    typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");

                    return typeToDeserialize;
                }

                return null;
            }
        }

        /// <summary>
        /// Saves the object.
        /// Use only for editor!
        /// </summary>
        /// <typeparam name="T">Type of object. Can be anything as long as it has the <see cref="SerializableAttribute"/>.</typeparam>
        /// <param name="serializableObject">The object itself.</param>
        /// <param name="filePath">The file path to save.</param>
        /// <param name="OverWrite">Should we overwrite our save?</param>
        public static void Save<T>(T serializableObject, string filePath, bool OverWrite = false)
        {
            // Make sure the generic is serializable.
            Contract.Requires(typeof(T).GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0);

            try
            {
                if (OverWrite)
                {
                    File.Delete(filePath);
                }
                else if (File.Exists(filePath))
                {
                    Debug.Log("[Additionals::Save] File already exists, creating new file name.");

                    // When no linq
                    /** File path for <see cref="Directory.GetFiles(string)"/> */
                    string modifiedFPath = null;
                    /* Cutting everything after the last slash  *
                     * INFO : Use backward slash : '\'          */
                    int IndexOfmodifiedPath = filePath.LastIndexOf('\\');
                    if (IndexOfmodifiedPath > 0)
                    {
                        // To remove everything after the last '\'
                        modifiedFPath = filePath.Remove(IndexOfmodifiedPath);
                        Debug.Log($"Modified Path 1 : {modifiedFPath}");
                    }

                    /** Parsing the file directory to parts for overriding. */
                    string[] existing = Directory.GetFiles(modifiedFPath, "*.bytes", SearchOption.TopDirectoryOnly);
                    string[] splitLast_Existing = existing[existing.Length - 1].Split('\\');
                    string LastName = splitLast_Existing[splitLast_Existing.Length - 1];

                    string[] splitLast_Extension = LastName.Split('.');

                    /* Cutting the extension */
                    string FileExtension = $".{splitLast_Extension[splitLast_Extension.Length - 1]}";
                    string FileName = null;
                    /* If the size is larger than 2, we assume the stupid user has put dots inside the file name. 
                     * Generate FileName string */
                    if (splitLast_Extension.Length > 2)
                    {
                        /* This for loop 'might' be flawed because of the length */
                        for (int i = 0; i < splitLast_Extension.Length - 1; i++)
                        {
                            /* Split ignores the dots */
                            FileName += $"{splitLast_Extension[i]}.";
                        }
                    }
                    else
                    { FileName = splitLast_Extension[0]; }

                    /* Cutting everything after the last slash */
                    int IndexOfFilePath = filePath.LastIndexOf('\\');
                    if (IndexOfFilePath > 0)
                    {
                        /* To remove everything after the last '\' *
                         * Incremented 1 as we need the last '\'   */
                        filePath = filePath.Remove(IndexOfFilePath + 1);
                        /* Generate new filePath. */
                        filePath += $"{FileName}{existing.Length}{FileExtension}";
                        // Debug.Log($"Generated new filePath : {filePath}");
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
            { throw new SerializationException($"[Additionals::Load] An Error occured while deserializing.\n->{e.Message}\n->{e.StackTrace}"); }
        }
        /// <summary>
        /// TODO : Async (oof) and compression
        /// Loads binary saved data from path.
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="filePath">File path to load from.</param>
        /// <returns></returns>
        public static ExpectT Load<ExpectT>(string filePath)
        {
            /* Make sure the generic is serializable.
             * Reflection is bad but idc. */
            Contract.Requires(typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0);

            ExpectT DSerObj;

            try
            {
                using (Stream stream = File.OpenRead(filePath))
                {
                    BinaryFormatter bformatter = new BinaryFormatter
                    {
                        Binder = new VersionDeserializationBinder()
                    };

                    stream.Position = 0;
                    // Might check hash here..
                    DSerObj = (ExpectT)bformatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            /* So what's the purpose? To give better info about what went wrong. */
            { throw new SerializationException($"[Additionals::Load] An Error occured while deserializing.\n->{e.Message}\n->{e.StackTrace}"); }

            return DSerObj;
        }
        /// <summary>
        /// TODO : Async (oof) and compression
        /// Loads binary saved data from text.
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="fileContents">The content of the folder to make data from. Make sure the file is utf8.</param>
        /// <returns></returns>
        public static ExpectT Load<ExpectT>(char[] fileContents)
        {
            /* Make sure the generic is serializable. */
            Contract.Requires(typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0);

            byte[] fileContentData = new byte[fileContents.Length];
            for (int i = 0; i < fileContents.Length; i++)
            { fileContentData[i] = Convert.ToByte(fileContents[i]); }

            return Load<ExpectT>(fileContentData);
        }
        /// <summary>
        /// TODO : Async and compression (oof)
        /// Loads binary saved data from bytes.
        /// </summary>
        /// <typeparam name="ExpectT"></typeparam>
        /// <param name="fileContents">The content of the folder to make data from.</param>
        /// <returns></returns>
        public static ExpectT Load<ExpectT>(byte[] fileContents)
        {
            /* Make sure the generic is serializable. */
            Contract.Requires(typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0);

            ExpectT DSerObj;

            try
            {
                using (MemoryStream ms = new MemoryStream(fileContents))
                {
                    BinaryFormatter bformatter = new BinaryFormatter
                    {
                        Binder = new VersionDeserializationBinder()
                    };

                    ms.Position = 0;
                    DSerObj = (ExpectT)bformatter.Deserialize(ms);
                }
            }
            catch (Exception e)
            { throw new SerializationException($"[Additionals::Load] An Error occured while deserializing.\n->{e.Message}\n->{e.StackTrace}"); }

            /* No check of md5 as we are just loading from editor, sike we just loadin */
            return DSerObj;
        }

        /// <summary>
        /// Returns a byte array from an object.
        /// </summary>
        /// <param name="obj">Object that has the <see cref="SerializableAttribute"/>.</param>
        /// <returns>byte array</returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj is null)
            {
                Debug.LogError("[Additionals::ObjectToByteArray] The given object is null!");
                return null;
            }

            /* Require attribute. */
            Contract.Requires(obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length != 0);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        #endregion

        #endregion
    }

    [Flags]
    /// <summary>
    /// Transform Axis.
    /// <br>(Used in few helper methods)</br>
    /// </summary>
    public enum TransformAxis
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,
        ZAxis = 1 << 2,

        XYAxis = XAxis | YAxis,
        YZAxis = YAxis | ZAxis,
        XZAxis = XAxis | ZAxis,

        // All Axis
        XYZAxis = XAxis | YAxis | ZAxis,
    }

    [Flags]
    /// <summary>
    /// Transform axis, used in 2D space.
    /// <br>NOTE : This is an axis value for position.
    /// For rotation, please use the <see cref="TransformAxis"/>.</br>
    /// </summary>
    public enum TransformAxis2D
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,

        XYAxis = XAxis | YAxis
    }

    /// <summary>
    /// Serializable dictionary.
    /// <br>NOTE : Array types <c>TKey[]</c> or <c>TValue[]</c> are NOT serializable. Wrap them with container class.</br>
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        where TValue : new()
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        /// <summary> Save the dictionary to lists. </summary> 
        public void OnBeforeSerialize()
        {
            // -- Only useful if we directly add to the dictionary, then serialize
            //if (keys.Count != Keys.Count) // If the actual dictionary is more up to date.
            //{
            //    keys.Clear();
            //    values.Clear();

            //    foreach (KeyValuePair<TKey, TValue> pair in this)
            //    {
            //        keys.Add(pair.Key);
            //        values.Add(pair.Value);
            //    }
            //}
        }

        /// <summary> Load dictionary from lists. </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
            {
                values.Resize(keys.Count);

                // Unity moment
                if (keys.Count != values.Count)
                {
                    throw new Exception($"There are {keys.Count} keys and {values.Count} values after deserialization. Make sure that both key and value types are serializable.");
                }
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (Keys.Contains(keys[i]))
                {
                    // TODO : 
                    // Ignore for now, don't update the dictionary.
                    // Maybe add a flag for update requirement?

                    continue;
                }

                Add(keys[i], values[i]);
            }
        }
    }
}

namespace BXFW.Editor
{
    #region Unity Editor Additionals
#if UNITY_EDITOR
    /// <summary>
    /// Allows for coroutine execution in Edit Mode.
    /// </summary>
    public sealed class EditModeCoroutineExec
    {
        /// <summary>
        /// Add coroutine to execute.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IEnumerator StartCoroutine(IEnumerator c)
        {
            CoroutineInProgress.Add(c);
            return c;
        }
        public static IEnumerator StopCoroutine(IEnumerator c)
        {
            CoroutineInProgress.Remove(c);
            return c;
        }
        public static void StopAllCoroutines()
        {
            CoroutineInProgress.Clear();
        }

        /// <summary>
        /// Coroutines to execute. Managed by the EditModeCoroutineExec.
        /// </summary>
        private static List<IEnumerator> CoroutineInProgress = new List<IEnumerator>();

        /// <summary>
        /// Default static constructor assigning execution to update.
        /// </summary>
        static EditModeCoroutineExec()
        {
            EditorApplication.update += Update;
        }

        // private static int CurrentExec_Index = 0;
        private static void Update()
        {
            if (CoroutineInProgress.Count <= 0)
            { return; }

            for (int i = 0; i < CoroutineInProgress.Count; i++)
            {
                /* Null coroutine */
                if (CoroutineInProgress[i] == null)
                { continue; }

                /* Normal */
                if (!CoroutineInProgress[i].MoveNext())
                { CoroutineInProgress.Remove(CoroutineInProgress[i]); }
            }

            /* Previous Method 
            CurrentExec_Index = (CurrentExec_Index + 1) % CoroutineInProgress.Count;

            bool finish = !CoroutineInProgress[CurrentExec_Index].MoveNext();

            if (finish)
            { CoroutineInProgress.RemoveAt(CurrentExec_Index); }*/
        }
    }

    public enum MatchGUIActionOrder
    {
        Before = 0,
        After = 1,
        Omit = 2,
        OmitAndInvoke = 3
    }

    public static class EditorAdditionals
    {
        #region Property Field Helpers
        private static Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

        /// <summary>
        /// Returns the c# object.
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/>
        public static object GetTarget(this SerializedProperty prop)
        {
            string[] propertyNames = prop.propertyPath.Split('.');
            object target = prop.serializedObject.targetObject;
            object prevTarget = null;
            bool isNextPropertyArrayIndex = false;

            for (int i = 0; i < propertyNames.Length && target != null; ++i)
            {
                string propName = propertyNames[i];

                if (propName == "Array" && (target is IEnumerable || target is IEnumerable<object>))
                {
                    isNextPropertyArrayIndex = true;
                }
                else if (isNextPropertyArrayIndex)
                {
                    isNextPropertyArrayIndex = false;

                    Match m = ArrayIndexCapturePattern.Match(propName);

                    // Object is actually an array that unity serializes
                    if (m.Success)
                    {
                        var arrayIndex = int.Parse(m.Groups[1].Value);
                        var targetAsArray = target as IEnumerable;

                        if (targetAsArray == null)
                            throw new Exception(@$"[EditorAdditionals::GetTarget] Error while casting targetAsArray (Make sure the type extends from IEnumerable)
-> Invalid cast : Tried to cast type {target.GetType().Name} as IEnumerable.");

                        prevTarget = target;

                        // Should use 'MoveNext' but i don't care.
                        var cntIndex = 0;
                        var isSuccess = false;
                        foreach (var item in targetAsArray)
                        {
                            if (cntIndex == arrayIndex)
                            {
                                prevTarget = target;
                                target = item;
                                isSuccess = true;

                                break;
                            }

                            cntIndex++;
                        }

                        if (!isSuccess)
                            throw new Exception($"[EditorAdditionals::GetTarget] Couldn't find SerializedProperty {prop.propertyPath} in array {targetAsArray}.");
                    }
                    else
                    {
                        throw new Exception($"[EditorAdditionals::GetTarget] Invalid array index parsing on string : \"{propName}\"");
                    }
                }
                else
                {
                    prevTarget = target;
                    target = GetField(target, propName);
                }
            }

            // target may be null on certain occassions, that's why we keep a 'prevTarget' variable.
            return target ?? prevTarget;
        }
        /// <summary>
        /// Internal helper method for getting field from properties.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static object GetField(object target, string name, Type targetType = null)
        {
            if (targetType == null)
            {
                targetType = target.GetType();
            }

            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null)
            {
                return fi.GetValue(target);
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetField(target, name, targetType.BaseType);
            }

            return null;
        }

        public static Type GetFieldType(this SerializedProperty property)
        {
            return property.GetTarget().GetType();
        }
        #endregion

        #region Gizmos
        /// <summary>
        /// TODO : Need testing (these gizmo matrix manipulations never worked fine on my side)
        /// </summary>
        public static void DrawBoxCollider(this Transform transform, Color gizmoColor, BoxCollider boxCollider, float alphaForInsides = 0.3f)
        {
            //Save the color in a temporary variable to not overwrite changes in the inspector (if the sent-in color is a serialized variable).
            var color = gizmoColor;

            //Change the gizmo matrix to the relative space of the boxCollider.
            //This makes offsets with rotation work
            //Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, transform.lossyScale);

            //Draws the edges of the BoxCollider
            //Center is Vector3.zero, since we've transformed the calculation space in the previous step.
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

            //Draws the sides/insides of the BoxCollider, with a tint to the original color.
            color.a *= alphaForInsides;
            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, boxCollider.size);
        }
        public static void DrawArrow_Gizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrow_Gizmos(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void DrawArrow_Debug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrow_Debug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }

        public static void DrawText(string text, Vector3 worldPos, Color? colour = null, bool cullText = true, float oX = 0f, float oY = 0f)
        {
            Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;

            var view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (cullText)
            {
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreColor;
                    Handles.EndGUI();
                    return;
                }
            }

            Handles.Label(TransformByPixel(worldPos, oX, oY), text);

            GUI.color = restoreColor;
            Handles.EndGUI();
        }
        private static Vector3 TransformByPixel(Vector3 position, float x, float y)
        {
            return TransformByPixel(position, new Vector3(x, y));
        }
        private static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;
            if (cam)
                return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
            else
                return position;
        }
        #endregion

        #region Inspector-Editor Draw
        public static void CreateReadOnlyTextField(string label, string text = null)
        {
            EditorGUILayout.BeginHorizontal();

            if (label != null)
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            if (text != null)
                EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw default inspector with gui inbetween.
        /// WARNING : Please do not use big <see cref="SerializeField"/> arrays because those are slower than default <see cref="Editor.DrawDefaultInspector()"/>.
        /// </summary>
        /// <param name="target">The targeted method target.</param>
        /// <param name="onStringMatchEvent">The event <see cref="MatchGUIActionOrder"/> match.</param>
        /// <example>
        /// // Info : The Generic '/\' is replaced with '[]'.
        /// serializedObject.DrawDefaultInspector(new Dictionary[string, KeyValuePair[MatchGUIActionOrder, Action]] 
        /// {
        ///     { nameof(FromAnyClass.ElementNameYouWant), new KeyValuePair[MatchGUIActionOrder, System.Action](MatchGUIActionOrder.Before, () => 
        ///         {
        ///             // Write your commands here.
        ///         })
        ///     }
        /// });
        /// </example>
        public static void DrawDefaultInspector(this SerializedObject target, Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> onStringMatchEvent, bool renderArrays = false)
        {
            if (target == null)
            {
                Debug.LogError("[EditorAdditionals] The SerializedObject target is null.");
                return;
            }

            using (SerializedProperty iter = target.GetIterator())
            {
                while (iter.NextVisible(true))
                {
                    if (iter.isArray && !renderArrays)
                    { continue; }

        #region String ID Event
                    // -- Check if there is a match
                    string MatchingKey = null;
                    foreach (string s in onStringMatchEvent.Keys)
                    {
                        if (s.Equals(iter.name))
                        {
                            MatchingKey = s;
                            break;
                        }
                    }

                    // -- If there's a match of events, use the event.
                    if (!string.IsNullOrEmpty(MatchingKey))
                    {
                        var Pair = onStringMatchEvent[MatchingKey];

                        if (Pair.Key == MatchGUIActionOrder.OmitAndInvoke)
                        {
                            Pair.Value?.Invoke();
                            continue;
                        }

                        // -- Omit GUI
                        if (Pair.Key == MatchGUIActionOrder.Omit)
                        { continue; }

                        // -- Standard draw
                        if (Pair.Key == MatchGUIActionOrder.After)
                        { EditorGUILayout.PropertyField(iter); }

                        Pair.Value?.Invoke();

                        if (Pair.Key == MatchGUIActionOrder.Before)
                        { EditorGUILayout.PropertyField(iter); }

                        continue;
                    }
        #endregion

                    // -- If no match, draw as it is. -- //

                    // Disable GUI if we are drawing the script thing.
                    // INFO : The iterator name for the 'Script' field is 'm_Script'.
                    if (iter.name.Equals("m_Script"))
                    {
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(iter);
                        GUI.enabled = true;

                        continue;
                    }
                    // -- Skip the visible children as we draw this iteration.
                    if (iter.hasVisibleChildren)
                    {
                        // -- TODO
                        /*
                        switch (iter.type)
                        {
                            case "Vector3":
                                for (int i = 0; i < 3; i++)
                                {
                                    iter.NextVisible(true);
                                }
                                break;
                            default:
                                break;
                        }
                        */
                        /*
                        int childAmount = 0;

                        iter.value

                        while (iter.NextVisible(true)) childAmount++;

                        for (int i = 0; i < childAmount; i++)
                        {
                            iter.NextVisible(true);
                        }
                        */
                    }

                    // Debug.LogFormat("{0} | {1}", iter.type, iter.hasVisibleChildren);

                    EditorGUILayout.PropertyField(iter);
                }

                target.ApplyModifiedProperties();
            }
        }

        [Flags]
        public enum EditorListOption
        {
            None = 0,
            ListSize = 1,
            ListLabel = 2,
            Default = ListSize | ListLabel
        }

        public static void ShowEditorList(SerializedProperty list, EditorListOption options = EditorListOption.Default)
        {
            bool showListLabel = (options & EditorListOption.ListLabel) != 0, showListSize = (options & EditorListOption.ListSize) != 0;

            if (showListLabel)
            {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }

            if (!showListLabel || list.isExpanded)
            {
                if (showListSize)
                { EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size")); }

                for (int i = 0; i < list.arraySize; i++)
                { EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i)); }
            }

            if (showListLabel)
            { EditorGUI.indentLevel -= 1; }
        }
        /// <summary>
        /// Create array with fields in case of bug.
        /// INFO : This is a more primitive array drawer, but it works.
        /// </summary>
        /// <param name="obj">Serialized object of target.</param>
        /// <param name="arr_name">Array field name.</param>
        public static void UnityArrayGUI(this SerializedObject obj, string arr_name, Action<int> OnArrayFieldDrawn = null)
        {
            int prev_indent = EditorGUI.indentLevel;

            // Get size of array
            int arr_Size = obj.FindProperty($"{arr_name}.Array.size").intValue;

            // Create the size field
            EditorGUI.indentLevel = 1;
            int curr_arr_Size = EditorGUILayout.IntField("Size", arr_Size);
            // Clamp
            curr_arr_Size = curr_arr_Size < 0 ? 0 : curr_arr_Size;
            EditorGUI.indentLevel = 3;

            if (curr_arr_Size != arr_Size)
            {
                obj.FindProperty($"{arr_name}.Array.size").intValue = curr_arr_Size;
            }

            // Create the array fields (stupid)
            for (int i = 0; i < arr_Size; i++)
            {
                // Create property field. TODO : Add dropdown
                var prop = obj.FindProperty($"{arr_name}.Array.data[{i}]");
                // If our property is null, ignore
                // This is necessary as we don't update on time.
                if (prop == null)
                    continue;

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop);
                OnArrayFieldDrawn?.Invoke(i);
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prev_indent;
        }

        /// System namespace doesn't have a thing like this so.
        public delegate void RefIndexDelegate<T>(int arg1, ref T arg2);

        /// <summary>Internal unity icon for icon pointing downwards.</summary>
        public const string UInternal_PopupTex = "Icon Dropdown";
        private static Texture2D UnityArrayGUICustom_CurrentBG;
        /// <summary>
        /// Create custom array with fields.
        /// <br>Known issues : Only support standard arrays (because <see cref="List{T}"/> is read only), everything has to be passed by reference.</br>
        /// <br>Need to take an persistant bool value for dropdown menu. Pass true always if required to be open.</br>
        /// </summary>
        /// <param name="toggleDropdwnState">Toggle boolean for the dropdown state. Required to keep an persistant state. Pass true if not intend to use.</param>
        /// <param name="GenericDrawList">Generic draw target array. Required to be passed by reference as it's resized automatically.</param>
        /// <param name="OnArrayFieldDrawn">Event to draw generic ui when fired. <c>THIS IS REQUIRED.</c></param>
        public static void UnityArrayGUICustom<T>(ref bool toggleDropdwnState, ref T[] GenericDrawList, RefIndexDelegate<T> OnArrayFieldDrawn) where T : new()
        {
            int prev_indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = prev_indent + 2;
            // Create the size & dropdown field
            GUILayout.BeginHorizontal();
            //toggleDropdwnState = GUILayout.Toggle(toggleDropdwnState, new GUIContent(EditorGUIUtility.IconContent(UInternal_PopupTex)));
            toggleDropdwnState = GUILayout.Toggle(toggleDropdwnState, string.Empty, EditorStyles.popup, GUILayout.MaxWidth(20f));
            EditorGUILayout.LabelField($"{typeof(T)} List", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            int curr_arr_Size = EditorGUILayout.IntField("Size", GenericDrawList.Length, GUILayout.MaxWidth(200f), GUILayout.MinWidth(150f));
            curr_arr_Size = curr_arr_Size < 0 ? 0 : curr_arr_Size;
            GUILayout.EndHorizontal();

            //var gsBG = new GUIStyle();
            //if (UnityArrayGUICustom_CurrentBG == null)
            //{
            //    UnityArrayGUICustom_CurrentBG = new Texture2D(1, 1);
            //}

            //UnityArrayGUICustom_CurrentBG.SetPixel(0, 0, Color.red);
            //Debug.Log(UnityArrayGUICustom_CurrentBG.GetPixel(0, 0));
            ////gsBG.normal.background = UnityArrayGUICustom_CurrentBG;
            ////gsBG.active.background = UnityArrayGUICustom_CurrentBG;

            if (toggleDropdwnState)
            {
                // Resize array
                if (curr_arr_Size != GenericDrawList.Length)
                {
                    Array.Resize(ref GenericDrawList, curr_arr_Size);
                }

                EditorGUI.indentLevel = prev_indent + 3;
                // Create the array fields (stupid)
                for (int i = 0; i < GenericDrawList.Count(); i++)
                {
                    if (GenericDrawList[i] == null) continue;

                    // GUILayout.BeginVertical(gsBG);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField($"Element {i} --------", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    OnArrayFieldDrawn.Invoke(i, ref GenericDrawList[i]);
                    // GUILayout.EndVertical();
                }

                EditorGUI.indentLevel = prev_indent + 1;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    Array.Resize(ref GenericDrawList, curr_arr_Size + 1);
                }
                if (GUILayout.Button("-"))
                {
                    Array.Resize(ref GenericDrawList, curr_arr_Size - 1);
                }
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prev_indent;
        }

        /// <summary>
        /// Draws a straight line in editor.
        /// </summary>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thiccness of the line.</param>
        /// <param name="padding">Padding of the line. (Space left for the line, between properties)</param>
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {        
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        /* 
            var targetObj = property.serializedObject.targetObject;
            var targetObjClassType = targetObj.GetType();

            var field = targetObjClassType.GetField(property.propertyPath);
            if (field == null)
            {
                Debug.LogWarning("[EditorAdditionals::SerializedPropertyObjectValue] The field is null.");
                return;
            }
         */
        #endregion
    }
#endif

    #region Read Only Inspector Variables
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
    #endregion

    #endregion
}
