// Standard
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

// Serialization
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

using BXFW;
using BXFW.Tools.Editor;

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

        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this GameObject target)
        {
            return target.transform.GetPath();
        }

        /// <summary>
        /// Get the path of the gameobject.
        /// </summary>
        public static string GetPath(this Transform target)
        {
            if (target.parent == null)
                return string.Format("/{0}", target.name);

            return string.Format("{0}/{1}", target.parent.GetPath(), target.name);
        }

        /// <summary>Clamps rigidbody velocity.</summary>
        public static void ClampVelocity(this Rigidbody rb, float MaxSpeed)
        {
            if (rb is null)
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
        public static Vector3[] VerticesToWorldSpace(this MeshFilter filter)
        {
            if (filter == null)
            {
                Debug.LogWarning("[Additionals::VerticesToWorldSpace] The mesh filter reference is null.");
                return new Vector3[0];
            }

            var vertsMesh = Application.isPlaying ? filter.mesh : filter.sharedMesh;
            if (vertsMesh == null)
            {
                Debug.LogWarning("[Additionals::VerticesToWorldSpace] The mesh filter mesh is null.");
                return new Vector3[0];
            }

            Matrix4x4 localToWorld = filter.transform.localToWorldMatrix;
            Vector3[] world_v = new Vector3[vertsMesh.vertices.Length];

            for (int i = 0; i < vertsMesh.vertices.Length; i++)
            {
                world_v[i] = localToWorld.MultiplyPoint3x4(vertsMesh.vertices[i]);
            }

            return world_v;
        }
        public static Vector3[] WorldVertsToLocalSpace(this MeshFilter filter, Vector3[] worldV)
        {
            if (filter == null)
            {
                Debug.LogWarning("[Additionals::WorldVertsToLocalSpace] The mesh filter reference is null.");
                return new Vector3[0];
            }

            var vertsMesh = Application.isPlaying ? filter.mesh : filter.sharedMesh;
            if (vertsMesh == null)
            {
                Debug.LogWarning("[Additionals::WorldVertsToLocalSpace] The mesh filter mesh is null.");
                return new Vector3[0];
            }
            if (vertsMesh.vertexCount != worldV.Length)
            {
                Debug.LogError("[Additionals::WorldVertsToLocalSpace] The vertex amount is not equal.");
                return new Vector3[0];
            }

            Matrix4x4 worldToLocal = filter.transform.worldToLocalMatrix;
            Vector3[] local_v = new Vector3[worldV.Length];

            for (int i = 0; i < worldV.Length; i++)
            {
                local_v[i] = worldToLocal.MultiplyPoint3x4(worldV[i]);
            }

            return local_v;
        }
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
        /// <summary>
        /// Returns the center of an virtual circle, between the <paramref name="cPoint0"/>, <paramref name="cPoint1"/> and <paramref name="cPoint2"/>.
        /// </summary>
        public static Vector3 CircleCenter(Vector3 cPoint0, Vector3 cPoint1, Vector3 cPoint2, out Vector3 cPointNormal)
        {
            // two circle chords
            var v1 = cPoint1 - cPoint0;
            var v2 = cPoint2 - cPoint0;

            // Normal related stuff
            cPointNormal = Vector3.Cross(v1, v2);
            if (cPointNormal.sqrMagnitude < Mathf.Epsilon)
                return Vector3.one * float.NaN;
            cPointNormal.Normalize();

            // Perpendicular of both chords
            var pd1 = Vector3.Cross(v1, cPointNormal).normalized;
            var pd2 = Vector3.Cross(v2, cPointNormal).normalized;
            // Distance between the chord midpoints
            var r = (v1 - v2) * 0.5f;
            // Center angle between the two perpendiculars
            var c = Vector3.Angle(pd1, pd2);
            // Angle between first perpendicular and chord midpoint vector
            var a = Vector3.Angle(r, pd1);
            // Law of sine to calculate length of p2
            var d = r.magnitude * Mathf.Sin(a * Mathf.Deg2Rad) / Mathf.Sin(c * Mathf.Deg2Rad);

            if (Vector3.Dot(v1, cPoint2 - cPoint1) > 0f)
                return cPoint0 + (v2 * 0.5f) - (pd2 * d);

            return cPoint0 + (v2 * 0.5f) + (pd2 * d);
        }
        /// <summary>
        /// Returns the biggest axis in the <paramref name="target"/>.
        /// </summary>
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
#if UNITY_ANDROID && !UNITY_EDITOR
            // In android you have to do dumb stuff in order to get keyboard height
            // This 'may not be necessary' in more updated versions of unity, but here we are.
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
        /// <summary>
        /// Whether if the <paramref name="mask"/> contains the <paramref name="layer"/>.
        /// </summary>
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
            if (!relativeCam.orthographic)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] Camera relative to resize isn't ortographic. The resizing won't be correct.");
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
        /// <summary>Fixes euler rotation to Unity Editor instead of the code ranges.</summary>
        public static Vector3 FixEulerRotation(Vector3 eulerRot)
        {
            Vector3 TransformEulerFixed = new Vector3(
                eulerRot.x > 180f ? eulerRot.x - 360f : eulerRot.x,
                eulerRot.y > 180f ? eulerRot.y - 360f : eulerRot.y,
                eulerRot.z > 180f ? eulerRot.z - 360f : eulerRot.z
                );

            return TransformEulerFixed;
        }
        #endregion

        #region Helper Functions
        // -- Random Utils
        /// <summary>Returns a random boolean.</summary>
        public static bool RandBool()
        {
            // Using floats here is faster and more random.
            // (for some reason, maybe the System.Convert.ToBoolean method takes more time than float comparison?)
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
                    (string.Format("[Additionals::DirectoryCopy] Source directory does not exist or could not be found: {0}", sourceDirName));
            }

            if (sourceDirName.Equals(destDirName))
            {
                Debug.LogWarning("[Additionals::DirectoryCopy] The directory you are trying to copy is the same as the destination directory.");
                return;
            }
            // These lines are probably a bad idea, c# 'probably' will just throw an exception if these lines aren't satisfied.
            //else if (sourceDirName.Equals(destDirName, StringComparison.OrdinalIgnoreCase) && 
            //    (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor))
            //{
            //    // In windows, it's same directory
            //    // On arch btw systems, directories are case sensitive
            //    // unlike winbloat, it's case sensitive as long as there's no file with same name but different casing)
            //
            //    Debug.LogWarning("[Additionals::DirectoryCopy] The directory you are trying to copy is the same as the destination directory. (case doesn't match but IsWindows == true)");
            //    return;
            //}

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

                // Set source.
                for (int j = 0; j < width; j++)
                    tgt[i][j] = src[i, j];
            }

            // Return it
            return tgt;
        }
        /// <summary>
        /// Converts a 3 dimensional array to an array of arrays.
        /// </summary>
        /// <typeparam name="TSrc">Type of array.</typeparam>
        /// <typeparam name="TDest">Destination type <c>(? Undocumented)</c></typeparam>
        /// <param name="src">3 Dimensional array.</param>
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
        /// <summary>
        /// Get a random enum from enum type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="EnumToIgnore">Enum list of values to ignore from.</param>
        /// <returns>Randomly selected enum.</returns>
        /// <exception cref="Exception">Thrown when the type isn't enum. (<see cref="Type.IsEnum"/> is false)</exception>
        public static T GetRandomEnum<T>(T[] EnumToIgnore = null)
        {
            if (!typeof(T).IsEnum)
                throw new Exception(string.Format("[Additionals::GetRandomEnum] Error while getting random enum : Type '{0}' is not a valid enum type.", typeof(T).Name));

            Array values = Enum.GetValues(typeof(T));
            List<T> ListValues = new List<T>();

            if (EnumToIgnore.Length >= values.Length)
            {
                Debug.LogWarning(string.Format("[Additionals::GetRandomEnum] EnumToIgnore list is longer than array, returning null. Bool : {0} >= {1}", EnumToIgnore.Length, values.Length));
                return default;
            }

            for (int i = 0; i < values.Length; i++)
            {
                var value = (T)values.GetValue(i);

                // Ignore duplicate values.
                // This isn't very important, but makes the removing cleaner.
                if (ListValues.Contains(value))
                {
                    // Debug.LogWarning(string.Format("[Additionals::GetRandomEnum] Multiple enum value '{0}' passed in array. Ignoring.", value));
                    continue;
                }
                
                ListValues.Add(value); 
            }

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
#if CSHARP_7_OR_LATER 
        // Tuple definition like (a, b) was added in c# 7
        /// <summary>
        /// Similar to the python's <c>'enumerate()'</c> keyword for it's <see langword="for"/> loops.
        /// </summary>
        /// <typeparam name="T">Type of the actual object to enumerate.</typeparam>
        /// <param name="enumerable">The enumerated object.</param>
        /// <returns>Object + Index of <c><see langword="foreach"/></c>.</returns>
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> enumerable)
        {
            int i = -1;

            foreach (var obj in enumerable)
            {
                i++;
                yield return (i, obj);
            }
        }
