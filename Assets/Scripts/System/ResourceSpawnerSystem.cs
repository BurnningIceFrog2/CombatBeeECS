using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ResourceSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    RandomSystem randomSystem;
    BlobAssetReference<PropertiesBlob> Blob;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        randomSystem = World.GetOrCreateSystem<RandomSystem>();
        Blob = PropertiesBlob.CreatePropertiesBlob();
        RequireSingletonForUpdate<GridSummaryComp>();
    }
    protected override void OnUpdate()
    {
        NativeArray<Random> randomTLS = new NativeArray<Random>(randomSystem.randomTLS,Allocator.TempJob);
        var gridInfo=GetSingleton<GridSummaryComp>();
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float3 fieldSize = Blob.Value.FieldSize;
        float resourceSize = Blob.Value.ResourceSize;
        Dependency=Entities.WithName("ResourceSpawnerSystem")
            .WithReadOnly(randomTLS)
             .ForEach((Entity entity,int entityInQueryIndex,int nativeThreadIndex, ref ResourceSpawnerComp spawnData,in LocalToWorld location) => {
                 Random r = randomTLS[nativeThreadIndex];
                 for (int i = 0; i < spawnData.Count; i++)
                 {
                     var resource=commandBuffer.Instantiate(entityInQueryIndex,spawnData.Prefab);
                     float3 pos = new float3(gridInfo.MinPos.x * .25f + r.NextFloat(1.0f) * fieldSize.x * .25f, r.NextFloat(1.0f) * 10f, gridInfo.MinPos.y + r.NextFloat(1.0f) * fieldSize.z);
                     commandBuffer.AddComponent<ResourceTagComp>(entityInQueryIndex,resource);
                     commandBuffer.AddComponent(entityInQueryIndex, resource,new VelocityComp { Value=float3.zero});
                     commandBuffer.SetComponent(entityInQueryIndex, resource,new Translation { Value=pos});
                     commandBuffer.AddComponent(entityInQueryIndex, resource, new GridIndexComp {Value=int2.zero,StackHeight=0 });
                 }
                 spawnData.Count = 0;
             }).ScheduleParallel(Dependency);
        randomTLS.Dispose(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
