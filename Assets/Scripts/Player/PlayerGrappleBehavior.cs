using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappleBehavior : MonoBehaviour
{
    private bool isGrappling = false;

    [Header("Refs")]
    PlayerManager manager;
    public LineRenderer GrappleLine;
    public Transform GrappleHead;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 5;
    public float timeToGrapple = .2f;
    private int grappleDir;
    public LayerMask hitLayers;

    public AnimationCurve grapplelineOutreachOverTime;
    public AnimationCurve grappleHeadScaleOverTime;

    [Header("Trackers")]
    [SerializeField] private Vector3 grappleStartPoint;
    [SerializeField] private Vector3 grappleEndPoint;
    [SerializeField] private float timeOfLastGrapple = float.NegativeInfinity;
    private bool grapplingEnemy;
    private RaycastHit2D lastHit;

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        GrappleLine.useWorldSpace = true;
    }

    public void FixedUpdate()
    {
        if (Input.GetKey("f"))
        {
            // Initate a grapple

            // Check if there is a target within range
            grappleDir = (manager.playerFacingRight ? 1 : -1) * (manager.playerMovement.onWall ? -1 : 1);

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, .5f, Vector2.right * grappleDir, maxGrappleDistance, hitLayers);

            if (hit)
            {
                isGrappling = true;
                timeOfLastGrapple = Time.time;

                grappleStartPoint = transform.position;
                grappleEndPoint = hit.point;

                // Set this lil flag fo shits 
                grapplingEnemy = hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy");

                lastHit = hit;
            }

            // Check if its an enemy or not

        }

        if (isGrappling)
        {
            manager.playerMovement.disableDefaultPlayerMovement = true;

            float t = Time.time - timeOfLastGrapple;

            if (t > timeToGrapple)
            {
                isGrappling = false;
                manager.playerMovement.disableDefaultPlayerMovement = false;

                if (grapplingEnemy)
                {
                    // Deal damage
                    EnemyHealthContainer container = lastHit.collider.GetComponent<EnemyHealthContainer>();
                    container.DealDamage(5);

                    // Player bounce
                    manager.playerMovement.ApplyStompYForce();

                    ScreenShakeManager.Instance?.InitiateDefaultSinShake();
                }
                else
                    // Make the shake proportional to the distance of the grapple
                    ScreenShakeManager.Instance?.InitiateSinShake(.08f * lastHit.distance / 18, .3f, 40);
            }
            else
                transform.position = Vector3.Lerp(grappleStartPoint, grappleEndPoint, easeInOutQuint(t / timeToGrapple));

            // Update the grapple visuals
            // LINE:
            Vector3 grappleHeadPos = Vector3.Lerp(transform.position, grappleEndPoint, grapplelineOutreachOverTime.Evaluate(t / timeToGrapple));

            Vector3[] grappleLineVerts = new Vector3[2]
            {
                transform.position,
                grappleHeadPos
            };

            GrappleLine.SetPositions(grappleLineVerts);
            GrappleLine.enabled = true;

            // HEAD:
            GrappleHead.position = grappleHeadPos;
            GrappleHead.rotation = Quaternion.Euler(0, 0, grappleDir >= 1 ? -90 : 90);
            GrappleHead.localScale = new Vector3(0.1f, 0.3f, 1) * grappleHeadScaleOverTime.Evaluate(t / timeToGrapple);

            GrappleHead.gameObject.SetActive(true);
        }
        else
        {
            GrappleLine.enabled = false;
            GrappleHead.gameObject.SetActive(false);   
        }
    }

    private float easeInOutQuint(float x)
    {
        return x< 0.5 ? 16 * Mathf.Pow(x, 5) : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }
}
