using UnityEngine;
using System.Collections.Generic;

namespace BXFW
{
    /// This transform move component does not depend on any GameObject followings, so it can solely update in the 'Update' method.
    /// <summary>
    /// Transform that can freely move around the scene. (this is not a specific camera behaviour)
    /// <br>The movement keys are WASD or arrow keys and the user can use the mouse to look around.</br>
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class FreeMoveCamera : MonoBehaviour
    {
        /// <summary>
        /// Component list to disable/enable when the camera is enable/disabled.
        /// </summary>
        [Header("-- Debug Settings")]
        public List<Behaviour> disableComponentsOnEnable = new List<Behaviour>();

        [Header("-- Camera Settings")]
        public Transform moveTransform;
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set
            {
                m_IsEnabled = value;
                enabled = value;
            }
        }
        [SerializeField] private bool m_IsEnabled = true;
        public float moveSpeed = 10f;
        public float boostedMoveSpeedAdd = 10f;
        public MinMaxValue xRotationRange = MinMaxValue.Zero;
        public MinMaxValue yRotationRange = MinMaxValue.Zero;

        [Header("Input Settings")]
        public bool lookRawInput = true;
        public float lookSensitivity = 10f;
        public bool mouseWheelAdjustSpeed = false;
        [InspectorLine(LineColor.Gray)]
        public CustomInputEvent inputMoveForward = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent inputMoveBackward = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent inputMoveLeft = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent inputMoveRight = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent inputMoveBoost = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
        public CustomInputEvent inputMoveDescend = new KeyCode[] { KeyCode.Q };
        public CustomInputEvent inputMoveAscend = new KeyCode[] { KeyCode.E };

        /// <summary>
        /// The target transform to move around.
        /// </summary>
        public Transform TargetTransform { get; protected set; }
        protected Quaternion m_originalRotation;
        protected bool m_isInit = false;
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
            if (m_isInit)
            {
                return;
            }

            TargetTransform = moveTransform == null ? GetComponent<Transform>() : moveTransform;

            // Initial rotation shouldn't have X axis rotation, otherwise the angleaxis does stupid stuff and the Z rotates too.
            TargetTransform.rotation = Quaternion.Euler(0f, TargetTransform.rotation.eulerAngles.y, 0f);
            m_originalRotation = TargetTransform.rotation;
            m_isInit = true;
        }

        protected float GetMouseAxis(string axisName)
        {
            return lookRawInput ? Input.GetAxisRaw(axisName) : Input.GetAxis(axisName);
        }

        /// <summary>
        /// Whether if this class was the one that toggled the cursor state.
        /// </summary>
        private bool toggledCursorLockState = false;
        protected float CurrentRotationX = 0f, CurrentRotationY = 0f;
        protected virtual void Update()
        {
            if (Input.GetMouseButton(1))
            {
                if (Cursor.lockState != CursorLockMode.Confined && Cursor.visible && !toggledCursorLockState)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = false;
                    toggledCursorLockState = true;
                }

                // Rotate camera
                CurrentRotationX += GetMouseAxis("Mouse X") * lookSensitivity;
                CurrentRotationY += GetMouseAxis("Mouse Y") * lookSensitivity;
                if (xRotationRange != MinMaxValue.Zero)
                {
                    CurrentRotationX = xRotationRange.ClampBetween(CurrentRotationX);
                }
                if (yRotationRange != MinMaxValue.Zero)
                {
                    CurrentRotationY = yRotationRange.ClampBetween(CurrentRotationY);
                }

                TargetTransform.rotation = m_originalRotation *
                    (Quaternion.AngleAxis(CurrentRotationX, Vector3.up) * Quaternion.AngleAxis(CurrentRotationY, -Vector3.right));

                // Move player
                Vector3 inputVector = Vector3.zero;
                if (inputMoveForward)
                {
                    inputVector += Vector3.forward;
                }
                if (inputMoveBackward)
                {
                    inputVector += Vector3.back;
                }
                if (inputMoveLeft)
                {
                    inputVector += Vector3.left;
                }
                if (inputMoveRight)
                {
                    inputVector += Vector3.right;
                }
                // Ascend / Descend
                if (inputMoveDescend)
                {
                    inputVector += Vector3.down;
                }
                if (inputMoveAscend)
                {
                    inputVector += Vector3.up;
                }
                // Normalize movement to make 'r = 1' and to make diagonal movements consistent
                inputVector = inputVector.normalized;

                Vector3 MoveTranslate;
                if (inputMoveBoost)
                {
                    MoveTranslate = (boostedMoveSpeedAdd + moveSpeed) * Time.deltaTime * inputVector;
                }
                else
                {
                    MoveTranslate = moveSpeed * Time.deltaTime * inputVector;
                }

                // Adjust Move Speed
                if (mouseWheelAdjustSpeed)
                {
                    if (Input.mouseScrollDelta != Vector2.zero)
                    {
                        moveSpeed = Mathf.Clamp(moveSpeed + Input.mouseScrollDelta.y, 1f, float.MaxValue);
                    }
                }

                TargetTransform.Translate(MoveTranslate);
            }
            else if (toggledCursorLockState)
            {
                if (Cursor.lockState == CursorLockMode.Confined || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                toggledCursorLockState = false;
            }
        }

        private void OnEnable()
        {
            foreach (var comp in disableComponentsOnEnable)
            {
                comp.enabled = false;
            }

            if (!m_isInit)
            {
                Initilaze();
            }
        }
        private void OnDisable()
        {
            foreach (Behaviour behaviour in disableComponentsOnEnable)
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = true;
            }
        }
    }
}
