using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Mouse inputting axis flags.
    /// </summary>
    [Flags]
    public enum MouseInputAxis
    {
        None = 0,         // 0
        MouseX = 1 << 0,  // 1
        MouseY = 1 << 1,  // 2
    }

    /// <summary>
    /// <see cref="CharacterController"/> based player movement component.
    /// <br>Can be used with any script that can drive this class but is designed to be used with the main player interacting with the game.</br>
    /// </summary>
    [RequireComponent(typeof(CharacterController)), DisallowMultipleComponent]
    public sealed class PlayerMovement : MonoBehaviour
    {
        /// <summary>Character controller component attached on this class.</summary>
        public CharacterController Controller { get; private set; }

        [Header("Primary Settings")]
        public bool canMove = true;
        /// <summary>
        /// Speed of this <see cref="PlayerMovement"/>.
        /// </summary>
        public float speed = 200f;
        /// <summary>
        /// Running speed of this movement.
        /// </summary>
        public float runSpeed = 300f;
        public float jumpSpeed = 5f;
        /// <summary>
        /// Power applied to <see cref="Rigidbody"/>-ies interacting with this <see cref="CharacterController"/>.
        /// <br>Setting this to 0 or lower will make rigidbodies not be pushable.</br>
        /// </summary>
        public float rbPushPower = 1f;
        /// <summary>
        /// The weight used in rigidbody pushing calculation.
        /// <br>Has no effect if <see cref="rbPushPower"/> &lt;= 0</br>
        /// </summary>
        public float rbWeight = 1f;
        /// <summary>
        /// Rotation dampening for the <see cref="CamViewType.TPS"/>.
        /// </summary>
        [Range(0f, .999f)] public float tpsCamRotateDamp = .1f;
        
        /// <summary>
        /// Whether to use the internal inputing system to move.
        /// <br>Setting this false will disable and will require your own input implementation.</br>
        /// </summary>
        [InspectorLine(.4f, .4f, .4f), Header("Input")]
        public bool useInternalInputMove = true;
        public CustomInputEvent moveForwardInput  = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent moveBackwardInput = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        public CustomInputEvent moveLeftInput     = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent moveRightInput    = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent moveRunInput      = new KeyCode[] { KeyCode.LeftShift };
        public CustomInputEvent moveJumpInput     = new KeyCode[] { KeyCode.Space };
        public CustomInputEvent moveCrouchInput   = new KeyCode[] { KeyCode.LeftControl };
        /// <summary>
        /// Given external input movement to the player.
        /// <br>Can be used to give the <see cref="PlayerMovement"/> scripted movement or attaching your own input implementation.</br>
        /// </summary>
        [HideInInspector, NonSerialized] public Vector2 moveInput;
        /// <summary>
        /// Given external running state to the player.
        /// </summary>
        [HideInInspector, NonSerialized] public bool runInput = false;
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
                return canMove && (MoveInputIsPressed || (!useInternalInputMove && moveInput != Vector2.zero));
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
        /// <summary>
        /// The type of viewing relative to the movement.
        /// </summary>
        public enum CamViewType { FPS, TPS, Free, FreeRelativeCam }
        [InspectorLine(.4f, .4f, .4f), Header("Player Reference")]
        public CamViewType currentCameraView = CamViewType.FPS;
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
        /// <summary>
        /// This 'wishdir' is set to input in <see cref="PlayerMove(Vector2)"/>.
        /// </summary>
        private Vector3 m_WishDir;
        /// <summary>
        /// Direction that the input wishes to move.
        /// <br>Set in <see cref="PlayerMove(Vector2)"/>.</br>
        /// </summary>
        public Vector3 WishDir
        {
            get { return m_WishDir; }
        }

        /// NOTE :
        /// ******** Adding velocity variable rules ********
        /// 1 : Create a private or public variable
        ///     Naming scheme :
        ///         m_             ]--> Used for private fields.
        ///         [velocityName] ]--> Name of the velocity (in camelCase).
        /// 2 : Apply the velocity to <see cref="m_internalVelocity"/>.
        /// <summary>The internal velocity, applied to the actual movement.</summary>
        [SerializeField, ReadOnlyView] private Vector3 m_internalVelocity;
        /// <summary><c>[External Velocity]</c> Total velocity changed by other scripts.</summary>
        [SerializeField, ReadOnlyView] private Vector3 m_externVelocity;
        /// <summary><c>[Internal Velocity]</c> Gravity velocity.</summary>
        [SerializeField, ReadOnlyView] private Vector3 m_gravityVelocity;

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
            {
                Debug.LogError("[PlayerMovement] Player ground check is null. Please assign one.");
            }

            if (targetCamera == null && currentCameraView == CamViewType.TPS)
            {
                Debug.LogWarning(string.Format("[PlayerMovement] Player cam is null. (Move style [{0}] is relative to camera!)", currentCameraView));
            }
        }

        /////  Persistent Function Variables /////
        /// <summary>
        /// Player's current <see cref="CamViewType.TPS"/> turn speed / velocity?
        /// Changes with <see cref="Mathf.SmoothDampAngle(float, float, ref float, float)"/>.
        /// </summary>
        private float m_tpsRotateVelocity;

        private void Update()
        {
            if (!useInternalInputMove)
            {
                return;
            }

            moveForwardInput.Poll();
            moveBackwardInput.Poll();
            moveLeftInput.Poll();
            moveRightInput.Poll();
            moveCrouchInput.Poll();
            moveJumpInput.Poll();
            moveRunInput.Poll();
        }
        private void FixedUpdate()
        {
            // Don't move.
            if (!canMove)
            {
                return;
            }

            //// Is Player Grounded? 
            IsGrounded = useGravity && Physics.CheckSphere(groundCheckTransform.position, groundCheckDistance, groundMask);

            //// Main Movement    ///
            if (useInternalInputMove)
            {
                moveInput = new Vector2(
                    Convert.ToInt32(moveRightInput) - Convert.ToInt32(moveLeftInput),      // h
                    Convert.ToInt32(moveForwardInput) - Convert.ToInt32(moveBackwardInput) // v
                );
            }

            Vector3 inputVelocity = PlayerMove(moveInput) * Time.fixedDeltaTime;

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
        /// Player movement. Returns relative movement <b>depending on the settings. (! this means speed is applied !)</b>
        /// </summary>
        /// <returns>Player movement vector. (NOT multiplied with <c>deltaTime</c>)</returns>
        public Vector3 PlayerMove(Vector2 input)
        {
            if (!canMove || input == Vector2.zero)
            {
                return Vector3.zero;
            }

            Vector3 moveActualDir; // Dir on return;
            float moveCurrentSpeed = speed;
            if (useInternalInputMove)
            {
                runInput = moveRunInput;
            }

            if (runInput)
            {
                moveCurrentSpeed = runSpeed;
            }

            float moveH = input.x;
            float moveV = input.y;

            Vector3 moveInputDir = new Vector3(moveH, 0f, moveV).normalized;

            /// If player wants to move
            if (moveInputDir.sqrMagnitude >= 0.1f)
            {
                switch (currentCameraView)
                {
                    case CamViewType.FPS:
                        //// Just move to the forward, assume the camera script rotating the player. ////
                        moveActualDir = ((transform.right * moveInputDir.x) + (transform.forward * moveInputDir.z)).normalized;
                        break;
                    case CamViewType.TPS:
                        {
                            //// Rotation relative to camera ////
                            // Get target angle, according to the camera's direction and the player movement INPUT direction
                            float moveTargetAngle = (Mathf.Atan2(moveInputDir.x, moveInputDir.z) * Mathf.Rad2Deg) + targetCamera.transform.eulerAngles.y;
                            // Interpolate the current angle
                            float moveInterpAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, moveTargetAngle, ref m_tpsRotateVelocity, tpsCamRotateDamp);
                            // Apply damped rotation.
                            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, moveInterpAngle, transform.rotation.eulerAngles.z);

                            //// Movement (Relative to character pos and rot) ////
                            // Add camera affected movement vector to the movement vec.
                            moveActualDir = (Quaternion.Euler(0f, moveTargetAngle, 0f) * Vector3.forward).normalized;
                        }
                        break;

                    case CamViewType.FreeRelativeCam:
                        {
                            // Do TPS movement input without rotating the player.
                            float moveTargetAngle = (Mathf.Atan2(moveInputDir.x, moveInputDir.z) * Mathf.Rad2Deg) + targetCamera.transform.eulerAngles.y;
                            moveActualDir = (Quaternion.Euler(0f, moveTargetAngle, 0f) * Vector3.forward).normalized;
                        }
                        break;

                    default:
                    case CamViewType.Free:
                        moveActualDir = moveInputDir;
                        break;
                }
            }
            else
            {
                // Return no movement.
                moveActualDir = Vector3.zero;
            }

            m_WishDir = moveActualDir;
            return moveActualDir * moveCurrentSpeed;
        }

        private const float DefaultGroundedGravity = -2f;
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
            if (IsGrounded && m_gravityVelocity.MaxAxis() <= 0f && m_internalVelocity.MaxAxis() <= 0f)
            {
                m_gravityVelocity = -gravity.normalized * DefaultGroundedGravity;
            }
        }

        /// <summary>
        /// Makes the player jump.
        /// </summary>
        public void PlayerJump()
        {
            // Don't jump if not in ground.
            if (!IsGrounded)
            {
                return;
            }

            /// The '2f' added to this is required as <see cref="PlayerGravity()"/> function sets player gravity to -2f always.
            //m_gravityVelocity.y += Mathf.Sqrt(Player_JumpSpeed * -2f * Player_Gravity.y) + 2f;
            m_gravityVelocity = -gravity.normalized * (jumpSpeed + DefaultGroundedGravity);
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (rbPushPower <= 0f)
            {
                return;
            }

            // Push rigidbodies 
            Rigidbody rb = hit.rigidbody;
            Vector3 force;

            if (rb == null || rb.isKinematic)
            {
                return;
            }

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
            if (groundCheckTransform == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckDistance);
            Gizmos.color = Color.white;
        }
#endif
    }
}
