using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public static DecalManager SharedInstance { get; private set; }

    [SerializeField] private int maxDecals = 10000;
    [SerializeField] private float replacementDistance = 0.1f;
    [SerializeField] private LayerMask layerMask;

    [System.NonSerialized] public List<DecalData> ActiveDecals;

    public float BloodParticleSize;
    public float BloodParticlePlacementMult = 1.5f;

    public struct DecalData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Size;
        public Color Color;
    }

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
            DontDestroyOnLoad(transform.parent);
        }
        else Destroy(this.gameObject);

        ActiveDecals = new List<DecalData>(maxDecals);
    }

    public void AddDecal(
        Vector3 position,
        Quaternion rotation,
        Vector2 size,
        LayerMask layerMask,
        Color? color = null
        )
    {
        Collider2D hit = Physics2D.OverlapPoint(position, layerMask);

        if (hit == null) return;

        // Remove close decals
        for (int i = 0; i < ActiveDecals.Count; i++)
        {
            if ((ActiveDecals[i].Position - position).sqrMagnitude < replacementDistance * replacementDistance)
            {
                // TODO: Make original decal bigger
                ActiveDecals.RemoveAt(i);
                i--;
            }
        }

        if (ActiveDecals.Count >= maxDecals) ActiveDecals.RemoveAt(0);

        DecalData newDecal = new DecalData
        {
            Position = position,
            Rotation = rotation,
            Size = size,
            Color = color ?? new Color(168 / 255f, 20 / 255f, 29 / 255f) + .1f * Color.Lerp(Color.white, Color.black, Random.Range(0.0f, 1f))      
        };

        ActiveDecals.Add(newDecal);
    }

    public void ReportBloodParticleCollision(Vector3 partPos, Vector3 normal)
    {
        partPos.z -= .001f;

        Debug.Log(Mathf.Atan2(-Mathf.RoundToInt(normal.x), Mathf.RoundToInt(normal.y)) * Mathf.Rad2Deg);
        AddDecal(
            partPos - normal.normalized * BloodParticlePlacementMult,
            rotation: Quaternion.Euler(0, 0, Mathf.Atan2(-Mathf.RoundToInt(normal.x), Mathf.RoundToInt(normal.y)) * Mathf.Rad2Deg),
            size: Vector2.one * (BloodParticleSize) + new Vector2(+Random.Range(0, .6f), 0),
            layerMask
        );
    }
}
