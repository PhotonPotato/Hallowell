using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedeemerPriestBehavior : MonoBehaviour
{
    [Header("Refs")]
    public RedeemerBehavior RedeemerBehaviors;
    public HealthContainer Health;

    public GameObject PlatformPrefab;

    public List<GameObject> spawnedPlatforms;

    public Transform Adversary;

    [Space]

    public Transform GodRaySpawnOrigin;
    public GameObject GodRayPrefab;

    public struct GodRay
    {
        public float initiationTimeStamp;
        public LineRenderer renderer;

        public Transform particlesParent;

        public ParticleSystem.EmissionModule trackingParticles;
        public ParticleSystem.EmissionModule explosionParticles;
        public EdgeCollider2D edgeCollider;

        public Vector2 lastTargetPos;
        // Determines how much the ray will look ahead of the target
        public float predictionStrength;
    }

    public List<GodRay> CurrentGodRays;

    [Header("Settings")]
    public bool active = false;

    public int numPlatforms = 3;

    [Space(2)]
    public PriestBossState bossState = PriestBossState.Idle;
    public int stage = 1;

    public float distanceBetweenPlatforms = 2;
    public float platformXOffset = 4;
    public float platformXOffsetIncrement = 2;

    public float spawnPlatformsWaitTimer;

    public float lowStabHoldTime = 10;
    public float lowStabHoldTimer;

    [Space]
    float healthAtStartOfVulnerable;
    public float maxHealthLossDuringVulnerabilities = 80;

    public int attacksBetweenVulnerabilities = 3;
    public int attacksSinceLastVulnerability = 0;

    [Header("God Rays")]
    public LayerMask godRayLayers;
    public int numGodRays = 6;
    public float godRaySpwanRadius = 15f;
    public float godRayMinPredictionStrenght = 0;
    public float godRayMaxPredictionStrenght = 5;
    public float godRayTrackingTime = 2f;
    public float godRatTrackingWidth = 1f;
    public float godRayWaitTime = .5f;
    public float godRayActivatedTime = .3f;
    public float godRayActivatedWidth = 2f;

    public Gradient GodRayWindingUpGradient;
    public Gradient GodRayBlastGradient;

    public void Start()
    {
        Health = GetComponent<HealthContainer>();

        spawnedPlatforms = new List<GameObject>();

        RedeemerBehaviors.Activate(Adversary);

        CurrentGodRays = new List<GodRay>();

        bossState = PriestBossState.InitiateGodRays;
    }

    public void Update()
    {
        if (active)
        {
            Debug.Log("state" + bossState.ToString());

            switch (bossState)
            {
                case PriestBossState.Idle:
                    // Check to see if we have reached enough attacks to be vulnerable
                    if (attacksSinceLastVulnerability >= attacksBetweenVulnerabilities)
                    {
                        // Then begin vulnerability period.
                        Debug.Log("platform spawn");

                        // Spawn the platforms that the player can jump on
                        SpawnPlatforms();

                        spawnPlatformsWaitTimer = Random.Range(1f, 2.5f);

                        bossState = PriestBossState.SpawnPlatformsWait;
                    }
                    else
                    {
                        // Start a new attack
                        bossState = PriestBossState.Attacking;

                        ChooseAndBeginNewAttack();

                        attacksSinceLastVulnerability++;

                        break;
                    }

                    break;

                case PriestBossState.SpawnPlatformsInit:
                    // Spawn the platforms that the player can jump on
                    SpawnPlatforms();

                    spawnPlatformsWaitTimer = Random.Range(1f, 2.5f);

                    bossState = PriestBossState.SpawnPlatformsWait;
                    break;

                case PriestBossState.SpawnPlatformsWait:

                    if (spawnPlatformsWaitTimer <= 0)
                    {
                        lowStabHoldTimer = lowStabHoldTime;

                        bossState = PriestBossState.InitLowStab;
                    }
                    else
                    {
                        spawnPlatformsWaitTimer -= Time.deltaTime;
                    }

                    healthAtStartOfVulnerable = Health.currentHealth;

                    break;

                case PriestBossState.InitLowStab:
                    RedeemerBehaviors.InitiateLowStab();

                    break;

                case PriestBossState.PriestVulnerable:
                    if (lowStabHoldTimer <= 0)
                    {
                        RedeemerBehaviors.EndLowStab();

                        bossState = PriestBossState.EndPriestVulnerable;
                    }
                    else
                    {
                        lowStabHoldTimer -= Time.deltaTime;
                    }

                    // Check if the priest has lost enough health
                    if (healthAtStartOfVulnerable - Health.currentHealth > maxHealthLossDuringVulnerabilities)
                    {
                        // And end the state of vulnerability early if so
                        RedeemerBehaviors.EndLowStab();

                        bossState = PriestBossState.EndPriestVulnerable;
                    }

                    break;

                case PriestBossState.EndPriestVulnerable:
                    foreach (GameObject obj in spawnedPlatforms)
                    {
                        Destroy(obj);
                    }

                    spawnedPlatforms.Clear();

                    // Reset attacks since last vulnerability
                    attacksSinceLastVulnerability = 0;
                    break;

                case PriestBossState.Attacking:
                    // Play some raised arms praying/cummoning anim
                    break;

                case PriestBossState.EndAttacking:
                    // Back to priest chilling anim

                    // For now just reset to idle
                    bossState = PriestBossState.Idle;
                    break;

                case PriestBossState.InitiateGodRays:
                    // Spawn the god rays
                    SpawnGodRays(numGodRays);

                    bossState = PriestBossState.GodRaying;

                    break;

                case PriestBossState.GodRaying:
                    // Check that there are rays to update
                    if (CurrentGodRays.Count == 0)
                    {
                        bossState = PriestBossState.EndGodRays;
                    }

                    // Update the god rays
                    UpdateGodRays();

                    break;

                case PriestBossState.EndGodRays:
                    // Go back to idle
                    bossState = PriestBossState.Idle;

                    break;
            }
        }
    }

    public void SpawnPlatforms()
    {
        float platformYDelta = 6;
        float platformXOffset = 0;

        int balance = 0;

        for (int i = 0; i < numPlatforms; i++)
        {
            // Spawn a new platform
            spawnedPlatforms.Add(Instantiate(PlatformPrefab, transform.position - new Vector3(platformXOffset, platformYDelta, 0), Quaternion.identity));

            // Increase the delta to spawn a platform below the last
            platformYDelta += distanceBetweenPlatforms;

            // Move the next platform to the left or right randomly
            int direction = Random.Range(0, 2) == 0 ? 1 : -1;

            // Keep teh stack of plaforms random but around the center x of teh priest
            if (balance >= 2) direction = -1;
            if (balance <= -2) direction = 1;

            // Update the balance
            balance += direction;

            platformXOffset += (this.platformXOffset + platformXOffsetIncrement * i) * direction;
        }
    }

    public void SpawnGodRays(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            //Decide spawn location
            Vector3 spawnPos = GodRaySpawnOrigin.position + new Vector3(Random.Range(godRaySpwanRadius * -1, godRaySpwanRadius), 0, 0);

            // Create a new god ray
            GodRay newGodRay = new GodRay();

            // Spawn the line renderer
            newGodRay.renderer = Instantiate(GodRayPrefab, spawnPos, Quaternion.identity).GetComponent<LineRenderer>();
            newGodRay.renderer.startWidth = godRatTrackingWidth;

            // Get the particle parent
            newGodRay.particlesParent = newGodRay.renderer.gameObject.GetComponentInChildren<ParticleSystem>().transform.parent;

            // Save particle reference
            newGodRay.trackingParticles = newGodRay.renderer.gameObject.GetComponentsInChildren<ParticleSystem>()[0].emission;
            newGodRay.explosionParticles = newGodRay.renderer.gameObject.GetComponentsInChildren<ParticleSystem>()[1].emission;

            newGodRay.edgeCollider = newGodRay.renderer.gameObject.GetComponent<EdgeCollider2D>();
            newGodRay.edgeCollider.enabled = false;

            newGodRay.initiationTimeStamp = Time.time;

            // Create a random prediction strength
            newGodRay.predictionStrength = Random.Range(godRayMinPredictionStrenght, godRayMaxPredictionStrenght);

            newGodRay.lastTargetPos = Adversary.position;

            // Save new god ray to list
            CurrentGodRays.Add(newGodRay);
        }
    }

    public void UpdateGodRays()
    {
        for (int i = 0; i < CurrentGodRays.Count; i++)
        {
            GodRay ray = CurrentGodRays[i];

            float rayLifetime = Time.time - ray.initiationTimeStamp;

            // Check the time since initiation
            if (rayLifetime > godRayTrackingTime)
            {
                // Check if it is time to execute the ray
                if (rayLifetime > godRayTrackingTime + godRayWaitTime)
                {
                    // Execute the array
                    ray.trackingParticles.enabled = false;
                    ray.explosionParticles.enabled = true;

                    ray.renderer.colorGradient = GodRayBlastGradient;
                    ray.renderer.startWidth = godRayActivatedWidth;

                    // Turn on the collider
                    ray.edgeCollider.enabled = true;

                    if (rayLifetime > godRayTrackingTime + godRayWaitTime + godRayActivatedTime)
                    {
                        // Delete the ray from the list of active rays
                        CurrentGodRays.Remove(ray);

                        // Destry it from the world
                        Destroy(ray.renderer.gameObject);

                        i--;

                        continue;
                    }
                }
                else
                {
                    // Else just let it stay put
                }
            }
            else
            {
                ray.trackingParticles.enabled = true;
                ray.explosionParticles.enabled = false;

                // We are still in the tracking period
                ray.renderer.colorGradient = GodRayWindingUpGradient;

                // Make an array to store the collider points
                Vector2[] colliderEdgePoints = new Vector2[2];
                colliderEdgePoints[0] = Vector2.zero;

                // Update vertices
                // Position zero should be the ray origin
                ray.renderer.SetPosition(0, Vector3.zero);
                CurrentGodRays[i].edgeCollider.points[0] = Vector3.zero;

                // Calc predicted point
                Vector2 predictedPosition = Adversary.position;
                // Check if the adversary has a rigidbody
                if (Adversary.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    predictedPosition += rb.velocity.normalized * ray.predictionStrength;
                }

                // Store the predicted pos for smoothing
                ray.lastTargetPos += (predictedPosition - ray.lastTargetPos) / 10;

                // Position one should be the a raycast towards the predicted position
                RaycastHit2D raycastHit = Physics2D.Raycast(ray.renderer.transform.position, ((Vector3)ray.lastTargetPos - ray.renderer.transform.position).normalized, 400, godRayLayers);

                Debug.Log("Raycast hit collider: " + raycastHit.collider);

                Vector3 raycastHitPos = raycastHit.point;
                ray.renderer.SetPosition(1, raycastHitPos - ray.renderer.transform.position);
                colliderEdgePoints[1] = raycastHitPos - ray.renderer.transform.position;

                ray.particlesParent.position = raycastHitPos;

                // Add randomness to the time by subtracting from the initiation timestamp randomly
                ray.initiationTimeStamp -= Random.Range(-5, 10f) * Time.deltaTime;

                CurrentGodRays[i].edgeCollider.points = colliderEdgePoints;

                Debug.Log("ray " + i + " timestamp: " + ray.initiationTimeStamp);
            }

            // Finish by updating the ray in the array
            CurrentGodRays[i] = ray;
        }
    }

    public void ChooseAndBeginNewAttack()
    {
        // Check the stage
        if (stage == 1)
        {
            // Update boss state
            bossState = PriestBossState.Attacking;

            // Choose a random attack based off a random number
            switch (Random.Range(0, 2))
            {
                // Targeted swing
                case 0:
                    RedeemerBehaviors.InitiateTargetedSwing();

                    break;

                // 
                case 1:
                    RedeemerBehaviors.InitiateTargetedSwing();

                    //bossState = PriestBossState.InitiateGodRays;
                    break;
            }
        }
    }
}

public enum PriestBossState
{
    Idle,
    SpawnPlatformsInit,
    SpawnPlatformsWait,
    InitLowStab,
    PriestVulnerable,
    EndPriestVulnerable,
    Attacking,
    EndAttacking,
    Waiting,
    InitiateGodRays,
    GodRaying,
    EndGodRays
}
