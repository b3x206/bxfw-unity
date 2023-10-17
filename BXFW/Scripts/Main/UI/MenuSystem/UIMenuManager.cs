using UnityEngine;
using System.Collections.Generic;

namespace BXFW.UI
{
    /// <summary>
    /// UI Menu system manager.
    /// <br>Extend from this class to automatically have menu management capabilities.</br>
    /// </summary>
    public class UIMenuManager : MonoBehaviour
    {
        [Header(":: UI Menu Manager ::")]
        public bool allowTwoMenuAtOnce = false;

        // Status
        public bool InsideMenu { get { return m_CurrentUIMenu != null; } }

        [SerializeField] protected UIMenu m_CurrentUIMenu;
        /// <summary>
        /// Currently open <see cref="UIMenu"/>.
        /// </summary>
        public UIMenu CurrentUIMenu { get { return m_CurrentUIMenu; } }
        protected List<UISubMenu> m_CurrentUISubMenus = new List<UISubMenu>();
        /// <summary>
        /// Currently open sub-menus.
        /// <br>This shouldn't be directly added/removed from, use the
        /// <see cref="OpenMenu(UIMenu)"/>or <see cref="CloseMenu(UIMenu)"/> functions.</br>
        /// </summary>
        public IReadOnlyList<UISubMenu> CurrentUISubMenus
        {
            get { return m_CurrentUISubMenus; }
        }
        
        #region -- Menu Management Functions
        // -- Used for opening different menus
        /// <summary>
        /// Opens a menu.
        /// <br>If the <see cref="allowTwoMenuAtOnce"/> is <see langword="false"/> and 
        /// manager is <see cref="InsideMenu"/> the current menu will be closed.</br>
        /// </summary>
        /// <param name="menu">Target menu to open. This mustn't be null.</param>
        public void OpenMenu(UIMenu menu)
        {
            if (InsideMenu && !allowTwoMenuAtOnce)
            {
                ExitCurrentMenu();
            }

            menu.OpenMenu();
            m_CurrentUIMenu = menu;
        }
        /// <summary>
        /// Closes a menu.
        /// <br>If the <see cref="allowTwoMenuAtOnce"/> is <see langword="false"/> and
        /// the manager is <see cref="InsideMenu"/> all current menu will be closed.</br>
        /// </summary>
        /// <param name="menu">Target menu to open. This mustn't be null.</param>
        public void CloseMenu(UIMenu menu)
        {
            if (InsideMenu && !allowTwoMenuAtOnce)
            {
                // If we are not actually exiting the current menu
                if (m_CurrentUIMenu != menu)
                {
                    ExitCurrentMenu();
                }
            }

            menu.CloseMenu();
            if (m_CurrentUIMenu == menu)
            {
                m_CurrentUIMenu = null;
            }
        }
        /// <summary>
        /// Exits the <see cref="CurrentUIMenu"/> open.
        /// <br>If <see cref="InsideMenu"/> is <see langword="false"/> this does nothing.</br>
        /// </summary>
        public void ExitCurrentMenu()
        {
            if (!InsideMenu)
                return;

            m_CurrentUIMenu.CloseMenu();
            m_CurrentUIMenu = null;
        }
        // -------- SubMenu
        /// <summary>
        /// Opens a SubMenu.
        /// <br>There can be any amount of SubMenus open all at once.</br>
        /// </summary>
        /// <param name="menu">Target menu to open. This mustn't be null.</param>
        public void OpenSubMenu(UISubMenu menu)
        {
            if (!m_CurrentUISubMenus.Contains(menu))
            {
                m_CurrentUISubMenus.Add(menu);
            }
            else
            {
                Debug.LogWarning($"[UIMenuManager::OpenSubMenu] SubMenu '{name}' is already open.", menu);
                return;
            }

            menu.OpenMenu();
        }
        /// <summary>
        /// Closes a SubMenu.
        /// </summary>
        /// <param name="menu">Target menu to open. This mustn't be null.</param>
        public void CloseSubMenu(UISubMenu menu)
        {
            menu.CloseMenu();
            m_CurrentUISubMenus.Remove(menu);
        }
        /// <summary>
        /// Closes all currently closed SubMenu's.
        /// </summary>
        public void CloseAllSubMenus()
        {
            for (int i = m_CurrentUISubMenus.Count - 1; i > -1; i--)
            {
                var subMenu = m_CurrentUISubMenus[i];
                if (subMenu != null)
                    subMenu.CloseMenu();

                m_CurrentUISubMenus.RemoveAt(i);
            }
        }
        #endregion
    }
}