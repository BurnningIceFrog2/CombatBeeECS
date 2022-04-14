using Unity.Entities;
using Unity.Mathematics;

public struct GridStackHeightChangeComp : IComponentData
{
    public int2 GridIndex;
    public int height;
    public int oldHeight;
}
