using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyHealthContainer : MonoBehaviour
{
    //This is just a universal container to store health in the game.
    public int objID;
    public bool destroyOnNoHealth = false;
    public float maxHealth;
    public float currentHealth;

    public float damageDealt;

    //A public mehtod to deal damage
    public void dealDamage(float damage)
    {
        currentHealth -= damage;

        if (destroyOnNoHealth) Destroy(this.transform.parent.gameObject);
    }
}
