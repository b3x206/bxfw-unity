using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Character controller base for anything that wants to move in 2 dimensions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterControl2DBase : MonoBehaviour
    {
        [Header("Movement Base Settings")]
        public float moveSpeed = 5f;
        public float jumpSpeed = 5f;

        [SerializeField] private bool canJump = false;
        /// <summary>
        /// Value whether if we can jump.
        /// <br>Note : Always <see langword="false"/> if <see cref="moveAxis"/> is set to <see cref="TransformAxis2D.XYAxis"/>.</br>
        /// </summary>
        public bool CanJump
        {
            get { return canJump && moveAxis != TransformAxis2D.XYAxis; }
            set { canJump = value; }
        }
        /// <summary>
        /// Axis that we are allowed to move.
        /// </summary>
        public TransformAxis2D moveAxis = TransformAxis2D.XAxis;
        /// <summary>
        /// Axis to apply the jump force to.
        /// </summary>
        public TransformAxis2D jumpAxis = TransformAxis2D.YAxis;

        [Header("Kinematic Physics Settings")]
        public Vector2 gravity = new Vector2(0f, -9.81f);
        public float mass = 1f;
        public bool UseGravity => useGravity;
        public void SetUseGravity(bool value, bool resetGravity = false)
        {
            useGravity = value;

            if (resetGravity)
                GravityVelocity = Vector3.zero;
        }
        [SerializeField] private bool useGravity = true; // Editor script TODO : useGravity hides these properties.
        // Ground checking
        public Transform groundCheck;
        private const int DEFAULT_LAYER_MASK = 1 << 0;
        public LayerMask groundLayer = DEFAULT_LAYER_MASK;
        public float groundCheckRadius = .4f;

        [Header("Movement Base Input Map")]
        // Editor script TODO : Hide / Show depending on the move/jump axis.
        public CustomInputEvent MoveLeftInput = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent MoveRightInput = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent MoveUpInput = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent MoveDownInput = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        // Hidden if moveAxis is X and Y.
        public CustomInputEvent MoveJumpInput = new KeyCode[] { KeyCode.Space };

        // Runtime variables
        public bool IsJumping { get; private set; }
        public bool IsGrounded
        {
            get { return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); }
        }
        /// <summary>
        /// Whether if we are moving at all. (This considers all type of movement)
        /// </summary>
        public bool IsMoving 
        { 
            get { return Velocity != Vector2.zero; }
        }
        /// <summary>
        /// Whether if we are moving using input.
        /// <br>Set by <see cref="GetMoveVelocity"/>.</br>
        /// </summary>
        public bool IsMovingInput { get; private set; }
        public Vector2 Velocity
        {
            get
            {
                return m_totalVelocity;
            }
            set
            {
                ExternVelocity = value;
            }
        }
        /// <summary>
        /// Total applied velocity.
        /// <br>Returns the all of the currently applied velocities.</br>
        /// </summary>
        private Vector2 m_totalVelocity;
        /// <summary>
        /// Externally applied velocity. (i.e <see cref="Velocity"/>'s setter setts this value.)
        /// </summary>
        public Vector2 ExternVelocity { get; private set; }
        /// <summary>
        /// Gravity velocity. (gravity velocity = gravity const * time.deltatime^2)
        /// </summary>
        public Vector2 GravityVelocity { get; private set; }
        /// <summary>
        /// Current move velocity of the character.
        /// </summary>
        public Vector2 MoveVelocity { get; private set; }
        public Rigidbody2D CharacterRB
        {
            get
            {
                if (currentRB == null)
                    currentRB = GetComponent<Rigidbody2D>();

                // Apply / Verify Rigidbody settings.
                //currentRB.isKinematic = true;
                // Cheat the 'Kinematic' collision thing by using a dynamic rb instead.
                currentRB.bodyType = RigidbodyType2D.Dynamic;
                currentRB.mass = 0.0001f;
                currentRB.drag = 0f;
                currentRB.angularDrag = 0f;
                currentRB.gravityScale = 0f;
                currentRB.sleepMode = RigidbodySleepMode2D.NeverSleep;

                return currentRB;
            }
        }
        public Collider2D CharacterCollider
        {
            get
            {
                if (currentCollider == null)
                    currentCollider = GetComponent<Collider2D>();

                return currentCollider;
            }
        }
        // Internal values
        [SerializeField, HideInInspector] private Rigidbody2D currentRB; // Do not use this rigidbody.
        [SerializeField, HideInInspector] private Collider2D currentCollider; // Do not use this collider.

        private void Awake()
        {
            // If current ground layer is still the default layer.
            if (groundLayer == DEFAULT_LAYER_MASK)
            {
                Debug.LogWarning($"[CharacterControl2DBase::Awake] Warning : The '{nameof(groundLayer)}' is set to the default layer. Please make a ground layer.");
            }
        }

        private Vector3 previousPosition;
        private void FixedUpdate()
        {
            previousPosition = CharacterRB.position;

            TickInternalTime();

            ApplyGravityVelocity();
            ApplyJumpForce();
            ApplyMoveVelocity();

            // Apply velocity, move kinematically and handle collision.
            m_totalVelocity = (ExternVelocity + GravityVelocity + MoveVelocity) * Time.fixedDeltaTime;
            CharacterRB.MovePosition(CharacterRB.position + m_totalVelocity);
        }

        private float jumpTimer = 0f;
        private float jumpDelay = .2f;
        /// <summary>
        /// Updates the timer-based events.
        /// </summary>
        private void TickInternalTime()
        {
            // -- Jump Delay
            if (IsJumping)
            {
                jumpTimer += Time.fixedDeltaTime;
            
                if (jumpTimer >= jumpDelay)
                {
                    jumpTimer = 0f;
                    // Start applying gravity if we are not jumping currently.
                    IsJumping = false;
                }
            }
            else
            {
                jumpTimer = 0f;
            }

            // -- Other
        }

        /// <summary>
        /// Applies gravity velocity <c>if <see cref="useGravity"/> is enabled.</c>
        /// </summary>
        protected void ApplyGravityVelocity()
        {
            if (!useGravity) return;
            // Do not apply gravity if we are grounded.
            if (IsGrounded || IsJumping)
            {
                // Set velocity to zero as we fell on the ground.
                // TODO : This will have issues with clipping on higher gravity velocities
                GravityVelocity = Vector2.zero;
                return;
            }

            GravityVelocity += Time.fixedDeltaTime * gravity;
        }
        /// <summary>
        /// Applies movement velocity <c>if player wants to move.</c>
        /// </summary>
        protected void ApplyMoveVelocity()
        {
            var moveDir = Vector2.zero;
            
            if (MoveLeftInput)
            {
                moveDir.x -= 1f;
            }
            if (MoveRightInput)
            {
                moveDir.x += 1f;
            }

            IsMovingInput = moveDir != Vector2.zero;

            MoveVelocity = moveDir * moveSpeed;
        }
        /// <summary>
        /// Applies jumping force <c>if the player wants to jump.</c>
        /// </summary>
        protected void ApplyJumpForce()
        {
            if (!canJump) return;
            if (!MoveJumpInput.IsKeyDown()) return;

            // Zero out the gravity velocity and add the jump force.
            IsJumping = true;
            GravityVelocity = jumpAxis.GetVectorUsingTransformAxis(new Vector2(jumpSpeed, jumpSpeed));
        }

        /// <summary>
        /// Cast the controller directly to the <see cref="Rigidbody2D"/> class.
        /// </summary>
        public static explicit operator Rigidbody2D(CharacterControl2DBase control2DBase)
        {
            return control2DBase.CharacterRB;
        }

        /// <summary>
        /// Draw velocity arrows.
        /// </summary>
        private void OnDrawGizmos()
        {
            var gColor = Gizmos.color;

            Gizmos.color = Color.green;
            Additionals.DrawArrowDebug(transform.position, Velocity);
            Gizmos.color = Color.red;
            Additionals.DrawArrowDebug(transform.position, CharacterRB.velocity);
            
            Gizmos.color = gColor;
        }
    }
}