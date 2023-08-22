using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Script attached to player character
    public PlayerMovementScript playerMovement;
    public PlayerCameraController playerCameraController;

    Rigidbody2D rb;

    void Start()
    {
        playerMovement= GetComponent<PlayerMovementScript>();
        playerCameraController= GetComponent<PlayerCameraController>();

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Reset position for debug when R is pressed
        if (Input.GetKey("r"))
        {
            playerMovement.resetPlayerInLevel();
        }

        //If the player is touching anything.
        Collider2D[] cols = new Collider2D[10];
        if (rb.GetContacts(cols) != 0)
        {
            //Run through all of the collisions
            foreach(Collider2D col in cols)
            {
                if (col == null) continue;

                if (col.gameObject.tag == "Spikes")
                {
                    //The player is touching spikes.
                    playerMovement.resetPlayerInLevel();
                }
            }
        }
    }
}
