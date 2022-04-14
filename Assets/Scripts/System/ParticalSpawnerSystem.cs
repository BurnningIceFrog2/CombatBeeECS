using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ParticalSpawnerSystem : SystemBase
{
    EntityQuery particalquery;
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    RandomSystem randomSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        randomSystem = World.GetOrCreateSystem<RandomSystem>();
        particalquery = GetEntityQuery(ComponentType.ReadOnly<ParticalGenerateComp>());
        RequireForUpdate(particalquery);
        RequireSingletonForUpdate<ParticalSpawnerComp>();
    }
    protected override void OnUpdate()
    {
        var commandBuffer= commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        NativeArray<Random> randomTLS = randomSystem.randomTLS;
        var spawnData = GetSingleton<ParticalSpawnerComp>();
        uint seed = (uint)(UnityEngine.Random.Range(0.1f, 0.8f) * uint.MaxValue);
        float3 one = new float3(1, 1, 1);
        Entities.WithName("ParticalSpawnerSystem")
            .WithReadOnly(randomTLS)
            .ForEach((Entity entity, int entityInQueryIndex,int nativeThreadIndex, in ParticalGenerateComp generateData) =>
            {
                Random r = randomTLS[nativeThreadIndex];
                r.InitState((uint)(entityInQueryIndex + seed));
                for (int i = 0; i < generateData.Count; i++)
                {
                    Entity particalEntity;
                    if (generateData.Type == 0)
                    {
                        particalEntity = commandBuffer.Instantiate(entityInQueryIndex, spawnData.BloodPrefab);
                        float3 velocity = generateData.Velocity + r.NextFloat3(-one, one) * generateData.VelocityJitter;
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity,new VelocityComp { Value= velocity });
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity,new LifeComp
                        {
                            LifeTime = 1f,
                            LifeDuration=r.NextFloat(3f,5f)
                        });
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity, new NonUniformScale { Value = one * r.NextFloat(0.1f, 0.2f) });
                    }
                    else 
                    {
                        particalEntity = commandBuffer.Instantiate(entityInQueryIndex, spawnData.WhitePrefab);
                        float3 velocity = generateData.Velocity + r.NextFloat3(-one, one) * 5f;
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity, new VelocityComp { Value = velocity });
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity, new LifeComp
                        {
                            LifeTime = 1f,
                            LifeDuration = r.NextFloat(0.25f, 0.5f)
                        });
                        commandBuffer.AddComponent(entityInQueryIndex, particalEntity, new NonUniformScale { Value = one * r.NextFloat(1f, 2f) });
                    }
                    commandBuffer.SetComponent(entityInQueryIndex, particalEntity,new Translation { Value=generateData.Position});
                    commandBuffer.AddComponent<ParticalTagComp>(entityInQueryIndex, particalEntity);
                }
                commandBuffer.DestroyEntity(entityInQueryIndex,entity);
            }).ScheduleParallel();
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
