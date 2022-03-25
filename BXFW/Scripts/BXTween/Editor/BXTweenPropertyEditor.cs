using BXFW.Tools.Editor;
using BXFW.Tweening;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(BXTweenPropertyBase), true)]
    public class BXTweenPropertyEditor : PropertyDrawer
    {
        private Rect GetPropertyRect(Rect parentRect, int index, float customHeight = -1f)
        {
            // Always add +1 to property rect as in this class we call this after 'EditorGUI.BeginProperty()'.
            var propHeight = (customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight);
            return new Rect(parentRect.xMin, parentRect.yMin + (EditorGUIUtility.singleLineHeight * (index + 1)) + 8, parentRect.size.x, propHeight);
        }

        private SerializedProperty propDuration;
        private SerializedProperty propDelay;
        //private SerializedProperty propUseTweenCurve;
        private SerializedProperty propAllowCustomCurveOvershoot;
        private SerializedProperty propTweenCurve;
        private SerializedProperty propTweenEase;
        private SerializedProperty propInvokeOnManualStop;
        private SerializedProperty propOnEndAction;

        /// <summary>
        /// Sets up the relative property variables inside this class.
        /// </summary>
        private void SetupSerializedPropertyRelative(SerializedProperty property)
        {
            // Refresh properties always because this code is reused for all properties
            // Basically there's one instance of this script running.
            if (property == null)
            {
                Debug.LogError("[CTweenPropertyEditor] Error : Passed property is null for initilazing 'SerializedProperty' variables.");
                return;
            }

            propDuration = property.FindPropertyRelative("_Duration");
            propDelay = property.FindPropertyRelative("_Delay");
            //propUseTweenCurve = property.FindPropertyRelative("_UseTweenCurve");
            propAllowCustomCurveOvershoot = property.FindPropertyRelative("_AllowInterpolationEaseOvershoot");
            propTweenCurve = property.FindPropertyRelative("_TweenCurve");
            propTweenEase = property.FindPropertyRelative("_TweenEase");
            // -- event
            propInvokeOnManualStop = property.FindPropertyRelative("InvokeEventOnManualStop");
            propOnEndAction = property.FindPropertyRelative("OnEndAction");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupSerializedPropertyRelative(property);

            EditorGUI.BeginProperty(position, label, property); // This also sets property.isExpanded and other stuff.
            Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

            // Current property index drawing 
            var targetTw = property.GetTarget();
            var targetValue = (BXTweenPropertyBase)targetTw.Value;
            
            bool useTwCurve = false;
            if (targetValue != null)
            {
                useTwCurve = targetValue.UseTweenCurve;
            }

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(GetPropertyRect(position, 0), propDuration, new GUIContent("Duration", "The duration of the tween. Should be higher than 0."));
                EditorGUI.PropertyField(GetPropertyRect(position, 1), propDelay, new GUIContent("Delay", "The delay of the tween. Values lower than 0 is ignored."));
                EditorGUI.PropertyField(GetPropertyRect(position, 2), propAllowCustomCurveOvershoot, new GUIContent("Allow Curve/Ease Overshoot", "Tween curve/ease can exceed time values over 0-1."));

                targetValue.UseTweenCurve = EditorGUI.Toggle(GetPropertyRect(position, 3), new GUIContent("Use Custom Curve", "Use a custom easing curve."), targetValue.UseTweenCurve);

                if (useTwCurve)
                    EditorGUI.PropertyField(GetPropertyRect(position, 4), propTweenCurve, new GUIContent("Curve", "Custom easing curve."));
                else 
                    EditorGUI.PropertyField(GetPropertyRect(position, 4), propTweenEase, new GUIContent("Ease", "Pre-defined easing curve."));
                
                EditorGUI.PropertyField(GetPropertyRect(position, 5), propInvokeOnManualStop, new GUIContent("Invoke Ending On Manual Stops", 
                    "When 'StartTween' is called, if the tween is already running 'StopTween' will invoke 'OnEnd' function (this may produce unwanted results on certain occassions). This prevents that. [Property-specific issue.]"));
                EditorGUI.PropertyField(GetPropertyRect(position, 6, EditorGUI.GetPropertyHeight(propOnEndAction)), propOnEndAction, new GUIContent("OnTweenEnd", "Ending action for the tween. Assign object listeners here."));
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalLinesDrawn = 1; // Always include one for the 'BeginProperty' call

            if (property.isExpanded)
            {
                totalLinesDrawn = 13;
            }

            return EditorGUIUtility.singleLineHeight * totalLinesDrawn + 4;
        }
    }
}