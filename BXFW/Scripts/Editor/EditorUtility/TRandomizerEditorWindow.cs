using UnityEngine;
using UnityEditor;

using System.Linq;

namespace BXFW.Tools.Editor
{
    public class TRandomizerEditorWindow : EditorWindow
    {
        #region Enum
        public enum RandomizationAxis
        {
            XAxis = 0,
            YAxis = 1,
            ZAxis = 2
        }
        public enum RandomizationAspect
        {
            Rotation = 0,
            Position = 1
        }
        #endregion

        #region Open Window
        //private static System.Action OnWindowClose;
        [MenuItem("Tools/Transform Randomizer")]
        protected static void OpenRandomizerWindow()
        { OpenWindow(); }

        public static void OpenWindow()
        {
            var window = GetWindow<TRandomizerEditorWindow>("Transform Randomizer");
            //SceneView.duringSceneGui += OnSceneGUI;
            //OnWindowClose = () =>
            //{
            //    SceneView.duringSceneGui -= OnSceneGUI;
            //};
        }
        //private void OnDestroy()
        //{
        //    OnWindowClose?.Invoke();
        //}
        #endregion

        #region Window
        [SerializeField] private Transform[] Transforms = new Transform[0];
        private static RandomizationAspect RandAspect = RandomizationAspect.Rotation;
        private static RandomizationAxis RandAxis = RandomizationAxis.XAxis;
        private static Vector2 V2Field = Vector2.zero;

        private Vector2 ScrollPos = Vector2.zero;

        // (Maybe) TODO : Draw an object pivot editor
        //private static void OnSceneGUI(SceneView currentView)
        //{
        //    Handles.BeginGUI();
        //
        //    var boxSize = new Vector2(200, 200);
        //    // For some reason boxPadding.y needs to be doubled by 2 to get into the correct position.
        //    var boxPadding = new Vector2(20, 20);
        //    var boxRect = new Rect(currentView.position.width - (boxSize.x + boxPadding.x), currentView.position.height - (boxSize.y + (boxPadding.y * 2)),
        //        boxSize.x, boxSize.y);
        //
        //    var areaPadding = new Vector2(10, 10);
        //    var areaRect = new Rect(boxRect.x + (areaPadding.x / 2), boxRect.y + (areaPadding.y / 2), boxRect.width - areaPadding.x, boxRect.height - areaPadding.y);
        //
        //    GUI.Box(boxRect, string.Empty, /*GUI.skin.box*/EditorStyles.helpBox);
        //    GUILayout.BeginArea(areaRect);
        //    GUILayout.EndArea();
        //    Handles.EndGUI();
        //}

        private void OnGUI()
        {
            // --- Draw GUI
            // "target" can be any class derrived from ScriptableObject 
            // (could be EditorWindow, MonoBehaviour, etc)
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty tProperty = so.FindProperty("Transforms");

            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            EditorGUILayout.PropertyField(tProperty, true); // True means show children

            V2Field = EditorGUILayout.Vector2Field("Random Vector Range", V2Field);
            EditorGUILayout.LabelField("Randomizated Element", EditorStyles.boldLabel);
            RandAspect = (RandomizationAspect)EditorGUILayout.EnumPopup(RandAspect);
            EditorGUILayout.LabelField("Randomizated Axis", EditorStyles.boldLabel);
            RandAxis = (RandomizationAxis)EditorGUILayout.EnumPopup(RandAxis);

            so.ApplyModifiedProperties(); // Remember to apply modified properties

            EditorGUILayout.LabelField("WARNING : This operation might not be reversible with undo.\nProceed with caution.",
                EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30f));

            if (GUILayout.Button("Randomize"))
            {
                // Check if the randomizer is setup valid
                if (Transforms == null || Transforms.Length <= 0)
                { Debug.LogError("[TransformRandomizer::Randomize] Couldn't randomize as the list is blank."); return; }
                if (Transforms.All(x => x == null))
                { Debug.LogWarning("[TransformRand::Randomize] There is a null property in the transform list."); return; }

                Undo.RecordObjects(Transforms, "Undo randomized transforms.");

                // Do an 'very optimized' loop
                foreach (Transform t in Transforms)
                {
                    switch (RandAspect)
                    {
                        case RandomizationAspect.Rotation:
                            switch (RandAxis)
                            {
                                case RandomizationAxis.XAxis:
                                    t.rotation = Quaternion.Euler(Random.Range(V2Field.x, V2Field.y),
                                        t.rotation.eulerAngles.y,
                                        t.rotation.eulerAngles.z);
                                    break;
                                case RandomizationAxis.YAxis:
                                    t.rotation = Quaternion.Euler(t.rotation.eulerAngles.x,
                                        Random.Range(V2Field.x, V2Field.y),
                                        t.rotation.eulerAngles.z);
                                    break;
                                case RandomizationAxis.ZAxis:
                                    t.rotation = Quaternion.Euler(t.rotation.eulerAngles.x,
                                        t.rotation.eulerAngles.y,
                                        Random.Range(V2Field.x, V2Field.y));
                                    break;
                            }
                            break;
                        case RandomizationAspect.Position:
                            switch (RandAxis)
                            {
                                case RandomizationAxis.XAxis:
                                    t.position = new Vector3(Random.Range(V2Field.x, V2Field.y),
                                        t.position.y,
                                        t.position.z);
                                    break;
                                case RandomizationAxis.YAxis:
                                    t.position = new Vector3(t.position.x,
                                        Random.Range(V2Field.x, V2Field.y),
                                        t.position.z);
                                    break;
                                case RandomizationAxis.ZAxis:
                                    t.position = new Vector3(t.position.x,
                                        t.position.y,
                                        Random.Range(V2Field.x, V2Field.y));
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            GUILayout.EndScrollView();
        }
        #endregion
    }
}