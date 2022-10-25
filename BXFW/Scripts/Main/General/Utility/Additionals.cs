// Standard
using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

// Serialization
using System.IO;
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
    public enum TransformAxis2D
    {
        None = 0,

        XAxis,
        YAxis,

        XYAxis
    }

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
                var sX = pos + (radius * v[(0 * len) + i]);
                var eX = pos + (radius * v[(0 * len) + ((i + 1) % len)]);
                var sY = pos + (radius * v[(1 * len) + i]);
                var eY = pos + (radius * v[(1 * len) + ((i + 1) % len)]);
                var sZ = pos + (radius * v[(2 * len) + i]);
                var eZ = pos + (radius * v[(2 * len) + ((i + 1) % len)]);

                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }
        #endregion

        // -- GameObject
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

        // -- Rigidbody
        /// <summary>
        /// Clamps rigidbody velocity.
        /// </summary>
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
        
        // -- Mesh + Transform (TODO : Convert these methods to take a transform matrix and a mesh.
        /// <summary>
        /// Converts vertex position to world position on the mesh.
        /// <br>Applies matrix transformations of <paramref name="filter"/>.transform, so rotations / scale / other stuff are also calculated.</br>
        /// </summary>
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
        /// <summary>
        /// Converts vertex position to world position on the mesh.
        /// <br>Applies matrix transformations of <paramref name="coll"/>.transform, so rotations / scale / other stuff are also calculated.</br>
        /// </summary>
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
        /// Converts world vertices to local mesh space.
        /// <br>Useful for chaning vertex position on the world space using <see cref="VerticesToWorldSpace(MeshFilter)"/>, and re-applying it back to the target mesh.</br>
        /// </summary>
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

        // -- Math
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
        /// <summary>
        /// Returns the biggest axis in the <paramref name="target"/>.
        /// </summary>
        public static float GetBiggestAxis(this Vector2 target)
        {
            if (target.x > target.y)
                return target.x;
            if (target.y > target.x)
                return target.y;

            return target.x;
        }
        /// <summary>
        /// Sets or removes the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
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
        /// <summary>
        /// Get the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
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
        /// <summary>
        /// Sets or removes the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
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
        /// <summary>
        /// Get the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
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
        /// Converts <see cref="Vector2"/> to positive values.
        /// </summary>
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        /// <summary>
        /// Converts <see cref="Vector3"/> to positive values.
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        /// <summary>
        /// Converts <see cref="Vector4"/> to positive values.
        /// </summary>
        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
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
        /// <summary>
        /// Fixes euler rotation to Unity Editor instead of the code ranges.
        /// </summary>
        public static Vector3 FixEulerRotation(Vector3 eulerRot)
        {
            Vector3 TransformEulerFixed = new Vector3(
                eulerRot.x > 180f ? eulerRot.x - 360f : eulerRot.x,
                eulerRot.y > 180f ? eulerRot.y - 360f : eulerRot.y,
                eulerRot.z > 180f ? eulerRot.z - 360f : eulerRot.z
                );

            return TransformEulerFixed;
        }

        // -- OS Specific (Mostly android specific)
        /// <summary>
        /// Returns the keyboard height ratio.
        /// </summary>
        public static float GetKeyboardHeightRatio(bool includeInput)
        {
            return Mathf.Clamp01((float)GetKeyboardHeight(includeInput) / Display.main.systemHeight);
        }
        /// <summary>
        /// Returns the keyboard height in display pixels.
        /// </summary>
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

            return ((to - from) * ((value - from2) / (to2 - from2))) + from;
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
        /// <summary>
        /// Get an iterator of the base types of <paramref name="type"/>.
        /// <br>Returns a blank iterator if no base type.</br>
        /// </summary>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(type.BaseType.GetBaseTypes());
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
        /// <summary
        /// >Resize array.
        /// </summary>
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
                throw new InvalidOperationException(string.Format("[Additionals::SetEnum] Error while setting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));

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
                throw new InvalidOperationException(string.Format("[Additionals::GetEnum] Error while getting enum : Type '{0}' is not a valid enum type.", typeof(T).Name));

            if (string.IsNullOrEmpty(SaveKey))
            {
                Debug.LogError(string.Format("[Additionals::SetEnum] Couldn't get the savekey because it is null. Key={0}", SaveKey));
                return default;
            }

            return (T)(object)PlayerPrefs.GetInt(string.Format("{0}_ENUM:{1}", SaveKey, typeof(T).Name));
        }
        /// <summary>
        /// Use this method to control whether your save key was serialized as type <typeparamref name="T"/>.
        /// </summary>
        public static bool HasPlayerPrefsKey<T>(string SaveKey)
        {
            if (string.IsNullOrEmpty(SaveKey))
                return false;

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
}
