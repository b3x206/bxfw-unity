using BXFW.Tools.Editor;
using BXFW.Tweening;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(BXTweenPropertyBase), true)]
    public class BXTweenPropertyEditor : PropertyDrawer
    {
        private int currentPropRect = -1;
        private Rect GetPropertyRect(Rect parentRect, float customHeight = -1f)
        {
            // Always add +1 to property rect as in this class we call this after 'EditorGUI.BeginProperty()'.
            currentPropRect++;
            var propHeight = customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight;
            return new Rect(parentRect.xMin, parentRect.yMin + (EditorGUIUtility.singleLineHeight * (currentPropRect + 1)) + 8, parentRect.size.x, propHeight);
        }

        private SerializedProperty propDuration;
        private SerializedProperty propDelay;
        private SerializedProperty propRepeatAmount;
        private SerializedProperty propRepeatType;
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
            propRepeatAmount = property.FindPropertyRelative("_RepeatAmount");
            propRepeatType = property.FindPropertyRelative("_TweenRepeatType");
            // This is drawn using EditorGUI.Toggle to activate the property getter-setter.
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

            // Reset 'GetPropertyRect' positioning.
            currentPropRect = -1;
            if (property.isExpanded)
            {
                EditorGUI.BeginChangeCheck();

                // (maybe) TODO : Use properties of 'targetValue' for updating the property.
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(GetPropertyRect(position), propDuration, new GUIContent("Duration", "The duration of the tween. Should be higher than 0."));
                EditorGUI.PropertyField(GetPropertyRect(position), propDelay, new GUIContent("Delay", "The delay of the tween. Values lower than 0 is ignored."));
                EditorGUI.PropertyField(GetPropertyRect(position), propRepeatAmount, new GUIContent("Repeat Amount", "Repeat amount of the tween. 0 and lower is no repeat."));
                // Show repeat type if we are using repeats
                if (propRepeatAmount.intValue > 0)
                    EditorGUI.PropertyField(GetPropertyRect(position), propRepeatType, new GUIContent("Repeat Type", "Repeat type of the tween. PingPong: Switch values for 1 repeat, Reset:"));
                EditorGUI.PropertyField(GetPropertyRect(position), propAllowCustomCurveOvershoot, new GUIContent("Allow Curve/Ease Overshoot", "Tween curve/ease can exceed time values over 0-1."));

                // This is an 'EditorGUI.Toggle' for proper checking of the inspector.
                targetValue.UseTweenCurve = EditorGUI.Toggle(GetPropertyRect(position), new GUIContent("Use Custom Curve", "Use a custom easing curve."), targetValue.UseTweenCurve);

                if (useTwCurve)
                    EditorGUI.PropertyField(GetPropertyRect(position), propTweenCurve, new GUIContent("Curve", "Custom easing curve."));
                else 
                    EditorGUI.PropertyField(GetPropertyRect(position), propTweenEase, new GUIContent("Ease", "Pre-defined easing curve."));
                
                EditorGUI.PropertyField(GetPropertyRect(position), propInvokeOnManualStop, new GUIContent("Invoke Ending On Manual Stops", 
                    "When 'StartTween' is called, if the tween is already running 'StopTween' will invoke 'OnEnd' function (this may produce unwanted results on certain occassions). This prevents that. [Property-specific issue.]"));
                EditorGUI.PropertyField(GetPropertyRect(position, EditorGUI.GetPropertyHeight(propOnEndAction)), propOnEndAction, new GUIContent("OnTweenEnd", "Ending action for the tween. Assign object listeners here."));
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    targetValue.UpdateProperty();
                    EditorUtility.SetDirty(property.serializedObject.targetObject); // FIXME : this may be not required
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalLinesDrawn = 1; // Always include one for the 'BeginProperty' call

            if (property.isExpanded)
            {
                // Was 15, this automatically receives the height from 'GetPropertyRect' calls.
                totalLinesDrawn = currentPropRect + 7;
            }

            return (EditorGUIUtility.singleLineHeight * totalLinesDrawn) + 4;
        }
    }
}