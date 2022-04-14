using Unity.Entities;
using Unity.Mathematics;

public struct ParticalGenerateComp : IComponentData
{
    public float3 Position;
    public float3 Velocity;
    public int Type;
    public int Count;
    public float VelocityJitter;
}
