using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class BeeDeadSystem : SystemBase
{
    EntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var commandBuffer1 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var commandBuffer2 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var commandBuffer3 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = Time.DeltaTime;
        Dependency=Entities.WithName("BeeDeadSystem")
            .WithAll<DeadStateComp,BeeTagComp>()
            .ForEach((Entity entity,int entityInQueryIndex,ref DeadStateComp stateComp) =>
            {
                stateComp.DeathTimer -= deltaTime / 10f;
                if (stateComp.DeathTimer < 0) 
                {
                    var particalGenerateEntity = commandBuffer1.CreateEntity(entityInQueryIndex);
                    commandBuffer1.AddComponent(entityInQueryIndex, particalGenerateEntity,new ParticalGenerateComp { 
                        Count=1,
                        Type=0,
                        VelocityJitter=6,
                        Velocity=float3.zero,
                        Position=GetComponent<Translation>(entity).Value
                    });
                    commandBuffer1.DestroyEntity(entityInQueryIndex, entity);
                }
            }).ScheduleParallel(Dependency);

        Dependency = Entities.WithName("ResourceDeadSystem")
            .WithAll<DeadStateComp, ResourceTagComp>()
            .ForEach((Entity entity, int entityInQueryIndex,ref DeadStateComp stateComp) =>
            {
                stateComp.DeathTimer -= deltaTime;
                if (stateComp.DeathTimer < 0)
                {
                    commandBuffer2.DestroyEntity(entityInQueryIndex, entity);
                }
            }).ScheduleParallel(Dependency);

        Dependency = Entities.WithName("ParticalDeadSystem")
            .WithAll<DeadStateComp, ParticalTagComp>()
            .ForEach((Entity entity, int entityInQueryIndex, ref DeadStateComp stateComp) =>
            {
                stateComp.DeathTimer -= deltaTime;
                if (stateComp.DeathTimer < 0)
                {
                    commandBuffer3.DestroyEntity(entityInQueryIndex, entity);
                }
            }).ScheduleParallel(Dependency);

        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
