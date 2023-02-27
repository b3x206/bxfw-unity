using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Input Axis for input related things.
    /// </summary>
    [Flags]
    public enum InputAxis
    {
        None = 0,         // 0
        MouseX = 1 << 0,  // 1
        MouseY = 1 << 1,  // 2
    }

    /// <summary>
    /// <see cref="CharacterController"/> based player movement component.
    /// </summary>
    [RequireComponent(typeof(CharacterController)), DisallowMultipleComponent]
    public sealed class PlayerMovement : MonoBehaviour
    {
        /// <summary>Character controller on this class.</summary>
        public CharacterController Controller { get; private set; }

        [Header("Primary Settings")]
        public bool canMove = true;
        public float speed = 200f;
        public float runSpeed = 300f;
        public float jumpSpeed = 5f;
        public float rbPushPower = 1f;
        public float rbWeight = 1f;
        [Range(0f, .999f)] public float TPS_tsRotateDamp = .1f;
        
        [InspectorLine(.4f, .4f, .4f), Header("Input")]
        public bool canInputMove = true;
        public CustomInputEvent moveForwardInput  = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent moveBackwardInput = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent moveLeftInput     = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent moveRightInput    = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent moveRunInput      = new KeyCode[] { KeyCode.LeftShift };
        public CustomInputEvent moveJumpInput     = new KeyCode[] { KeyCode.Space };
        public CustomInputEvent moveCrouchInput   = new KeyCode[] { KeyCode.LeftControl };
        /// <summary>
        /// Returns whether if the any of the 'move' input events is being done.
        /// <br>Excluding <see cref="moveRunInput"/>, as that sets a toggle.</br>
        /// </summary>
        public bool MoveInputIsPressed
        {
            get
            {
                return moveForwardInput || moveBackwardInput || moveLeftInput || moveRightInput;
            }
        }
        /// <summary>
        /// Returns =&gt; <c><see cref="canMove"/> &amp;&amp; <see cref="MoveInputIsPressed"/></c>
        /// </summary>
        public bool MustMove
        {
            get
            {
                return canMove && MoveInputIsPressed;
            }
        }

        [InspectorLine(.4f, .4f, .4f), Header("Player Kinematic Physics")]
        [SerializeField] private bool useGravity = true;
        public bool UseGravity
        {
            get { return useGravity; }
            set { useGravity = value; }
        }
        /// <summary>
        /// Current gravity for the player controller.
        /// </summary>
        public Vector3 gravity = Physics.gravity;
        /// <summary>
        /// The collision mask to check what is considered ground.
        /// </summary>
        public LayerMask groundMask;
        /// <summary>
        /// Check whether the player is in ground.
        /// </summary>
        public bool IsGrounded { get; private set; } = false;

        // -- Player Reference
        public enum PlayerViewType { FPS, TPS, Free, FreeRelativeCam }
        [InspectorLine(.4f, .4f, .4f), Header("Player Reference")]
        public PlayerViewType currentCameraView = PlayerViewType.FPS;
        [Tooltip("Players camera.")]
        public Camera targetCamera;
        [Tooltip("Transform for ground checking. This should be on the position of the legs.")]
        public Transform groundCheckTransform;
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
        private Vector3 mWishDir;
        /// <summary>
        /// Direction that the input wishes to move.
        /// <br>Set in <see cref="PlayerMove(Vector2)"/>.</br>
        /// </summary>
        public Vector3 WishDir
        {
            get { return mWishDir; }
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
        /// 2 : Apply the velocity to <see cref="m_internalVelocity"/>.

        /// <summary>The internal velocity, applied to the actual movement.</summary>
        [SerializeField, InspectorReadOnlyView] private Vector3 m_internalVelocity;
        /// <summary><c>[External Velocity]</c> Total velocity changed by other scripts.</summary>
        [SerializeField, InspectorReadOnlyView] private Vector3 m_externVelocity;
        /// <summary><c>[Internal Velocity]</c> Gravity velocity.</summary>
        [SerializeField, InspectorReadOnlyView] private Vector3 m_gravityVelocity;

        ///////////////// Function
        private void Awake()
        {
            //// Player Controller  ////
            Controller = GetComponent<CharacterController>();
        }
        private void Start()
        {
            //// Variable Control  ////
            if (groundCheckTransform == null)
                Debug.LogError("[PlayerMovement] Player ground check is null. Please assign one.");
            if (targetCamera == null && currentCameraView == PlayerViewType.TPS)
                Debug.LogWarning(string.Format("[PlayerMovement] Player cam is null. (Move style [{0}] is relative to camera!)", currentCameraView));
        }

        /////  Persistent Function Variables /////
        /// <summary>
        /// Player's current <see cref="PlayerViewType.TPS"/> turn speed / velocity?
        /// Changes with <see cref="Mathf.SmoothDampAngle(float, float, ref float, float)"/>.
        /// </summary>
        private float m_TPSRotateV;

        // TODO : Fix 'CustomInputEvent' polling.
        //private void Update()
        //{
        //    moveForwardInput.Poll();
        //    moveBackwardInput.Poll();
        //    moveLeftInput.Poll();
        //    moveRightInput.Poll();
        //    moveCrouchInput.Poll();
        //    moveJumpInput.Poll();
        //    moveRunInput.Poll();
        //}
        private void FixedUpdate()
        {
            //// Is Player Grounded? 
            IsGrounded = useGravity && Physics.CheckSphere(groundCheckTransform.position, groundCheckDistance, groundMask);

            //// Main Movement    ///
            Vector3 inputVelocity = canInputMove ? PlayerMove(new Vector2(
                Convert.ToInt32(moveRightInput) - Convert.ToInt32(moveLeftInput),      // h
                Convert.ToInt32(moveForwardInput) - Convert.ToInt32(moveBackwardInput) // v
            )) * Time.fixedDeltaTime : Vector3.zero;

            //// Gravity         ////
            PlayerGravity();

            //// Set velocity.   ////
            m_internalVelocity = inputVelocity + m_gravityVelocity + m_externVelocity;

            //// Jumping         ////
            if (moveJumpInput.IsKeyDown())
            {
                PlayerJump();
            }

            //// Apply Movement  ////
            Controller.Move(Velocity * Time.deltaTime);
        }

        /// <summary>
        /// Player movement. Returns relative movement depending on the settings.
        /// </summary>
        /// <returns>Player movement vector. (NOT multiplied with <see cref="Time.deltaTime"/>)</returns>
        public Vector3 PlayerMove(Vector2 input)
        {
            if (!canMove)
                return Vector3.zero;

            Vector3 move_actualDir; // Dir on return;
            float move_currentSpeed = moveRunInput ? runSpeed : speed;

            float move_h = input.x;
            float move_v = input.y;

            Vector3 move_inputDir = new Vector3(move_h, 0f, move_v).normalized;

            /// If player wants to move
            if (move_inputDir.sqrMagnitude >= 0.1f)
            {
                switch (currentCameraView)
                {
                    case PlayerViewType.FPS:
                        //// Just move to the forward, assume the camera script rotating the player. ////
                        move_actualDir = ((transform.right * move_inputDir.x) + (transform.forward * move_inputDir.z)).normalized;
                        break;
                    case PlayerViewType.TPS:
                        {
                            //// Rotation relative to camera ////
                            // Get target angle, according to the camera's direction and the player movement INPUT direction
                            float move_targetAngle = (Mathf.Atan2(move_inputDir.x, move_inputDir.z) * Mathf.Rad2Deg) + targetCamera.transform.eulerAngles.y;
                            // Interpolate the current angle
                            float move_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, move_targetAngle, ref m_TPSRotateV, TPS_tsRotateDamp);
                            // Apply damped rotation.
                            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, move_angle, transform.rotation.eulerAngles.z);

                            //// Movement (Relative to character pos and rot) ////
                            // Add camera affected movement vector to the movement vec.
                            move_actualDir = (Quaternion.Euler(0f, move_targetAngle, 0f) * Vector3.forward).normalized;
                        }
                        break;

                    case PlayerViewType.FreeRelativeCam:
                        {
                            // Do TPS movement without rotating the player.
                            float move_targetAngle = (Mathf.Atan2(move_inputDir.x, move_inputDir.z) * Mathf.Rad2Deg) + targetCamera.transform.eulerAngles.y;
                            move_actualDir = (Quaternion.Euler(0f, move_targetAngle, 0f) * Vector3.forward).normalized;
                        }
                        break;

                    default:
                    case PlayerViewType.Free:
                        move_actualDir = move_inputDir;
                        break;
                }
            }
            else
            {
                // Return no movement.
                move_actualDir = Vector3.zero;
            }

            mWishDir = move_actualDir;
            return move_actualDir * move_currentSpeed;
        }

        private const float DEFAULT_GROUNDED_GRAVITY = -2f;
        /// <summary>
        /// Player gravity.
        /// Applies the velocity to <see cref="m_gravityVelocity"/>.
        /// </summary>
        private void PlayerGravity()
        {
            // -- No gravity
            if (!useGravity)
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
            if (IsGrounded && m_gravityVelocity.GetBiggestAxis() <= 0f && m_internalVelocity.GetBiggestAxis() <= 0f)
            {
                m_gravityVelocity = -gravity.normalized * DEFAULT_GROUNDED_GRAVITY;
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
            m_gravityVelocity = -gravity.normalized * (jumpSpeed + DEFAULT_GROUNDED_GRAVITY);
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (rbPushPower <= 0f)
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
                force = hit.controller.velocity * rbPushPower;
            }

            rb.AddForceAtPosition(force, hit.point);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (groundCheckTransform == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckDistance);
            Gizmos.color = Color.white;
        }
#endif
    }
}
