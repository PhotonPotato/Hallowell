using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RolyPolyBehavior : EnemyBehaviorComponent, IStaggerable
{
    Rigidbody2D rb;

    //[Header("Trackers")]
    [SerializeField] public bool IsStaggered { get; set; }

    [Header("Enemy Specific Settings")]
    public float whackXForce = 3f;
    public float whackYForce = 3f;
    public float inheritedWhackVelocityMultiplier = .5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnDamageReceived(EnemyDamageInfo info)
    {
        // We don't really care about the damage amount for this enemy
        Vector2 whackedVelocity = Vector2.zero;

        switch (info.direction)
        {
            case AttackDirection.Right:
                if (IsStaggered)
                    whackedVelocity = Vector2.right * whackXForce;
                break;

            case AttackDirection.Left:
                if (IsStaggered)
                    whackedVelocity = Vector2.left * whackXForce;
                break;

            case AttackDirection.Up:
                whackedVelocity = Vector2.up * whackYForce;

                IsStaggered = true;
                break;
        }

        rb.velocity = new Vector2(info.inheritedVelocity.x * inheritedWhackVelocityMultiplier, 0) + whackedVelocity;
    }
}
