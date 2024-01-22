using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralQuadropedAnimation : MonoBehaviour
{
    public class LegObject
    {
        public LineRenderer legRender;

        public Vector3[] LegPoints;
        public Vector3 targetPos;
        Vector3 lastMovePos;
        public Vector3 currentPos;

        public float timeOfLastMove = 0;
        public bool movingLeg = false;

        public int numJoints;
        public int id;

        public float legYOffset = 1.5f;
        public float legMoveSpeedTimePerUnit = .4f;

        public float maxDist = 2.4f;

        public float distanceToNextTarget = 0;

        public float realLength = 0;

        public LegObject(int id, int numJoints, float legYOffset = 1.5f, float legMoveSpeedTimePerUnit = .4f)
        {
            this.id = id;
            this.numJoints = numJoints;

            this.legYOffset = legYOffset;
            this.legMoveSpeedTimePerUnit = legMoveSpeedTimePerUnit;

            LegPoints = new Vector3[numJoints];

            currentPos = new Vector3(0, 0, 0);
        }

        public int getLength()
        {
            return numJoints;
        }

        public float checkDistanceToNextTarget(Vector3 raycastedNewPos)
        {
            distanceToNextTarget = Vector3.Distance(raycastedNewPos, currentPos);
            return distanceToNextTarget;
        }

        public void setNewLegMove(Vector3 newPos)
        {
            targetPos = newPos;
            timeOfLastMove = Time.time;
            lastMovePos = currentPos;

            movingLeg = true;
        }

        public void updateLeg()
        {
            if (!movingLeg) return;

            float t = (Time.time - this.timeOfLastMove) / legMoveSpeedTimePerUnit;

            if (t >= 1)
            {
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
    }

    [Header("References")]
    public Transform[] targetPoints;
    Vector3 raycastedNewPos;
    public GameObject LegRendererBin;
    public LineRenderer[] LegRederers;

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
    public float bodyMoveSpeed = 4f;

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

    public GameObject LegColliderObjPrefab;
    public Transform LegColliderBin;
    [NonSerialized] public GameObject[] LegColliderObjects;

    private void Start()
    {
        //LegRederers = LegRendererBin.GetComponentsInChildren<LineRenderer>();
        InitLegs();
        completeLength = boneLength * legLength;

        //Set bones
        for (int i = 0; i < numLegs; i++)
        {
            //boidBehaviorScripts[i] = boidManager.AddComponent<BasicBoidBehavior>();

            boidBehaviorScripts[i].numJoints = legLength;
        }
        
    }

    public void Update()
    {
        //Update body position
        Vector3 newPos = transform.position;

        //Made in desmos
        float distanceFalloffMultiplier = -1 * Mathf.Pow(.5f, Mathf.Abs(transform.position.x - Camera.main.ScreenToWorldPoint(Input.mousePosition).x)) + 1;

        newPos.x += bodyMoveSpeed * Mathf.Sign(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x) * distanceFalloffMultiplier * Time.deltaTime;
        newPos.y += (CalculateBodyHeight() - newPos.y) / 100;

        transform.position = newPos;

        //transform.position = new(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);

        int furthestLegFromTarget = 0;

        numLegsOnGround = 0;
        for (int i = 0; i < numLegs; i++)
        {
            if (!LegObjects[i].movingLeg)
            {
                numLegsOnGround++;
            }

            raycastedNewPos = GetNewTargetPosFromPoint(targetPoints[i].position);
            //SUPER UGLY IMPLEMENTATION, FIX LATER
            LegObjects[i].targetPos = raycastedNewPos;


            /*if (LegObjects[i].checkDistanceToNextTarget(raycastedNewPos) + Vector3.Distance(LegObjects[i].currentPos, transform.position)
               >= LegObjects[furthestLegFromTarget].distanceToNextTarget + Vector3.Distance(LegObjects[furthestLegFromTarget].currentPos, transform.position))
            {
                furthestLegFromTarget = i;
            }*/


            //Pretty ssure this updates something so if you dont call it per leg shit gets fucked
            LegObjects[i].checkDistanceToNextTarget(raycastedNewPos);

            if (LegObjects[i].realLength >= LegObjects[furthestLegFromTarget].realLength)
            {
                furthestLegFromTarget = i;
            }
        }
        //print(furthestLegFromTarget);

        //Start iterating on the most distant leg
        //for (int i = furthestLegFromTarget; i < numLegs + furthestLegFromTarget; i++)
        //int[] orderOfIteration = GetRandomizedIndicies(numLegs);

        //Debug.Log(orderOfIteration[0] + " " + orderOfIteration[1] + " " + orderOfIteration[2] + " " + orderOfIteration[3]);

        for (int i = furthestLegFromTarget; i < numLegs + furthestLegFromTarget; i++) //for (int i = 0; i < numLegs; i++)
        {
            int index = i;//orderOfIteration[i];
            if (index >= numLegs) index -= numLegs;

            if (LegObjects[index].distanceToNextTarget >= maxDist && !LegObjects[index].movingLeg && numLegsOnGround > numLegsNeededOnGround)
            {
                LegObjects[index].setNewLegMove(LegObjects[index].targetPos);
                numLegsOnGround--;
            }

            LegObjects[index].updateLeg();

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
            Debug.Log("move call correct");
            LegObjects[furthestLegFromTarget].setNewLegMove(LegObjects[furthestLegFromTarget].targetPos);
            numLegsOnGround--;
        }

        //Boid shit
        if (!useBoids) return;
        for (int i = 0; i < numLegs; i++)
        {
            for (int j = 0; j < legLength; j++)
            {
                boidBehaviorScripts[i].joints[j].transform.position = LegObjects[i].LegPoints[j];

            }
        }
    }

    public bool CheckDistToNextTarget()
    {
        bool dist = Vector3.Distance(raycastedNewPos, FootPlaceholder.transform.position) >= maxDist;

        //Maybe different distances?

        return dist;
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
    public void OnDrawGizmos()
    {
        var current = this.transform;
        

    }

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
            LegColliderObjects[i].GetComponent<enemyHealthContainer>().objID = i;
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

    public float CalculateBodyHeight()
    {
        float averageLegHeight = 0;
        int numLegsInCalculation = 0;

        foreach (LegObject legObject in this.LegObjects) 
        {
            if (!legObject.movingLeg)
            {
                numLegsInCalculation++;

                averageLegHeight += legObject.currentPos.y;
            }
        }

        averageLegHeight /= numLegsInCalculation;

        return averageLegHeight + averageBodyHeight;
    }
}
