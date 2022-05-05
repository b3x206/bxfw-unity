using UnityEngine;
using UnityEngine.Events;

using TMPro;
using System.Collections.Generic;

namespace BXFW.UI
{
    /// <summary>
    /// The fading type of TabButton.
    /// TODO : Make this class internal.
    /// </summary>
    public enum FadeType
    {
        None,
        ColorFade,
        SpriteSwap,
        CustomUnityEvent
    }

    /// <summary>
    /// The tab system. Only use properties if you don't identify as a button.
    /// </summary>
    [ExecuteInEditMode()]
    public class TabSystem : MonoBehaviour
    {
        [System.Serializable]
        public class IntUnityEvent : UnityEvent<int> { }
        [System.Serializable]
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
        public float TabButtonFadeSpeed = .15f;
        public Color TabButtonFadeColorTargetHover = new Color(.95f, .95f, .95f);
        public Color TabButtonFadeColorTargetClick = new Color(.9f, .9f, .9f);
        public bool TabButtonSubtractFromCurrentColor = false;
        // ButtonFadeType = SpriteSwap
        public Sprite HoverSpriteToSwap;
        public Sprite TargetSpriteToSwap;
        // ButtonFadeType = CustomUnityEvent
        public TabButton.TabButtonUnityEvent TabButtonCustomEventOnReset;
        public TabButton.TabButtonUnityEvent TabButtonCustomEventHover;
        public TabButton.TabButtonUnityEvent TabButtonCustomEventClick;

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

        /// <summary>
        /// Internal call of <see cref="GenerateTabs"/>
        /// <br>Required to check 0/1 disable-enable state.</br>
        /// </summary>
        /// <param name="prevIndex">Previous index passed by the <see cref="TabButtonAmount"/>'s setter.</param>
        private void GenerateTabs(int prevIndex)
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

        public void ClearTabs(bool ResetTbBtnAmount = true, bool ClearAll = false)
        {
            CleanTabButtonsList();

            // Destroy array.
            foreach (TabButton button in TabButtons)
            {
                if (button.ButtonIndex == 0 && !ClearAll) continue;

                if (Application.isPlaying)
                {
                    Destroy(button.gameObject);
                }

                if (Application.isEditor)
                {
                    DestroyImmediate(button.gameObject);
                }
            }

            if (TabButtons.Count > 1)
            {
                TabButtons.RemoveRange(1, Mathf.Max(1, TabButtons.Count - 1));
            }

            if (!ClearAll)
            {
                var tempTabBtn = TabButtons[0];
                TabButtons.Clear();
                TabButtons.Add(tempTabBtn);
                tempTabBtn.ButtonIndex = 0;
            }

            if (ResetTbBtnAmount)
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
                var TTextRect = ButtonText.rectTransform;
                TTextRect.anchorMin = new Vector2(0, 0);
                TTextRect.anchorMax = new Vector2(1, 1);
                TTextRect.offsetMin = Vector2.zero;
                TTextRect.offsetMax = Vector2.zero;
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

        /// <summary>
        /// Cleans the <see cref="TabButtons"/> list in case of null and other stuff.
        /// </summary>
        public void CleanTabButtonsList()
        {
            TabButtons.RemoveAll((x) => x == null);
        }
        /// <summary>
        /// Resets button appearances of unselected ones.
        /// </summary>
        public void CheckUnClickedButtons()
        {
            foreach (TabButton b in TabButtons)
            {
                if (b == null)
                { continue; }

                if (b != CurrentSelectedTab)
                {
                    b.ResetButtonAppearance();
                }
            }
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
                ButtonToSelScript.SelectButtonAppearance();
                if (!silentSelect)
                    OnTabButtonsClicked?.Invoke(IndexSelect);
                CheckUnClickedButtons();
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