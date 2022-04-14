using Unity.Entities;
using Unity.Transforms;

public struct ResourceTargetComp : IComponentData
{
    public Entity Resource;
    public bool IsHoldingBySelf;
    public Translation ResourceTrans;
}
