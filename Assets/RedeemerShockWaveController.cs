using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedeemerShockWaveController : MonoBehaviour
{
    [Header("Refs")]
    ParticleSystem.EmissionModule DirtParticles;
    public GameObject DamageZoneObject;

    float timeOfLastWave = float.NegativeInfinity;

    [Header("Settings")]
    public AnimationCurve waveVelocity;
    public float waveDistance = 10;
    public float waveDuration = .8f;

    Vector2 wavePosInit;
    Vector2 wavePosDesired;

    private bool shockwaving = false;

    public void Start()
    {
        DirtParticles = GetComponent<ParticleSystem>().emission;
    }

    public void Update()
    {
        // Only update shockwaving if its already enabled
        shockwaving = Time.time - timeOfLastWave < waveDuration;

        // Show/Hide the damage zone object
        DamageZoneObject.SetActive(shockwaving);
    }

    private void FixedUpdate()
    {
        // Check if we are currently in a wave
        if (shockwaving)
        {
            // Get the percentage compplete of such wave
            float interpolationValue = (Time.time - timeOfLastWave) / waveDuration;

            // Evaluate the animation curve for velocity to remap said percentage
            interpolationValue = waveVelocity.Evaluate(interpolationValue);

            // Linearly interpolate the position of this wave based on the interpolation value described above
            transform.position = Vector3.Lerp(wavePosInit, wavePosDesired, interpolationValue);
        }
        else
        {
            // Turn off the particle system
            DirtParticles.enabled = false;
        }
    }

    public void InitNewShockWave(Vector2 initPos, float distance = 10, float duration = .5f)
    {
        // Set up the vars
        waveDistance = distance;
        waveDuration = duration;

        // Calculate the init wave position
        RaycastHit2D hit = Physics2D.Raycast(initPos + new Vector2(0, 6), Vector2.down);
        // If no hit is found, discontinue the loop
        if (hit.collider == null) return;
        // Else update the init pos to the contact point
        wavePosInit = hit.point;

        // Do the same for the desired pos
        hit = Physics2D.Raycast(initPos + new Vector2(distance, 6), Vector2.down);
        if (hit.collider == null) return;
        wavePosDesired = hit.point;

        // Turn on the particle system
        DirtParticles.enabled = true;

        // Update the position off rip
        transform.position = wavePosInit;

        // Save the timestamp of this wave call to actually start the wave
        timeOfLastWave = Time.time;
    }
}
