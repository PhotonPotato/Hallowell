using UnityEngine;
using Pathfinding;

public class PathTest : MonoBehaviour
{
    public Transform origin;
    public Transform target;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetComponent<Seeker>().StartPath(
                origin.position,
                target.position
            );
        }
    }
}