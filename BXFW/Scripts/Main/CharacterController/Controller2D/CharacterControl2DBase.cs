using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// Note that this cheats collision detection by using an modified (no mass + no gravity etc) dynamic rigidbody.
    /// TODO : Please use an actual kinematic collision thing.
    /// <summary>
    /// Character controller base for anything that wants to move in 2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterControl2DBase : MonoBehaviour
    {
        [Header("Movement Base Settings")]
        public float moveSpeed = 5f;
        public float jumpSpeed = 5f;
        /// <summary>
        /// The amount of seconds to float if the jump button is held. 
        /// </summary>
        public float jumpAirTime = .2f; 

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
        public bool UseGravity
        {
            get
            {
                return useGravity && moveAxis != TransformAxis2D.XYAxis;
            }
        }

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
        // Both Move{Direction} CustomInputEvent is visible if moveAxis is X and Y
        // Hidden if moveAxis is only Y
        public CustomInputEvent MoveLeftInput = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent MoveRightInput = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        // Hidden if moveAxis is only X
        public CustomInputEvent MoveUpInput = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
        public CustomInputEvent MoveDownInput = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
        // Hidden if moveAxis is X and Y.
        public CustomInputEvent MoveJumpInput = new KeyCode[] { KeyCode.Space };

        // Runtime variables
        public bool IsJumping { get; private set; }
        public bool IsJumpAirTime { get; private set; }
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
                //currentRB.mass = 0.0001f;
                currentRB.drag = 0f;
                currentRB.angularDrag = 0f;
                currentRB.gravityScale = 0f;
                currentRB.sleepMode = RigidbodySleepMode2D.NeverSleep;
                currentRB.freezeRotation = true;

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
                Debug.LogWarning("[CharacterControl2DBase::Awake] Warning : The 'groundLayer' variable is set to the default layer. Please make a ground layer.");
            }
        }

        private void FixedUpdate()
        {
            TickInternalTime();

            ApplyGravityVelocity();
            ApplyJumpForce();
            ApplyMoveVelocity();

            // Apply velocity, move kinematically and handle collision.
            m_totalVelocity = (ExternVelocity + GravityVelocity + MoveVelocity) * Time.fixedDeltaTime;
            CharacterRB.MovePosition(CharacterRB.position + m_totalVelocity);
        }

        private float jumpTimer = 0f;
        private const float gravityDelayAfterJump = .16f;
        /// <summary>
        /// Updates the timer-based events.
        /// </summary>
        private void TickInternalTime()
        {
            // -- Jump Delay
            if (IsJumping)
            {
                jumpTimer += Time.fixedDeltaTime;
            
                if (jumpTimer >= gravityDelayAfterJump)
                {
                    jumpTimer = 0f;
                    // Start applying gravity if we are not jumping currently.
                    IsJumping = false;
                }
            }
            
            // -- Float playert if the player is holding the button while we are about to fall (by gravity)
            // This doesn't invoke with the top
            if (GravityVelocity.y <= 0f &&
                    IsJumpAirTime && MoveJumpInput)
            {
                jumpTimer += Time.deltaTime;

                switch (jumpAxis)
                {
                    default:
                        break;

                    case TransformAxis2D.XAxis:
                        GravityVelocity = new Vector2(0f, GravityVelocity.y);
                        break;
                    case TransformAxis2D.YAxis:
                        GravityVelocity = new Vector2(GravityVelocity.x, 0f);
                        break;
                }

                if (jumpTimer >= jumpAirTime)
                {
                    jumpTimer = 0f;
                    IsJumpAirTime = false;
                }
            }

            // -- Other
        }

        /// <summary>
        /// Applies gravity velocity <c>if <see cref="useGravity"/> is enabled.</c>
        /// </summary>
        protected void ApplyGravityVelocity()
        {
            if (!useGravity) return;
            // Jumping has higher priority. (applying gravity w jump is, uh, not practical)
            if (IsJumping) return;

            // Do not apply gravity if we are grounded.
            if (IsGrounded)
            {
                // Set velocity to zero as we fell on the ground.
                // TODO : This will have issues with clipping on higher gravity velocities
                GravityVelocity = new Vector2(0f, gravity.y > 0f ? 2f : -2f);
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
            IsJumpAirTime = true;
            GravityVelocity = jumpAxis.GetVectorUsingTransformAxis(new Vector2(jumpSpeed, jumpSpeed));
        }

        /// <summary>
        /// Cast the controller directly to the <see cref="Rigidbody2D"/> class.
        /// </summary>
        public static explicit operator Rigidbody2D(CharacterControl2DBase control2DBase)
        {
            return control2DBase.CharacterRB;
        }

        private const int MAX_GROUND_COLLISION_CONTACTS = 16;
        private ContactPoint2D[] groundContacts = new ContactPoint2D[MAX_GROUND_COLLISION_CONTACTS];

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Debug.Log($"[CharacterControl2D] Primarily colliding with : {collision.gameObject.name}");

            // Do not get extra velocity for the gravity.
            if (!UseGravity)
                return;

            collision.GetContacts(groundContacts);

            foreach (var contact in groundContacts)
            {
                if (contact.collider != null)
                {
                    if (contact.collider.gameObject.layer == groundLayer)
                    {
                        // Get the normal of the surface
                        // If the normal is slope relative to the gravity, get the angle of the slope, then apply a support force. 
                        // (Multiplied lerped value from Lerp(nSlopeAngle / maxSlopeLimit, 0, 1))

                        // Multiply the collision normal with the movement axis
                        // Unless we are moving TransformAxis.xy
                        MoveVelocity += moveAxis.GetVectorUsingTransformAxis(contact.normal);
                        // Debug.Log($"Collision normal : {contact.normal}");
                    }
                }
            }
        }

        /// <summary>
        /// Draw velocity arrows.
        /// </summary>
        private void OnDrawGizmos()
        {
            var gColor = Gizmos.color;

            Gizmos.color = Color.blue;
            Additionals.DrawArrowGizmos(transform.position, Velocity * 25f);
            
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            Gizmos.color = gColor;
        }
    }
}