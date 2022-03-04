using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// UI Menu system manager.
    /// </summary>
    public class UIMenuManager : MonoBehaviour
    {
        #region Variables

        #region Standard Vars
        [Header(":: UI Menu Manager ::")]
        // Inspector
        public bool AllowTwoMenuAtOnce = false;

        // Status
        public bool InsideMenu { get { return m_CurrentUIMenu != null; } }
        #endregion

        #region Menu Management Vars
        [SerializeField] protected UIMenu m_CurrentUIMenu;
        /// <summary>Currently open <see cref="UIMenu"/>.</summary>
        public UIMenu CurrentUIMenu { get { return m_CurrentUIMenu; } }
        protected List<UISubMenu> m_CurrentUISubMenus = new List<UISubMenu>();
        /// <summary>Currently open sub-menus.</summary>
        public ReadOnlyCollection<UISubMenu> CurrentUISubMenus
        {
            get { return m_CurrentUISubMenus.AsReadOnly(); }
        }
        #endregion

        #endregion

        #region -- Menu Management Functions
        // -- Used for opening different menus
        // WARNING : Use these methods for opening menus,
        // not the methods inside indiviual UIMenu.
        public void OpenMenu(UIMenu menu)
        {
            if (InsideMenu && !AllowTwoMenuAtOnce)
            {
                ExitCurrentMenu();
            }

            menu.OpenMenu();
            m_CurrentUIMenu = menu;
        }
        public void CloseMenu(UIMenu menu)
        {
            if (InsideMenu && !AllowTwoMenuAtOnce)
            {
                ExitCurrentMenu();
            }

            menu.CloseMenu();

            if (m_CurrentUIMenu == menu)
            {
                m_CurrentUIMenu = null;
            }
        }
        public void ExitCurrentMenu()
        {
            if (!InsideMenu) { return; }

            m_CurrentUIMenu.CloseMenu();
            m_CurrentUIMenu = null;
        }
        // -------- SubMenu
        public void OpenSubMenu(UISubMenu menu)
        {
            menu.OpenMenu();
            m_CurrentUISubMenus.Add(menu);
        }
        public void CloseSubMenu(UISubMenu menu)
        {
            menu.CloseMenu();
            m_CurrentUISubMenus.Remove(menu);
        }
        public void CloseAllSubMenus()
        {
            for (int i = m_CurrentUISubMenus.Count - 1; i > 0; i--)
            {
                m_CurrentUISubMenus[i].CloseMenu();
                m_CurrentUISubMenus.RemoveAt(i);
            }
        }
        #endregion
    }
}