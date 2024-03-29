using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace BXFW.Tools.Editor
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
            retValue.SetValue(Enum.ToObject(popupLocationType, 0), 0); // PopupLocation.Below,
            retValue.SetValue(Enum.ToObject(popupLocationType, 4), 1); // PopupLocation.Overlay

            return retValue;
        }

        private static BasicDropdown Instance;
        private Action onGUICall;
        /// <summary>
        /// Displays a BasicDropdown.
        /// </summary>
        /// <param name="parentRect">
        /// The display screen rect. Convert your GUI space rects to 
        /// Screen space rects by using <see cref="GUIUtility.GUIToScreenRect(Rect)"/>.
        /// </param>
        /// <param name="size">Size of the dropdown to display.</param>
        /// <param name="onGUICall">Callback done on the OnGUI.</param>
        public static void ShowDropdown(Rect parentRect, Vector2 size, Action onGUICall)
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
            DestroyImmediate(Instance);
        }
        public static Rect GetPosition()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("[BasicDropdown::GetPosition] Cannot get position while window is not being shown.");
            }

            return Instance.position;
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

            EditorGUI.DrawRect(new Rect(Vector2.zero, position.size), EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.62f, 0.62f, 0.62f));
            onGUICall();
        }
    }
}
