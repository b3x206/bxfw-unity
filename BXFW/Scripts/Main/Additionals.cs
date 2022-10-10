// Standard
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using BXFW.Tools.Editor;
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


namespace BXFW
{
    /// <summary>
    /// The additionals class.
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
        /// <summary>
        /// Draws a sphere in <see cref="Debug"/> context.
        /// </summary>
        public static void DrawSphereDebug(Vector4 pos, float radius, Color color)
        {
            // Make unit sphere.
            var lenSphere = 16;
            var v = new Vector4[lenSphere * 3]; // Sphere vector
            for (int i = 0; i < lenSphere; i++)
            {
                var f = i / (float)lenSphere;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[(0 * lenSphere) + i] = new Vector4(c, s, 0, 1);
                v[(1 * lenSphere) + i] = new Vector4(0, c, s, 1);
                v[(2 * lenSphere) + i] = new Vector4(s, 0, c, 1);
            }

            int len = v.Length / 3;
            for (int i = 0; i < len; i++)
            {
                var sX = pos + (radius * v[0 * len + i]);
                var eX = pos + (radius * v[0 * len + (i + 1) % len]);
                var sY = pos + (radius * v[1 * len + i]);
                var eY = pos + (radius * v[1 * len + (i + 1) % len]);
                var sZ = pos + (radius * v[2 * len + i]);
                var eZ = pos + (radius * v[2 * len + (i + 1) % len]);
                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }
        #endregion

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
        /// <summary>
        /// Adds an explosion to rigidbody.
        /// </summary>
        public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Force)
        {
            var explosionDir = rb.position - explosionPosition;
            var explosionDistance = (explosionDir.magnitude / explosionRadius);

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
        public static float GetBiggestAxis(this Vector2 target)
        {
            if (target.x > target.y)
                return target.x;
            if (target.y > target.x)
                return target.y;

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
        public static void ResizeSpriteToScreen(this Camera relativeCam, SpriteRenderer sr, TransformAxis2D axis = TransformAxis2D.XYAxis)
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
        /// <summary>Converts <see cref="Vector2"/> to positive values.</summary>
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        /// <summary>Converts <see cref="Vector3"/> to positive values.</summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        /// <summary>Converts <see cref="Vector4"/> to positive values.</summary>
        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
        }
        /// <summary>
        /// Mapped lerp.
        /// </summary>
        /// <param name="from">Returned value start.</param>
        /// <param name="to">Returned value end.</param>
        /// <param name="from2">Range value start.</param>
        /// <param name="to2">Range value end.</param>
        /// <param name="value">Value mapped.</param>
        public static float Map(float from, float to, float from2, float to2, float value)
        {
            if (value <= from2)
                return from;
            else if (value >= to2)
                return to;

            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
        /// <summary>Get types that has the <paramref name="AttributeType"/> attribute from <see cref="Assembly"/> <paramref name="AttributeAssem"/>.</summary>
        /// <returns>The types with the attribute <paramref name="AttributeType"/>.</returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Type AttributeType, Assembly AttributeAssem = null)
        {
            if (AttributeAssem == null)
            {
                AttributeAssem = AttributeType.Assembly;
            }

            foreach (Type type in AttributeAssem.GetTypes())
            {
                if (type.GetCustomAttributes(AttributeType, true).Length > 0)
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
            return Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where((Type myType) => { return myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)); });
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
#if CSHARP_7_3_OR_NEWER
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
        public static ExpectT BLoad<ExpectT>(char[] fileContents)
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

            return BLoad<ExpectT>(fileContentData);
        }
        /// <summary>
        /// Loads binary saved data from bytes.
        /// </summary>
        /// <typeparam name="ExpectT"></typeparam>
        /// <param name="fileContents">The content of the folder to make data from.</param>
        /// <returns></returns>
        public static ExpectT BLoad<ExpectT>(byte[] fileContents)
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

    /// <summary>
    /// GUI additionals.
    /// Provides GUI related utils.
    /// </summary>
    public static class GUIAdditionals
    {
        /// <summary>
        /// Hashes contained in the <see cref="GUI"/> class, 
        /// for use with <see cref="GUIUtility.GetControlID(int, FocusType, Rect)"/>.
        /// </summary>
        public static class HashList
        {
            public static readonly int BoxHash = "Box".GetHashCode();
            public static readonly int ButtonHash = "Button".GetHashCode();
            public static readonly int RepeatButtonHash = "repeatButton".GetHashCode();
            public static readonly int ToggleHash = "Toggle".GetHashCode();
            public static readonly int ButtonGridHash = "ButtonGrid".GetHashCode();
            public static readonly int SliderHash = "Slider".GetHashCode();
            public static readonly int BeginGroupHash = "BeginGroup".GetHashCode();
            public static readonly int ScrollviewHash = "scrollView".GetHashCode();
        }

        private static bool isBeingDragged = false; // Since we only have one mouse cursor lol
        private static int hotControlID = -1; // Gotta keep the id otherwise we can't differentiate what we are dragging
                                              // We could also use GUIUtility.hotControl or some field like that
        public static int HotControlID => hotControlID;
        private static int lastInteractedControlID = -1; // Keep the last interacted one too
        public static int LastHotControlID => lastInteractedControlID;

        public static int DraggableBox(Rect rect, GUIContent content, Action<Vector2> onDrag)
        {
            return DraggableBox(rect, content, GUI.skin.box, onDrag);
        }

        public static int DraggableBox(Rect rect, GUIContent content, GUIStyle style, Action<Vector2> onDrag)
        {
            return DraggableBox(rect, (bool _) =>
            {
                GUI.Box(rect, content, style);
            }, onDrag);
        }

        /// <summary>
        /// <br>Usage: Create a global rect for your draggable box. Pass the global variables here.</br>
        /// Puts a draggable box.
        /// <br>The <paramref name="onDrag"/> is invoked when the box is being dragged.</br>
        /// </summary>
        /// <returns>The control id of this gui.</returns>
        public static int DraggableBox(Rect rect, Action<bool> onDrawButton, Action<Vector2> onDrag)
        {
            int controlID = GUIUtility.GetControlID(HashList.RepeatButtonHash, FocusType.Passive, rect);
            
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    isBeingDragged = true;
                    hotControlID = controlID;
                    lastInteractedControlID = controlID;
                }
            }
            if (isBeingDragged && hotControlID == controlID)
            {
                if (Event.current.type == EventType.MouseDrag)
                { 
                    onDrag(Event.current.delta);
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isBeingDragged = false;
                    hotControlID = -1;
                }
            }

            // Or use an <see cref="GUI.RepeatButton"/> for 'isBeingDragged' lol
            // This is required for event to be drag.
            // This may allow more styles, but we are already using 'onDrawButton' delegate anyways
            GUI.Button(rect, GUIContent.none, GUIStyle.none);
            onDrawButton(isBeingDragged && hotControlID == controlID);
            
            return controlID;
        }

