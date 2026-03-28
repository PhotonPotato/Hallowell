using UnityEngine;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float decayRate = 18;
    [SerializeField] private float controlLockDuration = 0.15f;

    public Vector2 CurrentVelocity { get; private set; }

    private float controlLockUntil;

    public bool IsControlLocked => Time.time < controlLockUntil;

    public void ApplyKnockback(Vector2 velocity, float? overrideLockDuration = null)
    {
        CurrentVelocity = velocity;
        controlLockUntil = Time.time + (overrideLockDuration ?? controlLockDuration);
    }

    public void AddKnockback(Vector2 velocity, float? overrideLockDuration = null)
    {
        CurrentVelocity += velocity;
        controlLockUntil = Time.time + (overrideLockDuration ?? controlLockDuration);
    }

    public void ClearKnockback()
    {
        CurrentVelocity = Vector2.zero;
        controlLockUntil = 0f;
    }

    public void Tick(float deltaTime)
    {
        CurrentVelocity = Vector2.MoveTowards(
            CurrentVelocity,
            Vector2.zero,
            decayRate * deltaTime
        );
    }
}
