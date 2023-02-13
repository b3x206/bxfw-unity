using BXFW.Tweening;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("BXFW.Editor")]
namespace BXFW.UI
{
    /// <summary>
    /// Displays the progress of <see cref="SwipableUI"/>.
    /// </summary>
    public class SwipableUIProgressDisplay : MonoBehaviour
    {
        [Header(":: References")]
        public SwipableUI TargetSwipableUI;
        [Header(":: Settings")]
        public FadeType ChildImageFadeType = FadeType.ColorFade;
        public bool ChangeColorWithTween = true;
        public BXTweenPropertyFloat ChildImageColorFadeTween = new BXTweenPropertyFloat(.15f);
        public Color ActiveColor = Color.white;
        public Color DisabledColor = new Color(.7f, .7f, .7f, .7f);
        public Sprite ActiveSprite;
        public Sprite DisabledSprite;

        [SerializeField, InspectorReadOnlyView] private Image baseChildProgressImage;
        [SerializeField, HideInInspector] private List<Image> childProgressImages = new List<Image>();

        /// <summary>
        /// Generates a child image if it doesn't exist.
        /// </summary>
        internal void GenerateChildImage()
        {
            // Create a child image showing status
            if (baseChildProgressImage == null)
            {
                // Create new child gameobject
                var cProgressImage = new GameObject("ProgressImage");
                baseChildProgressImage = cProgressImage.AddComponent<Image>();

                // -- Hardcoded defaults
                // Add layout group & content resizer as this is 'probably' the first time this gameobject is created.
                if (!gameObject.TryGetComponent(out HorizontalLayoutGroup contentLayout))
                    contentLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
                contentLayout.spacing = 10f;
                contentLayout.childAlignment = TextAnchor.MiddleCenter;

                // Set transform as parent after 'content layout thing' as that resizes the object to an invalid size.
                baseChildProgressImage.transform.SetParent(transform);
                baseChildProgressImage.transform.localScale = Vector3.one;

                if (!gameObject.TryGetComponent(out ContentSizeFitter contentSizeFitter))
                    contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

                // 0'th image is special.
                childProgressImages.Add(baseChildProgressImage);
            }

            GenerateDisplay();
        }

        private void Awake()
        {
            if (TargetSwipableUI == null)
            {
                Debug.LogWarning($"[SwipableUIProgressDisplay::Awake] Field 'TargetSwipableUI' is null on object {transform.GetPath()}.");
                return;
            }

            GenerateDisplay();

            TargetSwipableUI.OnClampItemMenuChanged += GenerateDisplay;
            TargetSwipableUI.OnMenuChangeEvent.AddListener(OnMenuChanged);
            prevMenuIndex = TargetSwipableUI.CurrentMenu;
        }

        private int prevMenuIndex = 0; // Swipable menu can only start from this index, however we still assign it to it's 'CurrentMenu' variable.

        private void OnMenuChanged(int menuIndex)
        {
            if (prevMenuIndex == menuIndex) return; // Don't do anything if the menu index is the same.

            // Change images using the current fade type
            switch (ChildImageFadeType)
            {
                // These events are discarded
                case FadeType.None:
                case FadeType.CustomUnityEvent:
                // These are the only 2 valid ones.
                case FadeType.ColorFade:
                    if (ChangeColorWithTween)
                    {
                        // Since prevMenuIndex is changed after this tween is started, it will change the incorrect tweens color.
                        var pMenuIndexPersistent = prevMenuIndex;
                        ChildImageColorFadeTween.StartTween(0f, 1f, (float f) =>
                        {
                            SetAllChildExceptIndex(DisabledColor, 
                                Color.Lerp(childProgressImages[menuIndex].color, ActiveColor, f), menuIndex);
                            // Change the previous image seperately
                            childProgressImages[pMenuIndexPersistent].color = Color.Lerp(childProgressImages[pMenuIndexPersistent].color, DisabledColor, f);
                            //childProgressImages[menuIndex].color = Color.Lerp(childProgressImages[menuIndex].color, ActiveColor, f);
                        });
                    }
                    else
                    {
                        SetAllChildExceptIndex(DisabledColor, ActiveColor, menuIndex);
                    }
                    break;
                case FadeType.SpriteSwap:
                    SetAllChildExceptIndex(DisabledSprite, ActiveSprite, menuIndex);
                    //childProgressImages[menuIndex].sprite = ActiveSprite;
                    break;
            }

            prevMenuIndex = menuIndex;
        }

