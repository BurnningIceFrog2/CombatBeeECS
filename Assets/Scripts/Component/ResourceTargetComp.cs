using Unity.Entities;
using Unity.Transforms;

public struct ResourceTargetComp : IComponentData
{
    public Translation ResourceTrans;
    public bool IsHolding;
}
