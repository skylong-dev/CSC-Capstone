using UnityEngine;

public class Damage : MonoBehaviour
{
    public int maxHealth = 100;
    public int health;

    private bool defeated = false;

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (defeated)
        {
            return;
        }

        health -= damage;

        Debug.Log(gameObject.name + " took " + damage + " damage.");

        // Make sure health doesn't go below zero
        if (health < 0)
        {
            health = 0;
        }

        if (health <= 0)
        {
            Defeated();
        }
    }

    void Defeated()
    {
        defeated = true;

        Debug.Log(gameObject.name + " was defeated.");

        // This can be changed later to play a death.
        // animation instead of destroying the object.
        Destroy(gameObject);
    }

    public int GetHealth()
    {
        return health;
    }

    public bool IsDefeated()
    {
        return defeated;
    }
}