using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
//using Unity.Mathematics;

public partial class GridStackHeightManageSystem : SystemBase
{
    EntityQuery gridChangeQuery;
    EntityCommandBufferSystem commandBufferSystem;
    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        gridChangeQuery = GetEntityQuery(ComponentType.ReadOnly<GridStackHeightChangeComp>());
        RequireForUpdate(gridChangeQuery);
    }
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var gridChangeArray = gridChangeQuery.ToComponentDataArray<GridStackHeightChangeComp>(Allocator.TempJob);
        var gridChangeEntityArray = gridChangeQuery.ToEntityArray(Allocator.TempJob);
        Dependency =Entities.WithName("GridStackHeightManageSystem")
            .WithReadOnly(gridChangeArray)
            .WithReadOnly(gridChangeEntityArray)
            .ForEach((Entity entity,int entityInQueryIndex,ref GridComp grid) =>
            {
                for (int i = 0; i < gridChangeArray.Length; i++)
                {
                    if (grid.Index.Equals(gridChangeArray[i].GridIndex)) 
                    {
                        if (grid.StackHeight == gridChangeArray[i].oldHeight) 
                        {
                            grid.StackHeight += gridChangeArray[i].height;
                            commandBuffer.DestroyEntity(entityInQueryIndex, gridChangeEntityArray[i]);
                        }
                        break;
                    }
                }
            }).ScheduleParallel(Dependency);
        gridChangeArray.Dispose(Dependency);
        gridChangeEntityArray.Dispose(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
