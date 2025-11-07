using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedeemerSwordScript : MonoBehaviour
{
    public float damage = 50;

    public float timeOfLastDamage = float.NegativeInfinity;

    public float knockbackAmount = 40;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the sword is colliding with the player.
        if (collision.gameObject.tag == "Player")
        {
            // Checkk if it has been a second since already hitting the player.
            // Attempting to prevent double hits.
            if (Time.time - timeOfLastDamage > .4f)
            {
                // Update time of last damage
                timeOfLastDamage = Time.time;

                // Calculate knockback
                Vector2 adversaryKnockback = ((Vector2)collision.transform.position - collision.GetContact(0).point).normalized;

                adversaryKnockback *= knockbackAmount;

                PlayerMovementScript playerMovement;

                // Try to apply knockback to the players
                if (collision.transform.TryGetComponent<PlayerMovementScript>(out playerMovement))
                {
                    playerMovement.appliedVel.x = Mathf.Abs(adversaryKnockback.x);

                    // Always launch the player up
                    playerMovement.appliedVel.y = Mathf.Abs(adversaryKnockback.y);

                    Debug.Log("Applying forces " + adversaryKnockback);

                    // Apply Damages
                    collision.gameObject.GetComponent<PlayerManager>().DealDamage(damage);
                }
                else
                {
                    Debug.LogError("Redeemer sword trying to access player movement script that doesn't exist");
                }
            }
        }
    }
}
