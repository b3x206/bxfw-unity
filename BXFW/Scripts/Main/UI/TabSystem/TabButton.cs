using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

using System;
using System.Collections;

namespace BXFW.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class TabButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// A button related event, used as the fade type.
        /// </summary>
        [Serializable]
        public class ButtonEvent : UnityEvent<Image, TabButton> { }

        // primary type
        /// <summary>
        /// The fade type contained by the <see cref="ParentTabSystem"/>.
        /// </summary>
        public FadeType FadeType { get { return ParentTabSystem.ButtonFadeType; } }

        private Image m_BackgroundImage;
        public Image BackgroundImage
        {
            get
            {
                if (m_BackgroundImage == null)
                    m_BackgroundImage = GetComponent<Image>();

                return m_BackgroundImage;
            }
        }
        // color fade
        public Color PrevColor { get; private set; }
        // sprite swap
        public Sprite PrevSprite { get; private set; }

        [Header(":: Tab Button Content")]
        [Tooltip("Content of this button. Every button has unique content.\nSet this to update the image & icon.")]
        [SerializeField] private Content m_Content = new Content();
        /// <summary>
        /// Content of this button. Buttons can have different contents.
        /// </summary>
        public Content Content
        {
            get { return m_Content; }
            set
            {
                m_Content = value ?? new Content(); // Set new Content as 'GenerateButtonContent' doesn't like null content.

                GenerateButtonContent();
            }
        }

        [SerializeField] private bool mInteractable = true;
        /// <summary>
        /// Whether if this button is interactable.
        /// <br>Note : The parent tab system's interactability overrides this buttons.</br>
        /// </summary>
        public bool Interactable
        {
            get { return ParentTabSystem.Interactable && mInteractable; }
            set { mInteractable = value; }
        }

        [Header(":: Tab Button Reference")]
        [SerializeField] private TMP_Text m_ButtonText;
        [SerializeField] private Image m_ButtonImage;
        public TMP_Text ButtonText { get { return m_ButtonText; } internal set { m_ButtonText = value; } }
        public Image ButtonImage { get { return m_ButtonImage; } internal set { m_ButtonImage = value; } }

        [Header(":: Internal Reference")]
        [ReadOnlyView, SerializeField] internal int ButtonIndex = 0;
        /// <summary>
        /// The parent tab system that this tab button is a member of.
        /// </summary>
        public TabSystem ParentTabSystem => m_ParentTabSystem;
        [ReadOnlyView, SerializeField] internal TabSystem m_ParentTabSystem;

        // -- Initilaze
        private void Start()
        {
            if (ParentTabSystem == null)
            {
                Debug.LogWarning(string.Format("[TabButton (name -> '{0}')] The parent tab system is null. Will try to get it.", this.GetPath()));
                var parentTab = GetComponentInParent<TabSystem>();

                if (parentTab == null)
                {
                    Debug.LogWarning(string.Format("[TabButton (name -> '{0}')] The parent tab system is null. Failed to get component.", this.GetPath()));
                    return;
                }

                m_ParentTabSystem = parentTab;
            }

            // Set Colors
            PrevColor = BackgroundImage.color;

            // Set Images
            PrevSprite = BackgroundImage.sprite;

            // If selected object.
            if (ButtonIndex == 0)
            {
                ParentTabSystem.CurrentSelectedTab = this;

                // Set visuals.
                SetButtonAppearance(ButtonState.Click);
            }
        }

        /// <summary>
        /// <br>Generates content from <see cref="m_Content"/>.</br>
        /// </summary>
        /// <param name="onValidateCall">
        /// This parameter specifies whether if this method was called from an 'OnValidate' method.
        /// <br>Do not touch this unless you are calling this from 'OnValidate' (Changes <see cref="Debug.Log"/> behaviour)</br>
        /// </param>
        internal void GenerateButtonContent(bool onValidateCall = false)
        {
            if (ButtonText != null)
            {
                // Receive content if the 'image or sprite' does exist (& our content is null)
                if (!string.IsNullOrWhiteSpace(Content.text))
                {
                    ButtonText.SetText(Content.text);
                    ButtonText.gameObject.SetActive(true);
                }
                else
                {
                    ButtonText.gameObject.SetActive(false);
                }
            }
            else if (Application.isPlaying && !onValidateCall && !string.IsNullOrWhiteSpace(Content.text))
            {
                // Print only if tried to set content
                Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonText field in button \"{0}\" is null.", this.GetPath()));
            }

            if (ButtonImage != null)
            {
                if (Content.sprite != null)
                {
                    ButtonImage.sprite = Content.sprite;
                    ButtonImage.gameObject.SetActive(true);
                }
                else
                {
                    ButtonImage.gameObject.SetActive(false);
                }
            }
            else if (Application.isPlaying && !onValidateCall && Content.sprite != null)
            {
                Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonImage field in button \"{0}\" is null.", this.GetPath()));
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            GenerateButtonContent(true);
        }
