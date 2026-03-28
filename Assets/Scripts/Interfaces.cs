using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStaggerable
{
    public bool IsStaggered { get; set; }
}

public interface IDamagable
{
    void DealDamage(DamageInfo info);
}

public class DamageInfo
{
    public float amount;
    public AttackDirection direction;
    public Vector2 inheritedVelocity;
    public GameObject attacker;
    public Vector2 contactPoint;
}