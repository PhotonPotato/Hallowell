using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralQuadropedLegCollisionHandler : MonoBehaviour
{
    public int legID;
    public ProceduralQuadropedAnimation parentAnimationScript;
    [System.NonSerialized] public EdgeCollider2D legCollider;

    public void Init(ProceduralQuadropedAnimation animScript, int id = 0)
    {
        if (animScript == null) return;

        parentAnimationScript = animScript;
        legID = id;

        legCollider = GetComponent<EdgeCollider2D>();
    }

    //Update is called once per frame
    void Update()
    {
        if (parentAnimationScript == null) return;

        UpdateLegCollider();
    }

    public void UpdateLegCollider()
    {
        List<Vector2> pointList = new List<Vector2>();

        //Drop legPoints into List<Vector2> to be passed into edge collider
        foreach(Vector3 point in parentAnimationScript.LegObjects[legID].LegPoints)
        {
            pointList.Add(point);
        }

        legCollider.SetPoints(pointList);
    }
}
