using Unity.Entities;
using Unity.Mathematics;
[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst =true)]
public partial class GridSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    BlobAssetReference<PropertiesBlob> Blob;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        Blob = PropertiesBlob.CreatePropertiesBlob();
        RequireSingletonForUpdate<GridSpawnTagComp>();
    }
    protected override void OnUpdate()
    {
        float resourceSize = Blob.Value.ResourceSize;
        float3 fieldSize = Blob.Value.FieldSize;
        EntityArchetype gridType=EntityManager.CreateArchetype(typeof(GridComp));
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.WithName("GridSpawnerSystem")
            .WithAll<GridSpawnTagComp>()
            .ForEach((Entity entity,int entityInQueryIndex,ref GridSummaryComp gridSummary) => {
                int2 gridCounts = new int2(math.round(new float2(fieldSize.x, fieldSize.z) / resourceSize));
                float2 gridSize = new float2(fieldSize.x / gridCounts.x, fieldSize.z / gridCounts.y);
                float2 minGridPos = new float2((gridCounts.x - 1f) * -.5f * gridSize.x, (gridCounts.y - 1f) * -.5f * gridSize.y);
                commandBuffer.SetComponent(entityInQueryIndex,entity,new GridSummaryComp { 
                    Counts=gridCounts,
                    Size=gridSize,
                    MinPos=minGridPos
                });
                commandBuffer.RemoveComponent<GridSpawnTagComp>(entityInQueryIndex, entity);
                for (int i = 0; i < gridCounts.x; i++)
                {
                    for (int j = 0; j < gridCounts.y; j++)
                    {
                        var grid=commandBuffer.CreateEntity(entityInQueryIndex, gridType);
                        commandBuffer.SetComponent(entityInQueryIndex, grid,new GridComp { Index=new int2(i,j),StackHeight=0});
                    }
                }
            }).Run();
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
