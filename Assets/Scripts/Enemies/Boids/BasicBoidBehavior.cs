using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static BasicBoidBehavior;

public class BasicBoidBehavior : MonoBehaviour
{
    public struct Boid
    {
        public Vector2 position;
        public Vector2 velocity;
        public GameObject obj;

        public int targetJointIndex;

        public void updatePos()
        {
            obj.transform.position = position;
        }

        public void updateRot()
        {
            float radvalue = Mathf.Atan2(velocity.y, velocity.x);
            obj.transform.rotation = Quaternion.Euler(0,0,(radvalue - 90) * (180 / Mathf.PI));
        }
    }

    [Header("Settings")]
    public bool followProceduralAnimation;
    public int numBoids;
    public Transform BoidParent;
    public Vector2 spawnBounds;
    public float maxBoidVelocity = 8;
    public float initialMaximumVelocity = 30;

    //[Header("Rule 3", order = 1)]
    public float mergeRadius = 5;
    public float matchVelRadius = 1;
    public float avoidDist = 10;

    [Space]
    //Joint implementation
    public GameObject[] joints;
    public Transform JointParent;
    public float jointReachedDist = 1;
    public int numJoints = 2;

    [Space]
    public float MergeRuleWeight = 1;
    public float NearbyVelRuleWeight = 1;
    public float AvoidRuleWeight = 1;
    public float TargetPointInfluence = 1;

    [Space]
    public Vector2 targetPos;

    [Header("Prefabs")]
    public GameObject boidPrefab;

    [Header("Dynamic Array")]
    [SerializeField] public Boid[] boids;

    Vector2 mergeVel;
    Vector2 alignVel;
    Vector2 avoidVel;

    //Additional rule influences
    Vector2 targetPointVel;

