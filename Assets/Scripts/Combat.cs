using UnityEngine;
using UnityEngine.InputSystem;

public class Combat : MonoBehaviour
{
    // Basic attack settings
    public int lightDamage = 10;
    public int heavyDamage = 20;

    public float lightRange = 1.2f;
    public float heavyRange = 1.5f;

    // How long the player has to wait before attacking again
    public float attackCooldown = 0.4f;

    private float nextAttackTime = 0f;

    // Used to tell which direction the player is facing
    public bool facingRight = true;

    void Update()
    {
        // Make sure the player can attack again
        if (Time.time < nextAttackTime)
        {
            return;
        }

        // Keyboard controls
        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                LightAttack();
            }

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                HeavyAttack();
            }
        }

        // Controller controls
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                LightAttack();
            }
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                HeavyAttack();
            }
        }
    }

    void LightAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("Light attack");

        CheckForEnemies(lightRange, lightDamage, false);
    }

    void HeavyAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("Heavy attack");

        CheckForEnemies(heavyRange, heavyDamage, true);
    }

    void CheckForEnemies(float range, int damage, bool heavyAttack)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            range
        );

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Damage damageScript = enemy.GetComponent<Damage>();

                if (damageScript != null)
                {
                    damageScript.TakeDamage(damage);

                    // Heavy attacks should knock the enemy back more
                    Knockback knockback = enemy.GetComponent<Knockback>();

                    if (knockback != null)
                    {
                        if (heavyAttack)
                        {
                            knockback.ApplyKnockback(8f, 4f, facingRight);
                        }
                        else
                        {
                            knockback.ApplyKnockback(5f, 2f, facingRight);
                        }
                    }
                }
            }
        }
    }