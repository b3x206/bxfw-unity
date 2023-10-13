using UnityEngine;
using TMPro;

using System;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace BXFW.UI
{
    /// <summary>
    /// Defines an animation type for UI elements that are being animated.
    /// <br>Unlike <see cref="Selectable.Transition"/>, this one does not have animation.
    /// Instead it uses <see cref="CustomUnityEvent"/>.</br>
    /// </summary>
    public enum FadeType
    {
        None,
        ColorFade,
        SpriteSwap,
        CustomUnityEvent
    }

    /// <summary>
    /// The tab system.
    /// <br>Use this to make, well, tab systems in game.</br>
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class TabSystem : InteractableMultiUIManager<TabButton>
    {
        // -- Fade Styles
        // - This part is button settings.
        public FadeType ButtonFadeType = FadeType.ColorFade;
        // ButtonFadeType = ColorFade
        [Range(0f, 4f)] public float FadeSpeed = .15f;
        public Color FadeColorTargetDefault = new Color(1f, 1f, 1f);
        public Color FadeColorTargetHover = new Color(.95f, .95f, .95f);
        public Color FadeColorTargetClick = new Color(.9f, .9f, .9f);
        public Color FadeColorTargetDisabled = new Color(.5f, .5f, .5f, .5f);
        public bool FadeSubtractFromCurrentColor = false;
        // ButtonFadeType = SpriteSwap
        public Sprite DefaultSpriteToSwap;
        public Sprite HoverSpriteToSwap;
        public Sprite TargetSpriteToSwap;
        public Sprite DisabledSpriteToSwap;
        // ButtonFadeType = CustomUnityEvent
        public TabButton.ButtonEvent ButtonCustomEventOnReset;
        public TabButton.ButtonEvent ButtonCustomEventOnHover;
        public TabButton.ButtonEvent ButtonCustomEventOnClick;
        public TabButton.ButtonEvent ButtonCustomEventOnDisable;

        /// <summary>
        /// Called when any of the buttons in the TabSystem was pressed.
        /// </summary>
        public IndexEvent OnTabButtonsClicked;

        /// <summary>
        /// Returns the current selected tab.
        /// </summary>
        public TabButton CurrentSelectedTab { get; internal set; }

        /// <summary>
        /// Creates Button for TabSystem.
        /// Info : This command already adds to the list <see cref="tabButtons"/>.
        /// </summary>
        /// <param name="UseRefTab">Whether to use the referenced tab from index <see cref="CurrentReferenceTabButton"/>.</param>
        /// <returns>Creation button result.</returns>
        protected override TabButton OnCreateElement(TabButton referenceBtn)
        {
            TabButton button;

            if (referenceBtn == null)
            {
                GameObject tabButtonObject = new GameObject("Tab");
                tabButtonObject.transform.SetParent(transform);
                tabButtonObject.transform.localScale = Vector3.one;

                button = tabButtonObject.AddComponent<TabButton>();

                // -- Text
                GameObject tabTextObject = new GameObject("Tab Text");
                tabTextObject.transform.SetParent(tabButtonObject.transform);
                TextMeshProUGUI tabText = tabTextObject.AddComponent<TextMeshProUGUI>();
                button.ButtonText = tabText;
                // Set Text Options.
                tabText.SetText("Tab Button");
                tabText.color = Color.black;
                tabText.alignment = TextAlignmentOptions.Center;
                tabTextObject.transform.localScale = Vector3.one;
                // Set Text Anchor. (Stretch all)
                tabText.rectTransform.anchorMin = new Vector2(.33f, 0f);
                tabText.rectTransform.anchorMax = new Vector2(1f, 1f);
                tabText.rectTransform.offsetMin = Vector2.zero;
                tabText.rectTransform.offsetMax = Vector2.zero;

                // -- Image
                GameObject tabImageObject = new GameObject("Tab Image");
                tabImageObject.transform.SetParent(tabButtonObject.transform);
                Image tabImage = tabImageObject.AddComponent<Image>();
                button.ButtonImage = tabImage;
                // Image Options
                tabImageObject.transform.localScale = Vector3.one;
                tabImage.preserveAspect = true;
                // Set anchor to left & stretch along the anchor.
                tabImage.rectTransform.anchorMin = new Vector2(0f, 0f);
                tabImage.rectTransform.anchorMax = new Vector2(.33f, 1f);
                tabImage.rectTransform.offsetMin = Vector2.zero;
                tabImage.rectTransform.offsetMax = Vector2.zero;

                button.GenerateButtonContent();
            }
            else
            {
                button = Instantiate(referenceBtn);

                button.transform.SetParent(referenceBtn.transform.parent);
                button.transform.localScale = referenceBtn.transform.localScale;
            }

            // Init button
            button.ButtonIndex = m_Elements.Count; // This is called before adding to elements array
                                                   // Using the ElementCount property is incorrect
            button.m_ParentTabSystem = this;
            // Generate name
            button.gameObject.name = $"Button_{m_Elements.Count}";

            return button;
        }

        // Tab Cleanup
        /// <summary>
        /// Updates the appearances of the buttons.
        /// <br>Call this when you need to visually update the button.</br>
        /// </summary>
        public override void UpdateElementsAppearance()
        {
            foreach (var button in m_Elements)
            {
                if (button == null)
                    continue;

                if (!Interactable)
                {
                    button.SetButtonAppearance(TabButton.ButtonState.Disable);
                    continue;
                }

                button.SetButtonAppearance(CurrentSelectedTab == button ? TabButton.ButtonState.Click : TabButton.ButtonState.Reset);
            }
        }

        /// <summary>
        /// Selects a button if it's selectable.
        /// </summary>
        /// <param name="btnSelect">Index to select. Clamped value.</param>
        /// <param name="silentSelect">
        /// Whether if the <see cref="OnTabButtonsClicked"/> event should not invoke. 
        /// This is set to <see langword="false"/> by default.
        /// </param>
        public void SetSelectedButtonIndex(int btnSelect, bool silentSelect = false)
        {
            Assert.IsTrue(m_Elements.Count != 0, string.Format("[TabSystem::SetSelectedButtonIndex] There's no item in TabButtons array in TabSystem {0}.", this.GetPath()));
            int IndexSelect = Mathf.Clamp(btnSelect, 0, m_Elements.Count - 1);
            TabButton ButtonToSelScript = m_Elements[IndexSelect];

            if (ButtonToSelScript != null)
            {
                CurrentSelectedTab = ButtonToSelScript;
                ButtonToSelScript.SetButtonAppearance(TabButton.ButtonState.Click);

                if (!silentSelect)
                    OnTabButtonsClicked?.Invoke(IndexSelect);

                UpdateElementsAppearance();
            }
            else
            {
                Debug.LogError(string.Format("[TabSystem] The tab button to select is null. The index was '{0}'.", IndexSelect));
            }
        }
    }
}
