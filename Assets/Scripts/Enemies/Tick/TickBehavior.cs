using UnityEngine;

public class TickBehavior : MonoBehaviour, IDamagable
{
    Rigidbody2D rb;
    Animator Animator;
    SpriteRenderer Renderer;
    [SerializeField] ParticleSystem DamagedParticles;
    [SerializeField] ParticleSystem DamageAreaTelegraphParticles;
    [SerializeField] ParticleSystem ExplosionParticles;
    KnockbackReceiver Knockback;
    Collider2D CircleCollider;

    [Header("Detection")]
    public Transform PlayerTransform;
    public float playerDetectionRadius = 6;
    bool playerDetected = false;
    float distToPlayer;

    [Header("Behavior Settings")]
    [SerializeField] private int distToInitExplosion = 5;
    public bool exploding = false;

    [Header("Movement Settings")]
    public Vector2 velocity;
    public float movementSpeed = 4;
    public float maxMovementSpeed = 4;

    [Space]

    public float groundDetectionLen = .3f;
    public LayerMask groundMask;
    public bool touchingGround = false;

    [Header("Knockback Settings")]
    public float whackedXForce = 3f;
    public float whackedYForce = 3f;
    public float inheritedWhackVelocityMultiplier = .5f;

    [Header("Attack Settings")]
    public float damageAmount = 20;
    public float damageRadius = 5;
    [SerializeField] LayerMask damageLayers;

    [Header("Status settings")]
    public float health = 40;

    [Header("Refs")]
    [SerializeField] GameObject deathParticlesPrefab;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Renderer = GetComponent<SpriteRenderer>();
        Knockback = GetComponent<KnockbackReceiver>();
        CircleCollider = GetComponent<CircleCollider2D>();
    }

    private void FixedUpdate()
    {
        DetectGround();
        DetectPlayer();

        velocity = rb.velocity;

        velocity.y += -.3f;

        // Check if knockback control lock is on
        if (!Knockback.IsControlLocked)
        {
            // Move towards player
            if (playerDetected)
            {
                if (!exploding)
                {
                    velocity.x += Mathf.Sign(PlayerTransform.position.x - transform.position.x) * movementSpeed;

                    Animator.ResetTrigger("Explode");

                    // Init explotion
                    if (distToPlayer <= distToInitExplosion)
                    {
                        exploding = true;
                        Animator.SetTrigger("Explode");

                        PlayDamageAreaTelegraphingParticles();
                    }
                }
                else
                {
                    velocity.x *= .5f;
                }
            }    
        }


        Knockback.Tick(Time.fixedDeltaTime);

        rb.velocity = velocity + Knockback.CurrentVelocity;

        UpdateSpriteRenderer();
    }

    /// <summary>
    /// Checks if player is within detection radius
    /// </summary>
    void DetectPlayer()
    {
        if (PlayerTransform == null) PlayerTransform = FindFirstObjectByType<PlayerManager>().transform;

        if ((distToPlayer = Vector2.Distance(PlayerTransform.position, transform.position)) < playerDetectionRadius)
        {
            playerDetected = true;

            // Play a sound?
        }
    }

    void DetectGround()
    {
        touchingGround = Physics2D.Raycast(transform.position, Vector2.down, groundDetectionLen, groundMask).collider != null;
    }

    void UpdateSpriteRenderer()
    {
        // Only flip velocity if its not zero
        if (Mathf.Abs(velocity.x) > .1f) Renderer.flipX = velocity.x < 0;
        // Sometimes it will stay pointed away when landing after a lunge and winding up again, this should fix that
        else if (playerDetected) Renderer.flipX = PlayerTransform.position.x - transform.position.x < 0 ? true : false;
    }

    /// <summary>
    /// This is the take damage function
    /// </summary>
    public void DealDamage(DamageInfo info)
    {
        health -= info.amount;

        // Check death
        if (health <= 0)
        {
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }

        PlayDamageParticles(info);

        Vector2 knockbackVelocity = KnockbackUtility.BuildKnockback(
            info,
            whackedXForce,
            whackedYForce,
            inheritedWhackVelocityMultiplier
        );

        Knockback.ApplyKnockback(knockbackVelocity);
    }

    void PlayDamageParticles(DamageInfo info)
    {
        //DamagedParticles.transform.position = info.contactPoint;

        switch (info.direction)
        {
            case AttackDirection.Left:
                DamagedParticles.transform.rotation = Quaternion.Euler(0, 0, 135);
                break;

            case AttackDirection.Down:
                DamagedParticles.transform.rotation = Quaternion.Euler(0, 0, -110);
                break;

            case AttackDirection.Up:
                DamagedParticles.transform.rotation = Quaternion.Euler(0, 0, 65);
                break;

            // Default to a right attack
            default:
                DamagedParticles.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }

        DamagedParticles.Play();
    }

    private void PlayDamageAreaTelegraphingParticles()
    {
        DamageAreaTelegraphParticles.Play();
    }

    public void Explode()
    {
        Destroy(gameObject, 1f);
        ExplosionParticles?.Play();

        //exploding = false;

        ScreenShakeManager.Instance?.InitShakeByType(ScreenShakeType.Explosion);


        // Deal actual damage
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, damageRadius, damageLayers);

        foreach (Collider2D col in cols)
        {
            if (col.gameObject.tag == "Player")
            {
                PlayerManager.Instance.DealDamage(damageAmount);
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
