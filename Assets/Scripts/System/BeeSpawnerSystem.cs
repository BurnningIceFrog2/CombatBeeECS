using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class BeeSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    BlobAssetReference<PropertiesBlob> blob;
    

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        blob = PropertiesBlob.CreatePropertiesBlob();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        TeamSharedComp team0 = new TeamSharedComp { TeamCode = 0 };
        TeamSharedComp team1 = new TeamSharedComp { TeamCode = 1 };
        float fieldSizex= blob.Value.FieldSize.x;
        Entities.WithName("BeeSpawnerSystem")
            .WithoutBurst()
            .ForEach((Entity entity, int entityInQueryIndex, ref BeeSpawnComp spawnerData,in LocalToWorld location) =>
            {
                for (int i = 0; i < spawnerData.BeeCount; i++)
                {
                    int teamCode = i % 2;
                    Entity bee;
                    if (teamCode == 0)
                    {
                        bee = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.BlueBeePrefab);
                        commandBuffer.AddSharedComponent(entityInQueryIndex, bee, team0);
                    }
                    else 
                    {
                        bee = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.YellowBeePrefab);
                        commandBuffer.AddSharedComponent(entityInQueryIndex, bee, team1);
                    }
                    float3 pos = new float3(1,0,0) * (-fieldSizex * .4f + fieldSizex * .8f * teamCode);
                    var position = math.transform(location.Value,pos);
                    Random r = new Random((uint)(i+1));
                    float3 one = new float3(1,1,1);
                    float size=r.NextFloat(spawnerData.MinBeeSize, spawnerData.MaxBeeSize);
                    commandBuffer.SetComponent(entityInQueryIndex, bee, new Translation { Value = position });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new NonUniformScale { Value = one });
                    ;
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new VelocityComp { Value = r.NextFloat3(one)*spawnerData.InitVelocity });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new ColorComp { Value = new float4(1, 1, 1, 1) });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new SmoothRotationComp { 
                        SmoothPosition=pos+new float3(0.01f,0,0),
                        smoothDirection=float3.zero
                    });
                    commandBuffer.AddComponent(entityInQueryIndex,bee,new SizeComp { Size=size});
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new BeeTeamComp { TeamCode = teamCode });
                    commandBuffer.AddComponent(entityInQueryIndex, bee, new BeeTagComp { });
                }
                spawnerData.BeeCount = 0;
            }).ScheduleParallel();
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
