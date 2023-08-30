using System;
using UnityEngine;
using UnityEngine.Events;

namespace BXFW.UI
{
    /// <summary>
    /// The UI Menu.
    /// <br>Useful for easier management of full-panel canvas menus.</br>
    /// <br/>
    /// <br>Open/Close event invoke order is as following : </br>
    /// <br><b>1: </b><c>'objAction'</c> parameter passed to any of <see cref="OpenMenu(Action)"/> or <see cref="CloseMenu(Action)"/>.</br>
    /// <br><b>2: </b><see cref="OnUIMenuEvent(bool)"/></br>
    /// <br><b>3: </b><see cref="ExtraUIEvents"/></br>
    /// <br><b>4: </b><see cref="ExtraUIEvents_Simple"/></br>
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
                    menuRectTransform = GetComponent<RectTransform>();

                return menuRectTransform;
            }
        }
        [SerializeField, Tooltip("Optional canvas group for this menu.")]
        private CanvasGroup menuCanvasGroup;
        /// <summary>
        /// The optional canvas group for this UIMenu.
        /// <br>May not contain a canvas group.</br>
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (menuCanvasGroup == null)
                    TryGetComponent(out menuCanvasGroup);

                return menuCanvasGroup;
            }
            set
            {
                menuCanvasGroup = value;
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

            OnUIMenuEvent(true);
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

            OnUIMenuEvent(false);
            ExtraUIEvents?.Invoke(this, false);
            ExtraUIEvents_Simple?.Invoke(false);
        }

        /// <summary>
        /// A menu event that is called when any of the <see cref="OpenMenu(Action)"/> or <see cref="CloseMenu(Action)"/> is called with the appopriate parameters.
        /// <br>
        /// This way, there's no need to register to your own events from the editor or from <c>Awake()</c>, overriding this method should give you the <see cref="UIMenu"/>
        /// parameter as <see langword="this"/> and <see cref="bool"/> parameter as the <paramref name="show"/> one.
        /// </br>
        /// <br>This method does nothing by default. It is an optional override.</br>
        /// </summary>
        /// <param name="show">
        /// <br>if <see langword="true"/>, <see cref="OpenMenu(Action)"/> is called.</br>
        /// <br>if <see langword="false"/>, <see cref="CloseMenu(Action)"/> is called.</br>
        /// </param>
        protected virtual void OnUIMenuEvent(bool show)
        { }
    }
}