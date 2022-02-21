using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterControl2DBase : MonoBehaviour
    {
        [Header("Movement Base Settings")]
        public float moveSpeed = 5f;
        public float jumpSpeed = 5f;
        
        public TransformAxis2D moveAxis = TransformAxis2D.XYAxis; // Axis that we are allowed to move.
        public TransformAxis2D jumpAxis = TransformAxis2D.YAxis;  // Axis to apply the jump force to.

        [Header("Kinematic Physics Settings")]
        public Vector2 gravity = new Vector2(0f, 9.84f);
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
        public LayerMask groundLayer = 1 << 0;
        public float groundCheckRadius = .4f;

        [Header("Movement Base Input Map")]
        public CustomInputEvent MoveLeftInput = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
        public CustomInputEvent MoveRightInput = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
        public CustomInputEvent MoveJumpInput = new KeyCode[] { KeyCode.Space };

        // Runtime variables
        public bool IsGrounded
        {
            get
            {
                // Use a check circle on the defined layer and then return whether if the check circle was successful.
                return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
            }
        }
        public bool isMoving => false;
        /// <summary>
        /// Whether if we are moving using input.
        /// <br>Set by <see cref="GetMoveVelocity"/>.</br>
        /// </summary>
        public bool isMovingInput { get; private set; }
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
                currentRB.isKinematic = true;
                currentRB.sleepMode = RigidbodySleepMode2D.NeverSleep;

                return currentRB;
            }
        }

        // Internal values
        [SerializeField, HideInInspector] private Rigidbody2D currentRB; // Do not use this rigidbody.

        private void FixedUpdate()
        {
            ApplyGravityVelocity();
            ApplyJumpForce();
            ApplyMoveVelocity();

            // Apply velocity
            m_totalVelocity = (ExternVelocity + GravityVelocity + MoveVelocity) * Time.fixedDeltaTime;
            currentRB.MovePosition(CharacterRB.position + m_totalVelocity); // Move kinematically.
        }

        /// <summary>
        /// Applies gravity velocity <c>if <see cref="useGravity"/> is enabled.</c>
        /// </summary>
        protected void ApplyGravityVelocity()
        {
            if (!useGravity)
                return;

            if (!IsGrounded)
                return;

            GravityVelocity = Time.fixedDeltaTime * gravity;
        }
        /// <summary>
        /// Applies movement velocity <c>if player wants to move.</c>
        /// </summary>
        protected void ApplyMoveVelocity()
        {
            var moveDir = new Vector2();
            
            if (MoveLeftInput)
            {
                moveDir.x -= 1f;
            }
            if (MoveRightInput)
            {
                moveDir.x += 1f;
            }

            MoveVelocity = moveDir * moveSpeed;
        }
        /// <summary>
        /// Applies jumping force <c>if the player wants to jump.</c>
        /// </summary>
        protected void ApplyJumpForce()
        {
            if (!MoveJumpInput.IsKeyDown()) return;

            // Zero out the gravity velocity and add the jump force.
            GravityVelocity = Vector3.zero;

            GravityVelocity = jumpAxis.GetVectorFromTransformAxis(Vector3.zero, new Vector2(jumpSpeed, jumpSpeed));
        }
    }
}