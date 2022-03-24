using Unity.Entities;
using Unity.Mathematics;

public struct SmoothRotationComp : IComponentData
{
    public float3 SmoothPosition;
    public float3 smoothDirection;
}
