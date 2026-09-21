using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Respawn Settings")]

    // The location where the player will respawn.
    public Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Show in the Console what entered the Death Zone.
        Debug.Log("Something entered the Death Zone: " + other.gameObject.name);

        // Check if the object that entered is the Player.
        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER DETECTED! Respawning...");

            // Move the player to the respawn position.
            other.transform.position = respawnPoint.position;

            // Get the player's Rigidbody2D.
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // Stop the player's movement.
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }
    }
}