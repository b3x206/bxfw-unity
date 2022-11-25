﻿using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Input Axis for rotating the camera.
    /// </summary>
    [Flags]
    public enum CameraRotationAxes
    {
        None = 0,         // 0
        MouseX = 1 << 0,  // 1
        MouseY = 1 << 1,  // 2
        // MouseX | MouseY : 3
    }

    /// <summary>
    /// <see cref="CharacterController"/> based player movement component.
    /// </summary>
    /// TODO 1 : Make BXFW.Modules a thing (abstract scriptable object based module system, allowing custom variables + behaviour)
    /// TODO 2 : Extend from UnityEngine.CharacterController
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        /// <summary>Character controller on this class.</summary>
        public CharacterController Controller { get; private set; }

        [Header("Player Settings")]
        [SerializeField] private LayerMask Player_GroundMask;
        public float speed = 400f;
        public float runSpeed = 800f;
        public float jumpSpeed = 3f;
        public float rigidBodyPushPower = 1f;
        public float rbWeight = 1f;
        public bool canMove = true;
        [Range(0f, .999f)] public float TPS_tsRotateDamp = .1f;
        // Input
        public CustomInputEvent moveForwardInput  = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent moveBackwardInput = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent moveLeftInput     = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent moveRightInput    = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent moveRunInput      = new KeyCode[] { KeyCode.LeftShift };
        /// <summary>
        /// Returns whether if the any of the 'move' input events is being done.
        /// <br>Excluding <see cref="moveRunInput"/>, as that sets a toggle.</br>
        /// </summary>
        public bool ShouldMove
        {
            get
            {
                return moveForwardInput || moveBackwardInput || moveLeftInput || moveRightInput;
            }
        }
        public CustomInputEvent moveJumpInput   = new KeyCode[] { KeyCode.Space };
        public CustomInputEvent moveCrouchInput = new KeyCode[] { KeyCode.LeftControl };

        [Header("Player Kinematic Physics")]
        [SerializeField] private bool m_UseGravity = true;
        [Tooltip("Controls whether if the player can move kinematically. (User input)")]
        public bool canMoveKinematic = true;
        public bool UseGravity
        {
            get
            { return m_UseGravity; }
            set
            {
                // m_Player_GravityVelocity = value ? m_Player_GravityVelocity : Vector3.zero;
                m_UseGravity = value;
            }
        }
        /// <summary>Current gravity for the player controller.</summary>
        public Vector3 gravity = Physics.gravity;
        /// <summary>Control whether the player is in ground.</summary>
        public bool IsGrounded { get; private set; } = false;

        // -- Player Reference
        public enum PlayerViewType { FPS, TPS }
        [Header("Player Reference")]
        public PlayerViewType currentCameraView = PlayerViewType.FPS;
        [Tooltip("Players camera.")]
        public Camera targetCamera;
        [Tooltip("Transform for ground checking. This should be on the position of the legs.")]
        public Transform groundCheckTs;
        public float groundCheckDistance = .4f;

        /// <summary>
        /// Current variable player velocity.
        /// <br>Get : <see cref="m_internalVelocity"/> | Set : <see cref="m_externVelocity"/></br>
        /// </summary>
        public Vector3 Velocity
        {
            get { return m_internalVelocity; }
            set { m_externVelocity = value; }
        }

        /////////// ------------
        /////////// Private Vars
        /// NOTE :
        /// ******** Adding velocity variable rules ********
        /// 1 : Create a private or public variable
        ///     Naming scheme :
        ///         m_             ]--> Used for private variables.
        ///         Player_        ]--> Implies that this is a variable on a player
        ///         [VelocityName] ]--> Name of the velocity.
        /// 2 : Apply the velocity to <see cref="m_internalVelocity"/>. (Always apply the velocity consistently).

        /// <summary>
        /// <c>[ **Special** Internal Velocity]</c> Current Player veloicty.
        /// <br><c>Note :</c> Always add <see cref="m_externVelocity"/> to this velocity!</br>
        /// </summary>
        private Vector3 m_internalVelocity;
        /// <summary><c>[External Velocity]</c> Total velocity changed by other scripts.</summary>
        private Vector3 m_externVelocity;
        /// <summary><c>[Internal Velocity]</c> Gravity velocity.</summary>
        private Vector3 m_gravityVelocity;

        ///////////////// Function
        private void Awake()
        {
            //// Player Controller  ////
            Controller = GetComponent<CharacterController>();

            //// Variable Control  ////
            if (groundCheckTs == null)
                Debug.LogError("[PlayerMovement] Player ground check is null. Please assign one.");
            if (targetCamera == null && currentCameraView == PlayerViewType.TPS)
                Debug.LogWarning($"[PlayerMovement] Player cam is null. (Move style [{currentCameraView}] is relative to camera!)");
        }

        /////  Persistent Function Variables /////
        /// <summary>
        /// Player's current <see cref="PlayerViewType.TPS"/> turn speed / velocity?
        /// Changes with <see cref="Mathf.SmoothDampAngle(float, float, ref float, float)"/>.
        /// </summary>
        private float m_TPSRotateV;

        // TODO 3 : Create a player input method, that polls all the inputs in Update
        private bool jumpKeyInputQueue = false;
        private void Update()
        {
            if (moveJumpInput.IsKeyDown())
                jumpKeyInputQueue = true;
        }
        private void FixedUpdate()
        {
            //// Is Player Grounded? 
            IsGrounded = Physics.CheckSphere(groundCheckTs.position, groundCheckDistance, Player_GroundMask);
            if (!canMoveKinematic) return; // Can player move?

            //// Main Movement    ///
            Vector3 move_actualDir = PlayerMove() * Time.fixedDeltaTime;

            //// Gravity         ////
            PlayerGravity();

            //// Set velocity.   ////
            m_internalVelocity = move_actualDir + m_gravityVelocity + m_externVelocity;

            //// Jumping         ////
            if (jumpKeyInputQueue)
            {
                PlayerJump();
                jumpKeyInputQueue = false;
            }

            //// Apply Movement  ////
            Controller.Move(Velocity * Time.deltaTime);
        }

        /// <summary>
        /// Player movement. (similar to godot's move_and_slide())
        /// </summary>
        /// <returns>Player movement vector. (NOT multiplied with <see cref="Time.deltaTime"/>)</returns>
        private Vector3 PlayerMove()
        {
            if (!canMove)
            { return Vector3.zero; }

            Vector3 move_actualDir; // Dir on return;
            float move_currentSpeed = moveRunInput ? runSpeed : speed;

            float move_h = Convert.ToInt32(moveRightInput) - Convert.ToInt32(moveLeftInput);       // H
            float move_v = Convert.ToInt32(moveForwardInput) - Convert.ToInt32(moveBackwardInput); // V

            Vector3 move_inputDir = new Vector3(move_h, 0f, move_v).normalized; // Input (normalized)

            /// If player wants to move
            if (move_inputDir.sqrMagnitude >= 0.1f)
            {
                switch (currentCameraView)
                {
                    case PlayerViewType.FPS:
                        //// Just move to the forward, assume the camera script rotating the player. ////
                        move_actualDir = ((transform.right * move_inputDir.x) + (transform.forward * move_inputDir.z)) * move_currentSpeed;
                        break;
                    case PlayerViewType.TPS:
                        //// Rotation relative to camera ////
                        // Get target angle, according to the camera's direction and the player movement INPUT direction
                        float move_targetAngle = (Mathf.Atan2(move_inputDir.x, move_inputDir.z) * Mathf.Rad2Deg) + targetCamera.transform.eulerAngles.y;
                        // Interpolate the current angle
                        float move_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, move_targetAngle, ref m_TPSRotateV, TPS_tsRotateDamp);
                        // Apply damped rotation.
                        // TODO 4 : Preserve previous rotation + rotate in transform.up direction
                        transform.rotation = Quaternion.Euler(0f, move_angle, 0f);

                        //// Movement (Relative to character pos and rot) ////
                        // Add camera affected movement vector to the movement vec.
                        move_actualDir = (Quaternion.Euler(0f, move_targetAngle, 0f) * Vector3.forward).normalized * move_currentSpeed;
                        break;

                    // No direction,
                    default:
                        move_actualDir = Vector3.zero;
                        break;
                }
            }
            else
            {
                // Return no movement.
                move_actualDir = Vector3.zero;
            }

            return move_actualDir;
        }

        private const float DEFAULT_GROUNDED_GRAVITY = -2f;
        /// <summary>
        /// Player gravity.
        /// Applies the velocity to <see cref="m_gravityVelocity"/>.
        /// </summary>
        private void PlayerGravity()
        {
            // -- No gravity
            if (!m_UseGravity)
            {
                m_gravityVelocity = Vector3.zero;
                return;
            }

            // -- Has gravity
            // Apply gravity (Player isn't grounded)
            if (!IsGrounded)
            {
                m_gravityVelocity += gravity * Time.deltaTime;
            }
            // Quick workaround for player clipping.
            // If player is grounded but the player still has falling velocity.
            // TODO : Make this independent of gravity axis (not important but nice to have)
            if (IsGrounded && m_gravityVelocity.y <= 0f && m_internalVelocity.y <= 0f)
            {
                m_gravityVelocity.y = DEFAULT_GROUNDED_GRAVITY;
            }
        }

        /// <summary>
        /// Makes the player jump.
        /// </summary>
        private void PlayerJump()
        {
            // Don't jump if not in ground.
            if (!IsGrounded) return;

            /// The '2f' added to this is required as <see cref="PlayerGravity()"/> function sets player gravity to -2f always.
            //m_gravityVelocity.y += Mathf.Sqrt(Player_JumpSpeed * -2f * Player_Gravity.y) + 2f;
            m_gravityVelocity.y += jumpSpeed + -DEFAULT_GROUNDED_GRAVITY;
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (rigidBodyPushPower <= 0f)
                return;

            // Push rigidbodies 
            var rb = hit.rigidbody;
            Vector3 force;

            if (rb == null || rb.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3f)
            {
                // Gravity push
                force = Vector3.Scale(new Vector3(0.5f, 0.5f, 0.5f), m_gravityVelocity) * rbWeight;
            }
            else
            {
                // Normal push
                force = hit.controller.velocity * rigidBodyPushPower;
            }

            rb.AddForceAtPosition(force, hit.point);
        }

#if UNITY_EDITOR
        // Shows interaction bounding box. (incorrectly at least)
        private void OnDrawGizmos()
        {
            if (groundCheckTs == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTs.position, groundCheckDistance);
            Gizmos.color = Color.white;
        }
#endif
    }
}