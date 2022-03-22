using Unity.Entities;

public struct GridComp : IComponentData
{
    public int IndexX;
    public int IndexY;
    public int StackHeight;
}