    private void Start()
    {
        initializeJoints();
        initializeBoids();
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0)) targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        updateBoids();
    }

    public void initializeBoids()
    {
        //Init the array
        boids = new Boid[numBoids];

        for (int i = 0; i < numBoids; i++)
        {
            boids[i].obj = Instantiate(boidPrefab, new Vector3(Random.Range(spawnBounds.x * -1, spawnBounds.x), Random.Range(spawnBounds.y * -1, spawnBounds.y), 0), Quaternion.identity);

            boids[i].position = new Vector2(Random.Range(spawnBounds.x * -1, spawnBounds.x), Random.Range(spawnBounds.y * -1, spawnBounds.y));
            boids[i].velocity = new Vector2(Random.Range(initialMaximumVelocity, initialMaximumVelocity * -1), Random.Range(initialMaximumVelocity, initialMaximumVelocity * -1));

            //Init the joint type ting
            boids[i].targetJointIndex = Random.Range(0, numJoints);
            
            boids[i].obj.transform.SetParent(BoidParent);
        }
    }

    public void initializeJoints()
    {
        joints = new GameObject[numJoints];

        for (int i = 0; i < numJoints; i++)
        {
            GameObject obj = new GameObject("Joint " + i);

            obj.transform.SetParent(JointParent);
            obj.transform.position += new Vector3(i * 4, 0, 0);

            joints[i] = obj;
        }
    }

    public void updateBoids()
    {
        for (int i = 0; i < numBoids; i++)
        {
            Vector2 newVelocity = boids[i].velocity;

            //Test optimization
            optimizedAllInOne(boids[i]);

            targetPointVel = (boids[i].position - (Vector2) joints[boids[i].targetJointIndex].transform.position);

            //Apply the 3 rules
            newVelocity += mergeVel * MergeRuleWeight;//MergeToCenterOfFlock(boids[i]) * MergeRuleWeight;
            newVelocity += alignVel * NearbyVelRuleWeight;//FollowDirectionOfFlock(boids[i]) * NearbyVelRuleWeight;
            newVelocity += avoidVel * AvoidRuleWeight;//AvoidNearbyBoids(boids[i]) * AvoidRuleWeight;

            newVelocity -= targetPointVel * TargetPointInfluence;

            //Limit the speed
            newVelocity = Vector2.ClampMagnitude(newVelocity, maxBoidVelocity);

            //Update the boid
            boids[i].velocity = newVelocity;
            boids[i].position += newVelocity * Time.deltaTime;

            //Send the boids outisde bounds to other side
            if (Mathf.Abs(boids[i].position.x) > spawnBounds.x) boids[i].position.x = spawnBounds.x * .95f * Mathf.Sign(boids[i].position.x) * -1;
            if (Mathf.Abs(boids[i].position.y) > spawnBounds.y) boids[i].position.y = spawnBounds.y * .95f * Mathf.Sign(boids[i].position.y) * -1;

            //Update actual object transform
            boids[i].updatePos();
            boids[i].updateRot();

            //Update boids search for new pos
            if (Mathf.Abs(Vector2.Distance(boids[i].position, joints[boids[i].targetJointIndex].transform.position)) <= jointReachedDist)
            {
                //Choose randome joint to follow
                //boids[i].targetJointIndex = Random.Range(0, numJoints);

                //Alternate: increasing or decreasing joint index by one so that they dont just move aimlessly.
                boids[i].targetJointIndex += Random.Range(0, 2) == 1 ? 1 : -1;
                boids[i].targetJointIndex = Mathf.Clamp(boids[i].targetJointIndex, 0, numJoints - 1);
            }
        }
    }
    public void optimizedAllInOne(Boid bInit)
    {
        mergeVel = Vector2.zero;
        alignVel = Vector2.zero;
        avoidVel = Vector2.zero;

        //Counts number of boids within merge radius
        int i = 0;

        //Counts number of boids within alignRadius
        int j = 0;

        foreach (Boid bSearch in boids)
        {
            //Don't iterate over itself
            if (bInit.Equals(bSearch)) continue;

            //Merge function
            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= mergeRadius)
            {
                mergeVel += bSearch.position - bInit.position;
                i++;
            }

            //Align function
            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= matchVelRadius)
            {
                alignVel += bSearch.velocity;
                j++;
            }

            //Avoid funtction
            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= avoidDist)
            {
                avoidVel -= bSearch.position - bInit.position;
            }

            if (i == 0) mergeVel = Vector2.zero;
            else
            {
                mergeVel = (mergeVel / i);// - bInit.position;
            }

            if (j == 0) alignVel = Vector2.zero;
            else
            {
                alignVel = (alignVel / j);// - bInit.velocity;
            }
        }
    }

    public Vector2 MergeToCenterOfFlock(Boid bInit)
    {
        //Return vector
        Vector2 c = Vector2.zero;
        int i = 0;

        foreach (Boid bSearch in boids)
        {
            //Don't iterate over itself
            if (bInit.Equals(bSearch)) continue;

            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= mergeRadius)
            {
                c += bSearch.position;
                i++;
            }
        }

        //Make c the average of all nearby boid positions
        if (i != 0) c /= i;

        return c - bInit.position;
    }

    public Vector2 FollowDirectionOfFlock(Boid bInit)
    {
        //Return vector
        Vector2 c = Vector2.zero;
        int i = 0;

        foreach (Boid bSearch in boids)
        {
            //Don't iterate over itself
            if (bInit.Equals(bSearch)) continue;

            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= matchVelRadius)
            {
                c += bSearch.velocity;
                i++;
            }
        }

        //Make c the average of all nearby boid positions
        if (i != 0) c /= i;

        return c - bInit.velocity;
    }

    public Vector2 AvoidNearbyBoids(Boid bInit)
    {
        //Return vector
        Vector2 c = Vector2.zero;

        foreach(Boid bSearch in boids)
        {
            //Don't iterate over itself
            if (bInit.Equals(bSearch)) continue;

            if (Mathf.Abs(Vector2.Distance(bInit.position, bSearch.position)) <= avoidDist)
            {
                c -= bSearch.position - bInit.position;
            }
        }
        return c;
    }
}
