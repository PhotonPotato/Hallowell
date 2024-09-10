using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralQuadropedAnimation : MonoBehaviour
{
    public class LegObject
    {
        public LineRenderer legRender;
        public GameObject ColObj { private get; set; }

        public Vector3[] LegPoints;
        public Vector3 targetPos;
        Vector3 lastMovePos;
        public Vector3 currentPos;

        public Vector3 raycastedNewPos;

        public float timeOfLastMove = 0;
        public bool movingLeg = false;

        public int numJoints;
        public int id;

        public float legYOffset = 1.5f;
        public float legMoveSpeedTimePerUnit = .4f;

        public float maxDist = 2.4f;

        public float distanceToNextTarget = 0;

        public float realLength = 0;

        public bool thisLegAttacking = false;

        public LegObject(int id, int numJoints, float legYOffset = 1.5f, float legMoveSpeedTimePerUnit = .4f)
        {
            this.id = id;
            this.numJoints = numJoints;

            this.legYOffset = legYOffset;
            this.legMoveSpeedTimePerUnit = legMoveSpeedTimePerUnit;

            LegPoints = new Vector3[numJoints];

            currentPos = new Vector3(0, 0, 0);
        }

        public int GetLength()
        {
            return numJoints;
        }

        public float CheckDistanceToNextTarget()
        {
            distanceToNextTarget = Vector3.Distance(raycastedNewPos, currentPos);
            return distanceToNextTarget;
        }

        public void SetNewLegMove(Vector3 newPos)
        {
            targetPos = newPos;
            timeOfLastMove = Time.time;
            lastMovePos = currentPos;

            movingLeg = true;
        }

        public void UpdateLeg()
        {
            if (!movingLeg) return;

            //Seems like we need to keep updating the target pos for the legs to keep up - ty
            if (!thisLegAttacking) targetPos = raycastedNewPos;

            //Make time to move half if attacking
            float t = (Time.time - this.timeOfLastMove) / (legMoveSpeedTimePerUnit * (thisLegAttacking ? .5f : 1f));

            if (t >= 1)
            {
                if (thisLegAttacking)
                {
                    thisLegAttacking = false;

                    //Reset health container (no clue why i put teh light attack indicator there)
                    SetAttackState(false);

                    //Move leg back into locomotion
                    SetNewLegMove(raycastedNewPos);
                }
                movingLeg = false;

                return;
            }

            Vector3 newPos = Vector3.Lerp(lastMovePos, targetPos, t);

            newPos.y += Mathf.Sin(t * Mathf.PI) * legYOffset;

            currentPos = newPos;
        }

        public float CalcRealLength()
        {
            float output = 0;

            //Get the real worldspace sum of distance between all joints
            for (int i = 1; i < numJoints; i++)
            {
                output += Vector3.Distance(LegPoints[i], LegPoints[i - 1]);
            }

            realLength = output;

            return output;
        }

        public void SetAttackState(bool lightAttack)
        {
            ColObj.GetComponent<proceduralQuadropedHealthContainer>().performingLightAttack = lightAttack;
        }
    }

    [Header("References")]
    public Transform[] targetPoints;
    public GameObject LegRendererBin;
    public LineRenderer[] LegRederers;
    public BoidfangManager BoidfangManager;

    public GameObject FootPlaceholder;

    Vector3 newTargetPos;
    Vector3 lastMovePos;

    [Header("Settings")]
    public int numLegs = 1;

    public float maxDist = 2.4f;
    public float maxRestDist = 1f;
    public bool movingLeg = false;
    public float timeOfLastMove = 0;
    public float legmoveSpeedTimePerUnit = .4f;

    public float legYOffset;

    [Header("Inverse Kinematics Settings")]
    public LegObject[] LegObjects;

    public float averageBodyHeight = 5f;

    //public Vector3[,] LegPoints;

    public int legLength = 3;
    public float boneLength = 1.5f;
    public float completeLength;

    public int iterations = 30;
    public float Delta = .04f;

    public Vector3 IKBias;

    [Header("Boids")]
    public bool useBoids = false;
    public GameObject boidManager;
    public BasicBoidBehavior[] boidBehaviorScripts;

    public int numLegsNeededOnGround = 3;

    //Other internal vars
    public int numLegsOnGround = 0;
    public int startingTotalBoids = 0;
     public int currentBoidCountTotal = 0;

    public GameObject LegColliderObjPrefab;
    public Transform LegColliderBin;
    [NonSerialized] public GameObject[] LegColliderObjects;

    [NonSerialized] public bool entityDead = false;

    private void Start()
    {
        BoidfangManager = GetComponent<BoidfangManager>();

        //LegRederers = LegRendererBin.GetComponentsInChildren<LineRenderer>();
        InitLegs();
        completeLength = boneLength * legLength;

        //Set bones
        for (int i = 0; i < numLegs; i++)
        {
            //boidBehaviorScripts[i] = boidManager.AddComponent<BasicBoidBehavior>();

            boidBehaviorScripts[i].numJoints = legLength;

            //Use this iteration for boid counter
            startingTotalBoids += boidBehaviorScripts[i].numBoids;
        }

        LegRendererBin.transform.position = transform.localPosition * -1;
    }

    public void Update()
    {
        if (entityDead) return;

        //transform.position = new(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);

        int furthestLegFromTarget = 0;

        numLegsOnGround = 0;
        for (int i = 0; i < numLegs; i++)
        {
            if (!LegObjects[i].movingLeg)
            {
                numLegsOnGround++;
            }

            LegObjects[i].raycastedNewPos = GetNewTargetPosFromPoint(targetPoints[i].position);
            //SUPER UGLY IMPLEMENTATION, FIX LATER

            //Pretty ssure this updates something so if you dont call it per leg shit gets fucked

            LegObjects[i].CheckDistanceToNextTarget();

            if (LegObjects[i].realLength >= LegObjects[furthestLegFromTarget].realLength)
            {
                furthestLegFromTarget = i;
            }
        }

        //Start iterating on the most distant leg
        for (int i = furthestLegFromTarget; i < numLegs + furthestLegFromTarget; i++)
        {
            int index = i;
            if (index >= numLegs) index -= numLegs;

            if (LegObjects[index].distanceToNextTarget >= maxDist && !LegObjects[index].movingLeg && numLegsOnGround > numLegsNeededOnGround)
            {
                LegObjects[index].SetNewLegMove(LegObjects[index].raycastedNewPos);
                numLegsOnGround--;
            }

            LegObjects[index].UpdateLeg();

            MoveBones(index);
            UpdateLegIK(index);

            //Update complete leg length every other frame
            if (Time.frameCount % 2 == 0)
            {
                LegObjects[index].CalcRealLength();
            }
        }

        if (numLegsOnGround == numLegs && LegObjects[furthestLegFromTarget].distanceToNextTarget >= maxRestDist)
        {
            LegObjects[furthestLegFromTarget].SetNewLegMove(LegObjects[furthestLegFromTarget].raycastedNewPos);
            numLegsOnGround--;
        }

        //Boid shit
        if (!useBoids) return;

        currentBoidCountTotal = 0;

        for (int i = 0; i < numLegs; i++)
        {
            for (int j = 0; j < legLength; j++)
            {
                boidBehaviorScripts[i].joints[j].transform.position = LegObjects[i].LegPoints[j];
            }

            currentBoidCountTotal += boidBehaviorScripts[i].numBoids;
        }
    }

    public Vector3 GetNewTargetPosFromPoint(Vector3 point)
    {
        RaycastHit2D ray = Physics2D.Raycast(point, Vector2.down, 100, LayerMask.GetMask("Ground", "Wall"));

        return ray.point;
    }

    public void MoveLeg(int index = 0)
    {
        float t = (Time.time - timeOfLastMove) / legmoveSpeedTimePerUnit;

        if (t >= 1)
        {
            movingLeg = false;

            return;
        }

        Vector3 newPos = Vector3.Lerp(lastMovePos, newTargetPos, t);

        newPos.y += Mathf.Sin(t * Mathf.PI) * legYOffset;

        FootPlaceholder.transform.position = newPos;
    }

    //ADD INVERSE KINEMATICS PLEASE FUTURE TY

    //Yeah i gotchu.

    public void UpdateLegIK(int legIndex)
    {
        LegObjects[legIndex].legRender.positionCount = LegObjects[legIndex].numJoints;

        for (int i = 0; i < LegObjects[legIndex].LegPoints.Length; i++)
        {
            LegObjects[legIndex].legRender.SetPosition(i, LegObjects[legIndex].LegPoints[i]);
        }
    }

    public void InitLegs()
    {
        LegObjects = new LegObject[numLegs];
        LegColliderObjects = new GameObject[numLegs];

        for (int i = 0; i < numLegs; i++)
        {
            LegObjects[i] = new LegObject(i, legLength, legYOffset, legmoveSpeedTimePerUnit);

            LegObjects[i].legRender = LegRendererBin.GetComponentsInChildren<LineRenderer>()[i];

            //Instantiate the leg collider renderers
            LegColliderObjects[i] = Instantiate(LegColliderObjPrefab, Vector3.zero, Quaternion.identity, LegColliderBin);
            LegColliderObjects[i].GetComponent<ProceduralQuadropedLegCollisionHandler>().Init(this, i);

            proceduralQuadropedHealthContainer container = LegColliderObjects[i].GetComponent<proceduralQuadropedHealthContainer>();
            container.objID = i;
            container.lightAttackDamage = BoidfangManager.damageOfLightAttack;

            LegObjects[i].ColObj = LegColliderObjects[i];
        }
    }

    public void MoveBones(int legIndex)
    {
        LegObject currentLeg = LegObjects[legIndex];
        Vector3 currentTargetPos = currentLeg.currentPos;
        int currentLegLength = currentLeg.LegPoints.Length;

        currentLeg.LegPoints[0] = transform.position;
        currentLeg.LegPoints[currentLegLength - 1] = currentTargetPos;

        //Try to add a bias

        if ((currentTargetPos - currentLeg.LegPoints[0]).sqrMagnitude >= completeLength * completeLength)
        {
            Vector2 direction = (currentTargetPos - currentLeg.LegPoints[0]).normalized;

            for (int i = 1; i < currentLegLength; i++)
            {
                currentLeg.LegPoints[i] = currentLeg.LegPoints[i - 1] + (Vector3) direction * boneLength;
            }
        }
        else
        {
            for (int j = 0; j < iterations; j++)
            {
                for (int k = currentLegLength - 1; k > 0; k--)
                {
                    if (k == currentLegLength - 1)
                    {
                        currentLeg.LegPoints[k] = currentTargetPos;
                    }
                    else
                    {
                        currentLeg.LegPoints[k] = currentLeg.LegPoints[k + 1] + (currentLeg.LegPoints[k] - currentLeg.LegPoints[k + 1]).normalized * boneLength;
                    }
                }

                for (int i = 1; i < currentLegLength; i++)
                {
                    currentLeg.LegPoints[i] = currentLeg.LegPoints[i - 1] + (currentLeg.LegPoints[i] - currentLeg.LegPoints[i - 1]).normalized * boneLength;
                    if (i != currentLegLength - 1) currentLeg.LegPoints[i] += IKBias;
                }

                if ((currentLeg.LegPoints[currentLegLength - 1] - currentTargetPos).sqrMagnitude <= Delta * Delta)
                {
                    break;  
                }
            }
        }

        currentLeg.LegPoints[currentLegLength - 1] = currentTargetPos;
    }

    public int[] GetRandomizedIndicies(int range)
    {
        int[] output = new int[range];

        //Fill array with abriritrary large value
        Array.Fill<int>(output, 999);

        for (int i = 0; i < range; i++)
        {
            output[i] = GetNewNum(UnityEngine.Random.Range(0, range), output, range, 100);
        }
        
        return output;
    }

    public int GetNewNum(int a, int[] input, int len, int lim)
    {
        if (lim == 0)
        {
            return a;
        }
        if (Array.IndexOf(input, a) == -1)
        {
            return a;
        }
        else
        {
            return GetNewNum(UnityEngine.Random.Range(0, len), input, len, lim - 1);
        }
    }

    public float CalculateBodyHeight(float windUpOffset = 0, float availableYPosition = 0)
    {
        int samples = 3;
        float sampleSpacing = .5f;

        float startX = transform.position.x - (samples / 2 * sampleSpacing);
        int j = 0;

        float sumOfSampleY = 0;

        for (int i = 0; i < samples; i++)
        {
            float currentSampleX = startX + (i * sampleSpacing);

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(currentSampleX, transform.position.y), Vector2.down, 100, LayerMask.GetMask("Wall"));

            if (hit.collider == null) break;

            j++;
            sumOfSampleY += hit.point.y;
        }

        if (j == 0) return 0;

        //Cast otherwise its int/int = int
        float legDamagedMultiplier = easeOutQuint((float)currentBoidCountTotal / startingTotalBoids);

        return (sumOfSampleY / j) + (averageBodyHeight * legDamagedMultiplier) + windUpOffset;
    }

    float easeOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }
}
