using System;
using System.IO;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A script used to capture and save <see cref="RenderTexture"/>'s to local disk from a camera.
    /// <br>Additionaly, it can also be used to call <see cref="ScreenCapture.CaptureScreenshot(string)"/>.</br>
    /// <br>Captured files are stored inside a folder named <see cref="ScreenshotsFolderName"/>.</br>
    /// </summary>
    public class CameraCapture : MonoBehaviour
    {
        [Header("Capture Settings")]
        public Camera screenshotCamera;
        [ClampVector(1, 1, MaxScreenshotResolutionX, MaxScreenshotResoultionY)]
        public Vector2Int screenshotResolution = new Vector2Int(2000, 2000);
        public TextureFormat screenshotTextureFormat = TextureFormat.RGB24;
        [SearchableKeyCodeField] public KeyCode captureKey = KeyCode.F1;

        [InspectorLine(LineColor.Gray)]
        public bool allowFullScreenshot = false;
        [DrawIf(nameof(allowFullScreenshot)), Tooltip("Uses the ScreenCapture.CaptureScreenshot, which includes all rendered layers instead of taking a camera based screenshot.")]
        [SearchableKeyCodeField] public KeyCode captureScreenKey = KeyCode.F2;
        [DrawIf(nameof(allowFullScreenshot)), SerializeField, Clamp(1, 8), Tooltip("Scales the ScreenCapture.CameraScreenshot.")]
        private int m_CaptureSuperSamplingFactor = 1;
        public int CaptureSuperSamplingFactor
        {
            get => Mathf.Clamp(m_CaptureSuperSamplingFactor, 1, 16);
            set => m_CaptureSuperSamplingFactor = Mathf.Clamp(value, 1, 16);
        }

        public const string ScreenshotsFolderName = "000Screenshots";
        private const float MaxScreenshotResolutionX = 32768;
        private const float MaxScreenshotResoultionY = 32768;

        private void Update()
        {
            if (Input.GetKeyDown(captureKey))
            {
                TakeCameraShot();
            }
            if (allowFullScreenshot && Input.GetKeyDown(captureScreenKey))
            {
                TakeScreenShot();
            }
        }

        /// <summary>
        /// Takes a direct camera shot of what the camera has rendered.
        /// <br>* This does not include the UGUI layers and such, but only what the camera has rendered.</br>
        /// <br>* This may be problematic with the disabled object rendering, as <see cref="Camera.Render"/> is odd in the regards of rendering.</br>
        /// </summary>
        public void TakeCameraShot()
        {
            if (screenshotCamera == null)
            {
                if (!TryGetComponent(out screenshotCamera))
                {
                    Debug.LogError("[CameraCapture::TakeScreenshot] Screenshot camera is null.", this);
                    return;
                }
            }

            // Capture the virtuCam and save it as a PNG with correct aspect.
            float prevAspect = screenshotCamera.aspect;
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
            string dirString = Path.Combine(Directory.GetCurrentDirectory(), ScreenshotsFolderName);
            DirectoryInfo directoryInfo = new DirectoryInfo(dirString);
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(dirString);
            }

            string fileName;
            byte[] bytes = vImageTexture.EncodeToPNG();
            using (FileStream f = File.Create(Path.Combine(dirString, $"CameraShot{DateTime.Now:yyyy.MM.ddTHH.mm.ss}.png")))
            {
                f.Write(bytes, 0, bytes.Length);
                fileName = f.Name;
            }
            Debug.Log(string.Format("<b>[CameraCapture::TakeCameraShot]</b> Saved image at : <a href=\"file:///{0}\">{0}</a>\n<a href=\"file:///{1}\">Open Target Image Directory</a>", fileName, dirString), this);

            // Cleanup
            if (Application.isPlaying)
            {
                Destroy(tempRT);
            }
            else
            {
                DestroyImmediate(tempRT);
            }

            screenshotCamera.aspect = prevAspect;
        }

        public void TakeScreenShot()
        {
            if (!allowFullScreenshot)
            {
                Debug.LogWarning("[CameraCapture::TakeScreenShot] Called this method while not allowFullScreenshot. Set this setting to true to be able to take full screenshots.", this);
                return;
            }

            // Path on PC : Directory.GetCurrentDirectory(), Path on Mobiles : Application.persistentDataPath
            string relativePath = Path.Combine(ScreenshotsFolderName, $"ScreenShot{DateTime.Now:yyyy.MM.ddTHH.mm.ss}.png");
            string absoluteFolderPath = Application.platform switch
            {
                RuntimePlatform.Android => Application.persistentDataPath,
                RuntimePlatform.IPhonePlayer => Application.persistentDataPath,
                RuntimePlatform.WebGLPlayer => Application.persistentDataPath,
                _ => Directory.GetCurrentDirectory()
            };
            ScreenCapture.CaptureScreenshot(relativePath, CaptureSuperSamplingFactor);
            Debug.Log(string.Format("<b>[CameraCapture::TakeScreenShot]</b> Saved image at : <a href=\"file:///{0}\">{0}</a>\n<a href=\"file:///{1}\">Open Target Image Directory</a>", Path.Combine(absoluteFolderPath, relativePath), Path.Combine(absoluteFolderPath, ScreenshotsFolderName)), this);
        }
    }
}
