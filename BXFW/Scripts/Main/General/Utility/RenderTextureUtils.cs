﻿using System;
using TMPro;
using UnityEngine;

namespace BXFW
{
    /// Changes Done :
    ///   * Fixed formatting and added discards for unused RenderTexture's.
    ///   * Renamed class from RTUtils -> RenderTextureUtils
    ///   * removed some pointless methods (such as DrawTextureGUI without rect input)
    ///   * (some) Methods can now take camera matrices
    /// 
    /// Notes :
    ///   * I have no idea what the 'Draw' methods do (the blit methods atleast do something)
    ///   
    /// RTUtils by Nothke
    /// 
    /// RenderTexture utilities for direct drawing meshes, texts and sprites and converting to Texture / Texture2D.
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
}