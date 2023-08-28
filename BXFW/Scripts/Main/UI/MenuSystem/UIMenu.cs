using System;
using UnityEngine;
using UnityEngine.Events;

namespace BXFW.UI
{
    /// <summary>
    /// The UI Menu.
    /// <br>Useful for easier management of full-panel canvas menus.</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIMenu : MonoBehaviour
    {
        // -- Event classes
        [Serializable]
        public sealed class UIMenuEventBool : UnityEvent<bool> { }
        [Serializable]
        public sealed class UIMenuEventMenuData : UnityEvent<UIMenu, bool> { }

        // -- Variables
        [Header("Menu Settings")]
        [Tooltip("Event sent whenever the ui opens or closes. Passes a bool param.")]
        public UIMenuEventBool ExtraUIEvents_Simple;
        public UIMenuEventMenuData ExtraUIEvents;
        public bool AddSetActiveEvent = true;
        public bool IsOpen { get { return gameObject.activeInHierarchy; } }

        [SerializeField] private RectTransform menuRectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (menuRectTransform == null)
                {
                    menuRectTransform = GetComponent<RectTransform>();
                }

                return menuRectTransform;
            }
        }
        [SerializeField, Tooltip("Optional canvas group for this menu.")]
        private CanvasGroup menuCanvasGroup;
        /// <summary>
        /// The optional canvas group for this UIMenu.
        /// <br>May not contain a canvas group at all.</br>
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (menuCanvasGroup == null)
                    TryGetComponent(out menuCanvasGroup);

                return menuCanvasGroup;
            }
        }

        /// <summary>
        /// <para>Opens menu.</para>
        /// <br>objAction is set to <c>() => { gameObject.SetActive(true); }</c>.</br>
        /// <br>Can be disabled using <see cref="AddSetActiveEvent"/>.</br>
        /// </summary>
        /// <param name="objAction">Action to invoke when the menu is opened.</param>
        public void OpenMenu(Action objAction = null)
        {
            if (AddSetActiveEvent)
            {
                objAction += () => { gameObject.SetActive(true); };
            }

            objAction?.Invoke();
            ExtraUIEvents?.Invoke(this, true);
            ExtraUIEvents_Simple?.Invoke(true);
        }

        /// <summary>
        /// <para>Closes menu.</para>
        /// <br>objAction is set to <c>() => { gameObject.SetActive(false); }</c>.</br>
        /// <br>Can be disabled using <see cref="AddSetActiveEvent"/>.</br>
        /// </summary>
        /// <param name="objAction">Action to invoke when the menu is opened.</param>
        public void CloseMenu(Action objAction = null)
        {
            if (AddSetActiveEvent)
            {
                objAction += () => { gameObject.SetActive(false); };
            }

            objAction?.Invoke();
            ExtraUIEvents?.Invoke(this, false);
            ExtraUIEvents_Simple?.Invoke(false);
        }
    }
}