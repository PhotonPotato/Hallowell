using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthContainer : MonoBehaviour
{
    //Sotre the container type real q
    public HealthContainerType type;

    //This is just a universal container to store health in the game.
    public float maxHealth;
    public float currentHealth;

    //A public mehtod to deal damage
    public virtual void DealDamage(float damage)
    {
        currentHealth -= damage;
    }

    public virtual void DealDamage(float damage, AttackDirection direction)
    {
        currentHealth -= damage;
    }

    public virtual void DealDamage(float damage, AttackDirection direction, Vector2 InheritedVelocity = default)
    {
        currentHealth -= damage;
    }

    public virtual float GetHealth()
    {
        return currentHealth;
    }
}
