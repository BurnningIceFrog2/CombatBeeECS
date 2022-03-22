using Unity.Entities;
using Unity.Mathematics;

public struct MeshMatrixComp : IComponentData
{
    public float4x4 Value;
}
