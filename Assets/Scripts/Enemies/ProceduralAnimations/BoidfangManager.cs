using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidfangManager : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyFeelings
    {
        public float aggression;
        public float fear;
        public float curiosity;
        public float hunger;
        public float energy;
        public float desiredDistToEnemy;
        public float adversaryAggression;

        public void init()
        {
            hunger = Random.Range((float) 0, 1);

            curiosity = Random.Range((float)0, 1);
        }
    }

    public struct HorizontalScanResults
    {
        public float? dist;
        public float? availableY;
    }

    [System.NonSerialized] public ProceduralQuadropedAnimation animationScript;
    [System.NonSerialized] public EnemyHealthContainer m_healthContainer;

    [Header("Refs")]
    public PlayerManager PlayerManager;
    public Transform Adversary;

    [Header("Current Conditions")]
    public bool dead = false;
 
    [Header("Settings")]
    public float maxBodyMoveSpeed = 9;
    public float currentRawSpeedMult = 1;
    public float currentSmoothedSpeedMult = 1;

    [Space]
    public float hungerRate = .5f;
    public float randomPositionWaypointOffsetMaximum = 10;
    private float currentPositionWaypointOffset = 0;
    public float minimumDistanceToWaypoint = 1;

    public float timeUntilInvestigation = 5f;
    public float timeUntilGiveUpInvestigation = 5f;

    [Space]
    public float timeToLightAttack = .5f;
    public float damageOfLightAttack = 20f;
    public float lightAttackDist = 10f;
    public float distToInitiateLightAttack = 6f;
    public float attackCooldownTime = 3; //seconds
    public float attackWindUpTime = .5f; //seconds
    public float attackWindUpHeight = 1.5f; //How high this entity will rise while winding up

    [Header("Trackers")]
    public Vector2 relativePlayerDirection;
    public bool clearLineOfSight = false;
    bool lostLoS = false;
    public EnemyFeelings BoidfangFeelings;
    public Vector3 desiredStationaryPosition;
    public Vector3 lastSeenAdversaryPosition;
    public float timeSinceLostLoS = 0;
    private Vector3 entityPositionAtLostLoS;
    private Vector3 initialEntityPos;
    private float timeOfLastAttack = Mathf.NegativeInfinity;
    private float timeOfLastWindUp = Mathf.Infinity;

    private Vector3 posOfAdversaryOnAttack;
    private int legOfAttack;
    [System.NonSerialized] public float m_direction;

    [Header("Enemy Reactions")]
    public float enemyHunger = 0;
    public float anxiousness = 0;
    public bool investigating = false;
    public bool attacking = false;
    public bool windingUpForAttack = false;

    public AnimationCurve DistToAdversaryVsHunger;
    public AnimationCurve AnxiousnessVsAnxiousnessRate;
    public AnimationCurve AnxiousnessVsSkitterSpeed;
    public AnimationCurve DistToObstacleSpeedFalloff;
    public GameObject lastSeenPosMarker;
    public Transform myPosLastSeen;

    private void Start()
    {
        animationScript = GetComponent<ProceduralQuadropedAnimation>();
        m_healthContainer = GetComponent<EnemyHealthContainer>();

        PlayerManager = FindObjectOfType<PlayerManager>();

        BoidfangFeelings.init();

        initialEntityPos = transform.position;
    }

    public void Update()
    {
        if (dead) return;

        lastSeenPosMarker.transform.position = lastSeenAdversaryPosition;
        myPosLastSeen.position = entityPositionAtLostLoS;

        //Check for death
        if(m_healthContainer.currentHealth <= 0)
        {
            KillThisEnemy();
            return;
        }

        //Check for investigating condition
        investigating = !clearLineOfSight && timeSinceLostLoS >= timeUntilInvestigation && timeSinceLostLoS < timeUntilGiveUpInvestigation;

        CalcDesiredPos();

        //Pathfinding
        PathfindToPlayer();

        //Update emotions
        if (lostLoS)
        {
            //Make a curve so it gets more anxious exponentially
            anxiousness += AnxiousnessVsAnxiousnessRate.Evaluate(anxiousness);

            lastSeenAdversaryPosition = Adversary.position;
            timeSinceLostLoS = 0;

            //Update the desiredPos type ting
            entityPositionAtLostLoS = transform.position;

            //Reset lostLoS
            lostLoS = false;
        }

        //Update body position
        Vector3 newPos = transform.position;

        //Made in desmos
        float distanceFalloffMultiplier = -1 * Mathf.Pow(.5f, Mathf.Max(Mathf.Abs(desiredStationaryPosition.x - transform.position.x), 0)) + 1;

        //Smooth out the speed mult
        currentSmoothedSpeedMult += (currentRawSpeedMult - currentSmoothedSpeedMult) / 10;

        m_direction = Mathf.Sign(desiredStationaryPosition.x - transform.position.x);

        float deltaX = maxBodyMoveSpeed
                    * m_direction //Direction to desired pos
                    * currentSmoothedSpeedMult
                    * distanceFalloffMultiplier //Speed falloff as approaching adversary
                    * DistToObstacleSpeedFalloff.Evaluate(GetAvgHorizScanDistance(1, 4) ?? 10) //Avoid walls in fron of object
                    * EaseOutQuint((float)animationScript.currentBoidCountTotal / animationScript.startingTotalBoids)
                    * Time.deltaTime;
        

        //Update the body x
        newPos.x += deltaX;

        //Calc in a hiehgt offset if it is winding up
        newPos.y += (animationScript.CalculateBodyHeight(relativePlayerDirection.x > 0 ? 0 : 5, 20, windingUpForAttack ? EaseInOutQuint((Time.time - timeOfLastWindUp) / attackWindUpTime) * attackWindUpHeight : 0) - newPos.y) / 10;

        transform.position = newPos;

        //Surge attack

        //TESTING FOR ATTACK (real basic)
        if (Vector3.Distance(transform.position, Adversary.position) < distToInitiateLightAttack && Time.time - timeOfLastAttack > attackCooldownTime && windingUpForAttack == false)
        {
            //Trigger wind up
            windingUpForAttack = true;
            timeOfLastWindUp = Time.time;
        }

        if (windingUpForAttack && Time.time - timeOfLastWindUp >= attackWindUpTime)
        {
            //Now we can attack
            OnAttack();

            enemyHunger -= .3f;

            //Reset wind up
            windingUpForAttack = false;
        }

        //Shitty way to update this things attack
        if (animationScript.LegObjects[legOfAttack].thisLegAttacking == false) attacking = false;
    }

    public void KillThisEnemy()
    {
        dead = true;

        animationScript.entityDead = true;

        //Disable boid update in enemy
        foreach (BasicBoidBehavior man in animationScript.boidBehaviorScripts)
        {
            man.OnParentDeath();
        }

        //Sprite rend
        GetComponent<SpriteRenderer>().color = Color.red;

        this.gameObject.AddComponent<Rigidbody2D>();

        m_healthContainer.enabled = false;
        Destroy(m_healthContainer.barObj);

        //Destroy leg colliders
        foreach (GameObject obj in animationScript.LegColliderObjects)
        {
            Destroy(obj);
        }

        //Change this gameobjects tag so it won't damage the player
        gameObject.layer = 0;
    }

    public void PathfindToPlayer()
    {
        //Move in direciton
        relativePlayerDirection = (Adversary.position - transform.position).normalized;

        //Clear line of sight
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2) transform.position + (relativePlayerDirection * 2), relativePlayerDirection);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.tag == "Ground" || hit.transform.gameObject.layer == LayerMask.GetMask("Wall"))
            {
                //Detect a loss of LoS (clear LoS went from true to false
                if (clearLineOfSight) lostLoS = true;

                clearLineOfSight = false;

                //Upadate timers
                timeSinceLostLoS += Time.deltaTime;
                break;
            }else if (hit.transform.tag == "Player")
            {
                clearLineOfSight = true;
                break;
            }
        }

        if (clearLineOfSight && Vector3.Distance(transform.position, Adversary.position) <= 50)
        {
            enemyHunger += hungerRate * Time.deltaTime;
        }

        float distToDesiredPos = Mathf.Abs(transform.position.x - desiredStationaryPosition.x);

        if (distToDesiredPos <= minimumDistanceToWaypoint)
        {
            //Then find a new Waypoint offset (within specified range)
            currentPositionWaypointOffset = randomPositionWaypointOffsetMaximum * Random.Range(-1f, 1f);

            if (investigating) currentPositionWaypointOffset *= Random.Range(1f, 2f);
        }

        //Arbitrary value of 1.3
        if (distToDesiredPos <= randomPositionWaypointOffsetMaximum * (investigating ? 2.4f : 1.3f))
        {
            currentRawSpeedMult = AnxiousnessVsSkitterSpeed.Evaluate(anxiousness);
        }
        else
        {
            currentRawSpeedMult = 1f;
        }

        if (attacking || windingUpForAttack)
        {
            currentRawSpeedMult = 0;
            //currentSmoothedSpeedMult;
        }
    }

    float EaseOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }

    float EaseInOutQuint(float x)
    {
        return x < 0.5 ? 16 * x* x* x* x* x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    public void CalcDesiredPos()
    {
        //Calc the desired distance
        BoidfangFeelings.desiredDistToEnemy = Mathf.Max(DistToAdversaryVsHunger.Evaluate(Mathf.Min(enemyHunger, 1)), 6);
        Vector3 desiredPos = (!clearLineOfSight && !investigating) ? entityPositionAtLostLoS : Adversary.position;
        if (investigating) desiredPos = lastSeenAdversaryPosition;
        else if (timeSinceLostLoS < timeUntilGiveUpInvestigation && timeSinceLostLoS > timeUntilInvestigation) desiredPos = initialEntityPos;

        //Subtract because relPlayerDir points toward the adversary from this obj and we want the opposite.
        //ONLY IF it can see the adversary, otherwise just idle
        if (clearLineOfSight) desiredPos -= (Vector3) relativePlayerDirection * BoidfangFeelings.desiredDistToEnemy;

        desiredPos.x += currentPositionWaypointOffset;

        desiredStationaryPosition = desiredPos;
    }

    private void OnAttack()
    {
        //Check so that it only runs for one frame when attacking
        if (attacking) return;

        attacking = true;

        timeOfLastAttack = Time.time;

        posOfAdversaryOnAttack = Adversary.position;


        //Choose closest leg
        int indexOfClosestLeg = -1;
        float minDist = 100; //Stores distance to adversary of the closest leg (value is abitrarily initialized)

        for (int i = 0; i < animationScript.numLegs; i++)
        {
            //Don't bother with already moving legs
            if (animationScript.LegObjects[i].movingLeg) continue;

            float dist = Vector2.Distance(animationScript.LegObjects[i].currentPos, Adversary.position);

            if (dist < minDist)
            {
                indexOfClosestLeg = i;
                minDist = dist;
                continue;
            }
        }

        //Global variable that stores the index
        legOfAttack = indexOfClosestLeg;

        //Max out the attack distance at the light attack distance (choose the lesser)
        minDist = Mathf.Min(lightAttackDist, minDist);

        //Get the position it should move to
        Vector3 attackEndPos = transform.position + (Vector3) (relativePlayerDirection * (minDist + 3f));

        //Actually set up the attack
        animationScript.LegObjects[legOfAttack].thisLegAttacking = true;
        animationScript.LegObjects[legOfAttack].SetNewLegMove(attackEndPos);
        animationScript.LegObjects[legOfAttack].SetAttackState(true);
    }

    public float? GetAvgHorizScanDistance(float horizontalScanArea, int horizontalPrecision)
    {
        Vector2 horizontalRayOrigin = (Vector2) transform.position - new Vector2(0, horizontalScanArea / 2);

        int horizontalHits = 0;
        float totalHitDistance = 0;
        float horizontalScanIncrements = horizontalScanArea / horizontalPrecision;

        for (int i = 0; i < horizontalPrecision; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(horizontalRayOrigin, Vector2.right * m_direction, 10, LayerMask.GetMask("Wall"));

            if (hit.collider?.gameObject.tag == "Ground")
            {
                horizontalHits++;
                totalHitDistance += hit.distance;
            }

            horizontalRayOrigin.y += horizontalScanIncrements;
        }
        return horizontalHits == 0 ? null : totalHitDistance / horizontalHits;
    }
}
