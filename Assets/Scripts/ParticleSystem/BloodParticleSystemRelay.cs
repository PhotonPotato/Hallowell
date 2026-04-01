using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BloodParticleSystemRelay : MonoBehaviour
{
    private ParticleSystem ps;
    private readonly List<ParticleCollisionEvent> collisionEvents = new();

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int count = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < count; i++)
        {
            var col = collisionEvents[i];

            DecalManager.SharedInstance?.ReportBloodParticleCollision(col.intersection, col.normal);
        }
    }
}
