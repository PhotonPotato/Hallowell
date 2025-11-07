using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterBeeWatcherBehavior : MonoBehaviour
{
    [Header("Refs")]
    public LineRenderer lineOfSight;
    public Transform target => PlayerManager.Instance.transform;

    public bool FacingRight = false;

    public float TimeToShoot = 0.1f;
    bool spotted = false;
    float timeSpotted = float.NegativeInfinity;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, FacingRight ? 180 : 0, 0);
       
        // Check if player is even in direction
        if (Mathf.Sign(target.position.x - transform.position.x) == (FacingRight ? 1 : -1))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position);
            
            if (hit.collider != null && hit.transform.tag == "Player" && !PlayerManager.Instance.cloaked)
            {
                if (!spotted)
                    timeSpotted = Time.time;

                spotted = true;
                lineOfSight.enabled = true;
                lineOfSight.SetPositions(new Vector3[2] { Vector3.zero, target.position - transform.position});
            }
            else
            {
                spotted = false;
                lineOfSight.enabled = false;
            }
        }


        if (spotted && Time.time - timeSpotted > TimeToShoot)
        {
            PlayerManager.Instance.DealDamageWithReset(20, true);
        }
    }
}
