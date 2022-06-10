using BXFW.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace BXFW.UI
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class UIProgressBar : MonoBehaviour
    {
        // -- Variables -- //
        // -- Private
        [SerializeField, Range(0f, 1f)] private float m_ProgressValue;
        [SerializeField] private Image m_ProgressBarImg;

        // -- Public but hidden
        /// <summary>
        /// Current progress bar Image.
        /// </summary>
        public Image ProgressBarImg
        {
            get { return m_ProgressBarImg; }
            set
            {
                m_ProgressBarImg = value;

                if (m_ProgressBarImg == null) return;
                ChangeProgress(m_ProgressValue);
            }
        }
        /// <summary>
        /// Current progress value.
        /// <br>Clamped / Accepts values between 0-1.</br>
        /// </summary>
        public float ProgressValue
        {
            get { return m_ProgressValue; }
            set
            {
                m_ProgressValue = Mathf.Clamp01(value);

                if (m_ProgressBarImg == null) return;
                ChangeProgress(m_ProgressValue);
            }
        }
        private RectTransform _rectTransform;
        /// <summary>
        /// The <see cref="UnityEngine.RectTransform"/> of this Image.
        /// </summary>
        public RectTransform RectTransform
        { 
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            } 
        }

        // -- Public
        /// <summary>
        /// Progress bar tweened move interpolation.
        /// <br>Only works when <see cref="ChangeProgress(float, bool)"/> is called with UseTween = true.</br>
        /// </summary>
        public BXTweenPropertyFloat ProgressInterp = new BXTweenPropertyFloat(.1f);

        // -- Methods -- //
        // Initilaze
        private void Awake()
        {
            Initilaze();
        }
        public void Initilaze()
        {
            if (m_ProgressBarImg == null)
            {
                if (Application.isPlaying)
                {
                    // Only print out the error while we are playing.
                    // This error occurs due to Initilaze being called without setting up in the runtime.
                    Debug.LogWarning($"[UIProgressBar::Initilaze] There is no progress bar image assigned on object \"{name}.\"");
                }

                return;
            }

            if (m_ProgressBarImg.sprite == null)
            {
                // bad solution, but the designer should provide their own image.
                m_ProgressBarImg.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
                m_ProgressBarImg.sprite.name = "GenSprite";
                m_ProgressBarImg.type = Image.Type.Filled;
                m_ProgressBarImg.fillMethod = Image.FillMethod.Horizontal; // Default fillMethod.
            }
        }
        
        // Progress
        private void OnValidate()
        {
            ChangeProgress(m_ProgressValue);
        }
        public void ChangeProgress(float ProgressSet, bool UseTween = false)
        {
            if (m_ProgressBarImg == null)
                return; // Image is null, don't put any error, as it fills the console because of the 'OnValidate' call.

            ProgressSet = Mathf.Clamp01(ProgressSet);
            if (UseTween)
            {
                if (!ProgressInterp.IsSetup)
                {
                    // Setup tween
                    ProgressInterp.SetupProperty((float f) => { m_ProgressBarImg.fillAmount = f; });
                }

                ProgressInterp.StartTween(m_ProgressBarImg.fillAmount, ProgressSet);
            }
            else
            {
                m_ProgressBarImg.fillAmount = ProgressSet;
                m_ProgressBarImg.color = m_ProgressBarImg.color;
            }

            m_ProgressValue = ProgressSet;
        }

        // Editor
#if UNITY_EDITOR
        /// <summary>
        /// Editor creation shortcut.
        /// </summary>
        [UnityEditor.MenuItem("GameObject/UI/Progress Bar")]
        private static void CreateUIProgressBar(UnityEditor.MenuCommand cmd)
        {
            // Create & Align
            var PBar = new GameObject("Progress Bar").AddComponent<UIProgressBar>();
            UnityEditor.GameObjectUtility.SetParentAndAlign(PBar.gameObject, (GameObject)cmd.context);
            PBar.RectTransform.sizeDelta = new Vector2(200f, 75f);
            // Create the mask for the 'PBar'
            var PBarBGImage = PBar.gameObject.AddComponent<Image>();
            PBar.gameObject.AddComponent<Mask>();
            PBarBGImage.color = new Color(1f, 1f, 1f, .4f);

            // Create another gameObject, with stretch of this object.
            var PBarImage = new GameObject("Progress Bar Fill").AddComponent<Image>();
            PBarImage.transform.SetParent(PBar.transform);
            // Scale is weird when you put something to a rect transform
            PBarImage.transform.localScale = Vector3.one;
            // This sets the UI stretch (after setting parent ofc)
            PBarImage.rectTransform.anchorMin = new Vector2(0, 0);
            PBarImage.rectTransform.anchorMax = new Vector2(1, 1);
            PBarImage.rectTransform.offsetMin = Vector2.zero;
            PBarImage.rectTransform.offsetMax = Vector2.zero;

            // Set the 'PBarImage' as the target image
            PBar.m_ProgressBarImg = PBarImage;
            PBar.Initilaze(); // Call initilaze for setting up the image.
        }
#endif
    }
}