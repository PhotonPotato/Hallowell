using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KnockbackUtility
{
    public static Vector2 DirectionFromAttack(AttackDirection direction)
    {
        switch (direction)
        {
            case AttackDirection.Left: return Vector2.left;
            case AttackDirection.Right: return Vector2.right;
            case AttackDirection.Up: return Vector2.up;
            case AttackDirection.Down: return Vector2.down;
            default: return Vector2.zero;
        }
    }

    public static Vector2 BuildKnockback(
        DamageInfo info,
        float xForce,
        float yForce,
        float inheritedXMultiplier = 0f)
    {
        Vector2 dir = DirectionFromAttack(info.direction);

        Vector2 knockback = new Vector2(
            dir.x * xForce,
            dir.y * yForce
        );

        knockback += new Vector2(info.inheritedVelocity.x * inheritedXMultiplier, 0f);

        return knockback;
    }
}