#endif
        /// <summary>Resize array.</summary>
        /// <param name="newT">
        /// The instance of a new generic.
        /// This is added due to '<typeparamref name="T"/>' not being a '<see langword="new"/> <typeparamref name="T"/>()' able type.
        /// </param>
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
            T genDefValue = (T)default;

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = genDefValue;
            }
        }
        /// <summary>
        /// Converts a <see cref="Array"/> to a typed array.
        /// </summary>
        public static T[] ToTypeArray<T>(this Array target)
        {
            T[] arrayReturn = new T[target.Length];

            for (int i = 0; i < target.Length; i++)
            {
                arrayReturn[i] = (T)target.GetValue(i);
            }

            return arrayReturn;
        }
        #endregion

        #region Serializing

        #region PlayerPrefs
        public static void SetBool(string SaveKey, bool Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetBool] Couldn't set the savekey because it is null. Key={0}", SaveKey));
                return;
            }

            PlayerPrefs.SetInt(SaveKey, Value ? 1 : 0);
        }
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
        public static void SetVector2(string SaveKey, Vector2 Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetVector2] Couldn't set the savekey because it is null. Key={0}", SaveKey));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_X", SaveKey), Value.x);
            PlayerPrefs.SetFloat(string.Format("{0}_Y", SaveKey), Value.y);
        }
        public static Vector2 GetVector2(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::GetVector2] Couldn't get the savekey because it is null. Key={0}", SaveKey));
                return default;
            }

            return new Vector2(PlayerPrefs.GetFloat(string.Format("{0}_X", SaveKey)),
                PlayerPrefs.GetFloat(string.Format("{0}_Y", SaveKey)));
        }
        public static void SetVector3(string SaveKey, Vector3 Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetVector3] Couldn't set the savekey because it is null. Key={0}", SaveKey));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_X", SaveKey), Value.x);
            PlayerPrefs.SetFloat(string.Format("{0}_Y", SaveKey), Value.y);
            PlayerPrefs.SetFloat(string.Format("{0}_Z", SaveKey), Value.z);
        }
        public static Vector3 GetVector3(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::GetVector3] Couldn't get the savekey because it is null. Key={0}", SaveKey));
                return default;
            }

            return new Vector3(PlayerPrefs.GetFloat(string.Format("{0}_X", SaveKey)),
                PlayerPrefs.GetFloat(string.Format("{0}_Y", SaveKey)), PlayerPrefs.GetFloat(string.Format("{0}_Z", SaveKey)));
        }
        public static void SetColor(string SaveKey, Color Value)
        {
            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetColor] Couldn't set the savekey because it is null. Key={0}", SaveKey));
                return;
            }

            PlayerPrefs.SetFloat(string.Format("{0}_R", SaveKey), Value.r);
            PlayerPrefs.SetFloat(string.Format("{0}_G", SaveKey), Value.g);
            PlayerPrefs.SetFloat(string.Format("{0}_B", SaveKey), Value.b);
            PlayerPrefs.SetFloat(string.Format("{0}_A", SaveKey), Value.a);
        }
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
        public static void SetEnum<T>(string SaveKey, T value)
        {
            if (!typeof(T).IsEnum)
                throw new Exception(string.Format("[Additionals::SetEnum] Error while setting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));

            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetEnum] Couldn't set the savekey because it is null. Key={0}", SaveKey));
                return;
            }

            PlayerPrefs.SetInt(string.Format("{0}_ENUM:{1}", SaveKey, typeof(T).Name), Convert.ToInt32(value));
        }
        public static T GetEnum<T>(string SaveKey)
        {
            if (!typeof(T).IsEnum)
                throw new Exception(string.Format("[Additionals::SetEnum] Error while getting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));

            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetEnum] Couldn't get the savekey because it is null. Key={0}", SaveKey));
                return default;
            }

            return (T)(object)PlayerPrefs.GetInt(string.Format("{0}_ENUM:{1}", SaveKey, typeof(T).Name));
        }
        public static bool HasPlayerPrefsKey<T>(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey)) return false;

            var tType = typeof(T);
            if (tType == typeof(Vector2))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_X", SaveKey)) && PlayerPrefs.HasKey(string.Format("{0}_Y", SaveKey));
            }
            if (tType == typeof(Vector3))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_X", SaveKey)) && PlayerPrefs.HasKey(string.Format("{0}_Y", SaveKey)) && PlayerPrefs.HasKey(string.Format("{0}_Z", SaveKey));
            }
            if (tType == typeof(Color))
            {
                return PlayerPrefs.HasKey(string.Format("{0}_R", SaveKey)) && PlayerPrefs.HasKey(string.Format("{0}_G", SaveKey))
                    && PlayerPrefs.HasKey(string.Format("{0}_B", SaveKey)) && PlayerPrefs.HasKey(string.Format("{0}_A", SaveKey));
            }
            if (tType == typeof(bool))
            {
                return PlayerPrefs.HasKey(SaveKey);
            }
            if (tType.IsEnum)
            {
                return PlayerPrefs.HasKey(string.Format("{0}_ENUM:{1}", SaveKey, typeof(T).Name));
            }

            return PlayerPrefs.HasKey(SaveKey);
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
                    typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

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
        public static void BSave<T>(T serializableObject, string filePath, bool OverWrite = false)
        {
            // Make sure the generic is serializable.
            // Require attribute.
            if (typeof(T).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                Debug.LogError(string.Format("[Additionals::BSave] Is serializable is false for given type '{0}'.", typeof(T).Name));
                return;
            }

            try
            {
                if (OverWrite)
                {
                    File.Delete(filePath);
                }
                else if (File.Exists(filePath))
                {
                    Debug.Log(string.Format("[Additionals::Save] File '{0}' already exists, creating new file name.", filePath));

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
                    { FileName = splitLast_Extension[0]; }

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
            { throw new SerializationException(string.Format("[Additionals::Load] An error occured while deserializing.\n->{0}\n->{1}", e.Message, e.StackTrace)); }
        }
        /// <summary>
        /// Loads binary saved data from path.
        /// <br>NOTE : These binary serialization functions are unsafe, refrain from using them as much as possible.</br>
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="filePath">File path to load from.</param>
        /// <returns>Data that is loaded. NOTE : Please don't invoke/parse/do anything with this data.</returns>
        public static ExpectT BLoad<ExpectT>(string filePath)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                Debug.LogError(string.Format("[Additionals::BLoad] Is serializable is false for given type '{0}'.", typeof(ExpectT).Name));
                return default;
            }

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
                    // You should use json instead anyway
                    DSerObj = (ExpectT)bformatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            { throw new SerializationException(string.Format("[Additionals::Load] An error occured while deserializing.\n->{0}\n->{1}", e.Message, e.StackTrace)); }

            return DSerObj;
        }
        /// <summary>
        /// Loads binary saved data from text.
        /// </summary>
        /// <typeparam name="ExpectT">The expected type. If you get it wrong you will get an exception.</typeparam>
        /// <param name="fileContents">The content of the folder to make data from. Make sure the file is utf8.</param>
        /// <returns></returns>
        public static ExpectT Load<ExpectT>(char[] fileContents)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                Debug.LogError(string.Format("[Additionals::ObjectToByteArray] Is serializable is false for given type '{0}'.", typeof(ExpectT).Name));
                return default;
            }

            byte[] fileContentData = new byte[fileContents.Length];
            for (int i = 0; i < fileContents.Length; i++)
            { fileContentData[i] = Convert.ToByte(fileContents[i]); }

            return Load<ExpectT>(fileContentData);
        }
        /// <summary>
        /// Loads binary saved data from bytes.
        /// </summary>
        /// <typeparam name="ExpectT"></typeparam>
        /// <param name="fileContents">The content of the folder to make data from.</param>
        /// <returns></returns>
        public static ExpectT Load<ExpectT>(byte[] fileContents)
        {
            // Require attribute.
            if (typeof(ExpectT).GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                Debug.LogError(string.Format("[Additionals::ObjectToByteArray] Is serializable is false for given type '{0}'.", typeof(ExpectT).Name));
                return default;
            }

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
            { throw new SerializationException(string.Format("[Additionals::Load] An error occured while deserializing.\n->{0}\n->{1}", e.Message, e.StackTrace)); }

            return DSerObj;
        }

        /// <summary>
        /// Returns a byte array from an object.
        /// </summary>
        /// <param name="obj">Object that has the <see cref="SerializableAttribute"/>.</param>
        /// <returns>Object as serializd byte array.</returns>
        /// <exception cref="InvalidDataContractException"/>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj is null)
            {
                Debug.LogError("[Additionals::ObjectToByteArray] The given object is null.");
                return null;
            }

            // Require attribute.
            if (obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length <= 0)
            {
                Debug.LogError(string.Format("[Additionals::ObjectToByteArray] Is serializable is false for given object '{0}'.", obj));
                return null;
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
    }

    #region Helper Enums
    /// <summary>
    /// Transform Axis.
    /// <br>(Used in few helper methods)</br>
    /// </summary>
    [Flags]
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

    /// <summary>
    /// Transform axis, used in 2D space.
    /// <br>NOTE : This is an axis value for position.
    /// For rotation, please use the <see cref="TransformAxis"/>.</br>
    /// </summary>
    [Flags]
    public enum TransformAxis2D
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,

        XYAxis = XAxis | YAxis
    }
    #endregion

    #region Helper Class / Struct
    /// <summary>
    /// Serializable dictionary.
    /// <br>NOTE : Array types such as <c>TKey[]</c> or <c>TValue[]</c> are NOT serializable (by unity). Wrap them with container class.</br>
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        where TValue : new()
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        // Save the dictionary to lists
        public void OnBeforeSerialize()
        {
            // The 'keys' and 'values' are already serialized, just add them 

            // If a key is removed
            if (keys.Count != values.Count)
            {
                // Removing or adding keys, set to defualt value
                values.Resize(keys.Count);
            }

            // Directly adding to dictionary (from c#, not from editor)
            // --Only useful if we directly add to the dictionary, then serialize
            if (keys.Count < Keys.Count) // If the actual dictionary is more up to date.
            {
                keys.Clear();
                values.Clear();

                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
            {
                values.Resize(keys.Count);

                // Unity moment
                if (keys.Count != values.Count)
                {
                    throw new IndexOutOfRangeException(string.Format(@"[SerializableDictionary] There are {0} keys and {1} values after deserialization.
Make sure that both key and value types are serializable.", keys.Count, values.Count));
                }
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (Keys.Contains(keys[i]))
                {
                    // NOTE : 
                    // Ignore for now, don't update the dictionary.
                    // There is no elegant solution to the 'duplicate' issue.
                    // Just make sure that the dev is notified about the issue.

                    if (Application.isPlaying)
                    {
                        Debug.LogWarning(string.Format("[SerializableDictionary] Note : Key {0} is already contained in the dictionary. Please make sure your keys are all unique.", keys[i]));
                    }

                    continue;
                }

                Add(keys[i], values[i]);
            }
        }
    }
    /// <summary>
    /// Obfuscated integer. Stores values with offsets.
    /// </summary>
    public struct ObfuscatedInt
    {
        private int Value;
        private int RandShiftValue;

        public ObfuscatedInt(int value)
        {
            RandShiftValue = UnityEngine.Random.Range(sizeof(int) + 1, int.MaxValue);

            Value = value << RandShiftValue;
        }

        public static implicit operator ObfuscatedInt(int i)
        {
            return new ObfuscatedInt(i);
        }
        public static implicit operator int(ObfuscatedInt i)
        {
            return i.Value >> i.RandShiftValue;
        }
    }

    #endregion
}

#region Unity Editor Additionals

#region ---- Utils
namespace BXFW.Tools.Editor
{
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
                // Null coroutine
                if (CoroutineInProgress[i] == null)
                { continue; }

                // Normal
                if (!CoroutineInProgress[i].MoveNext())
                { CoroutineInProgress.Remove(CoroutineInProgress[i]); }
            }
        }
    }

    /// <summary>
    /// Order of drawing GUI when a match is satisfied in method :
    /// <see cref="EditorAdditionals.DrawCustomDefaultInspector(SerializedObject, Dictionary{string, KeyValuePair{MatchGUIActionOrder, Action}})"/>.
    /// </summary>
    [Flags]
    public enum MatchGUIActionOrder
    {
        // Default Value
        Before = 0,
        After = 1 << 0,
        Omit = 1 << 1,

        OmitAndInvoke = After | Omit
    }

    public static class EditorAdditionals
    {
        #region Property Field Helpers
        private static Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

        /// <summary>
        /// Returns the c# object's fieldInfo and the instance object it comes with.
        /// <br>Important NOTE : The instance object that gets returned with this method may be null.
        /// In these cases use the <returns>return </returns></br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/>
        public static KeyValuePair<FieldInfo, object> GetTarget(this SerializedProperty prop)
        {
            object target = prop.serializedObject.targetObject;
            FieldInfo targetInfo = null;

            string[] propertyNames = prop.propertyPath.Split('.');
            bool isNextPropertyArrayIndex = false;

            for (int i = 0; i < propertyNames.Length && target != null; ++i)
            {
                // Alias the string name. (but we need for for the 'i' variable)
                string propName = propertyNames[i];

                if (propName == "Array" && target is IEnumerable)
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
                            throw new Exception(string.Format(@"[EditorAdditionals::GetTarget] Error while casting targetAsArray.
-> Invalid cast : Tried to cast type {0} as IEnumerable. Current property is {1}.", target.GetType().Name, propName));

                        // FIXME : Should use 'MoveNext' but i don't care. (stupid 'IEnumerator' wasn't started errors).
                        var cntIndex = 0;
                        var isSuccess = false;
                        foreach (var item in targetAsArray)
                        {
                            if (cntIndex == arrayIndex)
                            {
                                target = item;
                                isSuccess = true;

                                break;
                            }

                            cntIndex++;
                        }

                        if (!isSuccess)
                            throw new Exception(string.Format("[EditorAdditionals::GetTarget] Couldn't find SerializedProperty {0} in array {1}.", prop.propertyPath, targetAsArray));
                    }
                    else
                    {
                        throw new Exception(string.Format("[EditorAdditionals::GetTarget] Invalid array index parsing on string : \"{0}\"", propName));
                    }
                }
                else
                {
                    targetInfo = GetField(target, propName);
                    target = targetInfo.GetValue(target);
                }
            }

            return new KeyValuePair<FieldInfo, object>(targetInfo, target);
        }
        /// <summary>
        /// Returns the type of the property's target.
        /// </summary>
        /// <param name="property">Property to get type from.</param>
        public static Type GetFieldType(this SerializedProperty property)
        {
            return property.GetTarget().Key.FieldType;
        }
        /// <summary>
        /// Internal helper method for getting field from properties.
        /// <br>Gets the target normally, if not found searches in <see cref="Type.BaseType"/>.</br>
        /// </summary>
        private static FieldInfo GetField(object target, string name, Type targetType = null)
        {
            if (targetType == null)
            {
                targetType = target.GetType();
            }

            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // If the field info is present.
            if (fi != null)
            {
                return fi;
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetField(target, name, targetType.BaseType);
            }

            // Name field on doesn't exist? Some weird unity bug? Help 
            throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Could not find {0} on {1} and it's children.", name, target));
        }
        #endregion

        #region Gizmos
        /// <summary>
        /// Draws box collider gizmo according to the rotation of the parent transform.
        /// </summary>
        public static void DrawBoxCollider(this Transform transform, Color gizmoColor, BoxCollider boxCollider, float alphaForInsides = 0.3f)
        {
            // Save the color in a temporary variable to not overwrite changes in the inspector (if the sent-in color is a serialized variable).
            var color = gizmoColor;

            // Change the gizmo matrix to the relative space of the boxCollider.
            // This makes offsets with rotation work
            // Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, transform.lossyScale);

            // Draws the edges of the BoxCollider
            // Center is Vector3.zero, since we've transformed the calculation space in the previous step.
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

            // Draws the sides/insides of the BoxCollider, with a tint to the original color.
            color.a *= alphaForInsides;
            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, boxCollider.size);
        }
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
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

            return cam != null ? cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy) : position;
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
        /// Draw default inspector with commands inbetween. (Allowing to put custom gui between)
        /// </summary>
        /// <param name="target">Method target.</param>
        /// <param name="onStringMatchEvent">
        /// The event <see cref="MatchGUIActionOrder"/> match. 
        /// If passed <see langword="null"/> this method will act like <see cref="UnityEditor.Editor.DrawDefaultInspector"/>.
        /// </param>
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
        /// <returns><see cref="EditorGUI.EndChangeCheck"/> (whether if a field was modified inside this method)</returns>
        public static bool DrawCustomDefaultInspector(this SerializedObject obj, Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> onStringMatchEvent)
        {
            if (obj == null)
            {
                Debug.LogError("[BXFW::EditorAdditionals::DrawCustomDefaultInspector] Passed serialized object is null.");
                return false;
            }

            // Do the important steps (otherwise the inspector won't work)
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();

            // Loop through properties and create one field (including children) foreach top level property.
            // Why unity? includeChildren = 'expanded'
            SerializedProperty property = obj.GetIterator();
            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                // Disable if 'm_Script' field that unity adds automatically is being drawn.
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    // -- Check if there is a match
                    if (onStringMatchEvent != null)
                    {
                        string MatchingKey = null;
                        foreach (string s in onStringMatchEvent.Keys)
                        {
                            if (s.Equals(property.name))
                            {
                                MatchingKey = s;
                                break;
                            }
                        }

                        // -- If there's a match of events, use the event.
                        // Once the scope enters here, the upcoming 'EditorGUILayout.PropertyField' is not invoked.
                        if (!string.IsNullOrEmpty(MatchingKey))
                        {
                            var Pair = onStringMatchEvent[MatchingKey];

                            if (Pair.Key == MatchGUIActionOrder.OmitAndInvoke)
                            {
                                if (Pair.Value != null)
                                    Pair.Value();

                                expanded = false;
                                continue;
                            }

                            // -- Omit GUI
                            if (Pair.Key == MatchGUIActionOrder.Omit)
                            {
                                expanded = false;
                                continue;
                            }

                            // -- Standard draw
                            if (Pair.Key == MatchGUIActionOrder.After)
                            { EditorGUILayout.PropertyField(property, true); }

                            if (Pair.Value != null)
                                Pair.Value();

                            if (Pair.Key == MatchGUIActionOrder.Before)
                            { EditorGUILayout.PropertyField(property, true); }

                            expanded = false;
                            continue;
                        }
                    }

                    EditorGUILayout.PropertyField(property, true);
                }

                expanded = false;
            }

            // Save & end method
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
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
        /// Create array with fields.
        /// This is a more primitive array drawer, but it works.
        /// </summary>
        /// <param name="obj">Serialized object of target.</param>
        /// <param name="arr_name">Array field name.</param>
        public static void UnityArrayGUI(this SerializedObject obj, string arr_name, Action<int> OnArrayFieldDrawn = null)
        {
            int prev_indent = EditorGUI.indentLevel;
            var propertyTarget = obj.FindProperty(string.Format("{0}.Array.size", arr_name));

            // Get size of array
            int arrSize = propertyTarget.intValue;

            // Create the size field
            EditorGUI.indentLevel = 1;
            int curr_arr_Size = EditorGUILayout.IntField("Size", arrSize);
            // Clamp
            curr_arr_Size = curr_arr_Size < 0 ? 0 : curr_arr_Size;
            EditorGUI.indentLevel = 3;

            if (curr_arr_Size != arrSize)
            {
                propertyTarget.intValue = curr_arr_Size;
            }

            // Create the array fields (stupid)
            for (int i = 0; i < arrSize; i++)
            {
                // Create property field.
                var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", arr_name, i));

                // If our property is null, ignore.
                if (prop == null)
                    continue;

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prop);
                if (OnArrayFieldDrawn != null)
                    OnArrayFieldDrawn(i);
                GUILayout.EndHorizontal();
            }

            // Keep previous indent
            EditorGUI.indentLevel = prev_indent;
        }

        /// System namespace doesn't have a thing like this so.
        public delegate void RefIndexDelegate<T>(int arg1, ref T arg2);

        ///// <summary>Internal unity icon for icon pointing downwards.</summary>
        //private const string UInternal_PopupTex = "Icon Dropdown";
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
            EditorGUILayout.LabelField(string.Format("{0} List", typeof(T).Name), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            int curr_arr_Size = EditorGUILayout.IntField("Size", GenericDrawList.Length, GUILayout.MaxWidth(200f), GUILayout.MinWidth(150f));
            curr_arr_Size = curr_arr_Size < 0 ? 0 : curr_arr_Size;
            GUILayout.EndHorizontal();

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
                    EditorGUILayout.LabelField(string.Format("Element {0} --------", i), EditorStyles.boldLabel);
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
        /// <summary>
        /// Draws a ui line and returns the padded position rect.
        /// </summary>
        /// <param name="parentRect">Parent rect to draw relative to.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thiccness of the line.</param>
        /// <param name="padding">Padding of the line. (Space left for the line, between properties)</param>
        /// <returns>The new position target rect, offseted.</returns>
        public static Rect DrawUILine(Rect parentRect, Color color, int thickness = 2, int padding = 3)
        {
            // Rect that is passed as an parameter.
            Rect drawRect = new Rect(parentRect.position, new Vector2(parentRect.width, thickness));

            drawRect.y += padding / 2;
            drawRect.x -= 2;
            drawRect.width += 6;

            // Rect with proper height.
            Rect returnRect = new Rect(new Vector2(parentRect.position.x, drawRect.position.y + (thickness + padding)), parentRect.size);
            EditorGUI.DrawRect(drawRect, color);

            return returnRect;
        }
        #endregion
    }
}

