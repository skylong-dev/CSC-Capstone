using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditorInternal.ReorderableList;

public class PlayerMovement : MonoBehaviour
{

    // Handles animation changes
    public Animator animator;
    // Handles sprite rendering
    SpriteRenderer spriteRenderer;



    // =========================================================
    // GROUND MOVEMENT
    // Controls how fast the player moves on the ground.
    // =========================================================

    [Header("Ground Movement")]

    // Maximum running speed on the ground.
    public float maxRunSpeed = 8f;

    // How quickly the player reaches their running speed.
    public float groundAcceleration = 60f;

    // How quickly the player stops when they release A/D.
    public float groundDeceleration = 70f;


    // =========================================================
    // AIR MOVEMENT
    // Controls movement while the player is in the air.
    // =========================================================

    [Header("Air Movement")]

    // Maximum horizontal speed while in the air.
    public float maxAirSpeed = 7f;

    // How quickly the player can move horizontally in the air.
    public float airAcceleration = 35f;

    // How quickly the player slows down in the air.
    public float airDeceleration = 20f;


    // =========================================================
    // JUMP
    // Controls normal jumping, double jumping,
    // and variable jump height.
    // =========================================================

    [Header("Jump")]

    // How strong the player's jump is.
    public float jumpForce = 14f;

    // Number of jumps the player can perform.
    // 2 means the player can jump once from the ground
    // and once again while in the air.
    public int maxJumps = 2;

    // Controls how much the jump is reduced when
    // the player releases Space early.
    //
    // 1.0 = no reduction
    // 0.5 = cuts the jump about in half
    // 0.3 = stronger short hop
    public float jumpCutMultiplier = 0.5f;


    // =========================================================
    // JUMP ASSISTANCE
    // These features make the controls feel smoother.
    // =========================================================

    [Header("Jump Assistance")]

    // Coyote time allows the player to jump for a tiny
    // amount of time after walking off a platform.
    public float coyoteTime = 0.12f;

    // Jump buffering remembers a jump press made shortly
    // before landing and performs the jump after landing.
    public float jumpBufferTime = 0.12f;


    // =========================================================
    // FAST FALL
    // Allows the player to fall faster when pressing S.
    // =========================================================

    [Header("Fast Fall")]

    // Maximum downward speed when fast falling.
    public float fastFallSpeed = 18f;


    // =========================================================
    // GROUND CHECK
    // Used to determine if the player is touching the ground.
    // =========================================================

    [Header("Ground Check")]

    // The small object placed underneath the player.
    // It checks whether the player is touching the ground.
    public Transform groundCheck;

    // Size of the circle used for detecting the ground.
    public float groundCheckRadius = 0.15f;

    // Tells Unity which layer counts as the ground.
    public LayerMask groundLayer;


    // =========================================================
    // PRIVATE VARIABLES
    // These are used internally by the movement system.
    // They do not need to be changed in the Inspector.
    // =========================================================

    // The player's Rigidbody2D.
    // This is used to control physics and velocity.
    private Rigidbody2D rb;

    // Stores whether the player is pressing A or D.
    // -1 = left
    //  0 = no movement
    //  1 = right
    private float horizontalInput;

    // Keeps track of how many jumps the player has left.
    private int jumpsRemaining;

    // True when the player is touching the ground.
    private bool isGrounded;

    // Stores whether the player was grounded during
    // the previous check.
    private bool wasGrounded;

    // Timer used for coyote time.
    private float coyoteTimer;

    // Timer used for jump buffering.
    private float jumpBufferTimer;


    // =========================================================
    // START
    // Runs once when the game begins.
    // =========================================================

    void Start()
    {
        // Get the Rigidbody2D attached to the Player.
        rb = GetComponent<Rigidbody2D>();
        
        // Get the SpriteRenderer attached to the Player.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Give the player their full number of jumps.
        jumpsRemaining = maxJumps;
    }


    // =========================================================
    // UPDATE
    // Runs every frame.
    // Used for player input and checks.
    // =========================================================

    void Update()
    {
        // Read the player's A/D input.
        GetInput();

        // Check if the player is touching the ground.
        CheckGround();

        // Handle jumping and jump assistance.
        HandleJump();

        // Handle fast falling.
        HandleFastFall();
    }


    // =========================================================
    // FIXED UPDATE
    // Runs on Unity's physics update.
    // Movement using Rigidbody2D is handled here.
    // =========================================================

    void FixedUpdate()
    {
        // Handle the player's horizontal movement.
        HandleHorizontalMovement();
    }


    // =========================================================
    // GET INPUT
    // Checks whether A or D is being pressed.
    // =========================================================

    void GetInput()
    {
        // Start with no horizontal movement.
        horizontalInput = 0f;

        // Pressing A moves the player left.
        if (Keyboard.current.aKey.isPressed || (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().x < 0))
        {
            animator.SetBool("IsWalking", true);
            spriteRenderer.flipX = true;
            horizontalInput = -1f;
        }

        // Pressing D moves the player right.
        else if (Keyboard.current.dKey.isPressed || (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().x > 0))
        {
            animator.SetBool("IsWalking", true);
            spriteRenderer.flipX = false;
            horizontalInput = 1f;
        }

        //Defaults to idle animation if no movement
        else 
        {
            animator.SetBool("IsWalking", false);
        }

    }


