using System;
using UnityEngine;

namespace BXFW
{
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
        /// <summary>
        /// Circle material, with customizable parameters.
        /// </summary>
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
}