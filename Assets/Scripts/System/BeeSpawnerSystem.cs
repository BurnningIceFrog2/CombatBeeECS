using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup),OrderLast =true)]
public partial class BeeSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    RandomSystem randomSystem;
    BlobAssetReference<PropertiesBlob> blob;
    EntityQuery generateQuery;
    

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        randomSystem = World.GetOrCreateSystem<RandomSystem>();
        blob = PropertiesBlob.CreatePropertiesBlob();
        generateQuery = GetEntityQuery(ComponentType.ReadOnly<BeeGenerateComp>());
        RequireSingletonForUpdate<BeeSpawnComp>();
        RequireForUpdate(generateQuery);
    }
    protected override void OnUpdate()
    {
        var spawnerData = GetSingleton<BeeSpawnComp>();
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        NativeArray<Random> randomTLS = new NativeArray<Random>(randomSystem.randomTLS, Allocator.TempJob);
        uint seed = (uint)(UnityEngine.Random.Range(0.1f, 0.8f) * uint.MaxValue);
        float fieldSizex= blob.Value.FieldSize.x;
        Dependency=Entities.WithName("BeeSpawnerSystem")
            .WithReadOnly(randomTLS)
            .ForEach((Entity entity, int entityInQueryIndex,int nativeThreadIndex, in BeeGenerateComp generateData) =>
            {
                Random r = randomTLS[nativeThreadIndex];
                r.InitState((uint)(seed+entityInQueryIndex));
                for (int i = 0; i < generateData.BeeCount; i++)
                {
                    int teamCode = generateData.TeamCode;
                    if (teamCode == -1) 
                    {
                        teamCode = i % 2;
                    }
                    Entity bee;
                    if (teamCode == 0)
                    {
                        bee = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.BlueBeePrefab);
                        commandBuffer.AddComponent<Team0TagComp>(entityInQueryIndex, bee);
                        commandBuffer.AddComponent(entityInQueryIndex, bee, new BeeTeamComp { TeamCode = 0 });
                    }
                    else 
                    {
                        bee = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.YellowBeePrefab);
                        commandBuffer.AddComponent<Team1TagComp>(entityInQueryIndex, bee);
                        commandBuffer.AddComponent(entityInQueryIndex, bee, new BeeTeamComp { TeamCode = 1 });
                    }
                    float3 pos = new float3(1,0,0) * (-fieldSizex * .4f + fieldSizex * .8f * teamCode);
                    
                    float3 one = new float3(1,1,1);
                    float size=r.NextFloat(spawnerData.MinBeeSize, spawnerData.MaxBeeSize);
                    commandBuffer.SetComponent(entityInQueryIndex, bee, new Translation { Value = pos });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new NonUniformScale { Value = one });
                    ;
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new VelocityComp { Value = r.NextFloat3(-one,one)*spawnerData.InitVelocity });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new ColorComp { Value = new float4(1, 1, 1, 1) });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new SmoothRotationComp { 
                        SmoothPosition=pos+new float3(0.01f,0,0),
                        smoothDirection=float3.zero
                    });
                    commandBuffer.AddComponent(entityInQueryIndex,bee,new SizeComp { Size=size});
                    commandBuffer.AddComponent<BeeTagComp>(entityInQueryIndex, bee);
                }
                commandBuffer.DestroyEntity(entityInQueryIndex,entity);
            }).ScheduleParallel(Dependency);
        randomTLS.Dispose(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
