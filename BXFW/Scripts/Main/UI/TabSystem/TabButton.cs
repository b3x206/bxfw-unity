using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

using System;
using System.Collections;
using System.Collections.Generic;

namespace BXFW.UI
{
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class TabButtonUnityEvent : UnityEvent<Image, TabButton> { }

        // primary type
        public FadeType FadeType { get { return ParentTabSystem.ButtonFadeType; } }

        private Image buttonBackgroundImage;
        public Image ButtonBackgroundImage
        {
            get
            {
                if (buttonBackgroundImage == null)
                    buttonBackgroundImage = GetComponent<Image>();

                return buttonBackgroundImage;
            }
        }
        // color fade
        public Color PrevColor { get; private set; }
        public Color DisableColor { get { return ParentTabSystem.FadeColorTargetDisabled; } }
        public Color HoverColor { get { return ParentTabSystem.FadeColorTargetHover; } }
        // sprite swap
        private Sprite PrevSprite;

        [Header(":: Tab Button Content")]
        [Tooltip("Content of this button. Every button has unique content.\n" +
            "Set this to update the image & icon.")]
        [SerializeField] private Content buttonContent = new Content();
        public Content ButtonContent
        {
            get { return buttonContent; }
            set
            {
                buttonContent = value ?? new Content(); // Set new Content as 'GenerateButtonContent' doesn't like null content.

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
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image buttonImage;
        public TMP_Text ButtonText { get { return buttonText; } internal set { buttonText = value; } }
        public Image ButtonImage { get { return buttonImage; } internal set { buttonImage = value; } }

        [Header(":: Internal Reference")]
        [InspectorReadOnlyView, SerializeField] internal int ButtonIndex = 0;
        [InspectorReadOnlyView, SerializeField] internal TabSystem ParentTabSystem;

        // -- Initilaze
        private void Start()
        {
            if (ParentTabSystem == null)
            {
                Debug.LogWarning($"[TabButton (name -> '{transform.GetPath()}')] The parent tab system is null. Will try to get it.");
                var parentTab = GetComponentInParent<TabSystem>();

                if (parentTab == null)
                {
                    Debug.LogWarning($"[TabButton (name -> '{transform.GetPath()}')] The parent tab system is null. Failed to get component.");
                    return;
                }

                ParentTabSystem = parentTab;
            }

            // Set Colors
            PrevColor = ButtonBackgroundImage.color;

            // Set Images
            PrevSprite = ButtonBackgroundImage.sprite;

            // If selected object.
            if (ButtonIndex == 0)
            {
                ParentTabSystem.CurrentSelectedTab = this;

                // Set visuals.
                SetButtonAppearance(ButtonState.Click);
            }
        }

        /// <summary>
        /// <br>Generates content from <see cref="buttonContent"/>.</br>
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
                if (!string.IsNullOrWhiteSpace(ButtonContent.text))
                {
                    ButtonText.SetText(ButtonContent.text);
                    ButtonText.gameObject.SetActive(true);
                }
                else if (!string.IsNullOrWhiteSpace(ButtonText.text) && ButtonContent.receiveContentFromComponents)
                {
                    ButtonContent.text = ButtonText.text;
                    ButtonText.gameObject.SetActive(true);
                }
                else
                {
                    ButtonText.gameObject.SetActive(false);
                }
            }
            else if (Application.isPlaying && !onValidateCall && !string.IsNullOrWhiteSpace(ButtonContent.text))
            {
                // Print only if tried to set content
                Debug.LogWarning($"[TabButton::GenerateButtonContent] ButtonText field in button \"{transform.GetPath()}\" is null.");
            }

            if (ButtonImage != null)
            {
                if (ButtonContent.image != null)
                {
                    ButtonImage.sprite = ButtonContent.image;
                    ButtonImage.gameObject.SetActive(true);
                }
                else if (ButtonImage.sprite != null && ButtonContent.receiveContentFromComponents)
                {
                    ButtonContent.image = ButtonImage.sprite;
                    ButtonImage.gameObject.SetActive(true);
                }
                else
                {
                    ButtonImage.gameObject.SetActive(false);
                }
            }
            else if (Application.isPlaying && !onValidateCall && ButtonContent.image != null)
            {
                Debug.LogWarning($"[TabButton::GenerateButtonContent] ButtonImage field in button \"{transform.GetPath()}\" is null.");
            }
        }
        private void OnValidate()
        {
            GenerateButtonContent(true);
        }

        #region PointerClick Events
        // -- Invoke the actual click here.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable)
                return;

            ParentTabSystem.OnTabButtonsClicked?.Invoke(transform.GetSiblingIndex());

            ParentTabSystem.CurrentSelectedTab = this;
            ParentTabSystem.UpdateButtonAppearances();
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
                            if (PrevSprite != null) { ButtonBackgroundImage.sprite = ParentTabSystem.DefaultSpriteToSwap; }
                            else { ButtonBackgroundImage.sprite = null; }
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnReset?.Invoke(ButtonBackgroundImage, this);
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
                            ButtonBackgroundImage.sprite = ParentTabSystem.HoverSpriteToSwap;
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnHover?.Invoke(ButtonBackgroundImage, this);
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
                            ButtonBackgroundImage.sprite = ParentTabSystem.TargetSpriteToSwap;
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnClick?.Invoke(ButtonBackgroundImage, this);
                            break;
                    }
                    break;
                case ButtonState.Disable:
                    switch (FadeType)
                    {
                        case FadeType.ColorFade:
                            TweenColorFade(DisableColor, ParentTabSystem.FadeSpeed);
                            break;
                        case FadeType.SpriteSwap:
                            if (PrevSprite != null) { ButtonBackgroundImage.sprite = ParentTabSystem.DisabledSpriteToSwap; }
                            else { ButtonBackgroundImage.sprite = null; }
                            break;
                        case FadeType.CustomUnityEvent:
                            ParentTabSystem.ButtonCustomEventOnDisable?.Invoke(ButtonBackgroundImage, this);
                            break;
                    }
                    break;

                default:
                    // Reset if no state was assigned.
                    Debug.LogWarning($"[TabButton::SetButtonAppearance] No behaviour defined for state : \"{state}\". Reseting instead.");
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
            Color CurrentPrevColor = ButtonBackgroundImage.color;
            bool TargetIsPrevColor = Target == PrevColor;

            if (ParentTabSystem.FadeSubtractFromCurrentColor)
                Target = TargetIsPrevColor ? Target : CurrentPrevColor - Target;
            // else, leave it unchanged

            if (!Application.isPlaying)
            {
                // Set the color instantly as the 'UnityEditor' doesn't support tween.
                ButtonBackgroundImage.color = Target;

                yield break;
            }

            if (Duration <= 0f)
            {
                ButtonBackgroundImage.color = Target;

                yield break;
            }

            // Fade
            float T = 0f;

            while (T <= 1.0f)
            {
                T += Time.deltaTime / Duration;
                ButtonBackgroundImage.color = Color.Lerp(CurrentPrevColor, Target, Mathf.SmoothStep(0, 1, T));
                yield return null;
            }

            // Set end value.
            ButtonBackgroundImage.color = Target;
        }
        #endregion
    }
}