    // =========================================================
    // CHECK GROUND
    // Determines whether the player is standing on a platform.
    // =========================================================

    void CheckGround()
    {
        // Remember the previous ground state.
        wasGrounded = isGrounded;

        // Check for a Ground object underneath the player.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // If the player is touching the ground,
        // reset their available jumps.
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // If the player was grounded but is no longer grounded,
        // start the coyote-time timer.
        if (wasGrounded && !isGrounded)
        {
            coyoteTimer = coyoteTime;
        }

        // Count down the coyote timer while in the air.
        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            // Keep the timer full while grounded.
            coyoteTimer = coyoteTime;
        }
    }


    // =========================================================
    // HORIZONTAL MOVEMENT
    // Controls left/right movement on the ground and in the air.
    // =========================================================

    void HandleHorizontalMovement()
    {
        // Stores the speed we want the player to reach.
        float targetSpeed;


        // -----------------------------------------------------
        // GROUND MOVEMENT
        // -----------------------------------------------------

        if (isGrounded)
        {
            // Convert A/D input into the target speed.
            targetSpeed = horizontalInput * maxRunSpeed;

            float acceleration;

            // If the player is pressing A or D,
            // use the normal ground acceleration.
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = groundAcceleration;
            }
            else
            {
                // If no movement key is pressed,
                // use the ground deceleration.
                acceleration = groundDeceleration;
            }

            // Smoothly move the player's horizontal velocity
            // toward the target speed.
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
            // Air movement uses a slightly different maximum speed.
            targetSpeed = horizontalInput * maxAirSpeed;

            float acceleration;

            // Allow the player to control their movement in the air.
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = airAcceleration;
            }
            else
            {
                // Slow down gradually when no direction is pressed.
                acceleration = airDeceleration;
            }

            // Change horizontal velocity while keeping
            // the player's vertical velocity unchanged.
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
    // Handles normal jumping, double jumping,
    // jump buffering, coyote time, and variable jump height.
    // =========================================================

    void HandleJump()
    {
        // -----------------------------------------------------
        // JUMP BUFFER
        // -----------------------------------------------------

        // If Space is pressed, remember the input.
        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            (Gamepad.current != null && (Gamepad.current.xButton.wasPressedThisFrame || Gamepad.current.yButton.wasPressedThisFrame)))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // Count down the jump buffer timer.
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        // -----------------------------------------------------
        // NORMAL JUMP / COYOTE TIME
        // -----------------------------------------------------

        // If a jump was recently pressed AND
        // the player is either grounded OR still within
        // the coyote-time window, perform a jump.
        if (jumpBufferTimer > 0 &&
            (isGrounded || coyoteTimer > 0) &&
            jumpsRemaining > 0)
        {
            // Perform the jump.
            PerformJump();

            // Clear the jump buffer.
            jumpBufferTimer = 0f;

            // Clear coyote time.
            coyoteTimer = 0f;
        }


        // -----------------------------------------------------
        // DOUBLE JUMP
        // -----------------------------------------------------

        // If Space is pressed while in the air and
        // the player has another jump available,
        // perform the second jump.
        else if ((Keyboard.current.spaceKey.wasPressedThisFrame ||
            (Gamepad.current != null && (Gamepad.current.xButton.wasPressedThisFrame || Gamepad.current.yButton.wasPressedThisFrame))) &&
                 !isGrounded &&
                 coyoteTimer <= 0 &&
                 jumpsRemaining > 0)
        {
            PerformJump();
        }


        // -----------------------------------------------------
        // VARIABLE JUMP HEIGHT
        // -----------------------------------------------------

        // If the player releases Space while moving upward,
        // reduce the upward velocity.
        //
        // This creates a short hop when Space is tapped
        // and a higher jump when Space is held.
        if ((Keyboard.current.spaceKey.wasReleasedThisFrame || 
            (Gamepad.current != null && (Gamepad.current.xButton.wasReleasedThisFrame || Gamepad.current.yButton.wasReleasedThisFrame))) &&
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
    // Actually applies the jump force.
    // =========================================================

    void PerformJump()
    {
        // Give the player upward velocity.
        //
        // Horizontal velocity stays the same.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        // Use one of the player's available jumps.
        jumpsRemaining--;
    }


    // =========================================================
    // FAST FALL
    // Makes the player fall faster when pressing S.
    // =========================================================

    void HandleFastFall()
    {
        // Fast fall only works when:
        // 1. The player is not grounded.
        // 2. S is being held.
        // 3. The player is already moving downward.
        if (!isGrounded &&
            (Keyboard.current.sKey.isPressed || (Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().y < 0))) &&
            rb.linearVelocity.y < 0)
        {
            // Keep horizontal movement the same,
            // but increase downward speed.
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -fastFallSpeed
            );
        }
    }
}