        /// <summary>
        /// Draws line.
        /// <br>Color defaults to <see cref="Color.white"/>.</br>
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, int width)
        {
            DrawLine(start, end, width, Color.white);
        }

        /// <summary>
        /// Draws line with color.
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, int width, Color col)
        {
            var gc = GUI.color;
            GUI.color = col;
            DrawLine(start, end, width, Texture2D.whiteTexture);
            GUI.color = gc;
        }

        /// <summary>
        /// Draws line with texture.
        /// <br>The texture is not used for texture stuff, only for color if your line is not thick enough.</br>
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, int width, Texture2D tex)
        {
            var guiMat = GUI.matrix;

            if (start == end) return;
            if (width <= 0) return;

            Vector2 d = end - start;
            float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
            if (d.x < 0)
                a += 180;

            int width2 = (int)Mathf.Ceil(width / 2);

            GUIUtility.RotateAroundPivot(a, start);
            GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), tex);

            GUI.matrix = guiMat;
        }

        /// <summary>
        /// Draws a ui line and returns the padded position rect.
        /// <br>For angled / rotated lines, use the  method.</br>
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
            if (Event.current.type == EventType.Repaint)
            {
                var gColor = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(drawRect, Texture2D.whiteTexture);
                GUI.color = gColor;
            }

            return returnRect;
        }

        /// <summary>
        /// Draws a straight line in the gui system. (<see cref="GUILayout"/> method)
        /// <br>For angled / rotated lines, use the <see cref="DrawLine"/> method.</br>
        /// </summary>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thiccness of the line.</param>
        /// <param name="padding">Padding of the line. (Space left for the line, between properties)</param>
        public static void DrawUILineLayout(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = GUILayoutUtility.GetRect(1f, float.MaxValue, padding + thickness, padding + thickness);

            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;

            if (Event.current.type == EventType.Repaint)
            {
                var gColor = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = gColor;
            }
        }

        #region RenderTexture Based
        private static readonly RenderTexture tempRT = new RenderTexture(1, 1, 1, RenderTextureFormat.ARGB32);
        /// <summary>
        /// An unlit material with color.
        /// </summary>
        private static readonly Material tempUnlitMat = new Material(Shader.Find("Custom/Unlit/UnlitTransparentColorShader"));
        private static readonly Material tempCircleMat = new Material(Shader.Find("Custom/Vector/Circle"));

        /// <summary>
        /// Get a texture of a circle.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="circleColor"></param>
        /// <param name="strokeColor"></param>
        /// <param name="strokeThickness"></param>
        /// <returns></returns>
        public static Texture GetCircleTexture(Vector2 size, Color circleColor, Color strokeColor = default, float strokeThickness = 0f)
        {
            tempCircleMat.color = circleColor;
            tempCircleMat.SetColor("_StrokeColor", strokeColor);
            tempCircleMat.SetFloat("_StrokeThickness", strokeThickness);

            return BlitQuad(size, tempCircleMat);
        }

        /// <summary>
        /// Get a texture of a rendered mesh.
        /// </summary>
        public static Texture GetMeshTexture(Vector2 size, Mesh meshTarget, Material meshMat,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            // No need for a 'BlitMesh' method for this, as it's already complicated

            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(size.x);
            tempRT.height = Mathf.CeilToInt(size.y);
            Matrix4x4 matrixMesh = Matrix4x4.TRS(meshPos, meshRot, meshScale);

            // This is indeed redundant code, BUT: we also apply transform data to camera also so this is required
            if (camProj == Matrix4x4.identity)
                camProj = Matrix4x4.Ortho(-1, 1, -1, 1, 0.01f, 1024f);

            Matrix4x4 matrixCamPos = Matrix4x4.TRS(camPos, camRot, new Vector3(1, 1, -1));
            Matrix4x4 matrixCam = (camProj * matrixCamPos.inverse);

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, meshTarget, meshMat);
            return tempRT;
        }

        /// <summary>
        /// Get a texture of a quad, rendered using that material.
        /// <br>It is not recommended to use this method as shaders could be moving.
        /// Use the <see cref="DrawMaterialTexture(Rect, Texture2D, Material)"/> instead.</br>
        /// </summary>
        public static Texture GetMaterialTexture(Vector2 size, Texture2D texTarget, Material matTarget)
        {
            matTarget.mainTexture = texTarget;

            return BlitQuad(size, matTarget);
        }

        /// <summary>
        /// Utility method to bilt a quad (to variable <see cref="tempRT"/>)
        /// </summary>
        internal static RenderTexture BlitQuad(Vector2 size, Material matTarget)
        {
            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(size.x);
            tempRT.height = Mathf.CeilToInt(size.y);

            // Stretch quad (to fit into texture)
            Vector3 scale = new Vector3(1f, 1f * tempRT.Aspect(), 1f);
            // the quad that we get using GetQuad is offsetted.
            Matrix4x4 matrixMesh = Matrix4x4.TRS(new Vector3(0.5f, -0.5f, -1f), Quaternion.AngleAxis(-180, Vector3.up), scale);
            //Matrix4x4 matrixCam = Matrix4x4.Ortho(-1, 1, -1, 1, .01f, 1024f);
            Matrix4x4 matrixCam = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, .01f, 1024f);

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, RenderTextureUtils.GetQuad(), matTarget);

            return tempRT;
        }

        /// <summary>
        /// Internal utility method to draw quads (with materials).
        /// </summary>
        internal static void DrawQuad(Rect guiRect, Material matTarget)
        {
            GUI.DrawTexture(guiRect, BlitQuad(guiRect.size, matTarget));
        }

        /// <summary>
        /// Draws a circle at given rect.
        /// </summary>
        public static void DrawCircle(Rect guiRect, Color circleColor, Color strokeColor = default, float strokeThickness = 0f)
        {
            tempCircleMat.color = circleColor;
            tempCircleMat.SetColor("_StrokeColor", strokeColor);
            tempCircleMat.SetFloat("_StrokeThickness", strokeThickness);

            DrawQuad(guiRect, tempCircleMat);
        }

        /// <summary>
        /// Draws a texture with material.
        /// </summary>
        /// <param name="guiRect"></param>
        /// <param name="texTarget"></param>
        /// <param name="matTarget"></param>
        public static void DrawMaterialTexture(Rect guiRect, Texture2D texTarget, Material matTarget)
        {
            matTarget.mainTexture = texTarget;

            DrawQuad(guiRect, matTarget);
        }

        /// <summary>
        /// Draws a textured white mesh.
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Texture2D meshTexture,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            DrawMesh(guiRect, meshTarget, meshTexture, Color.white, meshPos, meshRot, meshScale, camPos, camRot, camProj);
        }

        /// <summary>
        /// Draws a mesh on the given rect.
        /// <br>Uses a default unlit material for the material field.</br>
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Color meshColor,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            DrawMesh(guiRect, meshTarget, null, meshColor, meshPos, meshRot, meshScale, camPos, camRot, camProj);
        }

        /// <summary>
        /// Draws a textured mesh with a color of your choice.
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Texture2D meshTexture, Color meshColor,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            var mcPrev = tempUnlitMat.color;
            tempUnlitMat.mainTexture = meshTexture;
            tempUnlitMat.color = meshColor;

            DrawMesh(guiRect, meshTarget, tempUnlitMat, meshPos, meshRot, meshScale, camPos, camRot, camProj);

            tempUnlitMat.color = mcPrev;
        }

        /// <summary>
        /// Draws a mesh on the given rect.
        /// <br>The mesh is centered to camera, however the position, rotation and the scale of the mesh can be changed.</br>
        /// <br>The target resolution is the size of the <paramref name="guiRect"/>.</br>
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Material meshMat,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(guiRect.size.x);
            tempRT.height = Mathf.CeilToInt(guiRect.size.y);
            Matrix4x4 matrixMesh = Matrix4x4.TRS(meshPos, meshRot, meshScale);

            // This is indeed redundant code, BUT: we also apply transform data to camera also so this is required
            if (camProj == Matrix4x4.identity)
                camProj = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, .01f, 1024f);

            Matrix4x4 matrixCamPos = Matrix4x4.TRS(camPos, camRot, new Vector3(1, 1, -1));
            Matrix4x4 matrixCam = (camProj * matrixCamPos.inverse);

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, meshTarget, meshMat);

            GUI.DrawTexture(guiRect, tempRT);
        }
        #endregion
    }

    /// Changes Done :
    ///   * Fixed formatting and added discards for unused RenderTexture's.
    ///   * Renamed class from RTUtils -> RenderTextureUtils
    ///   * removed some pointless methods (such as DrawTextureGUI without rect input)
    ///   * (some) Methods can now take camera matrixes
    /// 
    /// Notes :
    ///   * I have no idea what the 'Draw' methods do (the blit methods atleast do something)
    ///   
    /// RTUtils by Nothke
    /// 
    /// RenderTexture utilities for direct drawing meshes, texts and sprites and converting to Texture2D.
    /// Requires BlitQuad shader (or also known as a default sprite shader).
    /// 
    /// ============================================================================
    ///
    /// MIT License
    ///
    /// Copyright(c) 2021 Ivan Notaroš
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in all
    /// copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    /// SOFTWARE.
    /// 
    /// ============================================================================
    public static class RenderTextureUtils
    {
        #region Quad creation
        static Mesh quad;
        public static Mesh GetQuad()
        {
            if (quad)
                return quad;

            Mesh mesh = new Mesh();

            float width = 1;
            float height = 1;

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(width, 0, 0),
                new Vector3(0, height, 0),
                new Vector3(width, height, 0)
            };
            mesh.vertices = vertices;

            int[] tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
            mesh.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            mesh.uv = uv;

            quad = mesh;
            return quad;
        }

        static Shader blitShader;
        public static Shader GetBlitShader()
        {
            if (blitShader)
                return blitShader;

            const string SHADER_NAME = "Sprites/Default";
            var shader = Shader.Find(SHADER_NAME);

            if (!shader)
                Debug.LogError(string.Format("Shader with name '{0}' is not found, did you forget to include it in the project settings?", SHADER_NAME));

            blitShader = shader;
            return blitShader;
        }

        static Material blitMaterial;
        public static Material GetBlitMaterial()
        {
            if (!blitMaterial)
                blitMaterial = new Material(GetBlitShader());

            return blitMaterial;
        }
        #endregion

        static RenderTexture prevRT;

        public static void BeginOrthoRendering(this RenderTexture rt, float zBegin = -100, float zEnd = 100)
        {
            // Create an orthographic matrix (for 2D rendering)
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, zBegin, zEnd);

            rt.BeginRendering(projectionMatrix);
        }

        public static void BeginPixelRendering(this RenderTexture rt, float zBegin = -100, float zEnd = 100)
        {
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, rt.width, 0, rt.height, zBegin, zEnd);

            rt.BeginRendering(projectionMatrix);
        }

        public static void BeginPerspectiveRendering(
            this RenderTexture rt, float fov, in Vector3 position, in Quaternion rotation,
            float zNear = 0.01f, float zFar = 1000f)
        {
            float aspect = (float)rt.width / rt.height;
            Matrix4x4 projectionMatrix = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
            Matrix4x4 viewMatrix = Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

            Matrix4x4 cameraMatrix = (projectionMatrix * viewMatrix.inverse);

            rt.BeginRendering(cameraMatrix);
        }

        public static void BeginRendering(this RenderTexture rt, Matrix4x4 projectionMatrix)
        {
            // This fixes flickering (by @guycalledfrank)
            // (because there's some switching back and forth between cameras, I don't fully understand)
            if (Camera.current != null)
                projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

            // Remember the current texture and make our own active
            prevRT = RenderTexture.active;
            RenderTexture.active = rt;

            // Push the projection matrix
            GL.PushMatrix();
            GL.LoadProjectionMatrix(projectionMatrix);
        }

        public static void EndRendering(this RenderTexture _)
        {
            // Pop the projection matrix to set it back to the previous one
            GL.PopMatrix();

            // Revert culling
            GL.invertCulling = false;

            // Re-set the RenderTexture to the last used one
            RenderTexture.active = prevRT;
        }

        #region Blit Once Functions
        /// <summary>
        /// Draws a mesh to render texture.
        /// Position is defined in camera view 0-1 space where 0,0 is in bottom left corner.
        /// <para>
        /// For non-square textures, aspect ratio will be calculated so that the 0-1 space fits in the width.
        /// Meaning that, for example, wider than square texture will have larger font size per texture area.
        /// </para>
        /// </summary>
        public static void BlitTMPText(this RenderTexture rt, TMP_Text text, in Vector2 pos, float size,
            bool clear = true, Color clearColor = default)
        {
            float aspect = (float)rt.width / rt.height;
            Vector3 scale = new Vector3(size, size * aspect, 1f);

            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);
            BlitTMPText(rt, text, matrix, clear, clearColor);
        }

        public static void BlitTMPText(this RenderTexture rt, TMP_Text text, Matrix4x4 objectMatrix,
            bool clear = true, Color clearColor = default)
        {
            Material mat = text.fontSharedMaterial;
            BlitMesh(rt, objectMatrix, Matrix4x4.identity, text.mesh, mat, clear, true, clearColor);
        }

        public static void BlitMesh2D(this RenderTexture rt, Mesh mesh, in Vector2 pos, float size, Material material,
            bool invertCulling = true, bool clear = true, Color clearColor = default)
        {
            float aspect = (float)rt.width / rt.height;
            Vector3 scale = new Vector3(size, size * aspect, 1f);

            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);
            BlitMesh(rt, matrix, Matrix4x4.identity, mesh, material, invertCulling, clear, clearColor);
        }

        public static void BlitMesh(this RenderTexture rt, Mesh mesh, in Vector3 pos, Quaternion rot, in Vector3 scale, Material material,
            bool invertCulling = true, bool clear = true, Color clearColor = default)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scale);
            BlitMesh(rt, matrix, Matrix4x4.identity, mesh, material, invertCulling, clear, clearColor);
        }

        /// <summary>
        /// Draws a mesh to render texture. The camera space is defined in normalized 0-1 coordinates, near and far planes are -100 and 100.
        /// </summary>
        /// <param name="objectMatrix">The model-matrix of the object</param>
        /// <param name="invertCulling">In case the mesh renders inside-out, toggle this</param>
        /// <param name="clear">Clears a texture to clearColor before drawing</param>
        public static void BlitMesh(this RenderTexture rt, Matrix4x4 objectMatrix, Matrix4x4 projectionMatrix, Mesh mesh, Material material,
            bool invertCulling = true, bool clear = true, Color clearColor = default)
        {
            // Create an orthographic matrix (for 2D rendering)
            // You can otherwise use Matrix4x4.Perspective()
            if (projectionMatrix == Matrix4x4.identity)
                projectionMatrix = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, .01f, 1024f);

            // This fixes flickering (by @guycalledfrank)
            // (because there's some switching back and forth between cameras, I don't fully understand)
            if (Camera.current != null)
                projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

            // Remember the current texture and set our own as "active".
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;

            // Set material as "active". Without this, Unity editor will freeze.
            bool canRender = material.SetPass(0);

            // Push the projection matrix
            GL.PushMatrix();
            GL.LoadProjectionMatrix(projectionMatrix);

            // It seems that the faces are in a wrong order, so we need to flip them
            GL.invertCulling = invertCulling;

            // Clear the texture
            if (clear)
                GL.Clear(true, true, clearColor);
            
            // Draw the mesh!
            if (canRender)
                Graphics.DrawMeshNow(mesh, objectMatrix);
            else
                Debug.LogWarning($"[RenderTextureUtils::BlitMesh] Material with shader {material.shader.name} couldn't be rendered!");

            // Pop the projection matrix to set it back to the previous one
            GL.PopMatrix();

            // Revert culling
            GL.invertCulling = false;

            // Re-set the RenderTexture to the last used one
            RenderTexture.active = prevRT;
        }
        #endregion

        #region Draw Functions
        public static void DrawMesh(this RenderTexture _, Mesh mesh, Material material, in Matrix4x4 matrix, int pass = 0)
        {
            if (mesh == null)
                throw new NullReferenceException("[RenderTextureExtensions::DrawMesh] Argument 'mesh' cannot be null.");

            bool canRender = material.SetPass(pass);

            if (canRender)
                Graphics.DrawMeshNow(mesh, matrix);
        }

        public static void DrawTMPText(this RenderTexture rt, TMP_Text text, in Vector2 position, float size)
        {
            float aspect = (float)rt.width / rt.height;
            Vector3 scale = new Vector3(size, size * aspect, 1);

            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);

            rt.DrawTMPText(text, matrix);
        }

        public static void DrawTMPText(this RenderTexture rt, TMP_Text text, in Matrix4x4 matrix)
        {
            Material material = Application.isPlaying ? text.fontMaterial : text.fontSharedMaterial;
            rt.DrawMesh(text.mesh, material, matrix);
        }

        public static void DrawQuad(this RenderTexture rt, Material material, in Rect rect)
        {
            Matrix4x4 objectMatrix = Matrix4x4.TRS(
                rect.position, Quaternion.identity, rect.size);

            rt.DrawMesh(GetQuad(), material, objectMatrix);
        }

        public static void DrawSprite(this RenderTexture rt, Texture texture, in Rect rect)
        {
            Material material = GetBlitMaterial();
            material.mainTexture = texture;

            DrawQuad(rt, material, rect);
        }
        #endregion

        #region Utils
        public static float Aspect(this Texture rt) => (float)rt.width / rt.height;

        public static Texture2D ConvertToTexture2D(this RenderTexture rt,
            TextureFormat format = TextureFormat.RGB24,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, format, false)
            {
                filterMode = filterMode
            };

            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prevActive;
            return tex;
        }
        #endregion
    }

    #region Helper Enums
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
    public enum TransformAxis2D
    {
        None = 0,

        XAxis,
        YAxis,

        XYAxis
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
    #endregion
}

