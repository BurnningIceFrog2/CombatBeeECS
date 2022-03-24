using Unity.Entities;
using Unity.Transforms;

public struct EnemyTargetComp : IComponentData
{
    public Translation EnemyTrans;
    public bool IsAttacking;
}
