using UnityEngine;
using UnityEngine.UI;
using BXFW.Tweening.Next;

namespace BXFW.UI
{
    /// <summary>
    /// Displays the progress of <see cref="SwipableUI"/>.
    /// </summary>
    public sealed class SwipableUIProgressDisplay : MultiUIManager<Image>
    {
        [Header(":: References")]
        public SwipableUI targetSwipableUI;

        [Header(":: Settings")]
        public FadeType childImageFadeType = FadeType.ColorFade;

        public bool colorFadeUseTween = true;
        public BXSTweenFloatContext colorFadeTween = new BXSTweenFloatContext(.15f);
        public Color activeColor = Color.white;
        public Color disabledColor = new Color(.7f, .7f, .7f, .7f);

        public Sprite activeSprite;
        public Sprite disabledSprite;

        protected override void Awake()
        {
            base.Awake();

            if (targetSwipableUI == null)
            {
                Debug.LogWarning($"[SwipableUIProgressDisplay::Awake] Field 'TargetSwipableUI' is null on object {transform.GetPath()}.");
                return;
            }

            targetSwipableUI.OnMenuCountChanged += OnMenuCountChanged;
            targetSwipableUI.OnMenuChangeEvent.AddListener(OnMenuChanged);
            m_prevMenuIndex = targetSwipableUI.CurrentMenu;
        }
        protected override void OnDestroy()
        {
            targetSwipableUI.OnMenuCountChanged -= OnMenuCountChanged;
        }

        // -- Used for tweening
        private void SetAllChildExceptIndex(Color disabled, Color enabled, int enabledIndex)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                var img = m_Elements[i];
                if (img == null)
                {
                    CleanupElementsList();
                    continue;
                }

                img.color = i == enabledIndex ? enabled : disabled;
            }
        }
        private void SetAllChildExceptIndex(Sprite disabled, Sprite enabled, int enabledIndex)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                var img = m_Elements[i];
                if (img == null)
                {
                    CleanupElementsList();
                    continue;
                }

                img.sprite = i == enabledIndex ? enabled : disabled;
            }
        }

        private void OnMenuCountChanged()
        {
            ElementCount = targetSwipableUI.MenuCount;
        }

        // Swipable menu can only start from this index, however we still assign it to it's 'CurrentMenu' variable.
        private int m_prevMenuIndex = 0;
        private void OnMenuChanged(int menuIndex)
        {
            // Change images using the current fade type
            switch (childImageFadeType)
            {
                // These events are discarded
                case FadeType.None:
                case FadeType.CustomUnityEvent:
                // These are the only 2 valid ones.
                case FadeType.ColorFade:
                    if (colorFadeUseTween)
                    {
                        if (!colorFadeTween.IsValid)
                        {
                            colorFadeTween.SetStartValue(0f).SetEndValue(1f);
                        }

                        // Since prevMenuIndex is changed after this tween is started, it will change the incorrect tweens color.
                        var prevMenuPersistentIndex = m_prevMenuIndex;
                        colorFadeTween.SetSetter((float f) =>
                        {
                            SetAllChildExceptIndex(
                                disabledColor, 
                                Color.Lerp(disabledColor, activeColor, f), 
                                menuIndex
                            );

                            // Change the previous image seperately
                            m_Elements[prevMenuPersistentIndex].color = Color.Lerp(activeColor, disabledColor, f);
                        });
                        colorFadeTween.Play();
                    }
                    else
                    {
                        SetAllChildExceptIndex(disabledColor, activeColor, menuIndex);
                    }
                    break;
                case FadeType.SpriteSwap:
                    SetAllChildExceptIndex(disabledSprite, activeSprite, menuIndex);
                    break;
            }

            m_prevMenuIndex = menuIndex;
        }

        protected override Image OnCreateElement(Image referenceElement)
        {
            Image createImage;

            if (referenceElement == null)
            {
                // Create new child gameobject
                var cProgressImage = new GameObject();
                createImage = cProgressImage.AddComponent<Image>();

                // -- Hardcoded defaults
                // Add layout group & content resizer to this object as this moment is 'probably' the first time this gameobject is created.
                if (!gameObject.TryGetComponent(out HorizontalLayoutGroup contentLayout))
                    contentLayout = gameObject.AddComponent<HorizontalLayoutGroup>();

                contentLayout.spacing = 10f;
                contentLayout.childAlignment = TextAnchor.MiddleCenter;

                if (!gameObject.TryGetComponent(out ContentSizeFitter contentSizeFitter))
                    contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();

                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }
            else
            {
                createImage = Instantiate(referenceElement);
            }

            createImage.transform.SetParent(transform);
            createImage.name = $"ProgressImage{m_Elements.Count}";

            return createImage;
        }
        public override void UpdateElementsAppearance()
        {
            if (targetSwipableUI == null)
                return;

            switch (childImageFadeType)
            {
                // These events are discarded
                case FadeType.None:
                case FadeType.CustomUnityEvent:
                // These are the only 2 valid ones.
                case FadeType.ColorFade:
                    SetAllChildExceptIndex(disabledColor, activeColor, targetSwipableUI.CurrentMenu);
                    break;
                case FadeType.SpriteSwap:
                    SetAllChildExceptIndex(disabledSprite, activeSprite, targetSwipableUI.CurrentMenu);
                    break;
            }
        }
    }
}
