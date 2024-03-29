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

                if (m_ProgressBarImg == null)
                {
                    return;
                }

                SetProgress(m_ProgressValue);
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

                if (m_ProgressBarImg == null)
                {
                    return;
                }

                SetProgress(m_ProgressValue);
            }
        }
        private RectTransform m_rectTransform;
        /// <summary>
        /// The <see cref="UnityEngine.RectTransform"/> of this Image.
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (m_rectTransform == null)
                {
                    m_rectTransform = GetComponent<RectTransform>();
                }

                return m_rectTransform;
            }
        }
        [SerializeField] private Image m_background;
        /// <summary>
        /// Background image attached to this progress bar.
        /// <br>Does not have to be a valid value.</br>
        /// </summary>
        public Image Background
        {
            get
            {
                if (m_background == null)
                {
                    TryGetComponent(out m_background);
                }

                return m_background;
            }
            set
            {
                m_background = value;
            }
        }

        // -- Public
        /// <summary>
        /// Progress bar tweened move interpolation.
        /// <br>Only works when <see cref="SetProgress(float, bool)"/> is called with UseTween = true.</br>
        /// </summary>
        public BXSTweenFloatContext setProgressTween = new BXSTweenFloatContext(0.1f);

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
                    Debug.LogWarning(string.Format("[UIProgressBar::Initilaze] There is no progress bar image assigned on object \"{0}.\"", name));
                }

                return;
            }

            if (m_ProgressBarImg.sprite == null)
            {
                // bad solution, but the designer should provide their own image.
                // because to be an image to be filled, we need an explicitly existing sprite, not the placeholder that the unity puts out.

                // TODO : Use a scale/size based solution, like the slider.
                // With that way, 9 patch rect images will become possible to use and you won't have to use a 'Rect Mask'
                m_ProgressBarImg.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
                m_ProgressBarImg.sprite.name = "GenSprite";
                m_ProgressBarImg.type = Image.Type.Filled;
                m_ProgressBarImg.fillMethod = Image.FillMethod.Horizontal; // Default fillMethod.
            }
        }

        // Progress
        private void OnValidate()
        {
            SetProgress(m_ProgressValue);
        }
        /// <summary>
        /// Sets a progress.
        /// </summary>
        /// <param name="setProgress">Progress to set between 0f~1f.</param>
        /// <param name="useTween">Whether to use tween during the progress bar's interpolation.</param>
        public void SetProgress(float setProgress, bool useTween = false)
        {
            if (m_ProgressBarImg == null)
            {
                return; // Image is null, don't put any error, as it fills the console because of the 'OnValidate' call.
            }

            setProgress = Mathf.Clamp01(setProgress);
            if (useTween)
            {
                if (!setProgressTween.IsValid)
                {
                    // Setup tween
                    // But this allocates more garbage than expected.
                    // Eh whatever, it's only one time allocation anyways
                    setProgressTween.SetSetter((float f) => m_ProgressBarImg.fillAmount = f);
                }

                // This just allocates JIT garbage
                setProgressTween.SetStartValue(m_ProgressBarImg.fillAmount).SetEndValue(setProgress).Play();
            }
            else
            {
                m_ProgressBarImg.fillAmount = setProgress;
                m_ProgressBarImg.color = m_ProgressBarImg.color;
            }

            m_ProgressValue = setProgress;
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
            UIProgressBar progressBarComponent = new GameObject("Progress Bar").AddComponent<UIProgressBar>();
            UnityEditor.GameObjectUtility.SetParentAndAlign(progressBarComponent.gameObject, (GameObject)cmd.context);
            progressBarComponent.RectTransform.sizeDelta = new Vector2(200f, 75f);
            // Create the mask for the 'PBar'
            Image progressBarBackground = progressBarComponent.gameObject.AddComponent<Image>();
            progressBarComponent.gameObject.AddComponent<Mask>();
            progressBarBackground.color = new Color(1f, 1f, 1f, .4f);

            // Create another gameObject, with stretch of this object.
            Image progressBarImage = new GameObject("Progress Bar Fill").AddComponent<Image>();
            progressBarImage.transform.SetParent(progressBarComponent.transform);
            // Scale is weird when you put something to a rect transform
            progressBarImage.transform.localScale = Vector3.one;
            // This sets the UI stretch (after setting parent ofc)
            progressBarImage.rectTransform.anchorMin = new Vector2(0, 0);
            progressBarImage.rectTransform.anchorMax = new Vector2(1, 1);
            progressBarImage.rectTransform.offsetMin = Vector2.zero;
            progressBarImage.rectTransform.offsetMax = Vector2.zero;

            // Set the 'PBarImage' as the target image
            progressBarComponent.m_ProgressBarImg = progressBarImage;
            progressBarComponent.Initilaze(); // Call initilaze for setting up the image.

            UnityEditor.Undo.RegisterCreatedObjectUndo(progressBarComponent.gameObject, "create progress bar");
        }
#endif
    }
}