#region Unity Editor Additionals

#region ---- Utils
#if UNITY_EDITOR
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
        private static readonly List<IEnumerator> CoroutineInProgress = new List<IEnumerator>();

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
        #region Other
        /// <summary>
        /// Directory of the 'Resources' file (for bxfw assets generally).
        /// <br>Returns the 'Editor' and other necessary folders for methods that take absolute paths.</br>
        /// </summary>
        public static readonly string ResourcesDirectory = string.Format("{0}/Assets/Resources", Directory.GetCurrentDirectory());
        #endregion

        #region Prefab Utility
        /// <summary>
        /// NOTES ABOUT THIS CLASS:
        /// <para>
        ///     1: It handles creation 
        ///     <br>2: It edits (because it's callback of <see cref="ProjectWindowUtil.StartNameEditingIfProjectWindowExists(int, EndNameEditAction, string, Texture2D, string)"/>, what type of method is that?)</br>
        /// </para>
        /// </summary>
        internal class CreateAssetEndNameEditAction : EndNameEditAction
        {
            /// <summary>
            /// Called when the creation ends.
            /// <br>The int parameter returns the <see cref="UnityEngine.Object.GetInstanceID"/>.</br>
            /// </summary>
            internal event Action<int> OnRenameEnd;

            /// <summary>
            /// Action to invoke.
            /// <br>If the object exists (<paramref name="instanceId"/> isn't invalid) it will use <see cref="AssetDatabase.CreateAsset(UnityEngine.Object, string)"/></br>
            /// <br>If the object does NOT exist (<paramref name="instanceId"/> IS invalid) it will use <see cref="AssetDatabase.CopyAsset(string, string)"/>.</br>
            /// </summary>
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string uniqueName = AssetDatabase.GenerateUniqueAssetPath(pathName);
                if ((instanceId == 0 || instanceId == int.MaxValue - 1) && !string.IsNullOrEmpty(resourceFile))
                {
                    // Copy existing asset (if no reference asset was given)
                    AssetDatabase.CopyAsset(resourceFile, uniqueName);
                }
                else
                {
                    // Create new asset from asset (if reference asset was given)
                    AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), uniqueName);
                }

                // Handle events
                OnRenameEnd?.Invoke(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(uniqueName).GetInstanceID());
            }
        }

        /// <summary>
        /// Creates an instance of prefab <paramref name="prefabReferenceTarget"/> and renames it like an new object was created.
        /// <br><b>NOTE</b> : Make sure '<paramref name="prefabReferenceTarget"/>' is an prefab!</br>
        /// </summary>
        /// <param name="prefabReferenceTarget">The prefab target. Make sure this is an prefab.</param>
        /// <param name="path">Creation path. If left null the current folder will be selected.</param>
        /// <param name="onRenameEnd">Called when object is renamed. The <see cref="int"/> parameter is the InstanceID of the object.</param>
        // Use <see cref="EditorUtility"/> & <see cref="AssetDatabase"/>'s utility functions to make meaning out of it.
        public static void CopyPrefabReferenceAndRename(GameObject prefabReferenceTarget, string path = null, Action<int> onRenameEnd = null)
        {
            // Create at the selected directory
            if (string.IsNullOrEmpty(path))
                path = Selection.activeObject == null ? "Assets" : AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                Debug.LogError(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Directory '{0}' does not exist.", path));
                return;
            }
            if (PrefabUtility.GetCorrespondingObjectFromSource(prefabReferenceTarget) == null)
            {
                Debug.LogError(string.Format("[EditorAdditionals::CopyPrefabInstanceAndRename] Prefab to copy is invalid (not a prefab). prefabTarget was = '{0}'", prefabReferenceTarget));
                return;
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                // Get path & target prefab to copy
                GameObject targetPrefabInst = prefabReferenceTarget;
                path = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(path, targetPrefabInst.name)}.prefab"); // we are copying prefabs anyway

                // Register 'OnFileNamingEnd' function.
                var assetEndNameAction = ScriptableObject.CreateInstance<CreateAssetEndNameEditAction>();
                assetEndNameAction.OnRenameEnd += (int instanceIDSaved) =>
                {
                    var createdObject = EditorUtility.InstanceIDToObject(instanceIDSaved);

                    Selection.activeObject = createdObject; // Select renamed object
                };
                assetEndNameAction.OnRenameEnd += onRenameEnd;

                // wow very obvious (unity api moment)
                Texture2D icon = AssetPreview.GetMiniThumbnail(targetPrefabInst); // Get the thumbnail from the target prefab

                // Since this method is 'very well documented' here's what i found =>
                // instanceID   = Target instance ID to edit (this is handled in the file rename callback ending)
                //      (if it exists it will also edit that file alongside, we will create our own asset path so we pass invalid value, otherwise the object will be cloned.)
                // pathName     = Directory to file of the destination asset
                // resourceName = Directory to file of the source asset
                // icon         = Asset icon, not very necessary (can be null)

                // THIS. IS. SO. DUMB. (that even unity's asset developers wrote a wrapper function for this method lol)
                // Note : Pass invalid 'Instance ID' for editing an new object
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(int.MaxValue - 1, assetEndNameAction, path, icon, AssetDatabase.GetAssetPath(targetPrefabInst.GetInstanceID()));
            }
            else
            {
                Debug.LogWarning($"[ShopItemPreviewEditor::CopyPrefabInstanceAndRename] Path received for creating prefab '{path}' is not a path.");
            }
        }
        // TODO : Add method to also copy from already existing instance id (overwrite method?)
        #endregion

        #region Property Field Helpers
        // This allows for getting the property field target
        
        // we could use c# string method abuse or SerializedObject.GetArrayIndexSomething(index) method.
        // No, not really that is for getting the array object? idk this works good so no touchy unless it breaks
        private static readonly Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

        /// <summary>
        /// Returns the c# object's fieldInfo and the instance object it comes with.
        /// <br>Important NOTE : The instance object that gets returned with this method may be null.
        /// In these cases use the <returns>return </returns></br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/> <exception cref="NullReferenceException"/>
        public static KeyValuePair<FieldInfo, object> GetTarget(this SerializedProperty prop)
        {
            if (prop == null)
                throw new NullReferenceException("[EditorAdditionals::GetTarget] Field 'prop' is null!");

            return GetTarget(prop.serializedObject.targetObject, prop.propertyPath);
        }
        /// <summary>
        /// Returns the c# object's fieldInfo and the PARENT object it comes with. (this is useful with <see langword="struct"/>)
        /// <br>Important NOTE : The instance object that gets returned with this method may be null (or not).
        /// In these cases use the return (the FieldInfo)</br>
        /// <br/>
        /// <br>If you are using this for <see cref="CustomPropertyDrawer"/>, this class has an <see cref="FieldInfo"/> property named <c>fieldInfo</c>, 
        /// you can use that instead of the bundled field info.</br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="Exception"/> <exception cref="NullReferenceException"/>
        public static KeyValuePair<FieldInfo, object> GetParentOfTargetField(this SerializedProperty prop, int parentDepth = 1)
        {
            int lastIndexOfPeriod = prop.propertyPath.LastIndexOf('.');
            for (int i = 1; i < parentDepth; i++)
                lastIndexOfPeriod = prop.propertyPath.LastIndexOf('.', lastIndexOfPeriod - 1);

            if (lastIndexOfPeriod == -1)
            {
                // No depth, instead return the field info from this scriptable object (use the parent scriptable object ofc)
                var fInfo = GetField(prop.serializedObject.targetObject, prop.name);

                return new KeyValuePair<FieldInfo, object>(fInfo, prop.serializedObject.targetObject);
            }

            string lastPropertyName = prop.propertyPath.Substring(lastIndexOfPeriod + 1, prop.propertyPath.Length - (lastIndexOfPeriod + 1));
            string propertyNamesExceptLast = prop.propertyPath.Substring(0, lastIndexOfPeriod);

            var pair = GetTarget(prop.serializedObject.targetObject, propertyNamesExceptLast);

            return new KeyValuePair<FieldInfo, object>(pair.Key.FieldType.GetField(lastPropertyName), pair.Value);
        }

        /// <summary>
        /// Internal method to get parent from these given parameters.
        /// <br>Traverses <paramref name="targetObjOfSrprop"/> using reflection and with the help of <paramref name="propertyPath"/>.</br>
        /// </summary>
        /// <param name="targetObjOfSrprop">Target (parent) object of <see cref="SerializedProperty"/>. Pass <see cref="SerializedProperty.serializedObject"/>.targetObject.</param>
        /// <param name="propertyPath">Path of the property. Pass <see cref="SerializedProperty.propertyPath"/>.</param>
        /// <exception cref="Exception"/>
        private static KeyValuePair<FieldInfo, object> GetTarget(UnityEngine.Object targetObjOfSrprop, string propertyPath)
        {
            object target = targetObjOfSrprop; // This is kinda required
            FieldInfo targetInfo = null;
            string[] propertyNames = propertyPath.Split('.');

            bool isNextPropertyArrayIndex = false;

            for (int i = 0; i < propertyNames.Length && target != null; i++)
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

                        if (!(target is IEnumerable targetAsArray))
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
                            throw new Exception(string.Format("[EditorAdditionals::GetTarget] Couldn't find SerializedProperty {0} in array {1}.", propertyPath, targetAsArray));
                    }
                    else // Array parse failure, should only happen on the ends of the array (i.e size field)
                    {
                        // Instead of throwing an exception, get the object
                        // (as this may be called for the 'int size field' on the editor, for some reason)
                        try
                        {
                            targetInfo = GetField(target, propName);
                            target = targetInfo.GetValue(target);
                        }
                        catch
                        {
                            // It can also have an non-existent field for some reason
                            // Because unity, so we give up (with the last information we have)
                            // Maybe we should print a warning, but it's not too much of a thing (just a fallback)

                            return new KeyValuePair<FieldInfo, object>(targetInfo, target);
                        }
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
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Null 'name' field. (target: '{0}', targetType: '{1}')", target, targetType));

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

            throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Could not find '{0}' on '{1}' and it's children.", name, target));
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
        internal static Vector3 TransformByPixel(Vector3 position, float x, float y)
        {
            return TransformByPixel(position, new Vector3(x, y));
        }
        internal static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;

            return cam != null ? cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy) : position;
        }
        #endregion

        #region Inspector-Editor Draw
        /// <summary>
        /// Make gui area drag and droppable.
        /// </summary>
        public static void MakeDroppableAreaGUI(Action onDragAcceptAction, Func<bool> shouldAcceptDragCheck, Rect? customRect = null)
        {
            var shouldAcceptDrag = shouldAcceptDragCheck.Invoke();
            if (!shouldAcceptDrag) return;

            MakeDroppableAreaGUI(onDragAcceptAction, customRect);
        }
        /// <summary>
        /// Make gui area drag and drop.
        /// <br>This always accepts drops.</br>
        /// <br>Usage : <see cref="DragAndDrop.objectReferences"/> is all you need.</br>
        /// </summary>
        public static void MakeDroppableAreaGUI(Action onDragAcceptAction, Rect? customRect = null)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Rect dropArea = customRect ??
                        GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        onDragAcceptAction?.Invoke();
                    }
                    break;
            }
        }

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
        /// Draw default inspector with commands inbetween. (Allowing to put custom gui between).
        /// <br>This works as same as <see cref="UnityEditor.Editor.OnInspectorGUI"/>'s <see langword="base"/> call.</br>
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
                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
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

        /// <summary>
        /// Returns the children of the SerializedProperty.
        /// </summary>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);
            while (true)
            {
                if (SerializedProperty.EqualContents(property, nextElement))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets visible children of '<see cref="SerializedProperty"/>' at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent '<see cref="SerializedProperty"/>'.</param>
        /// <returns>Collection of '<see cref="SerializedProperty"/>' children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    // Use '.Copy' for making 'Linq ToArray' work
                    using var ret = currentProperty.Copy();
                    yield return ret;
                }
                while (currentProperty.NextVisible(false));
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
        /// <summary>
        /// Shows an array inspector (using unity default).
        /// </summary>
        /// <param name="list"></param>
        /// <param name="options"></param>
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
        /// <param name="arrayName">Array field name.</param>
        public static void UnityArrayGUI(this SerializedObject obj, string arrayName, Action<int> OnArrayFieldDrawn = null)
        {
            int prev_indent = EditorGUI.indentLevel;
            var propertyTarget = obj.FindProperty(string.Format("{0}.Array.size", arrayName));

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
                var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", arrayName, i));

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
        public static bool UnityArrayGUICustom<T>(bool toggleDropdwnState, ref T[] GenericDrawList, RefIndexDelegate<T> OnArrayFieldDrawn) where T : new()
        {
            int prev_indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = prev_indent + 2;
            // Create the size & dropdown field
            GUILayout.BeginHorizontal();

            var currToggleDropdwnState = GUILayout.Toggle(toggleDropdwnState, string.Empty, EditorStyles.popup, GUILayout.MaxWidth(20f));
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
                    EditorGUILayout.LabelField(string.Format("Element {0}", i), EditorStyles.boldLabel);
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
            return currToggleDropdwnState;
        }
        #endregion
    }
}
#endif

