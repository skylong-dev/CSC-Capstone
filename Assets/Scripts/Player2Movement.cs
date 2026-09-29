using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Movement : MonoBehaviour
{

    // Handles animation changes
    public Animator animator;

    // Handles sprite rendering
    SpriteRenderer spriteRenderer;





    // =========================================================
    // GROUND MOVEMENT
    // =========================================================

    [Header("Ground Movement")]

    // Maximum running speed on the ground.
    public float maxRunSpeed = 8f;

    // How quickly Player 2 reaches running speed.
    public float groundAcceleration = 60f;

    // How quickly Player 2 stops.
    public float groundDeceleration = 70f;


    // =========================================================
    // AIR MOVEMENT
    // =========================================================

    [Header("Air Movement")]

    // Maximum horizontal speed in the air.
    public float maxAirSpeed = 7f;

    // How quickly Player 2 moves horizontally in the air.
    public float airAcceleration = 35f;

    // How quickly Player 2 slows down in the air.
    public float airDeceleration = 20f;


    // =========================================================
    // JUMP
    // =========================================================

    [Header("Jump")]

    // Strength of the jump.
    public float jumpForce = 14f;

    // Number of jumps Player 2 can use.
    public int maxJumps = 2;

    // Controls how much the jump is shortened
    // when the player releases the jump button.
    public float jumpCutMultiplier = 0.5f;


    // =========================================================
    // JUMP ASSISTANCE
    // =========================================================

    [Header("Jump Assistance")]

    // Allows Player 2 to jump shortly after
    // walking off a platform.
    public float coyoteTime = 0.12f;

    // Remembers a jump press made shortly
    // before landing.
    public float jumpBufferTime = 0.12f;


    // =========================================================
    // FAST FALL
    // =========================================================

    [Header("Fast Fall")]

    // Maximum downward speed while fast falling.
    public float fastFallSpeed = 18f;


    // =========================================================
    // GROUND CHECK
    // =========================================================

    [Header("Ground Check")]

    // Object underneath Player 2 used to check the ground.
    public Transform groundCheck;

    // Size of the ground detection circle.
    public float groundCheckRadius = 0.15f;

    // Layer that counts as ground.
    public LayerMask groundLayer;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    // Player 2's Rigidbody2D.
    private Rigidbody2D rb;

    // Stores left/right input.
    private float horizontalInput;

    // Number of jumps remaining.
    private int jumpsRemaining;

    // True when Player 2 is touching the ground.
    private bool isGrounded;

    // Previous ground state.
    private bool wasGrounded;

    // Timer for coyote time.
    private float coyoteTimer;

    // Timer for jump buffering.
    private float jumpBufferTimer;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // Find the Rigidbody2D attached to Player 2.
        rb = GetComponent<Rigidbody2D>();

        // Get the SpriteRenderer attached to the Player.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Give Player 2 all available jumps.
        jumpsRemaining = maxJumps;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Read Player 2's keyboard controls.
        GetInput();

        // Check whether Player 2 is grounded.
        CheckGround();

        // Handle jumping.
        HandleJump();

        // Handle fast falling.
        HandleFastFall();
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    void FixedUpdate()
    {
        // Handle horizontal movement using physics.
        HandleHorizontalMovement();
    }


    // =========================================================
    // GET INPUT
    //
    // LEFT ARROW  = MOVE LEFT
    // RIGHT ARROW = MOVE RIGHT
    // UP ARROW    = JUMP
    // DOWN ARROW  = FAST FALL
    // =========================================================

    void GetInput()
    {
        // Start with no horizontal movement.
        horizontalInput = 0f;

        // Left Arrow moves Player 2 left.
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            animator.SetBool("IsWalking", true);
            spriteRenderer.flipX = true;
            horizontalInput = -1f;
        }

        // Right Arrow moves Player 2 right.
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            animator.SetBool("IsWalking", true);
            spriteRenderer.flipX = false;
            horizontalInput = 1f;
        }

        // Defaults to idle animation if no movement
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }


    // =========================================================
    // CHECK GROUND
    // =========================================================

    void CheckGround()
    {
        // Remember the previous ground state.
        wasGrounded = isGrounded;

        // Check for ground underneath Player 2.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Reset jumps when Player 2 touches the ground.
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // Start coyote time when leaving the ground.
        if (wasGrounded && !isGrounded)
        {
            coyoteTimer = coyoteTime;
        }

        // Count down coyote time while in the air.
        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = coyoteTime;
        }
    }


    // =========================================================
    // HORIZONTAL MOVEMENT
    // =========================================================

    void HandleHorizontalMovement()
    {
        // The speed Player 2 wants to reach.
        float targetSpeed;


        // -----------------------------------------------------
        // GROUND MOVEMENT
        // -----------------------------------------------------

        if (isGrounded)
        {
            // Convert the arrow-key input into movement speed.
            targetSpeed = horizontalInput * maxRunSpeed;

            float acceleration;

            // Use acceleration when moving.
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = groundAcceleration;
            }
            else
            {
                // Use deceleration when no direction is pressed.
                acceleration = groundDeceleration;
            }

            // Change horizontal velocity while keeping
            // vertical velocity the same.
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(
                    rb.linearVelocity.x,
                    targetSpeed,
                    acceleration * Time.fixedDeltaTime
                ),
                rb.linearVelocity.y
            );
        }


        // -----------------------------------------------------
        // AIR MOVEMENT
        // -----------------------------------------------------

        else
        {
            // Use the air movement speed.
            targetSpeed = horizontalInput * maxAirSpeed;

            float acceleration;

            // Allow Player 2 to control movement in the air.
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = airAcceleration;
            }
            else
            {
                acceleration = airDeceleration;
            }

            // Change horizontal velocity while keeping
            // vertical velocity the same.
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(
                    rb.linearVelocity.x,
                    targetSpeed,
                    acceleration * Time.fixedDeltaTime
                ),
                rb.linearVelocity.y
            );
        }
    }


    // =========================================================
    // JUMP
    // =========================================================

    void HandleJump()
    {
        // Remember when Player 2 presses the Up Arrow.
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // Count down the jump buffer.
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        // -----------------------------------------------------
        // NORMAL JUMP / COYOTE TIME
        // -----------------------------------------------------

        if (jumpBufferTimer > 0 &&
            (isGrounded || coyoteTimer > 0) &&
            jumpsRemaining > 0)
        {
            PerformJump();

            // Clear the jump buffer.
            jumpBufferTimer = 0f;

            // Clear coyote time.
            coyoteTimer = 0f;
        }


        // -----------------------------------------------------
        // DOUBLE JUMP
        // -----------------------------------------------------

        else if (Keyboard.current.upArrowKey.wasPressedThisFrame &&
                 !isGrounded &&
                 coyoteTimer <= 0 &&
                 jumpsRemaining > 0)
        {
            PerformJump();
        }


        // -----------------------------------------------------
        // VARIABLE JUMP HEIGHT
        // -----------------------------------------------------

        // Releasing Up Arrow early makes the jump shorter.
        if (Keyboard.current.upArrowKey.wasReleasedThisFrame &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }


    // =========================================================
    // PERFORM JUMP
    // =========================================================

    void PerformJump()
    {
        // Apply upward velocity.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        // Use one available jump.
        jumpsRemaining--;
    }


    // =========================================================
    // FAST FALL
    // =========================================================

    void HandleFastFall()
    {
        // Down Arrow makes Player 2 fall faster.
        if (!isGrounded &&
            Keyboard.current.downArrowKey.isPressed &&
            rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -fastFallSpeed
            );
        }
    }
}