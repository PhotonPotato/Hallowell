using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerHealthContainer : MonoBehaviour
{
    //This is just a universal container to store health in the game.
    public float maxHealth;
    public float currentHealth;

    //A public mehtod to deal damage
    public void dealDamage(float damage)
    {
        currentHealth -= damage;
    }
}