#region --- Inspector Variable Attributes
namespace BXFW
{
    /// <summary>
    /// Attribute to draw <see cref="Sprite"/> fields as a big preview.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    public class InspectorBigSpriteFieldAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly float spriteBoxRectHeight = 44f;
#endif
        public InspectorBigSpriteFieldAttribute(float spriteHeight = 44f)
        {
#if UNITY_EDITOR
            spriteBoxRectHeight = spriteHeight;
#endif
        }
    }

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
        internal float GetYPosHeightOffset()
        {
            return LineThickness + LinePadding;
        }
#endif
    }

    /// <summary>
    /// Attribute to disable gui on fields.
    /// </summary>
    public class InspectorReadOnlyViewAttribute : PropertyAttribute { }
}
#endregion

#endregion

#region ---- Additionals Class Inspectors
#if UNITY_EDITOR
namespace BXFW.ScriptEditor
{
    #region Inspector Attributes Drawers
    // (maybe) TODO : Carry these 'Inspector Attribute Drawers' over to a seperate file.
    // TODO : Use a class named => DecoratorDrawer
    //      -> This will be useful for the 'InspectorLine' and other drawers being compliant with each other
    //      -> (PropertyDrawer is the most manual way of doing it)
    /// <summary>
    /// Draws the '<see cref="Texture2D"/>' inspector for sprites.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorBigSpriteFieldAttribute))]
    internal class InspectorBigSpriteFieldDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private float targetBoxRectHeight
        {
            get
            {
                var targetAttribute = attribute as InspectorBigSpriteFieldAttribute;

                return targetAttribute.spriteBoxRectHeight;
            }
        }
        private KeyValuePair<FieldInfo, object> target;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
            {
                // Same story, calling 'GetPropertyHeight' before drawing gui or not allowing to dynamically change height while drawing is dumb
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += targetBoxRectHeight; // Hardcode the size as unity doesn't change it.
            }

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
            {
                EditorGUI.HelpBox(position,
                    string.Format("Warning : Usage of 'InspectorBigSpriteFieldDrawer' on field \"{0} {1}\" even though the field type isn't sprite.", property.type, property.name),
                    MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();

            // fixes position.height being incorrect on some cases
            position.height = EditorGUI.GetPropertyHeight(property, label, true) + targetBoxRectHeight;
            Sprite setValue = (Sprite)EditorGUI.ObjectField(position, new GUIContent(property.displayName, property.tooltip), property.objectReferenceValue, typeof(Sprite), false);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (property.objectReferenceValue != null)
                {
                    Undo.RecordObject(property.objectReferenceValue, "Inspector");
                }

                property.objectReferenceValue = setValue;
            }
        }
    }
    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    internal class InspectorLineDrawer : PropertyDrawer
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

            var posRect = GUIAdditionals.DrawUILine(position, targetAttribute.LineColor, targetAttribute.LineThickness, targetAttribute.LinePadding);
            EditorGUI.PropertyField(posRect, property, label, true);
        }
    }
    [CustomPropertyDrawer(typeof(InspectorReadOnlyViewAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
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
#endif
#endregion

#endregion
