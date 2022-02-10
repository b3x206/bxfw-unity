using BXFW.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace BXFW.UI
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class UIProgressBar : MonoBehaviour
    {
        // -- Private
        [SerializeField, Range(0f, 1f)] private float m_ProgressValue;
        [SerializeField] private Image m_ProgressBarImg;
        public Image ProgressBarImg
        {
            get { return m_ProgressBarImg; }
            set
            {
                m_ProgressBarImg = value;

                if (m_ProgressBarImg == null) return;
                ChangeProgress(m_ProgressValue, false);
            }
        }
        public float ProgressValue
        {
            get { return m_ProgressValue; }
            set
            {
                m_ProgressValue = Mathf.Clamp01(value);

                if (m_ProgressBarImg == null) return;
                ChangeProgress(m_ProgressValue, false);
            }
        }

        // -- Public but hidden
        public RectTransform RectTransform { get; private set; }

        // -- Public
        public CTweenPropertyFloat ProgressInterp = new CTweenPropertyFloat(.1f);

        // --------------- //
        // --- Methods --- //
        private void OnValidate()
        {
            ChangeProgress(m_ProgressValue, false);
        }
        private void Awake()
        {
            Initilaze();
        }
        public void Initilaze()
        {
            if (m_ProgressBarImg == null)
            {
                Debug.LogWarning($"[UIProgressBar::Initilaze] There is no progress bar image assigned on object \"{name}.\"");
                return;
            }

            if (m_ProgressBarImg.sprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(1, 1, Color.white);

                var sTex = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
                sTex.name = "GenSprite";
                m_ProgressBarImg.sprite = sTex;
            }

            RectTransform = GetComponent<RectTransform>();
        }
        public void ChangeProgress(float ProgressSet, bool UseTween = true)
        {
            if (m_ProgressBarImg == null)
            {
                // Try initilazing, maybe that works?
                Initilaze();
            }

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
    }
}