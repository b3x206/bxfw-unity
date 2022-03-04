using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BXFW.ScriptEditor
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
        public TextureFormat ScreenshotTexFormat = TextureFormat.RGBA32;
        public KeyCode CaptureKey = KeyCode.F1;
        [Header("Management")]
        public ScriptToggler CamMoveTest;

        private void OnValidate()
        {
            // Limit the screenshot resolution.
            ScreenshotResolution.x = Mathf.Clamp(ScreenshotResolution.x, 128, 16384);
            ScreenshotResolution.y = Mathf.Clamp(ScreenshotResolution.y, 128, 16384);
        }
 
        private void Awake()
        {
            DebugLogConsole.AddToStringDisplay($"\n{CaptureKey} : Capture Images\n{CamMoveTest.ToggleKey} : Free Look");
        }
        private void Update()
        {
            if (Input.GetKeyDown(CaptureKey))
            { TakeScreenshot(); }
        }

        // Other functions
        public int ScreenshotID
        {
            get { return PlayerPrefs.GetInt("LastScrID"); }
            set { PlayerPrefs.SetInt("LastScrID", value); }
        }
        public void TakeScreenshot()
        {
            if (ScreenshotCamera == null) { Debug.LogError("[CameraCapture::TakeScreenshot] Screenshot camera is null."); return; }

            // capture the virtuCam and save it as a square PNG.
            var prevAspect = ScreenshotCamera.aspect;
            ScreenshotCamera.aspect = ScreenshotResolution.x / (float)ScreenshotResolution.y;

            // recall that the height is now the "actual" size from now on
            RenderTexture tempRT = new RenderTexture(ScreenshotResolution.x, ScreenshotResolution.y, 32);
            // the last parameter can be 0,16,24,32 formats like RenderTextureFormat.Default, ARGB32 etc.

            ScreenshotCamera.targetTexture = tempRT;
            ScreenshotCamera.Render();

            RenderTexture.active = tempRT;
            Texture2D virtualPhoto =
                new Texture2D(ScreenshotResolution.x, ScreenshotResolution.y, ScreenshotTexFormat, false);
            // false, meaning no need for mipmaps
            virtualPhoto.ReadPixels(new Rect(0, 0, ScreenshotResolution.x, ScreenshotResolution.y), 0, 0);

            RenderTexture.active = null;            //can help avoid errors
            ScreenshotCamera.targetTexture = null;

            // Wait for the screenshot to be taken for destruction
            var dirString = $"{Directory.GetCurrentDirectory()}/000EditorScreenshots";
            DirectoryInfo Dir = new DirectoryInfo(dirString);
            if (!Dir.Exists) { Directory.CreateDirectory(dirString); }

            string fileName;
            byte[] bytes = virtualPhoto.EncodeToPNG();
            using (FileStream f = File.Create($"{dirString}/Screenshot{ScreenshotID:000}.png"))
            {
                f.Write(bytes, 0, bytes.Length);
                fileName = f.Name;
            }

            Debug.Log(string.Format("[CameraCapture::TakeScreenshot] Saved image at : {0}", fileName));
            ScreenshotID++;
            Destroy(tempRT);
            ScreenshotCamera.aspect = prevAspect;
        }
#endif
    }
}