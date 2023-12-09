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
        public Camera ScreenshotCamera;
        public Vector2Int ScreenshotResolution = new Vector2Int(2000, 2000);
        public TextureFormat ScreenshotTexFormat = TextureFormat.RGB24;
        public KeyCode CaptureKey = KeyCode.F1;

        private static readonly Vector2Int MAX_SCREENSHOT_RESOLUTION = new Vector2Int(32768, 32768);

        private void OnValidate()
        {
            // Limit the screenshot resolution.
            ScreenshotResolution.x = Mathf.Clamp(ScreenshotResolution.x, 1, MAX_SCREENSHOT_RESOLUTION.x);
            ScreenshotResolution.y = Mathf.Clamp(ScreenshotResolution.y, 1, MAX_SCREENSHOT_RESOLUTION.y);
        }
 
        private void Update()
        {
            if (Input.GetKeyDown(CaptureKey))
            { TakeCameraShot(); }
        }

        // Other functions
        public int ScreenshotID
        {
            get { return PlayerPrefs.GetInt("LastScrID"); }
            set { PlayerPrefs.SetInt("LastScrID", value); }
        }
        public void TakeCameraShot()
        {
            if (ScreenshotCamera == null) { Debug.LogError("[CameraCapture::TakeScreenshot] Screenshot camera is null."); return; }

            // Capture the virtuCam and save it as a PNG with correct aspect.
            var prevAspect = ScreenshotCamera.aspect;
            ScreenshotCamera.aspect = ScreenshotResolution.x / (float)ScreenshotResolution.y;

            // Recall that the height is now the "actual" size from now on
            RenderTexture tempRT = new RenderTexture(ScreenshotResolution.x, ScreenshotResolution.y, 32);
            // the last parameter can be 0,16,24,32 formats like RenderTextureFormat.Default, ARGB32 etc.

            ScreenshotCamera.targetTexture = tempRT;
            ScreenshotCamera.Render();

            RenderTexture.active = tempRT;
            Texture2D vImageTexture = new Texture2D(ScreenshotResolution.x, ScreenshotResolution.y, ScreenshotTexFormat, false);
            // false, meaning no need for mipmaps
            vImageTexture.ReadPixels(new Rect(0, 0, ScreenshotResolution.x, ScreenshotResolution.y), 0, 0);

            RenderTexture.active = null;            // Can help avoid errors
            ScreenshotCamera.targetTexture = null;

            // Save to Filesystem
            var dirString = $"{Directory.GetCurrentDirectory()}/000EditorScreenshots";
            DirectoryInfo Dir = new DirectoryInfo(dirString);
            if (!Dir.Exists) { Directory.CreateDirectory(dirString); }

            string fileName;
            byte[] bytes = vImageTexture.EncodeToPNG();
            using (FileStream f = File.Create(string.Format("{0}/Screenshot{1:000}.png", dirString, ScreenshotID)))
            {
                f.Write(bytes, 0, bytes.Length);
                fileName = f.Name;
            }
            Debug.Log(string.Format("[CameraCapture::TakeScreenshot] Saved image at : {0}", fileName));
            // Increment ScreenshotID for unique file names.
            ScreenshotID++;

            // Cleanup
            if (Application.isPlaying)
            {
                Destroy(tempRT);
            }
            else
            {
                DestroyImmediate(tempRT);
            }

            ScreenshotCamera.aspect = prevAspect;
        }
#endif
    }
}