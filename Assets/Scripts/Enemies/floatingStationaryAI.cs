using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class floatingStationaryAI : MonoBehaviour
{
    public Transform originTransform;
    private Vector3 originPos;

    public Vector2 velocity;
    public float maxVelocityX = 5;
    public float maxVelocityY = 5;
    public float maxDistToOrigin = 5;

    public float noiseFrequency = .3f;
    float noiseSampleSeed;
    public float speed;

    Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        noiseSampleSeed = Random.Range(0, 1000000);

        if (originTransform == null) originPos= transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //If theres a transform attached then use it as the origin posisiton.
        if (originTransform != null) originPos = originTransform.position;

        //Turn the velocity back towards the origin if it is too far away
        float distToOrgirin = Vector3.Distance(originPos, transform.position);
        if (distToOrgirin > maxDistToOrigin)
        {
            velocity += (Vector2)(originPos - transform.position).normalized * speed * distToOrgirin * Time.deltaTime * 20;// * .5f;
        }
        else
        {
            //Otherwise operate the velocity based on perlin noise.
            velocity.x += (Mathf.PerlinNoise(noiseFrequency * Time.time, noiseSampleSeed) - .5f) * speed;
            velocity.y += (Mathf.PerlinNoise(noiseFrequency * Time.time, noiseSampleSeed + 100) - .5f) * speed;
        }

        velocity.x = Mathf.Clamp(velocity.x, maxVelocityX * -1, maxVelocityX);
        velocity.y = Mathf.Clamp(velocity.y, maxVelocityY * -1, maxVelocityY);

        rb.velocity = velocity;
    }
}
