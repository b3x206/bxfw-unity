using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Input Axis for rotating the camera.
    /// </summary>
    [Flags]
    public enum CameraRotationAxes
    {
        None = 0,
        MouseX = 1 << 0,
        MouseY = 1 << 1,
    }

    /// <summary>
    /// <see cref="CharacterController"/> based player movement component.
    /// </summary>
    /// TODO 1 : Make BXFW.Modules a thing (abstract scriptable object based module system)
    /// TODO 2 : Extend from UnityEngine.CharacterController
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        /// <summary>Character controller on this class.</summary>
        public CharacterController Controller { get; private set; }

        [Header("Player Settings")]
        [SerializeField] private LayerMask Player_GroundMask;
        public float Player_Speed = 400f;
        public float Player_SpeedRun = 800f;
        public float Player_JumpSpeed = 3f;
        public float Player_RigidbodyPushPower = 1f;
        public float Player_Weight = 1f;
        public bool Player_CanMove = true;
        [Range(0f, .999f)] public float Player_TPSTurnDamp = .1f;
        // Input
        public CustomInputEvent PlayerMoveForward = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent PlayerMoveBackward = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent PlayerMoveLeft = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent PlayerMoveRight = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent PlayerMoveRun = new KeyCode[] { KeyCode.LeftShift };
        public bool ShouldMove
        {
            get
            {
                return PlayerMoveForward || PlayerMoveBackward || PlayerMoveLeft || PlayerMoveRight;
            }
        }
        public CustomInputEvent PlayerMoveJump = new KeyCode[] { KeyCode.Space };
        public CustomInputEvent PlayerMoveCrouch = new KeyCode[] { KeyCode.LeftControl };

        [Header("Player Kinematic Physics")]
        [SerializeField] private bool m_Player_UseGravity = true;
        public bool Player_CanMoveKinematic = true;
        public bool Player_UseGravity
        {
            get
            { return m_Player_UseGravity; }
            set
            {
                // m_Player_GravityVelocity = value ? m_Player_GravityVelocity : Vector3.zero;
                m_Player_UseGravity = value;
            }
        }
        /// <summary>Current gravity for the player controller.</summary>
        public Vector3 Player_Gravity = Physics.gravity;
        /// <summary>Control whether the player is in ground.</summary>
        public bool PlayerIsGrounded { get; private set; } = false;

        // -- Player Reference
        public enum PlayerPersonView { FPS = 0, TPS = 1 }
        [Header("Player Reference")]
        public PlayerPersonView Player_CurrentCameraView = PlayerPersonView.FPS;
        [Tooltip("Players camera.")]
        public Camera Player_Camera;
        [Tooltip("Transform for ground checking. This should be on the position of the legs.")]
        public Transform Player_GroundCheck;
        public float Player_GroundCheckDistance = .4f;

        /// <summary>
        /// Current variable player velocity.
        /// <br>Get : <see cref="m_Player_InternalVelocity"/> | Set : <see cref="m_Player_ExternVelocity"/></br>
        /// </summary>
        public Vector3 PlayerVelocity
        {
            get { return m_Player_InternalVelocity; }
            set { m_Player_ExternVelocity = value; }
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
        /// 2 : Apply the velocity to <see cref="m_Player_InternalVelocity"/>. (Always apply the velocity consistently).

        /// <summary>
        /// <c>[ **Special** Internal Velocity]</c> Current Player veloicty.
        /// <br><c>Note :</c> Always add <see cref="m_Player_ExternVelocity"/> to this velocity!</br>
        /// </summary>
        private Vector3 m_Player_InternalVelocity;
        /// <summary><c>[External Velocity]</c> Total velocity changed by other scripts.</summary>
        private Vector3 m_Player_ExternVelocity;
        /// <summary><c>[Internal Velocity]</c> Gravity velocity.</summary>
        private Vector3 m_Player_GravityVelocity;

#if UNITY_EDITOR
        /// <summary>
        /// Hides cursor
        /// TODO : Put this to another script.
        /// </summary>
        [SerializeField] private bool Player_EditorHideCursor = true;
#endif

        ///////////////// Function
        private void Awake()
        {
            //// Player Controller  ////
            Controller = GetComponent<CharacterController>();

            //// Variable Control  ////
            if (Player_GroundCheck == null)
                Debug.LogError("[PlayerMovement] Player ground check is null. Please assign one.");
            if (Player_Camera == null && Player_CurrentCameraView == PlayerPersonView.TPS)
                Debug.LogWarning("[PlayerMovement] Player cam is null. (Move style is relative to camera!)");

#if UNITY_EDITOR
            // Hide mouse. (TODO) Put this to other script.
            if (Player_EditorHideCursor)
            {
                Cursor.visible = !Player_EditorHideCursor;
                Cursor.lockState = Player_EditorHideCursor ? CursorLockMode.Locked : CursorLockMode.None;
            }
#endif
        }

        /////  Persistent Function Variables /////
        /// <summary>
        /// Player's current <see cref="PlayerPersonView.TPS"/> turn speed.
        /// Changes with <see cref="Mathf.SmoothDampAngle(float, float, ref float, float)"/>.
        /// </summary>
        private float m_Player_TurnSmoothVel;

        private bool JumpKeyInputQueue = false;
        private void Update()
        {
            if (PlayerMoveJump.IsKeyDown())
                JumpKeyInputQueue = true;
        }
        private void FixedUpdate()
        {
            //// Is Player Grounded? 
            PlayerIsGrounded = Physics.CheckSphere(Player_GroundCheck.position, Player_GroundCheckDistance, Player_GroundMask);
            if (!Player_CanMoveKinematic) return; // Can player move?

            //// Main Movement    ///
            Vector3 move_actualDir = PlayerMove() * Time.fixedDeltaTime;

            //// Gravity         ////
            PlayerGravity();

            //// Set velocity.   ////
            m_Player_InternalVelocity = move_actualDir + m_Player_GravityVelocity + m_Player_ExternVelocity;

            //// Jumping         ////
            if (JumpKeyInputQueue)
            {
                PlayerJump();
                JumpKeyInputQueue = false;
            }

            //// Apply Movement  ////
            Controller.Move(PlayerVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Player movement. (similar to godot's move_and_slide())
        /// </summary>
        /// <returns>Player movement vector. (NOT multiplied with <see cref="Time.deltaTime"/>)</returns>
        private Vector3 PlayerMove()
        {
            if (!Player_CanMove)
            { return Vector3.zero; }

            Vector3 move_actualDir; // Dir on return;
            float move_currentSpeed = PlayerMoveRun ? Player_SpeedRun : Player_Speed;

            float move_h = Convert.ToInt32(PlayerMoveRight) - Convert.ToInt32(PlayerMoveLeft);       // H
            float move_v = Convert.ToInt32(PlayerMoveForward) - Convert.ToInt32(PlayerMoveBackward); // V

            Vector3 move_inputDir = new Vector3(move_h, 0f, move_v).normalized; // Input (normalized)

            /// If player wants to move
            if (move_inputDir.magnitude >= 0.1f)
            {
                switch (Player_CurrentCameraView)
                {
                    case PlayerPersonView.FPS:
                        //// Just move to the forward, the camera script should rotate the player. ////
                        move_actualDir = ((transform.right * move_inputDir.x) + (transform.forward * move_inputDir.z)) * move_currentSpeed;
                        break;
                    case PlayerPersonView.TPS:
                        //// Rotation relative to camera ////
                        // Apply some maths that i don't even know what it means (i do know it but partially)
                        float move_targetAngle = (Mathf.Atan2(move_inputDir.x, move_inputDir.z) * Mathf.Rad2Deg) + Player_Camera.transform.eulerAngles.y;
                        // move_angle allows smooth movement
                        float move_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, move_targetAngle, ref m_Player_TurnSmoothVel, Player_TPSTurnDamp);
                        // Apply damped rotation.
                        transform.rotation = Quaternion.Euler(0f, move_angle, 0f);

                        //// Movement (Relative to character pos and rot) ////
                        // Add camera affected movement vector to the movement vec.
                        move_actualDir = (Quaternion.Euler(0f, move_targetAngle, 0f) * Vector3.forward).normalized * move_currentSpeed;
                        break;

                    // No move dir?
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
        /// Applies the velocity to <see cref="m_Player_GravityVelocity"/>.
        /// </summary>
        private void PlayerGravity()
        {
            // -- No gravity
            if (!m_Player_UseGravity)
            {
                m_Player_GravityVelocity = Vector3.zero;
                return;
            }

            // -- Has gravity
            // Apply gravity (Player isn't grounded)
            if (!PlayerIsGrounded)
            {
                m_Player_GravityVelocity += Player_Gravity * Time.deltaTime;
            }
            // Quick workaround for player clipping.
            // If player is grounded but the player still has falling velocity.
            // TODO : Make this independent of gravity axis (not important but nice to have)
            if (PlayerIsGrounded && m_Player_GravityVelocity.y <= 0f && m_Player_InternalVelocity.y <= 0f)
            {
                m_Player_GravityVelocity.y = DEFAULT_GROUNDED_GRAVITY;
            }
        }

        /// <summary>
        /// Makes the player jump.
        /// </summary>
        private void PlayerJump()
        {
            // Don't jump if not in ground.
            if (!PlayerIsGrounded) return;

            /// The '2f' added to this is required as <see cref="PlayerGravity()"/> function sets player gravity to -2f always.
            //m_Player_GravityVelocity.y += Mathf.Sqrt(Player_JumpSpeed * -2f * Player_Gravity.y) + 2f;
            m_Player_GravityVelocity.y += Player_JumpSpeed + -DEFAULT_GROUNDED_GRAVITY;
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (Player_RigidbodyPushPower <= 0f)
                return;

            // Push rigidbodies 
            var rb = hit.rigidbody;
            Vector3 force;

            if (rb == null || rb.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3f)
            {
                // Gravity push
                force = Vector3.Scale(new Vector3(0.5f, 0.5f, 0.5f), m_Player_GravityVelocity) * Player_Weight;
            }
            else
            {
                // Normal push
                force = hit.controller.velocity * Player_RigidbodyPushPower;
            }

            rb.AddForceAtPosition(force, hit.point);
        }

#if UNITY_EDITOR
        // Shows interaction bounding box. (incorrectly at least)
        private void OnDrawGizmos()
        {
            if (Player_GroundCheck == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Player_GroundCheck.position, Player_GroundCheckDistance);
            Gizmos.color = Color.white;
        }
#endif
    }
}
