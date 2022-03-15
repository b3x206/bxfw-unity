using BXFW.Tools.Editor;
using BXFW.Tweening;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(CTweenPropertyBase), true)]
    public class CTweenPropertyEditor : PropertyDrawer
    {
        private Rect GetPropertyRect(Rect parentRect, int index, float customHeight = -1f)
        {
            // Always add +1 to property rect as in this class we call this after 'EditorGUI.BeginProperty()'.
            var propHeight = customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight + 4;
            return new Rect(parentRect.min.x, parentRect.min.y + (EditorGUIUtility.singleLineHeight * (index + 1)), parentRect.size.x, propHeight);
        }

        private SerializedProperty propDuration;
        private SerializedProperty propDelay;
        private SerializedProperty propUseTweenCurve;
        private SerializedProperty propAllowCustomCurveOvershoot;
        private SerializedProperty propTweenCurve;
        private SerializedProperty propTweenEase;
        private SerializedProperty propOnEndAction;

        private bool isSerializedPropertySetup = false;

        /// <summary>
        /// Sets up the relative property variables inside this class.
        /// </summary>
        private void SetupSerializedPropertyRelative(SerializedProperty property)
        {
            if (isSerializedPropertySetup) return;

            if (property == null)
            {
                Debug.LogError("[CTweenPropertyEditor] Error : Passed property is null for initilazing 'SerializedProperty' variables.");
                return;
            }

            propDuration = property.FindPropertyRelative("_Duration");
            propDelay = property.FindPropertyRelative("_Delay");
            propUseTweenCurve = property.FindPropertyRelative("_UseTweenCurve");
            propAllowCustomCurveOvershoot = property.FindPropertyRelative("_AllowCustomCurveOvershoot");
            propTweenCurve = property.FindPropertyRelative("_TweenCurve");
            propTweenEase = property.FindPropertyRelative("_TweenEase");
            propOnEndAction = property.FindPropertyRelative("OnEndAction");

            isSerializedPropertySetup = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupSerializedPropertyRelative(property);

            EditorGUI.BeginProperty(position, label, property);
            Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

            // Current property index drawing 
            var targetTw = property.GetTarget();
            var targetValue = (CTweenPropertyBase)targetTw.Value;
            
            bool useTwCurve = false;
            if (targetValue != null)
            {
                useTwCurve = targetValue.UseTweenCurve;
            }

            if (property.isExpanded)
            {
                Debug.Log("[CTweenPropertyEditor] Rects from 0 to 6");
                for (int i = 0; i < 6; i++)
                {
                    Debug.Log($"{i} : {GetPropertyRect(position, i)}");
                }
                Debug.Log("[CTweenPropertyEditor] Why this doesn't correctly render.");

                //Rect rectType = new Rect(position.min.x + EditorGUIUtility.labelWidth, position.min.y + EditorGUIUtility.singleLineHeight, position.size.x - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                //Rect rectPower = new Rect(position.min.x, position.min.y + 3 * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                //Rect rectCooldown = new Rect(position.min.x, position.min.y + 2 * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                //Rect rectDuration = new Rect(position.min.x, position.min.y + 3 * EditorGUIUtility.singleLineHeight, position.size.x, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(GetPropertyRect(position, 0), propDuration, new GUIContent("Duration", "The duration of the tween. Should be higher than 0."));
                EditorGUI.PropertyField(GetPropertyRect(position, 1), propDelay, new GUIContent("Delay", "The delay of the tween. Values lower than 0 is ignored."));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), propAllowCustomCurveOvershoot, new GUIContent("Allow Curve Overshoot", "Tween curve can exceed time values over 0-1."));

                targetValue.UseTweenCurve = EditorGUI.Toggle(GetPropertyRect(position, 3), new GUIContent("Use Custom Curve", "Use a custom easing curve."), targetValue.UseTweenCurve);

                if (useTwCurve)
                    EditorGUI.PropertyField(GetPropertyRect(position, 4), propTweenCurve, new GUIContent("Curve", "Custom easing curve."));
                else 
                    EditorGUI.PropertyField(GetPropertyRect(position, 4), propTweenEase, new GUIContent("Ease", "Pre-defined easing curve."));
                
                EditorGUI.PropertyField(GetPropertyRect(position, 5, EditorGUI.GetPropertyHeight(propOnEndAction)), propOnEndAction, new GUIContent("OnTweenEnd", "Ending action for the tween. Assign object listeners here."));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalLinesDrawn = 1; // Always include one for the 'BeginProperty' call

            if (property.isExpanded)
            {
                totalLinesDrawn = 12;
            }

            return EditorGUIUtility.singleLineHeight * totalLinesDrawn + 4;
        }
    }
}