using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;

public partial class BeeAttackSystem : SystemBase
{
    EntityQuery team0beeQuery;
    EntityQuery team1beeQuery;
    BlobAssetReference<PropertiesBlob> Blob;
    EntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        team0beeQuery = GetEntityQuery(ComponentType.ReadOnly<BeeTagComp>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<TeamSharedComp>()
            );
        team0beeQuery.SetSharedComponentFilter(new TeamSharedComp { TeamCode = 0 });
        team1beeQuery = GetEntityQuery(ComponentType.ReadOnly<BeeTagComp>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<TeamSharedComp>()
            );
        team1beeQuery.SetSharedComponentFilter(new TeamSharedComp { TeamCode = 1 });
        RequireForUpdate(team0beeQuery);
        RequireForUpdate(team1beeQuery);
        Blob = PropertiesBlob.CreatePropertiesBlob();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        var handler1=Entities
            .WithName("BeeNonTargetAttack")
            .WithNone<DeadStateComp, EnemyTargetComp, ResourceTargetComp>()
            .WithAll<BeeTagComp>()
            .ForEach((Entity bee, int entityInQueryIndex, in Translation trans, in BeeTeamComp team) =>
            {
               
            }).ScheduleParallel(Dependency);
        var handler2= Entities
            .WithName("BeeEnemyTargetAttack")
            .WithNone<DeadStateComp, ResourceTargetComp>()
            .WithAll<BeeTagComp, EnemyTargetComp>()
            .ForEach((Entity bee, int entityInQueryIndex, in Translation trans,in BeeTeamComp team) =>
            {

            }).ScheduleParallel(Dependency);
        var handler3 = Entities
            .WithName("BeeResourceTargetAttack")
            .WithNone<DeadStateComp, EnemyTargetComp>()
            .WithAll<BeeTagComp, ResourceTargetComp>()
            .ForEach((Entity bee, int entityInQueryIndex, in Translation trans) =>
            {

            }).ScheduleParallel(Dependency);
        Dependency=JobHandle.CombineDependencies(handler1,handler2,handler3);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
