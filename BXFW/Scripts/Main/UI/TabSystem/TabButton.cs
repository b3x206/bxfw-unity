using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

using System.Collections;

namespace BXFW.UI
{
    /// TODO : Make this class extend from <see cref="Selectable"/> for proper unity ui support.
    /// However this is optional due to 'Selectable' introducting it's own bloat.
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [System.Serializable]
        public class TabButtonUnityEvent : UnityEvent<Image, TabButton> { }

        protected Image ButtonImage;
        // color fade
        public Color PrevColor { get; private set; }
        // sprite swap
        private Sprite PrevSprite;

        public FadeType FadeType { get; set; } = FadeType.ColorFade;

        [InspectorReadOnlyView] internal int ButtonIndex = 0;
        [InspectorReadOnlyView] internal TabSystem ParentTabSystem;
        [HideInInspector] public TextMeshProUGUI ButtonText;

        // -- Initilaze
        private void Start()
        {
            if (ParentTabSystem == null)
            {
                Debug.LogError($"[TabButton (name -> '{transform.GetPath()}')] The parent tab system is null.");
                return;
            }

            // Set Colors
            ButtonImage = GetComponent<Image>();
            PrevColor = ButtonImage.color;

            // Set Images
            PrevSprite = ButtonImage.sprite;

            // If first object.
            if (ButtonIndex == 0)
            {
                ParentTabSystem.CurrentSelectedTab = this;

                // Set visuals.
                SelectButtonAppearance();
            }

            // Setup tempoary variables
            FadeType = ParentTabSystem.ButtonFadeType;
        }

        #region PointerClick Events
        // -- Invoke the actual click here.
        public void OnPointerClick(PointerEventData eventData)
        {
            ParentTabSystem.OnTabButtonsClicked?.Invoke(transform.GetSiblingIndex());

            ParentTabSystem.CurrentSelectedTab = this;
            ParentTabSystem.CheckUnClickedButtons();
        }

        // -- Visual Updates
        public void OnPointerDown(PointerEventData eventData)
        {
            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        StartCoroutine(DoColorFade(ParentTabSystem.TabButtonFadeColorTargetClick, ParentTabSystem.TabButtonFadeSpeed));
                        break;
                    case FadeType.SpriteSwap:
                        ButtonImage.sprite = ParentTabSystem.TargetSpriteToSwap;
                        break;
                    case FadeType.CustomUnityEvent:
                        ParentTabSystem.TabButtonCustomEventClick?.Invoke(ButtonImage, this);
                        break;
                }
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        StartCoroutine(DoColorFade(ParentTabSystem.TabButtonFadeColorTargetHover, ParentTabSystem.TabButtonFadeSpeed));
                        break;
                    case FadeType.SpriteSwap:
                        ButtonImage.sprite = ParentTabSystem.HoverSpriteToSwap;
                        break;
                    case FadeType.CustomUnityEvent:
                        ParentTabSystem.TabButtonCustomEventHover?.Invoke(ButtonImage, this);
                        break;
                }
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (ParentTabSystem.CurrentSelectedTab != this)
            {
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        StartCoroutine(DoColorFade(PrevColor, ParentTabSystem.TabButtonFadeSpeed));
                        break;
                    case FadeType.SpriteSwap:
                        if (PrevSprite != null) { ButtonImage.sprite = PrevSprite; }
                        else { ButtonImage.sprite = null; }
                        break;
                    case FadeType.CustomUnityEvent:
                        ParentTabSystem.TabButtonCustomEventOnReset?.Invoke(ButtonImage, this);
                        break;
                }
            }
        }
        // -- Visual Update Methods
        public void ResetButtonAppearance()
        {
            switch (FadeType)
            {
                case FadeType.ColorFade:
                    StartCoroutine(DoColorFade(PrevColor, ParentTabSystem.TabButtonFadeSpeed));
                    break;
                case FadeType.SpriteSwap:
                    if (PrevSprite != null) { ButtonImage.sprite = PrevSprite; }
                    else { ButtonImage.sprite = null; }
                    break;
                case FadeType.CustomUnityEvent:
                    ParentTabSystem.TabButtonCustomEventOnReset?.Invoke(ButtonImage, this);
                    break;
            }
        }
        public void SelectButtonAppearance()
        {
            // Set visuals.
            switch (FadeType)
            {
                case FadeType.ColorFade:
                    StartCoroutine(DoColorFade(ParentTabSystem.TabButtonFadeColorTargetClick, ParentTabSystem.TabButtonFadeSpeed));
                    break;
                case FadeType.SpriteSwap:
                    ButtonImage.sprite = ParentTabSystem.TargetSpriteToSwap;
                    break;
                case FadeType.CustomUnityEvent:
                    ParentTabSystem.TabButtonCustomEventClick?.Invoke(ButtonImage, this);
                    break;
            }
        }
        #endregion

        #region Color Fading
        private IEnumerator DoColorFade(Color Target, float Duration)
        {
            // Color manipulation
            Color CurrentPrevColor = ButtonImage.color;
            bool TargetIsPrevColor = Target == PrevColor;

            Target = ParentTabSystem.TabButtonSubtractFromCurrentColor ? (TargetIsPrevColor ? Target : CurrentPrevColor - Target) : Target;

            if (Duration <= 0f)
            {
                ButtonImage.color = Target;

                yield break;
            }

            // Fade
            float T = 0f;

            while (T <= 1.0f)
            {
                T += Time.deltaTime / Duration;
                ButtonImage.color = Color.Lerp(CurrentPrevColor, Target, Mathf.SmoothStep(0, 1, T));
                yield return null;
            }

            // Set end value.
            ButtonImage.color = Target;
        }
        #endregion
    }
}