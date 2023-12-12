using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralQuadropedAnimation : MonoBehaviour
{
    [Header("References")]
    public Transform targetPoint;
    Vector3 raycastedNewPos;
    public LineRenderer Leg;

    public GameObject FootPlaceholder;

    Vector3 newTargetPos;
    Vector3 lastMovePos;

    [Header("Settings")]


    public float maxDist;
    public bool movingLeg = false;
    public float timeOfLastMove = 0;
    public float legmoveSpeedTimePerUnit = .4f;

    public float lefYOffset;

    [Header("Inverse Kinematics Settings")]
    public Vector3[] LegPoints;

    public int legLength = 3;
    public float boneLength = 1.5f;
    public float completeLength;

    public int iterations = 30;
    public float Delta = .04f;

    [Header("Boids")]
    public bool useBoids = false;
    public BasicBoidBehavior boidBehaviorScript;

    private void Start()
    {
        completeLength = boneLength * legLength;

        LegPoints = new Vector3[legLength];

        //Set bones
        boidBehaviorScript.numJoints = legLength;
    }

    public void Update()
    {
        raycastedNewPos = getNewTargetPos();

        if (checkDistToNextTarget() && !movingLeg)
        {
            //Check if other legs are on ground later.
            movingLeg = true;
            newTargetPos = raycastedNewPos;
            lastMovePos = FootPlaceholder.transform.position;
            timeOfLastMove = Time.time;
        }

        if (movingLeg == true)
        {
            moveLeg();
        }

        moveBones();
        updateLegIK();

        //Boid shit
        if (!useBoids) return;
        for (int i = 0; i < legLength; i++)
        {
            boidBehaviorScript.joints[i].transform.position = LegPoints[i];
        }
    }

    public bool checkDistToNextTarget()
    {
        bool dist = Vector3.Distance(raycastedNewPos, FootPlaceholder.transform.position) >= maxDist;

        //Maybe different distances?

        return dist;
    }

    public Vector3 getNewTargetPos()
    {
        RaycastHit2D ray = Physics2D.Raycast(targetPoint.position, Vector2.down, 10);

        return ray.point;
    }

    public void moveLeg(int index = 0)
    {
        float t = (Time.time - timeOfLastMove) / legmoveSpeedTimePerUnit;

        if (t >= 1)
        {
            movingLeg = false;

            return;
        }

        Vector3 newPos = Vector3.Lerp(lastMovePos, newTargetPos, t);

        newPos.y += Mathf.Sin(t * Mathf.PI) * lefYOffset;

        FootPlaceholder.transform.position = newPos;
    }

    //ADD INVERSE KINEMATICS PLEASE FUTURE TY

    //Yeah i gotchu.
    public void OnDrawGizmos()
    {
        var current = this.transform;


    }

    public void updateLegIK()
    {
        Leg.positionCount = legLength;

        for (int i = 0; i < LegPoints.Length; i++)
        {
            Leg.SetPosition(i, LegPoints[i]);
        }
    }

    public void moveBones()
    {
        Vector3 currentTargetPos = FootPlaceholder.transform.position;

        LegPoints[0] = transform.position;
        LegPoints[LegPoints.Length - 1] = currentTargetPos;

        if ((currentTargetPos - LegPoints[0]).sqrMagnitude >= completeLength * completeLength)
        {
            Vector2 direction = (currentTargetPos - LegPoints[0]).normalized;

            for (int i = 1; i < LegPoints.Length; i++)
            {
                LegPoints[i] = LegPoints[i - 1] + (Vector3) direction * boneLength;
            }
        }
        else
        {
            for (int j = 0; j < iterations; j++)
            {
                for (int k = LegPoints.Length - 1; k > 0; k--)
                {
                    if (k == LegPoints.Length - 1)
                    {
                        LegPoints[k] = currentTargetPos;
                    }
                    else
                    {
                        LegPoints[k] = LegPoints[k + 1] + (LegPoints[k] - LegPoints[k + 1]).normalized * boneLength;
                    }
                }

                for (int i = 1; i < LegPoints.Length; i++)
                {
                    LegPoints[i] = LegPoints[i - 1] + (LegPoints[i] - LegPoints[i - 1]).normalized * boneLength;
                }

                if ((LegPoints[LegPoints.Length - 1] - currentTargetPos).sqrMagnitude <= Delta * Delta)
                {
                    break;  
                }
            }
        }
    }
}