        /// <summary>
        /// Generates this UIProgressDisplay.
        /// <br><b>NOTE:</b> Only generates if <see cref="TargetSwipableUI"/> isn't null.</br>
        /// </summary>
        public void GenerateDisplay()
        {
            if (TargetSwipableUI == null) return;
            if (childProgressImages == null) GenerateChildImage();
            if (TargetSwipableUI.ClampItemMenu == 0)
            {
                // 0 is special case for only the base object being enabled.
                if (!baseChildProgressImage.gameObject.activeInHierarchy)
                    baseChildProgressImage.gameObject.SetActive(true);

                // Destroy all except base
                // Disable primary gameobject & destroy until 0
                for (int i = 1; i < childProgressImages.Count; i++)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(childProgressImages[i].gameObject);
                    else
                        Destroy(childProgressImages[i].gameObject);
                }

                CleanChildImageList();
                return;
            }

            // ClampItemMenu is an array index
            // Create
            while (childProgressImages.Count < TargetSwipableUI.ClampItemMenu + 1)
            {
                if (!baseChildProgressImage.gameObject.activeInHierarchy)
                    baseChildProgressImage.gameObject.SetActive(true);

                Image imageInst = Instantiate(baseChildProgressImage, transform);

                switch (ChildImageFadeType)
                {
                    // These events are discarded
                    case FadeType.None:
                    case FadeType.CustomUnityEvent:
                    // These are the only 2 valid ones.
                    case FadeType.ColorFade:
                        imageInst.color = childProgressImages.Count == TargetSwipableUI.CurrentMenu ? ActiveColor : DisabledColor;
                        break;
                    case FadeType.SpriteSwap:
                        imageInst.sprite = childProgressImages.Count == TargetSwipableUI.CurrentMenu ? ActiveSprite : DisabledSprite;
                        break;
                }

                childProgressImages.Add(imageInst);
            }
            // Destroy
            while (childProgressImages.Count > TargetSwipableUI.ClampItemMenu + 1)
            {
                // Destroy all if there's no clamp
                if (TargetSwipableUI.ClampItemMenu < 0)
                {
                    // Disable primary gameobject & destroy until 0
                    for (int i = 1; i < childProgressImages.Count; i++)
                    {
                        if (!Application.isPlaying)
                            DestroyImmediate(childProgressImages[i].gameObject);
                        else
                            Destroy(childProgressImages[i].gameObject);
                    }

                    CleanChildImageList();

                    baseChildProgressImage.gameObject.SetActive(false);
                    return;
                }

                // Destroy normally
                if (!Application.isPlaying)
                    DestroyImmediate(childProgressImages[childProgressImages.Count - 1].gameObject);
                else
                    Destroy(childProgressImages[childProgressImages.Count - 1].gameObject);

                CleanChildImageList();
            }
        }

        public void SetAllChildExceptIndex(Color disabled, Color enabled, int enabledIndex)
        {
            for (int i = 0; i < childProgressImages.Count; i++)
            {
                var img = childProgressImages[i];
                if (img == null)
                {
                    Debug.LogWarning($"[SwipableUIProgressDisplay] One of the images are null on object '{name}'.");
                    CleanChildImageList();
                    continue;
                }
                
                img.color = i == enabledIndex ? enabled : disabled;
            }
        }
        public void SetAllChildExceptIndex(Sprite disabled, Sprite enabled, int enabledIndex)
        {
            for (int i = 0; i < childProgressImages.Count; i++)
            {
                var img = childProgressImages[i];
                if (img == null)
                {
                    Debug.LogWarning($"[SwipableUIProgressDisplay] One of the images are null on object '{name}'.");
                    CleanChildImageList();
                    continue;
                }

                img.sprite = i == enabledIndex ? enabled : disabled;
            }
        }

        public void CleanChildImageList()
        {
            childProgressImages.RemoveAll((x) => x == null);
        }
    }
}