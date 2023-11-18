using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// <see cref="UnityEditor.IMGUI.Controls.AdvancedDropdown"/> but simpler and has no GUI by default.
    /// </summary>
    public class BasicDropdown : EditorWindow
    {
        private static readonly Type popupLocationType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PopupLocation");

        // Enum type should be 'UnityEditor.PopupLocation'.
        private static Array GetPopupLocations()
        {
            Array retValue = Array.CreateInstance(popupLocationType, 2);
            retValue.SetValue(Enum.ToObject(popupLocationType, 0), 0); /* PopupLocation.Below,  */
            retValue.SetValue(Enum.ToObject(popupLocationType, 4), 1); /* PopupLocation.Overlay */

            return retValue;
        }

        private static BasicDropdown Instance;
        private Action<BasicDropdown> onGUICall;
        public static void ShowDropdown(Rect parentRect, Vector2 size, Action<BasicDropdown> onGUICall)
        {
            if (Instance == null)
            {
                Instance = CreateInstance<BasicDropdown>();
            }
            
            // EditorWindow.ShowAsDropdown's public version tries to be a real dropdown by using mouse unfocus events
            // With those events it also tries to hook itself to a conceivable parent
            // This window will require manual management so we can use the window however we like to do
            // -- 
            // It does the same thing? TODO : Test this
            Instance.position = new Rect(Instance.position) { x = parentRect.xMin, y = parentRect.yMax, size = size };
            Instance.onGUICall = onGUICall;
            // internal void ShowAsDropDown(Rect buttonRect, Vector2 windowSize, PopupLocation[] priorities)
            MethodInfo showDropdown = typeof(EditorWindow).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(f => f.Name == "ShowAsDropDown" && f.GetParameters().Length == 3);
            showDropdown.Invoke(Instance, new object[] { parentRect, size, GetPopupLocations() });
        }
        /// <summary>
        /// Returns whether if a BasicDropdown is being shown.
        /// </summary>
        public static bool IsBeingShown()
        {
            return Instance != null;
        }
        /// <summary>
        /// Hides the dropdown.
        /// <br>If the BasicDropdown instance doesn't exist this does nothing.</br>
        /// </summary>
        public static void HideDropdown()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.Close();
        }
        /// <summary>
        /// Sets the position of the window.
        /// <br>If the BasicDropdown instance doesn't exist this does nothing.</br>
        /// </summary>
        public static void SetPosition(Rect screenPosition)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.position = screenPosition;
        }
        private void OnGUI()
        {
            if (onGUICall == null)
            {
                // Debug.LogWarning("[BasicDropdown::OnGUI] OnGUI call is null, closing window.");
                Close();
                return;
            }

            onGUICall(this);
        }
    }
}
