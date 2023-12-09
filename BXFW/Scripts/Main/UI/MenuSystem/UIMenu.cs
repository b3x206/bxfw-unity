using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BXFW.UI
{
    /// <summary>
    /// The UI Menu.
    /// <br>Useful for easier management of full-panel canvas menus.</br>
    /// <br/>
    /// <br>Open/Close event invoke order is as following : </br>
    /// <br><b>1: </b><c>'objAction'</c> parameter passed to any of <see cref="OpenMenu(Action)"/> or <see cref="CloseMenu(Action)"/>.</br>
    /// <br><b>2: </b><see cref="OnUIMenuEvent(bool)"/></br>
    /// <br><b>3: </b><see cref="openCloseMenuEvent"/></br>
    /// <br><b>4: </b><see cref="openCloseBoolEvent"/></br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIMenu : MonoBehaviour
    {
        // -- Event classes
        [Serializable]
        public sealed class BoolEvent : UnityEvent<bool> { }
        [Serializable]
        public sealed class MenuBoolEvent : UnityEvent<UIMenu, bool> { }

        // -- Variables
        /// <summary>
        /// The event called when the menu opens and closes.
        /// <br><see langword="true"/> =&gt; Menu is opening.</br>
        /// <br><see langword="false"/> =&gt; Menu is closing.</br>
        /// </summary>
        [Header("Menu Settings")]
        [FormerlySerializedAs("ExtraUIEvents_Simple")]
        public BoolEvent openCloseBoolEvent;
        /// <summary>
        /// The event called when the menu opens and closes.
        /// <br>This version also outputs a <see cref="UISubMenu"/> with it's invocation.</br>
        /// </summary>
        [FormerlySerializedAs("ExtraUIEvents")]
        public MenuBoolEvent openCloseMenuEvent;
        /// <summary>
        /// If this is true, the <see cref="UIMenuManager.OpenMenu(UIMenu)"/> or 
        /// <see cref="UIMenuManager.CloseMenu(UIMenu)"/> with this menu will set 
        /// the <see cref="Component.gameObject"/>'s activeness attached to this Menu.
        /// </summary>
        [FormerlySerializedAs("AddSetActiveEvent")]
        public bool addSetActiveEvent = true;
        public bool IsOpen => gameObject.activeInHierarchy;

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
        /// <br>May not contain a canvas group.</br>
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (menuCanvasGroup == null)
                {
                    TryGetComponent(out menuCanvasGroup);
                }

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
        /// <br>Can be disabled using <see cref="addSetActiveEvent"/>.</br>
        /// <br/>
        /// <br>
        /// Note : If you have a global <see cref="UIMenuManager"/>, 
        /// you are most likely calling the incorrect method. Call the <see cref="UIMenuManager.OpenMenu(UIMenu)"/> 
        /// with the given menu as the parameter instead.
        /// </br>
        /// </summary>
        /// <param name="objAction">Action to invoke when the menu is opened.</param>
        public void OpenMenu(Action objAction = null)
        {
            if (addSetActiveEvent)
            {
                objAction += () => { gameObject.SetActive(true); };
            }

            objAction?.Invoke();

            OnUIMenuEvent(true);
            openCloseMenuEvent?.Invoke(this, true);
            openCloseBoolEvent?.Invoke(true);
        }

        /// <summary>
        /// <para>Closes menu.</para>
        /// <br>objAction is set to <c>() => { gameObject.SetActive(false); }</c>.</br>
        /// <br>Can be disabled using <see cref="addSetActiveEvent"/>.</br>
        /// <br/>
        /// <br>
        /// Note : If you have a global <see cref="UIMenuManager"/>, 
        /// you are most likely calling the incorrect method. Call the <see cref="UIMenuManager.CloseMenu(UIMenu)"/> 
        /// with the given menu as the parameter instead.
        /// </br>
        /// </summary>
        /// <param name="objAction">Action to invoke when the menu is opened.</param>
        public void CloseMenu(Action objAction = null)
        {
            if (addSetActiveEvent)
            {
                objAction += () => { gameObject.SetActive(false); };
            }

            objAction?.Invoke();

            OnUIMenuEvent(false);
            openCloseMenuEvent?.Invoke(this, false);
            openCloseBoolEvent?.Invoke(false);
        }

        /// <summary>
        /// A menu event that is called when any of the <see cref="OpenMenu(Action)"/> or <see cref="CloseMenu(Action)"/> is called with the appopriate parameters.
        /// <br>
        /// This way, there's no need to register to your own events from the editor or from <c>Awake()</c>, overriding this method should give you the <see cref="UIMenu"/>
        /// parameter as <see langword="this"/> and <see cref="bool"/> parameter as the <paramref name="show"/> one.
        /// </br>
        /// <br>
        /// This method does nothing by default (<b>Only applies if class is only inheriting from <see cref="UIMenu"/>, other classes may apply their overrides</b>).
        /// It is an optional override.
        /// </br>
        /// </summary>
        /// <param name="show">
        /// <br>if <see langword="true"/>, <see cref="OpenMenu(Action)"/> is called.</br>
        /// <br>if <see langword="false"/>, <see cref="CloseMenu(Action)"/> is called.</br>
        /// </param>
        protected virtual void OnUIMenuEvent(bool show)
        { }
    }
}
