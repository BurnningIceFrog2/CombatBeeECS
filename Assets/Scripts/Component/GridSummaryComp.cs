using Unity.Entities;
using Unity.Mathematics;

public struct GridSummaryComp : IComponentData
{
    public int2 Counts;
    public float2 Size;
    public float2 MinPos;
    public float Gravity;
}
