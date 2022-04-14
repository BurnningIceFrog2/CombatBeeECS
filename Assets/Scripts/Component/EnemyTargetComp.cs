using Unity.Entities;
using Unity.Transforms;

public struct EnemyTargetComp : IComponentData
{
    public Entity Enemy;
    public Translation EnemyTrans;
    public bool IsAttacking;
}
