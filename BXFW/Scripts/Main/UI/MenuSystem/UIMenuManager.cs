using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// UI Menu system manager.
    /// <br>Extend from this class to automatically have menu management capabilities.</br>
    /// </summary>
    public class UIMenuManager : MonoBehaviour
    {
        #region -- Variables

        #region Standard Vars
        [Header(":: UI Menu Manager ::")]
        // Inspector
        public bool AllowTwoMenuAtOnce = false;

        // Status
        public bool InsideMenu { get { return _CurrentUIMenu != null; } }
        #endregion

        #region Menu Management Vars
        [SerializeField] protected UIMenu _CurrentUIMenu;
        /// <summary>Currently open <see cref="UIMenu"/>.</summary>
        public UIMenu CurrentUIMenu { get { return _CurrentUIMenu; } }
        protected List<UISubMenu> _CurrentUISubMenus = new List<UISubMenu>();
        /// <summary>Currently open sub-menus.</summary>
        public ReadOnlyCollection<UISubMenu> CurrentUISubMenus
        {
            get { return _CurrentUISubMenus.AsReadOnly(); }
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
            _CurrentUIMenu = menu;
        }
        public void CloseMenu(UIMenu menu)
        {
            if (InsideMenu && !AllowTwoMenuAtOnce)
            {
                // If we are not actually exiting the current menu
                if (_CurrentUIMenu != menu)
                {
                    ExitCurrentMenu();
                }
            }

            menu.CloseMenu();
            if (_CurrentUIMenu == menu)
            {
                _CurrentUIMenu = null;
            }
        }
        public void ExitCurrentMenu()
        {
            if (!InsideMenu) { return; }

            _CurrentUIMenu.CloseMenu();
            _CurrentUIMenu = null;
        }
        // -------- SubMenu
        public void OpenSubMenu(UISubMenu menu)
        {
            if (!_CurrentUISubMenus.Contains(menu))
            {
                _CurrentUISubMenus.Add(menu);
            }
            else
            {
                Debug.LogWarning(string.Format("[UIMenuManager::OpenSubMenu] Sub menu '{0}' is already open.", menu.transform.GetPath()));
                // No need to open sub menu twice.
                return;
            }

            menu.OpenMenu();
        }
        public void CloseSubMenu(UISubMenu menu)
        {
            menu.CloseMenu();
            _CurrentUISubMenus.Remove(menu);
        }
        public void CloseAllSubMenus()
        {
            for (int i = _CurrentUISubMenus.Count - 1; i > -1; i--)
            {
                _CurrentUISubMenus[i].CloseMenu();
                _CurrentUISubMenus.RemoveAt(i);
            }
        }
        #endregion
    }
}