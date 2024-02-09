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

    [System.NonSerialized] public ProceduralQuadropedAnimation animationScript;
    [System.NonSerialized] public enemyHealthContainer m_healthContainer;

    [Header("Refs")]
    public PlayerManager PlayerManager;
    public Transform Adversary;

    [Header("Current Conditions")]
    public bool dead = false;
 
    [Header("Settings")]
    public float maxBodyMoveSpeed = 9;
    public float currentSpeedMult = 1;
    public float hungerRate = .5f;

    public float curiosityAdversaryDistanceFloor = 10f;

    [Header("Trackers")]
    public Vector2 relativePlayerDirection;
    public bool clearLineOfSight = false;
    public EnemyFeelings BoidfangFeelings;
    public Vector3 desiredStationaryPosition;

    [Header("Enemy Reactions")]
    public AnimationCurve aggressionVsMovementSpeed;
    public AnimationCurve fearVsMovementSpeed;
    public AnimationCurve curiosityVsMovementSpeed;

    public AnimationCurve fearVsAggression;
    public AnimationCurve curiosityVsAggression;
    public AnimationCurve hungerVsAggression;

    public AnimationCurve healthVsFear;
    public AnimationCurve adversaryAggressionVsFear;

    public AnimationCurve CuriosityVsDistToAdversary;
    public AnimationCurve AggressionVsDistToAdversary;
    public AnimationCurve FearVsDistToAdversary;

    public AnimationCurve AdversaryAggressionVsFear;

    private void Start()
    {
        animationScript = GetComponent<ProceduralQuadropedAnimation>();
        m_healthContainer = GetComponent<enemyHealthContainer>();

        PlayerManager = FindObjectOfType<PlayerManager>();

        BoidfangFeelings.init();
    }

    public void Update()
    {
        if (dead) return;

        //Check for death
        if(m_healthContainer.currentHealth <= 0)
        {
            KillThisEnemy();
            return;
        }

        EvaluateAdversaryAggression();
        EvaluateCuriosity();
        EvaluateEmotions();
        CalcDesiredPos();

        //Pathfinding
        PathfindToPlayer();

        //Update body position
        Vector3 newPos = transform.position;

        //Made in desmos
        float distanceFalloffMultiplier = -1 * Mathf.Pow(.5f, Mathf.Clamp(Mathf.Abs(desiredStationaryPosition.x - transform.position.x), 0, 100)) + 1;

        //Update the body x
        newPos.x += maxBodyMoveSpeed * Mathf.Sign(desiredStationaryPosition.x - transform.position.x) * currentSpeedMult *
                    distanceFalloffMultiplier * EaseOutQuint((float)animationScript.currentBoidCountTotal / animationScript.startingTotalBoids) * Time.deltaTime;

        newPos.y += (animationScript.CalculateBodyHeight(relativePlayerDirection.x > 0 ? 0 : 5, 20) - newPos.y) / 10;

        transform.position = newPos;

        //Surge attack


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
        relativePlayerDirection = (PlayerManager.transform.position - transform.position).normalized;

        //Clear line of sight
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2) transform.position + (relativePlayerDirection * 2), relativePlayerDirection);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.tag == "Ground" || hit.transform.gameObject.layer == LayerMask.GetMask("Wall"))
            {
                clearLineOfSight = false;
                break;
            }else if (hit.transform.tag == "Player")
            {
                clearLineOfSight = true;
                break;
            }
        }
    }

    float EaseOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }

    public void EvaluateAdversaryAggression()
    {
        //Calculate the aggression of an adversary (from 0 to 1) using only the transform and rigidbody

        //TODO find new ways to calc this

        //Okay just check if the adversary is moving towards the enemy
        if (Mathf.Sign(Adversary.position.x - transform.position.x) != Mathf.Sign(Adversary.gameObject.GetComponent<Rigidbody2D>().velocity.x))
        {
            //Adversary moving toward this object (using ease in quad finnction: just squaring it)
            BoidfangFeelings.adversaryAggression += Mathf.Pow(Adversary.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude / 30, 2) * 10 * Time.deltaTime;
        }
        else
        {
            //Constant rate
            BoidfangFeelings.adversaryAggression -= Time.deltaTime;
        }

        BoidfangFeelings.adversaryAggression = Mathf.Clamp(BoidfangFeelings.adversaryAggression, 0, 1);
    }

    public void EvaluateCuriosity()
    {
        //Maybe check the distance to player is far away?
        if (!clearLineOfSight)
        { 
            BoidfangFeelings.curiosity = Mathf.Clamp(BoidfangFeelings.curiosity - Time.deltaTime, 0 , 1);
            return;
        }

        if (Vector3.Distance(transform.position, Adversary.position) < curiosityAdversaryDistanceFloor) return;

        //TODO Maybe add a more complex
        BoidfangFeelings.curiosity += Mathf.Pow(.04f, Vector3.Distance(transform.position, Adversary.position) - curiosityAdversaryDistanceFloor) * Time.deltaTime;


    }

    public void EvaluateEmotions()
    {
        //Evaluate FeAr
        float currentFear = healthVsFear.Evaluate(m_healthContainer.currentHealth / m_healthContainer.maxHealth) * ((float)2 / 3)
                          + adversaryAggressionVsFear.Evaluate(BoidfangFeelings.adversaryAggression) * 2;// ((float) 1/3);
        print("Niggers" + healthVsFear.Evaluate(m_healthContainer.currentHealth / m_healthContainer.maxHealth) * (2 / 3) + "NIggers#2 " + adversaryAggressionVsFear.Evaluate(BoidfangFeelings.adversaryAggression) * (1 / 3));
        BoidfangFeelings.fear = Mathf.Clamp(currentFear, 0, 1);

        //Eval Aggression
        float currentAggression = curiosityVsAggression.Evaluate(BoidfangFeelings.curiosity) / 3
                                + fearVsAggression.Evaluate(BoidfangFeelings.fear) / 3
                                + hungerVsAggression.Evaluate(BoidfangFeelings.hunger) / 2;

        BoidfangFeelings.aggression = Mathf.Clamp(currentAggression, 0, 1);

        //Eval dist to player based on curves and emotions
        float desiredDist = CuriosityVsDistToAdversary.Evaluate(BoidfangFeelings.curiosity) * 3//(4 / 9)
                          + AggressionVsDistToAdversary.Evaluate(BoidfangFeelings.aggression) * 2//(3 / 9)
                          + FearVsDistToAdversary.Evaluate(BoidfangFeelings.fear) * 4;// (2 / 9);

        BoidfangFeelings.desiredDistToEnemy = Mathf.Clamp(desiredDist, 0, 30);

        //Evaluate teh resulting movement speed (this will be multiplied into the maximum movement speed to modify the body's speed)
        float movementSpeed = (aggressionVsMovementSpeed.Evaluate(BoidfangFeelings.aggression) / 1.5f)
                            + (curiosityVsMovementSpeed.Evaluate(BoidfangFeelings.curiosity) / 2)
                            + (fearVsMovementSpeed.Evaluate(BoidfangFeelings.fear) / 2);

        currentSpeedMult = movementSpeed;

        BoidfangFeelings.hunger += hungerRate * Time.deltaTime;
        BoidfangFeelings.hunger = Mathf.Clamp(BoidfangFeelings.hunger, 0, 1);
    }

    public void CalcDesiredPos()
    {
        //
        Vector3 desiredPos = Adversary.position;

        //Subtract because relPlayerDir points toward the adversary from this obj and we want the opposite.
        desiredPos -= (Vector3) relativePlayerDirection * BoidfangFeelings.desiredDistToEnemy;

        desiredStationaryPosition = desiredPos;
    }
}
