using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ParticalMovementSystem : SystemBase
{
    EntityCommandBufferSystem commandBufferSystem;
    BlobAssetReference<PropertiesBlob> Blob;
    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        Blob = PropertiesBlob.CreatePropertiesBlob();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = Time.DeltaTime;
        float gravity = Blob.Value.Gravity;
        float3 fieldSize = Blob.Value.FieldSize;
        float3 up = new float3(0,1,0);
        Dependency=Entities.WithName("ParticalMovementSystem_notstuck")
            .WithNone<DeadStateComp, ParticalStuckTagComp>()
            .ForEach((Entity partical, int entityInQueryIndex,
            ref Translation trans,ref NonUniformScale scale,ref VelocityComp velocity,ref LifeComp life) =>
            {
                velocity.Value += up * gravity * deltaTime;
                trans.Value += velocity.Value * deltaTime;
                if (math.abs(trans.Value.x) > fieldSize.x * 0.5f) 
                {
                    trans.Value.x = fieldSize.x * 0.5f * math.sign(trans.Value.x);
                    float splat = math.abs(velocity.Value.x * 0.3f) + 1f;
                    scale.Value.y *= splat;
                    scale.Value.z *= splat;
                    commandBuffer.AddComponent<ParticalStuckTagComp>(entityInQueryIndex,partical);
                }
                if (math.abs(trans.Value.y) > fieldSize.y * 0.5f)
                {
                    trans.Value.y = fieldSize.y * 0.5f * math.sign(trans.Value.y);
                    float splat = math.abs(velocity.Value.y * 0.3f) + 1f;
                    scale.Value.x *= splat;
                    scale.Value.z *= splat;
                    commandBuffer.AddComponent<ParticalStuckTagComp>(entityInQueryIndex, partical);
                }
                if (math.abs(trans.Value.z) > fieldSize.z * 0.5f)
                {
                    trans.Value.z = fieldSize.z * 0.5f * math.sign(trans.Value.z);
                    float splat = math.abs(velocity.Value.z * 0.3f) + 1f;
                    scale.Value.x *= splat;
                    scale.Value.y *= splat;
                    commandBuffer.AddComponent<ParticalStuckTagComp>(entityInQueryIndex, partical);
                }
                life.LifeTime -= deltaTime / life.LifeDuration;
                if (life.LifeTime < 0) 
                {
                    commandBuffer.AddComponent(entityInQueryIndex, partical,new DeadStateComp { DeathTimer=0.1f});
                }
            }).ScheduleParallel(Dependency);

        Dependency=Entities.WithName("ParticalMovementSystem_stuck")
            .WithNone<DeadStateComp>()
            .WithAll<ParticalStuckTagComp>()
            .ForEach((Entity partical, int entityInQueryIndex,
            ref Translation trans, ref NonUniformScale scale, ref VelocityComp velocity, ref LifeComp life) =>
            {
                life.LifeTime -= deltaTime / life.LifeDuration;
                if (life.LifeTime < 0)
                {
                    commandBuffer.AddComponent(entityInQueryIndex, partical, new DeadStateComp { DeathTimer = 0.1f });
                }
            }).ScheduleParallel(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
