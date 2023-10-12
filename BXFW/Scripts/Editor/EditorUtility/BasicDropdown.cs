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

        // Enum type should be 'PopupLocation'.
        private static Array GetPopupLocations()
        {
            Array retValue = Array.CreateInstance(popupLocationType, 2);
            retValue.SetValue(Enum.ToObject(popupLocationType, 0), 0); /* PopupLocation.Below,  */
            retValue.SetValue(Enum.ToObject(popupLocationType, 4), 1); /* PopupLocation.Overlay */

            return retValue;
        }

        private static BasicDropdown Instance;
        private Action<BasicDropdown> onGUICall;
        public static bool IsBeingShown()
        {
            return Instance != null;
        }
        public static void ShowDropdown(Rect parentRect, Vector2 size, Action<BasicDropdown> onGUICall)
        {
            if (Instance == null)
            {
                Instance = CreateInstance<BasicDropdown>();
            }
            // ... :
            // Instance.ShowAsDropDown(parentRect, size);
            
            Instance.position = new Rect(Instance.position) { x = parentRect.xMin, y = parentRect.yMax, size = size };
            Instance.onGUICall = onGUICall;
            // void ShowAsDropDown(Rect buttonRect, Vector2 windowSize, PopupLocation[] priorities)
            MethodInfo showDropdown = typeof(EditorWindow).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(f => f.Name == "ShowAsDropDown" && f.GetParameters().Length == 3);
            //MethodInfo showDropdown = typeof(EditorWindow).GetMethod("ShowAsDropDown", BindingFlags.Instance | BindingFlags.NonPublic);
            showDropdown.Invoke(Instance, new object[] { parentRect, size, GetPopupLocations() });
        }
        public static void HideDropdown()
        {
            if (Instance != null)
            {
                Instance.Close();
            }
        }
        public static void SetPosition(Rect screenPosition)
        {
            Instance.position = screenPosition;
        }
        private void OnGUI()
        {
            if (onGUICall == null)
            {
                Close();
                return;
            }

            onGUICall(this);
        }
    }
}
