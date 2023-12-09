using UnityEngine;
using System.Collections.Generic;

namespace BXFW
{
    /// This camera component does not depend on any GameObject followings, so it can solely update in the 'Update' method.
    /// <summary>
    /// Camera that can freely move around the scene.
    /// <br>The movement keys are WASD or arrow keys and the user can use the mouse to look around.</br>
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class FreeMoveCamera : MonoBehaviour
    {
        [Header("-- Debug Settings")]
        public Behaviour[] DisableComponentOnEnable;

        [Header("-- Camera Settings")]
        public Transform MoveTransform;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                enabled = value;
            }
        }
        [SerializeField] private bool isEnabled = true;
        public bool LookRawInput = true;
        public float LookSensitivity = 10f;
        public float CameraMoveSpeed = 10f;
        public float BoostedCameraMoveSpeedAdd = 10f;
        public Vector2 MinMaxXRotation = Vector2.zero;
        public Vector2 MinMaxYRotation = Vector2.zero;

        // -- Input Settings -- //
        private readonly IList<string> InputLookAxis    = new string[] { "Mouse X", "Mouse Y" };
        [Header(":: Input Settings ::")]
        public bool InputAdjustMoveSpeedMouseWheel = false;
        public CustomInputEvent InputMoveForward        = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent InputMoveBackward       = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent InputMoveLeft           = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent InputMoveRight          = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent InputMoveBoost          = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
        public CustomInputEvent InputMoveDescend        = new KeyCode[] { KeyCode.Q };
        public CustomInputEvent InputMoveAscend         = new KeyCode[] { KeyCode.E };
        [InspectorLine(.4f, .4f, .4f)]
        public CustomInputEvent InputEventDisableEnable = new KeyCode[] { KeyCode.F8 };
        
        /// <summary>
        /// The target transform to move around.
        /// </summary>
        public Transform TargetTransform { get; protected set; }
        protected Quaternion originalRotation;
        protected bool isInit = false;
        private void Start()
        {
            if (!IsEnabled)
            {
                return;
            }

            // Initilaze resets quaternion rotation.
            Initilaze();
        }
        protected virtual void Initilaze()
        {
            if (isInit)
            {
                return;
            }

            TargetTransform = MoveTransform == null ? GetComponent<Transform>() : MoveTransform;

            // Initial rotation shouldn't have X axis rotation, otherwise the angleaxis does stupid stuff and the Z rotates too.
            TargetTransform.rotation = Quaternion.Euler(0f, TargetTransform.rotation.eulerAngles.y, 0f);
            originalRotation = TargetTransform.rotation;
            isInit = true;
        }

        protected float GetMouseAxis(int Axis)
        {
            var AxisName = InputLookAxis[Axis];
            return LookRawInput ? Input.GetAxisRaw(AxisName) : Input.GetAxis(AxisName);
        }

        protected float CurrentRotationX = 0f, CurrentRotationY = 0f;
        protected virtual void Update()
        {
            if (Input.GetMouseButton(1))
            {
                if (Cursor.lockState != CursorLockMode.Confined && Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = false;
                }

                // Rotate camera
                CurrentRotationX += GetMouseAxis(0) * LookSensitivity;
                CurrentRotationY += GetMouseAxis(1) * LookSensitivity;
                if (MinMaxXRotation.x != 0f && MinMaxXRotation.y != 0f)
                {
                    CurrentRotationX = Mathf.Clamp(CurrentRotationX, MinMaxXRotation.x, MinMaxXRotation.y);
                }
                if (MinMaxYRotation.x != 0f && MinMaxYRotation.y != 0f)
                {
                    CurrentRotationY = Mathf.Clamp(CurrentRotationY, MinMaxYRotation.x, MinMaxYRotation.y);
                }

                TargetTransform.rotation = originalRotation *
                    (Quaternion.AngleAxis(CurrentRotationX, Vector3.up) * Quaternion.AngleAxis(CurrentRotationY, -Vector3.right));

                // Move player
                Vector3 MoveVec = Vector3.zero;
                if (InputMoveForward)
                {
                    MoveVec += Vector3.forward;
                }
                if (InputMoveBackward)
                {
                    MoveVec += Vector3.back;
                }
                if (InputMoveLeft)
                {
                    MoveVec += Vector3.left;
                }
                if (InputMoveRight)
                {
                    MoveVec += Vector3.right;
                }
                // Ascend / Descend
                if (InputMoveDescend)
                {
                    MoveVec += Vector3.down;
                }
                if (InputMoveAscend)
                {
                    MoveVec += Vector3.up;
                }
                // Normalize movement to make 'r = 1' and to make diagonal movements consistent
                MoveVec = MoveVec.normalized;

                Vector3 MoveTranslate;
                if (InputMoveBoost)
                {
                    MoveTranslate = (BoostedCameraMoveSpeedAdd + CameraMoveSpeed) * Time.deltaTime * MoveVec;
                }
                else
                {
                    MoveTranslate = CameraMoveSpeed * Time.deltaTime * MoveVec;
                }

                // Adjust Move Speed
                if (InputAdjustMoveSpeedMouseWheel)
                {
                    if (Input.mouseScrollDelta != Vector2.zero)
                    {
                        CameraMoveSpeed = Mathf.Clamp(CameraMoveSpeed + Input.mouseScrollDelta.y, 1f, float.MaxValue);
                        Debug.Log(string.Format("[CameraDebugMove::Update] Set Move speed to : {0}", CameraMoveSpeed));
                    }
                }

                TargetTransform.Translate(MoveTranslate);
            }
            else
            {
                if (Cursor.lockState == CursorLockMode.Confined || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void OnEnable()
        {
            foreach (var comp in DisableComponentOnEnable)
            {
                comp.enabled = false;
            }

            if (!isInit)
            {
                Initilaze();
            }
        }
        private void OnDisable()
        {
            foreach (var comp in DisableComponentOnEnable)
            {
                if (comp == null)
                {
                    continue;
                }

                comp.enabled = true;
            }
        }
    }
}