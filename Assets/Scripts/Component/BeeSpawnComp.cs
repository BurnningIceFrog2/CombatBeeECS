using Unity.Entities;
using UnityEngine;

public struct BeeSpawnComp : IComponentData
{
    public Entity BeePrefab;
    public int BeeCount;
    public float MaxBeeSize;
    public float MinBeeSize;
    public float InitVelocity;
    public float TeamAttraction;
    public float TeamRepulsion;
}
