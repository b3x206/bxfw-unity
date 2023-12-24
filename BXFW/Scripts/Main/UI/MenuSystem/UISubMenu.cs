using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BXFW.UI
{
    /// <summary>
    /// The sub ui menu. Good for popup windows, etc.
    /// <br>Restrictions that apply to <see cref="UIMenu"/> (in <see cref="UIMenuManager"/>) doesn't apply to this sub menu.</br>
    /// <br/>
    /// <br>Call order for the events is exactly the same as it is in <see cref="UIMenu"/>.</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UISubMenu : MonoBehaviour
    {
        [Serializable]
        public sealed class BoolEvent : UnityEvent<bool> { }
        [Serializable]
        public sealed class MenuBoolEvent : UnityEvent<UISubMenu, bool> { }

        /// <summary>
        /// The event called when the menu opens and closes.
        /// <br><see langword="true"/> =&gt; Menu is opening.</br>
        /// <br><see langword="false"/> =&gt; Menu is closing.</br>
        /// </summary>
        [FormerlySerializedAs("UISubMenuEventSimple")]
        public BoolEvent openCloseBoolEvent;
        /// <summary>
        /// The event called when the menu opens and closes.
        /// <br>This version also outputs a <see cref="UISubMenu"/> with it's invocation.</br>
        /// </summary>
        [FormerlySerializedAs("UISubMenuEvent")]
        public MenuBoolEvent openCloseMenuEvent;

        [SerializeField, Tooltip("The menu rect transform. Leave blank if you don't want to set a custom rect transform.\n" +
            "It is recommended to set this as a child of the UISubMenu component object.")] 
        private RectTransform menuRectTransform;
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

        /// <summary>
        /// If this is true, the <see cref="UIMenuManager.OpenSubMenu(UISubMenu)"/> or 
        /// <see cref="UIMenuManager.CloseSubMenu(UISubMenu)"/> with this menu will set 
        /// the <see cref="Component.gameObject"/>'s activeness attached to this Menu.
        /// </summary>
        [FormerlySerializedAs("AddSetActiveEvent")]
        public bool addSetActiveEvent = true;
        [FormerlySerializedAs("_IsClosedOnAwake")]
        [SerializeField] private bool m_IsClosedOnAwake = true;
        public bool IsClosedOnAwake 
        { 
            get => m_IsClosedOnAwake; 
            protected set => m_IsClosedOnAwake = value;
        }
        public bool IsOpen => gameObject.activeInHierarchy;

        /// <summary>
        /// The awake method.
        /// <br>Call this method like <c><see langword="base"/>.<see cref="Awake"/></c> when overriden.</br>
        /// </summary>
        protected virtual void Awake()
        {
            // If the object is already inactive, it's not visible and closed.
            if (gameObject.activeInHierarchy && IsClosedOnAwake)
            {
                // Close the menu if the thing is active.
                // NOTE : This only applies for the sub menu.
                openCloseBoolEvent?.Invoke(false);
                openCloseMenuEvent?.Invoke(this, false);
            }
        }

        /// <summary>
        /// <para>Opens menu.</para>
        /// <br>objAction is set to <c>() => { gameObject.SetActive(true); }</c>.</br>
        /// <br>Can be disabled using <see cref="addSetActiveEvent"/>.</br>
        /// <br/>
        /// <br>
        /// Note : If you have a global <see cref="UIMenuManager"/>, 
        /// you are most likely calling the incorrect method. Call the <see cref="UIMenuManager.OpenSubMenu(UISubMenu)"/> 
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

            OnUISubMenuEvent(true);
            openCloseBoolEvent?.Invoke(true);
            openCloseMenuEvent?.Invoke(this, true);
        }

        /// <summary>
        /// <para>Closes menu.</para>
        /// <br>objAction is set to <c>() => { gameObject.SetActive(false); }</c>.</br>
        /// <br>Can be disabled using <see cref="addSetActiveEvent"/>.</br>
        /// <br/>
        /// <br>
        /// Note : If you have a global <see cref="UIMenuManager"/>, 
        /// you are most likely calling the incorrect method. Call the <see cref="UIMenuManager.CloseSubMenu(UISubMenu)"/> 
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

            OnUISubMenuEvent(false);
            openCloseBoolEvent?.Invoke(false);
            openCloseMenuEvent?.Invoke(this, false);
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
        protected virtual void OnUISubMenuEvent(bool show)
        { }
    }
}
