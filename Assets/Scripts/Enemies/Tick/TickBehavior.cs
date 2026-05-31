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
    public float movementSpeed = 4;
    public float maxMovementSpeed = 4;

    [Header("Physics Settings")]
    public Vector2 velocity;
    [SerializeField] private Vector2 GroundDir = Vector2.down;
    [SerializeField] private Vector2 ForwardDir = Vector2.right;
    public float thisColRadius = .875f;
    public float lookAheadLen = .875f;
    public float lookDownLen = 1.75f;

    [Space]

    public float groundDetectionLen = .3f;
    public LayerMask groundMask;
    public bool touchingGround = false;

    [Header("Rotation Settings")]
    public float rotationSpeed = 6;
    private float targetZRotation;

    private Vector3 targetPosition;
    [SerializeField] bool isRoundingCorner = false;
    [SerializeField] private float targetPositionArrivalThreshold = .05f;
    private float timeOfLastSmoothing = float.NegativeInfinity;
    [SerializeField] private float thresholdToGiveUpSmoothing = .8f;

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

    Vector3 cornerRayOriginTMP;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Renderer = GetComponent<SpriteRenderer>();
        Knockback = GetComponent<KnockbackReceiver>();
        CircleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        InitialGroundDetection();
    }

    private void FixedUpdate()
    {
        DetectGround();
        DetectPlayer();

        velocity = rb.velocity;

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

        // Smoothly interpolate towards the target corner rotation
        float currentZ = transform.eulerAngles.z;
        float newZ = Mathf.LerpAngle(currentZ, targetZRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newZ);

        if (isRoundingCorner)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * rotationSpeed);

            velocity = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
        else
        {
            // Otherwise apply normal velocities and moves
            MoveTick();

            Knockback.Tick(Time.fixedDeltaTime);

            rb.velocity = velocity + Knockback.CurrentVelocity;
        }


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
        touchingGround = Physics2D.Raycast(transform.position, GroundDir, groundDetectionLen, groundMask).collider != null;

        if (isRoundingCorner)
        {
            if (Vector3.Distance(transform.position, targetPosition) <= targetPositionArrivalThreshold || Time.time - timeOfLastSmoothing > thresholdToGiveUpSmoothing)
            {
                // Snap it
                transform.position = targetPosition;

                isRoundingCorner = false;
            }
        }

        // Look for wall in front
        if (!isRoundingCorner && Physics2D.Raycast(transform.position, ForwardDir, groundDetectionLen, groundMask) is { collider: not null } hit)
        {
            // Rotate
            GroundDir = ForwardDir;

            // Recalc player pos
            targetPosition = hit.point + (GroundDir * -1 * thisColRadius);
            targetZRotation = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;

            touchingGround = true;
            isRoundingCorner = true;
            timeOfLastSmoothing = Time.time;
        }

        cornerRayOriginTMP = (Vector2)transform.position   // origin
                                    + (ForwardDir * lookAheadLen)   // forward
                                    + (GroundDir * lookDownLen);   // down

        if (!isRoundingCorner && !touchingGround)
        {
            /// Look around corner ray origin vector
            ///     _<O__
            ///  ->|
            /// ^aka that
            Vector2 cornerRayOrigin = (Vector2)transform.position   // origin
                                    + (ForwardDir * lookAheadLen)   // forward
                                    + (GroundDir * lookDownLen);   // down

            // Then look for a corner we are rounding (looking back a little farther than we stepped forward)
            if (Physics2D.Raycast(cornerRayOrigin, ForwardDir * -1, lookAheadLen * 1.2f, groundMask) is { collider: not null} rayHit)
            {
                Debug.Log("Found around corner!");

                if (Physics2D.OverlapPoint(rayHit.point + (ForwardDir * thisColRadius), groundMask) == null)
                {
                    GroundDir = ForwardDir * -1;

                    targetPosition = rayHit.point + (GroundDir * -1 * thisColRadius);

                    targetZRotation = Mathf.Atan2(rayHit.normal.y, rayHit.normal.x) * Mathf.Rad2Deg - 90f;


                    touchingGround = true;
                    isRoundingCorner = true;
                    timeOfLastSmoothing = Time.time;
                }
                else
                {
                    Debug.Log("Was inside of object");
                }
            }
        }

        ForwardDir = new Vector2(GroundDir.y * -1, GroundDir.x);
    }

    /// <summary>
    /// Used at start to find nearest surface and set direction accordingly
    /// </summary>
    void InitialGroundDetection()
    {
        // Search cardinal directions
        for (int i = 0; i < 4; i++)
        {
            ForwardDir = new Vector2(GroundDir.y * -1, GroundDir.x);

            if (Physics2D.Raycast(transform.position, GroundDir, groundDetectionLen, groundMask) is { collider: not null } hit)
            {
                touchingGround = true;

                return;
            }

            GroundDir = ForwardDir;
        }
    }

    void MoveTick()
    {
        if (touchingGround)
        {
            // Choose direction to move towards player (prioritize the plane that we are on)

            velocity = ForwardDir * movementSpeed;
        }
        else
        {
            velocity.y += -.3f;
        }

        velocity.x = Mathf.Clamp(velocity.x, maxMovementSpeed * -1, maxMovementSpeed);
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

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cornerRayOriginTMP, .3f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, .3f);
    }
}
