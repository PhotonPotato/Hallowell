using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class floatingStationaryAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform originTransform;
    public Animator animator;

    private Vector3 originPos;

    [Header("Settings/Trackers")]
    public Vector2 velocity;
    public float maxVelocityX = 5;
    public float maxVelocityY = 5;
    public float maxDistToOrigin = 5;

    public float noiseFrequency = .3f;
    float noiseSampleSeed;
    public float speed;

    public float velocityDampening = .9f;

    Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        noiseSampleSeed = Random.Range(0, 1000000);
        
        if (originTransform == null) originPos= transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        RunBaseAI();
    }

    // Base movement functionality for stationary ai
    protected void RunBaseAI()
    {
        //If theres a transform attached then use it as the origin posisiton.
        if (originTransform != null) originPos = originTransform.position;

        //Turn the velocity back towards the origin if it is too far away
        float distToOrgirin = Vector3.Distance(originPos, transform.position);
        if (distToOrgirin > maxDistToOrigin)
        {
            velocity += (Vector2)(originPos - transform.position).normalized * speed * distToOrgirin * Time.deltaTime * 20;
        }
        else
        {
            //Otherwise operate the velocity based on perlin noise.
            velocity.x += (Mathf.PerlinNoise(noiseFrequency * Time.time, noiseSampleSeed) - .5f) * speed;
            velocity.y += (Mathf.PerlinNoise(noiseFrequency * Time.time, noiseSampleSeed + 100) - .5f) * speed;
        }

        velocity.x = Mathf.Clamp(velocity.x, maxVelocityX * -1, maxVelocityX);
        velocity.y = Mathf.Clamp(velocity.y, maxVelocityY * -1, maxVelocityY);

        // Check the dot product to find out if the thing is flying away from the desired position
        if (Vector2.Dot(velocity, (Vector2) (originPos - transform.position)) < 0)
        {
            // Dampen the velocity if it is
            velocity *= .9f;
        }

        rb.velocity = velocity;
    }
}