#endif

        #region PointerClick Events
        // -- Invoke the actual click here.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            ParentTabSystem.OnTabButtonsClicked?.Invoke(transform.GetSiblingIndex());

            ParentTabSystem.CurrentSelectedTab = this;
            ParentTabSystem.UpdateElementsAppearance();
        }

        // -- Visual Updates
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                SetButtonAppearance(ButtonState.Click);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                SetButtonAppearance(ButtonState.Hover);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                SetButtonAppearance(ButtonState.Reset);
            }
            else // ParentTabSystem.CurrentSelectedTab == this
            {
                SetButtonAppearance(ButtonState.Click);
            }
        }

        internal enum ButtonState { Reset, Hover, Click, Disable }

        internal void SetButtonAppearance(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Reset:
                    switch (FadeType)
                    {
                        case FadeType.ColorFade:
                            TweenColorFade(ParentTabSystem.FadeColorTargetDefault, ParentTabSystem.FadeSpeed);
                            break;
                        case FadeType.SpriteSwap:
                            if (PrevSprite != null) { BackgroundImage.sprite = ParentTabSystem.DefaultSpriteToSwap; }
                            else { BackgroundImage.sprite = null; }
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnReset?.Invoke(BackgroundImage, this);
                            break;
                    }
                    break;
                case ButtonState.Hover:
                    switch (FadeType)
                    {
                        case FadeType.ColorFade:
                            TweenColorFade(ParentTabSystem.FadeColorTargetHover, ParentTabSystem.FadeSpeed);
                            break;
                        case FadeType.SpriteSwap:
                            BackgroundImage.sprite = ParentTabSystem.HoverSpriteToSwap;
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnHover?.Invoke(BackgroundImage, this);
                            break;
                    }
                    break;
                case ButtonState.Click:
                    switch (FadeType)
                    {
                        case FadeType.ColorFade:
                            TweenColorFade(ParentTabSystem.FadeColorTargetClick, ParentTabSystem.FadeSpeed);
                            break;
                        case FadeType.SpriteSwap:
                            BackgroundImage.sprite = ParentTabSystem.TargetSpriteToSwap;
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnClick?.Invoke(BackgroundImage, this);
                            break;
                    }
                    break;
                case ButtonState.Disable:
                    switch (FadeType)
                    {
                        case FadeType.ColorFade:
                            TweenColorFade(ParentTabSystem.FadeColorTargetDisabled, ParentTabSystem.FadeSpeed);
                            break;
                        case FadeType.SpriteSwap:
                            if (PrevSprite != null) { BackgroundImage.sprite = ParentTabSystem.DisabledSpriteToSwap; }
                            else { BackgroundImage.sprite = null; }
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnDisable?.Invoke(BackgroundImage, this);
                            break;
                    }
                    break;

                default:
                    // Reset if no state was assigned.
                    Debug.LogWarning(string.Format("[TabButton::SetButtonAppearance] No behaviour defined for state : \"{0}\". Reseting instead.", state));
                    goto case ButtonState.Reset;
            }
        }
        #endregion

        #region Color Fading
        private void TweenColorFade(Color Target, float Duration)
        {
            if (!gameObject.activeInHierarchy) return; // Do not start coroutines if the object isn't active.

            StartCoroutine(CoroutineTweenColorFade(Target, Duration));
        }
        // We can use BXTween, but this tab button thing is older (and i really don't care)
        private IEnumerator CoroutineTweenColorFade(Color Target, float Duration)
        {
            // Color manipulation
            Color CurrentPrevColor = BackgroundImage.color;
            bool TargetIsPrevColor = Target == PrevColor;

            if (ParentTabSystem.FadeSubtractFromCurrentColor)
                Target = TargetIsPrevColor ? Target : CurrentPrevColor - Target;
            // else, leave it unchanged

            if (!Application.isPlaying)
            {
                // Set the color instantly as the 'UnityEditor' doesn't support tween.
                BackgroundImage.color = Target;

                yield break;
            }

            if (Duration <= 0f)
            {
                BackgroundImage.color = Target;

                yield break;
            }

            // Fade
            float T = 0f;

            while (T <= 1.0f)
            {
                T += Time.deltaTime / Duration;
                BackgroundImage.color = Color.Lerp(CurrentPrevColor, Target, Mathf.SmoothStep(0f, 1f, T));
                yield return null;
            }

            // Set end value.
            BackgroundImage.color = Target;
        }
        #endregion
    }
}