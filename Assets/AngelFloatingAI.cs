using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelFloatingAI : floatingStationaryAI
{
    float timeOfLastActivation = 0;

    bool followActive = false;

    public Transform adversaryTransform;

    public void Update()
    {
        if (Time.time - timeOfLastActivation > 5)
        {
            followActive = !followActive;

            timeOfLastActivation = Time.time;
        }

        originTransform = followActive ? adversaryTransform : transform.parent;

        // Run the default floating ai
        RunBaseAI();
    }
}
