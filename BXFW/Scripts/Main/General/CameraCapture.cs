using System;
using System.IO;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// An editor exclusive script, to capture and save <see cref="RenderTexture"/>'s from an camera.
    /// </summary>
    public class CameraCapture : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Capture Settings")]
        public Camera screenshotCamera;
        [ClampVector(1, 1, MAX_SCREENSHOT_RESOLUTION_X, MAX_SCREENSHOT_RESOLUTION_Y)]
        public Vector2Int screenshotResolution = new Vector2Int(2000, 2000);
        public TextureFormat screenshotTextureFormat = TextureFormat.RGB24;
        public KeyCode captureKey = KeyCode.F1;

        private const float MAX_SCREENSHOT_RESOLUTION_X = 32768;
        private const float MAX_SCREENSHOT_RESOLUTION_Y = 32768;

        private void Update()
        {
            if (Input.GetKeyDown(captureKey))
            {
                TakeCameraShot();
            }
        }

        public void TakeCameraShot()
        {
            if (screenshotCamera == null)
            {
                if (!TryGetComponent(out screenshotCamera))
                {
                    Debug.LogError("[CameraCapture::TakeScreenshot] Screenshot camera is null.");
                    return;
                }
            }

            // Capture the virtuCam and save it as a PNG with correct aspect.
            var prevAspect = screenshotCamera.aspect;
            screenshotCamera.aspect = screenshotResolution.x / (float)screenshotResolution.y;

            // Recall that the height is now the "actual" size from now on
            RenderTexture tempRT = new RenderTexture(screenshotResolution.x, screenshotResolution.y, 32);
            // the last parameter can be 0,16,24,32 formats like RenderTextureFormat.Default, ARGB32 etc.

            screenshotCamera.targetTexture = tempRT;
            screenshotCamera.Render();

            RenderTexture.active = tempRT;
            Texture2D vImageTexture = new Texture2D(screenshotResolution.x, screenshotResolution.y, screenshotTextureFormat, false);
            // false, meaning no need for mipmaps
            vImageTexture.ReadPixels(new Rect(0, 0, screenshotResolution.x, screenshotResolution.y), 0, 0);

            RenderTexture.active = null;            // Can help avoid errors
            screenshotCamera.targetTexture = null;

            // Save to Filesystem
            var dirString = Path.Combine(Directory.GetCurrentDirectory(), "000EditorScreenshots");
            DirectoryInfo directoryInfo = new DirectoryInfo(dirString);
            if (!directoryInfo.Exists) 
            { 
                Directory.CreateDirectory(dirString);
            }

            string fileName;
            byte[] bytes = vImageTexture.EncodeToPNG();
            using (FileStream f = File.Create(Path.Combine(dirString, $"Screenshot{DateTime.Now:yyyy.MM.ddTHH.mm.ss}.png")))
            {
                f.Write(bytes, 0, bytes.Length);
                fileName = f.Name;
            }
            Debug.Log(string.Format("[CameraCapture::TakeScreenshot] Saved image at : {0}", fileName));

            // Cleanup
            if (Application.isPlaying)
                Destroy(tempRT);
            else
                DestroyImmediate(tempRT);

            screenshotCamera.aspect = prevAspect;
        }
#endif
    }
}
