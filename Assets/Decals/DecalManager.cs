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

    public struct DecalData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Size;
    }

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        ActiveDecals = new List<DecalData>(maxDecals);
    }

    public void AddDecal(Vector3 position, Vector2 size, LayerMask layerMask)
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
            Rotation = Quaternion.identity,
            Size = size
        };

        ActiveDecals.Add(newDecal);
    }
}
