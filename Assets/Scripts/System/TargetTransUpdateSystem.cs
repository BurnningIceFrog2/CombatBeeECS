using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public partial class TargetTransUpdateSystem : SystemBase
{
    EntityQuery targetQuery;
    EntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        EntityQueryDesc queryDesc = new EntityQueryDesc();
        queryDesc.All = new ComponentType[] { ComponentType.ReadOnly<BeeTagComp>() };
        queryDesc.Any = new ComponentType[] { ComponentType.ReadOnly<EnemyTargetComp>(),
            ComponentType.ReadOnly<ResourceTargetComp>()
        };
        queryDesc.None = new ComponentType[] { ComponentType.ReadOnly<DeadStateComp>() };
        targetQuery = GetEntityQuery(queryDesc);
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        RequireForUpdate(targetQuery);
    }
    protected override void OnUpdate()
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Dependency=Entities.WithName("TargetTransUpdateSystem_enemy")
            .WithAll<BeeTagComp, EnemyTargetComp>()
            .WithNone<DeadStateComp>()
            .ForEach((Entity bee, int entityInQueryIndex, ref EnemyTargetComp enemyTarget) =>
            {
                if (!HasComponent<DeadStateComp>(enemyTarget.Enemy))
                {
                    enemyTarget.EnemyTrans = new Translation { Value = GetComponent<Translation>(enemyTarget.Enemy).Value };
                }
                else 
                {
                    commandBuffer.RemoveComponent<EnemyTargetComp>(entityInQueryIndex,bee);
                }
            }).ScheduleParallel(Dependency);

        Dependency=Entities.WithName("TargetTransUpdateSystem_resource")
            .WithAll<BeeTagComp, ResourceTargetComp>()
            .WithNone<DeadStateComp>()
            .ForEach((Entity bee, int entityInQueryIndex, ref ResourceTargetComp resourceTarget) =>
            {
                if (!HasComponent<DeadStateComp>(resourceTarget.Resource))
                {
                    resourceTarget.ResourceTrans = new Translation { Value = GetComponent<Translation>(resourceTarget.Resource).Value };
                }
                else 
                {
                    commandBuffer.RemoveComponent<ResourceTargetComp>(entityInQueryIndex, bee);
                }
            }).ScheduleParallel(Dependency);

        Dependency = Entities.WithName("TargetTransUpdateSystem_resource_holder")
            .WithAll<ResourceTagComp, HolderComp>()
            .WithNone<DeadStateComp>()
            .ForEach((Entity resource, int entityInQueryIndex, ref HolderComp holder) =>
            {
                holder.HolderTrans = new Translation { Value = GetComponent<Translation>(holder.Holder).Value };
            }).ScheduleParallel(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
