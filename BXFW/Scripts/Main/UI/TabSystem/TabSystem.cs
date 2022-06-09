﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BXFW.UI
{
    /// <summary>
    /// The fading type of TabButton.
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
    public class TabSystem : UIBehaviour
    {
        [Serializable]
        public class IntUnityEvent : UnityEvent<int> { }
        [Serializable]
        public class TabButtonUnityEvent : UnityEvent<int, TabButton> { }

        ///////////// Public
        /// <summary>
        /// The amount of the tab buttons. 0 means disabled.
        /// </summary>
        public int TabButtonAmount
        {
            get
            {
                return _TabButtonAmount;
            }
            set
            {
                int prevValue = _TabButtonAmount;
                // The weird value is because that the 'TabButtonAmount' will kill your pc if not clampped.
                _TabButtonAmount = Mathf.Clamp(value, 0, ushort.MaxValue);
                GenerateTabs(prevValue);
            }
        }
        [SerializeField] private int _TabButtonAmount = 1;

        /// <summary>
        /// The index of the currently referenced tab button.
        /// </summary>
        public int CurrentReferenceTabButton
        {
            get
            {
                // Also clamp the return as that's necessary to protect sanity
                // (Note : clamp with TabButtons.Count as that's the actual button amount).
                return Mathf.Clamp(_CurrentReferenceTabButton, 0, TabButtons.Count - 1);
            }
            set
            {
                if (_CurrentReferenceTabButton == value) return;

                _CurrentReferenceTabButton = Mathf.Clamp(value, 0, TabButtons.Count - 1);
            }
        }
        [SerializeField] private int _CurrentReferenceTabButton = 0;

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
        public TabButton.TabButtonUnityEvent ButtonCustomEventOnReset;
        public TabButton.TabButtonUnityEvent ButtonCustomEventOnHover;
        public TabButton.TabButtonUnityEvent ButtonCustomEventOnClick;
        public TabButton.TabButtonUnityEvent ButtonCustomEventOnDisable;

        // -- Standard event
        // This variable is added to take more control of the generation of the buttons.
        /// <summary>
        /// Called when a tab button is created.
        /// <br><see langword="int"/> parameter : Returns the index.</br> 
        /// <br><see cref="TabButton"/> parameter : Returns the created button.</br>
        /// </summary>
        public TabButtonUnityEvent OnCreateTabButton;
        public IntUnityEvent OnTabButtonsClicked;

        /// <summary>
        /// Returns the current selected tab.
        /// </summary>
        public TabButton CurrentSelectedTab { get; internal set; }
        public TabButton this[int index]
        {
            get
            {
                return TabButtons[index];
            }
        }

        // Private
        [SerializeField] private List<TabButton> TabButtons = new List<TabButton>();

        // UIBehaviour
        #region Interaction Status
        [Tooltip("Can the TabButton be interacted with?")]
        [SerializeField] private bool interactable = true;
        public bool Interactable
        {
            get { return IsInteractable(); }
            set
            {
                interactable = value;

                UpdateButtonAppearances();
            }
        }
        /// <summary>
        /// Runtime variable for whether if the object is allowed to be interacted with.
        /// </summary>
        private bool groupsAllowInteraction = true;
        /// <summary>
        /// Whether if the UI element is allowed to be interactable.
        /// </summary>
        /// <returns></returns>
        internal virtual bool IsInteractable()
        {
            if (groupsAllowInteraction)
            {
                return interactable;
            }
            return false;
        }
        private readonly List<CanvasGroup> canvasGroupCache = new List<CanvasGroup>();
        protected override void OnCanvasGroupChanged()
        {
            // This event is part of UIBehaviour.
            // Search for 'CanvasGroup' behaviours & apply preferences to this object.
            // 1: Search for transforms that contain 'CanvasGroup'
            // 2: Keep them in cache
            // 3: Update the interaction state accordingly
            bool groupAllowInteraction = true;
            Transform t = transform;

            while (t != null)
            {
                t.GetComponents(canvasGroupCache);
                bool shouldBreak = false;

                for (int i = 0; i < canvasGroupCache.Count; i++)
                {
                    if (!canvasGroupCache[i].interactable)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    if (canvasGroupCache[i].ignoreParentGroups)
                    {
                        shouldBreak = true;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }

                t = t.parent;
            }
            if (groupAllowInteraction != groupsAllowInteraction)
            {
                groupsAllowInteraction = groupAllowInteraction;
                UpdateButtonAppearances();
            }
        }
        #endregion

        /// <summary>
        /// Internal call of <see cref="GenerateTabs"/>
        /// <br>Required to check 0 / 1 tabs disable-enable state.</br>
        /// </summary>
        /// <param name="prevIndex">Previous index passed by the <see cref="TabButtonAmount"/>'s setter.</param>
        protected void GenerateTabs(int prevIndex)
        {
            // Ignore if count is 0 or less
            // While this isn't a suitable place for tab management, i wanted to add an '0' state to it. 
            TabButton firstTBtn = TabButtons[0];

            if (TabButtonAmount <= 0)
            {
                // Make sure the first tab button exists as we need to call 'GenerateTabs' for first spawn.
                if (firstTBtn != null)
                {
                    firstTBtn.gameObject.SetActive(false);

                    // Clean the buttons as that's necessary. (otherwise there's stray buttons)
                    for (int i = 1; i < TabButtons.Count; i++)
                    {
                        if (Application.isEditor)
                        {
                            DestroyImmediate(TabButtons[i].gameObject);
                        }
                        if (Application.isPlaying)
                        {
                            Destroy(TabButtons[i].gameObject);
                        }
                    }

                    CleanTabButtonsList();
                    return;
                }
                // In this case of this if statement, it's not necessary as the button amount is already 0.
            }
            else if (TabButtonAmount == 1 && prevIndex <= 0)
            {
                // Make sure the first tab button exists as we need to call 'GenerateTabs' for first spawn.
                if (firstTBtn != null)
                {
                    // This is bad, calling event here.
                    // But the thing is : 0 tab button amount mean disabled
                    firstTBtn.gameObject.SetActive(true);
                    // Do status update - management
                    // This should have been done all in 'CreateTab' method but yeah
                    firstTBtn.ParentTabSystem = this;
                    OnCreateTabButton?.Invoke(0, firstTBtn);
                }
                else
                {
                    // List needs to be cleaned (has null member that we can't access, will throw exceptions)
                    CleanTabButtonsList();
                }
            }

            // Generate tabs normally after dealing with the '0' stuff.
            GenerateTabs();
        }
        /// <summary>
        /// Generates tabs.
        /// </summary>
        public void GenerateTabs()
        {
            // Normal creation
            while (TabButtons.Count > TabButtonAmount)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(TabButtons[TabButtons.Count - 1].gameObject);
                }
                if (Application.isPlaying)
                {
                    Destroy(TabButtons[TabButtons.Count - 1].gameObject);
                }

                CleanTabButtonsList();
            }
            while (TabButtons.Count < TabButtonAmount)
            {
                CreateTab();
            }
        }
        /// <summary>
        /// Reset tabs.
        /// <br>Call this method if you have an issue with your tabs.</br>
        /// </summary>
        public void ResetTabs()
        {
            ClearTabs(true, true);

            // Destroy all childs
            if (TabButtons.Count <= 1 && transform.childCount > 1)
            {
                var tChild = transform.childCount;
                for (int i = 0; i < tChild; i++)
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(transform.GetChild(0).gameObject);
                    }
                    if (Application.isPlaying)
                    {
                        Destroy(transform.GetChild(0).gameObject);
                    }
                }
            }

            // Create new tab and refresh 
            var tab = CreateTab(false);
            tab.ButtonIndex = 0;
            TabButtons.Clear();
            TabButtons.Add(tab);
        }
        /// <summary>
        /// Clears tabs.
        /// </summary>
        /// <param name="resetTabBtnAmount">Sets internal variable of TabButtonAmount to be 1.</param>
        /// <param name="clearAll">Clears all of the buttons (hard reset parameter).</param>
        public void ClearTabs(bool resetTabBtnAmount = true, bool clearAll = false)
        {
            CleanTabButtonsList();

            // Destroy array.
            foreach (TabButton button in TabButtons)
            {
                if (button.ButtonIndex == 0 && !clearAll) continue;

                if (Application.isPlaying)
                {
                    Destroy(button.gameObject);
                }
                else if (Application.isEditor) // && !isPlaying
                {
                    DestroyImmediate(button.gameObject);
                }
            }

            if (TabButtons.Count > 1)
            {
                TabButtons.RemoveRange(1, Mathf.Max(1, TabButtons.Count - 1));
            }

            if (!clearAll)
            {
                var tempTabBtn = TabButtons[0];
                TabButtons.Clear();
                TabButtons.Add(tempTabBtn);
                tempTabBtn.ButtonIndex = 0;
            }

            if (resetTabBtnAmount)
            {
                _TabButtonAmount = 1;
            }
        }

        /// <summary>
        /// Creates Button for TabSystem.
        /// Info : This command already adds to the list <see cref="TabButtons"/>.
        /// </summary>
        /// <param name="UseRefTab">Whether to use the referenced tab from index <see cref="CurrentReferenceTabButton"/>.</param>
        /// <returns>Creation button result.</returns>
        public TabButton CreateTab(bool UseRefTab = true)
        {
            TabButton TabButtonScript;

            if (TabButtons.Count <= 0 || !UseRefTab)
            {
                GameObject TButton = new GameObject("Tab");
                TButton.transform.SetParent(transform);
                TButton.transform.localScale = Vector3.one;

                TabButtonScript = TButton.AddComponent<TabButton>();

                // -- Text
                GameObject TText = new GameObject("Tab Text");
                TText.transform.SetParent(TButton.transform);
                TextMeshProUGUI ButtonText = TText.AddComponent<TextMeshProUGUI>();
                TabButtonScript.ButtonText = ButtonText;
                // Set Text Options.
                ButtonText.SetText("Tab Button");
                ButtonText.color = Color.black;
                ButtonText.alignment = TextAlignmentOptions.Center;
                TText.transform.localScale = Vector3.one;
                // Set Text Anchor. (Stretch all)
                ButtonText.rectTransform.anchorMin = new Vector2(.33f, 0f);
                ButtonText.rectTransform.anchorMax = new Vector2(1f, 1f);
                ButtonText.rectTransform.offsetMin = Vector2.zero;
                ButtonText.rectTransform.offsetMax = Vector2.zero;

                // -- Image
                GameObject TImage = new GameObject("Tab Image");
                TImage.transform.SetParent(TButton.transform);
                Image ButtonImage = TImage.AddComponent<Image>();
                TabButtonScript.ButtonImage = ButtonImage;
                // Image Options
                TImage.transform.localScale = Vector3.one;
                ButtonImage.preserveAspect = true;
                // Set anchor to left & stretch along the anchor.
                ButtonImage.rectTransform.anchorMin = new Vector2(0f, 0f);
                ButtonImage.rectTransform.anchorMax = new Vector2(.33f, 1f);
                ButtonImage.rectTransform.offsetMin = Vector2.zero;
                ButtonImage.rectTransform.offsetMax = Vector2.zero;

                TabButtonScript.GenerateButtonContent();
            }
            else
            {
                var TabButtonInstTarget = TabButtons[CurrentReferenceTabButton];
                if (TabButtonInstTarget == null)
                {
                    // No reference tab.
                    return CreateTab(false);
                }

                TabButtonScript = Instantiate(TabButtonInstTarget);

                TabButtonScript.transform.SetParent(TabButtonInstTarget.transform.parent);
                TabButtonScript.transform.localScale = TabButtonInstTarget.transform.localScale;
            }

            // Init button
            TabButtonScript.ButtonIndex = TabButtons.Count;
            TabButtonScript.ParentTabSystem = this;
            TabButtonScript.name = string.Format("{0}_{1}", TabButtonScript.name, TabButtons.Count).Replace("(Clone)", string.Empty);

            TabButtons.Add(TabButtonScript);
            OnCreateTabButton?.Invoke(TabButtons.Count - 1, TabButtonScript);

            return TabButtonScript;
        }

        // Tab Cleanup
        /// <summary>
        /// Updates the appearances of the buttons.
        /// <br>Call this when you need to visually update the button.</br>
        /// </summary>
        public void UpdateButtonAppearances()
        {
            foreach (var button in TabButtons)
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
        /// Cleans the <see cref="TabButtons"/> list in case of null and other stuff.
        /// </summary>
        public void CleanTabButtonsList()
        {
            TabButtons.RemoveAll((x) => x == null);
        }
        /// <summary>
        /// Selects a button if it's selectable.
        /// </summary>
        /// <param name="btnSelect">Index to select. Clamped value.</param>
        /// <param name="silentSelect">
        /// Whether if the <see cref="OnTabButtonsClicked"/> event should invoke. 
        /// This is set to <see langword="true"/> by default.
        /// </param>
        public void SetSelectedButtonIndex(int btnSelect, bool silentSelect = false)
        {
            var IndexSelect = Mathf.Clamp(btnSelect, 0, TabButtons.Count - 1);
            TabButton ButtonToSelScript = TabButtons[IndexSelect];

            if (ButtonToSelScript != null)
            {
                CurrentSelectedTab = ButtonToSelScript;
                ButtonToSelScript.SetButtonAppearance(TabButton.ButtonState.Click);

                if (!silentSelect)
                    OnTabButtonsClicked?.Invoke(IndexSelect);

                UpdateButtonAppearances();
            }
            else
            {
                Debug.LogError($"[TabSystem] The tab button to select is null. The index was {IndexSelect}.");
            }
        }
        /// <summary>
        /// Returns the currently selected buttons index.
        /// </summary>
        public int GetSelectedButtonIndex()
        {
            return CurrentSelectedTab.ButtonIndex;
        }
    }
}