#region --- Inspector Variable Attributes
namespace BXFW
{
    /// <summary>
    /// Attribute to draw a line using <see cref="EditorAdditionals.DrawUILine(Color, int, int)"/>.
    /// </summary>
    public class InspectorLineAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Color LineColor { get; private set; }
        public int LineThickness { get; private set; }
        public int LinePadding { get; private set; }
#endif
        public InspectorLineAttribute(float colorR, float colorG, float colorB, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            LineColor = new Color(colorR, colorG, colorB);
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Return the draw height.
        /// </summary>
        /// <param name="parentRect"></param>
        /// <returns></returns>
        public float GetYPosHeightOffset()
        {
            return LineThickness + LinePadding;
        }
#endif
    }

    /// <summary>
    /// Attribute to disable gui on property fields.
    /// </summary>
    public class InspectorReadOnlyViewAttribute : PropertyAttribute { }
}
#endregion

#endregion

#region ---- Additionals Class Inspectors

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ObfuscatedInt))]
    public class ObfuscatedIntPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // This property is drawn like an integer.
            var targetPropField = property.GetTarget().Key;
            var targetProp = (ObfuscatedInt)property.GetTarget().Value;

            EditorGUI.BeginChangeCheck();
            var iField = EditorGUI.IntField(position, new GUIContent(label.text, "Edit obfuscated integer. NOTE : Won't display correctly on 'Debug' inspector"), targetProp);

            if (EditorGUI.EndChangeCheck())
            {
                targetPropField.SetValue(property.serializedObject, iField);
            }

            EditorGUI.HelpBox(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 4, position.width, position.height), "Not complete", MessageType.Warning);
        }
    }

    #region Inspector Attributes Drawers
    // (maybe) TODO : Carry these 'Inspector Attribute Drawers' over to a seperate file.
    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    public class InspectorLineDrawer : PropertyDrawer
    {
        private InspectorLineAttribute targetAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (targetAttribute != null)
                addHeight = targetAttribute.GetYPosHeightOffset();

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            targetAttribute = (InspectorLineAttribute)property.GetTarget().Key.GetCustomAttribute(typeof(InspectorLineAttribute));

            // Draw line and draw UI line.
            var posRect = EditorAdditionals.DrawUILine(position, targetAttribute.LineColor, targetAttribute.LineThickness, targetAttribute.LinePadding);
            EditorGUI.PropertyField(posRect, property, label, true);
        }
    }
    [CustomPropertyDrawer(typeof(InspectorReadOnlyViewAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var gEnabled = GUI.enabled;

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = gEnabled;
        }
    }

    #endregion
}
#endregion

#endregion
