using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BloodLeechBehavior : MonoBehaviour, IDamagable
{
    Rigidbody2D rb;
    Animator Animator;
    SpriteRenderer Renderer;
    [SerializeField] ParticleSystem DamagedParticles;
    KnockbackReceiver Knockback;
    Collider2D CircleCollider;

    [Header("Detection")]
    public float playerDetectionRadius = 6;
    public bool playerDetected = false;
    public Transform PlayerTransform;
    float distToPlayer;

    [Header("Physics")]
    public Vector2 velocity;
    public float movementSpeed = 4;
    public float maxMovementSpeed = 4;

    [Space]

    public float groundDetectionLen = .3f;
    public LayerMask groundMask;
    public bool touchingGround = false;

    [Header("Behavior Settings")]
    public bool animatorLunging = false;
    public float timeOfLastLunge = float.NegativeInfinity;
    public float distToInitLunge = 10;
    public float lungeXForce;
    public float lungeYForce;

    [Header("Knockback Settings")]
    public float whackedXForce = 3f;
    public float whackedYForce = 3f;
    public float inheritedWhackVelocityMultiplier = .5f;

    [Header("Attack Settings")]
    public float damageAmount = 20;

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

        UpdateAnimatorParams();

        velocity = rb.velocity;

        velocity.y += -.3f;

        // Check if knockback control lock is on
        if (!Knockback.IsControlLocked)
        {
            // Move towards player
            if (playerDetected && !animatorLunging)
            {
                velocity.x += Mathf.Sign(PlayerTransform.position.x - transform.position.x) * movementSpeed;

                Animator.ResetTrigger("Lunge");

                // Init lunge
                if (distToPlayer <= distToInitLunge)
                {
                    Animator.SetTrigger("Lunge");

                    velocity.x = 0;
                }
            }

            // Only clamp velocity when not lunging
            if (!animatorLunging)
            {
                velocity.x = Mathf.Clamp(velocity.x, -maxMovementSpeed, maxMovementSpeed);
                velocity.y = Mathf.Clamp(velocity.y, -12, lungeYForce);
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

    public void LungeAtPlayer()
    {
        velocity.x = lungeXForce * Mathf.Sign(PlayerTransform.position.x - transform.position.x);
        velocity.y = lungeYForce;

        rb.velocity = velocity;
    }

    void DetectGround()
    {
        touchingGround = Physics2D.Raycast(transform.position, Vector2.down, groundDetectionLen, groundMask).collider != null;

        if (animatorLunging && touchingGround && Time.time - timeOfLastLunge > 0.1f)
        {
            animatorLunging = false;
        }
    }

    void UpdateAnimatorParams()
    {
        Animator.SetBool("TouchingGround", touchingGround);
        Animator.SetBool("Lunging", animatorLunging);
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

    public void CheckForPlayerCol()
    {
        List<Collider2D> contactBin = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = LayerMask.GetMask("Player");

        if (CircleCollider.GetContacts(filter, contactBin) > 0)
        {
            if (contactBin.Any(c => c.tag != "Player")) return;

            DamagePlayer();

            contactBin.Clear();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (animatorLunging)
            {
                DamagePlayer();
            }
        }
    }

    void DamagePlayer()
    {
        PlayerManager.Instance.DealDamage(damageAmount);
    }
}