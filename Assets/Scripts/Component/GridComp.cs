using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 Index;
    public int StackHeight;
}
