using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackType
{
    Light,
    Heavy
}

public enum AttackVariation
{
    Neutral,
    Forward,
    Backward,
    Up,
    Down,
    ForwardUp,
    ForwardDown,
    BackwardUp,
    BackwardDown,
    AirDive
}

[System.Serializable]
public class AttackSettings
{
    public AttackType attackType;
    public AttackVariation variation;
    public bool isAirAttack;

    [Header("Damage")]
    public int damage = 10;

    [Header("Hitbox")]
    public float range = 1.2f;
    public Vector2 hitboxSize = new Vector2(1.2f, 1f);

    [Header("Knockback")]
    public float horizontalKnockback = 5f;
    public float verticalKnockback = 2f;
}

public class Combat : MonoBehaviour
{
    [Header("Attack Settings")]
    public AttackSettings[] attackSettings = new AttackSettings[36];
    public float attackCooldown = 0.4f;

    private float nextAttackTime;

    [Header("Facing Direction")]
    public bool facingRight = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;

    [Header("Direction Input")]
    private float horizontalInput;
    private float verticalInput;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ReadDirectionInput();
        UpdateGrounded();

        if (Time.time < nextAttackTime)
            return;

        // Keyboard attacks: J = Light, K = Heavy
        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame)
                LightAttack();

            if (Keyboard.current.kKey.wasPressedThisFrame)
                HeavyAttack();
        }

        // Controller attacks:
        // West button = Light, North button = Heavy
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonWest.wasPressedThisFrame)
                LightAttack();

            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                HeavyAttack();
        }
    }

    void ReadDirectionInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current == null)
            return;

        bool left =
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed;

        bool right =
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed;

        bool down =
            Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed;

        bool up =
            Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed;

        // Opposite directions cancel each other.
        if (left != right)
            horizontalInput = right ? 1f : -1f;

        if (up != down)
            verticalInput = up ? 1f : -1f;
    }

    void UpdateGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void LightAttack()
    {
        PerformAttack(AttackType.Light);
    }

    void HeavyAttack()
    {
        PerformAttack(AttackType.Heavy);
    }

    void PerformAttack(AttackType type)
    {
        bool airAttack = !isGrounded;
        AttackVariation variation = GetAttackVariation(airAttack);

        AttackSettings settings = null;

        foreach (AttackSettings item in attackSettings)
        {
            if (item == null)
                continue;

            if (item.attackType == type &&
                item.variation == variation &&
                item.isAirAttack == airAttack)
            {
                settings = item;
                break;
            }
        }

        if (settings == null)
        {
            Debug.LogWarning(
                "Missing settings for " + type + " " +
                variation + (airAttack ? " Air" : " Ground")
            );
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        Debug.Log(
            (airAttack ? "Air " : "Ground ") +
            type + " Attack: " + variation
        );

        CheckForEnemies(settings);
    }

    AttackVariation GetAttackVariation(bool airAttack)
    {
        // Use AirDive while falling and holding down.
        if (airAttack &&
            verticalInput < 0 &&
            rb != null &&
            rb.linearVelocity.y < -0.1f &&
            horizontalInput == 0)
        {
            return AttackVariation.AirDive;
        }

        if (horizontalInput == 0 && verticalInput == 0)
            return AttackVariation.Neutral;

        if (horizontalInput > 0 && verticalInput > 0)
            return AttackVariation.ForwardUp;

        if (horizontalInput > 0 && verticalInput < 0)
            return AttackVariation.ForwardDown;

        if (horizontalInput < 0 && verticalInput > 0)
            return AttackVariation.BackwardUp;

        if (horizontalInput < 0 && verticalInput < 0)
        {
            // BackwardDown is included in the air list.
            // The ground list has only eight variations.
            return airAttack
                ? AttackVariation.BackwardDown
                : AttackVariation.Backward;
        }

        if (horizontalInput > 0)
            return AttackVariation.Forward;

        if (horizontalInput < 0)
            return AttackVariation.Backward;

        if (verticalInput > 0)
            return AttackVariation.Up;

        return AttackVariation.Down;
    }

    void CheckForEnemies(AttackSettings settings)
    {
        // Place the hitbox in front of the character.
        float direction = facingRight ? 1f : -1f;

        Vector2 hitboxCenter = (Vector2)transform.position +
            new Vector2(direction * settings.range * 0.5f, 0f);

        // Move the hitbox upward or downward for vertical attacks.
        if (settings.variation == AttackVariation.Up ||
            settings.variation == AttackVariation.ForwardUp ||
            settings.variation == AttackVariation.BackwardUp)
        {
            hitboxCenter.y += settings.hitboxSize.y * 0.5f;
        }
        else if (settings.variation == AttackVariation.Down ||
                 settings.variation == AttackVariation.ForwardDown ||
                 settings.variation == AttackVariation.BackwardDown ||
                 settings.variation == AttackVariation.AirDive)
        {
            hitboxCenter.y -= settings.hitboxSize.y * 0.5f;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            hitboxCenter,
            settings.hitboxSize,
            0f
        );

        foreach (Collider2D enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
                continue;

            Damage damageScript = enemy.GetComponent<Damage>();

            if (damageScript == null)
                damageScript = enemy.GetComponentInParent<Damage>();

            if (damageScript != null)
            {
                damageScript.TakeDamage(settings.damage);

                Knockback knockback = enemy.GetComponent<Knockback>();

                if (knockback == null)
                    knockback = enemy.GetComponentInParent<Knockback>();

                if (knockback != null)
                {
                    knockback.ApplyKnockback(
                        settings.horizontalKnockback,
                        settings.verticalKnockback,
                        facingRight
                    );
                }
            }
        }
    }

    // Call this from the movement script if it controls facing.
    public void SetFacingDirection(bool lookingRight)
    {
        facingRight = lookingRight;
    }

    // Draw the ground check and attack area in the Scene view.
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}