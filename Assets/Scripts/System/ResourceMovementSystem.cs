using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;


public partial class ResourceMovementSystem : SystemBase
{
    EntityCommandBufferSystem commandBufferSystem;
    BlobAssetReference<PropertiesBlob> Blob;
    EntityQuery gridQuery;
    

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        Blob = PropertiesBlob.CreatePropertiesBlob();
        gridQuery = GetEntityQuery(ComponentType.ReadOnly<GridComp>());
        RequireSingletonForUpdate<GridSummaryComp>();
    }
    protected override void OnUpdate()
    {
        var commanBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        GridSummaryComp gridInfo = GetSingleton<GridSummaryComp>();
        NativeArray<GridComp> gridArrayOG = gridQuery.ToComponentDataArray<GridComp>(Allocator.TempJob);
        float resourceSize = Blob.Value.ResourceSize;
        float carryStiffness = Blob.Value.CarryStiffness;
        float snapStiffness = Blob.Value.SnapStiffness;
        int beesPerResource = Blob.Value.BeesPerResource;
        float gravity = Blob.Value.Gravity;
        float deltaTime = Time.DeltaTime;
        float3 fieldSize = Blob.Value.FieldSize;
        Dependency =Entities.WithName("ResourceMovementSystem")
            .WithNone<DeadStateComp>()
            .WithAll<ResourceTagComp>()
            .WithReadOnly(gridArrayOG)
            .ForEach((Entity resource, int entityInQueryIndex, ref Translation trans, ref VelocityComp velocity,ref GridIndexComp gridIndexComp) =>
            {
                
                HolderComp holder;
                if (GetComponentDataFromEntity<HolderComp>(true).TryGetComponent(resource, out holder))
                {
                    if (HasComponent<DeadStateComp>(holder.Holder))
                    {//如果持有者死亡
                        commanBuffer.RemoveComponent<HolderComp>(entityInQueryIndex, resource);
                    }
                    else 
                    {
                        var holderTrans = holder.HolderTrans;
                        var size = GetComponent<SizeComp>(holder.Holder);
                        var holderVelocity = holder.HolerVelocity;
                        float3 targetPos = holderTrans.Value - (resourceSize + size.Size) * new float3(0, 0.5f, 0);
                        trans.Value = math.lerp(trans.Value,targetPos,carryStiffness* deltaTime);
                        velocity.Value += holderVelocity.Value;
                    }
                }
                else if(!HasComponent<StackTagComp>(resource))
                {
                    trans.Value = math.lerp(trans.Value, Utility.NearestSnappedPos(trans.Value, gridInfo.Counts, gridInfo.MinPos, gridInfo.Size), snapStiffness * deltaTime);
                    velocity.Value.y += gravity * deltaTime;
                    trans.Value += velocity.Value * deltaTime;
                    int2 gridIndex=Utility.GetGridIndex(trans.Value, gridInfo.Counts, gridInfo.MinPos, gridInfo.Size);
                    gridIndexComp.Value = gridIndex;
                    int stackHeight = Utility.GetStackHeight(gridArrayOG, gridIndex);
                    float floorY = Utility.GetStackPos(gridIndex, stackHeight,fieldSize,resourceSize, gridInfo.Counts, gridInfo.MinPos, gridInfo.Size).y;
                    for (int j = 0; j < 3; j++)
                    {
                        if (math.abs(trans.Value[j]) > fieldSize[j] * .5f)
                        {
                            trans.Value[j] = fieldSize[j] * .5f * math.sign(trans.Value[j]);
                            velocity.Value[j] *= -.5f;
                            velocity.Value[(j + 1) % 3] *= .8f;
                            velocity.Value[(j + 2) % 3] *= .8f;
                        }
                    }
                    if (trans.Value.y < floorY)
                    {
                        trans.Value.y = floorY;
                        if (math.abs(trans.Value.x) > fieldSize.x * .4f)
                        {
                            int team = 0;
                            if (trans.Value.x > 0f)
                            {
                                team = 1;
                            }
                            var beeGenerateEntity=commanBuffer.CreateEntity(entityInQueryIndex);
                            commanBuffer.AddComponent(entityInQueryIndex,beeGenerateEntity,new BeeGenerateComp {
                                BeeCount = beesPerResource,
                                TeamCode=team
                            });
                            var particalGenerateEntity = commanBuffer.CreateEntity(entityInQueryIndex);
                            commanBuffer.AddComponent(entityInQueryIndex, beeGenerateEntity,new ParticalGenerateComp { 
                                Position=trans.Value,
                                Velocity=float3.zero,
                                Type=1,
                                Count=5,
                                VelocityJitter=6f
                            });
                            commanBuffer.AddComponent(entityInQueryIndex,resource,new DeadStateComp { DeathTimer=0.2f});
                        }
                        else
                        {
                            commanBuffer.AddComponent<StackTagComp>(entityInQueryIndex,resource);
                            int oldHeight = Utility.GetStackHeight(gridArrayOG, gridIndexComp.Value);
                            if (gridIndexComp.StackHeight == 0) 
                            {
                                gridIndexComp.StackHeight=oldHeight+1;
                                if ((gridIndexComp.StackHeight + 1) * resourceSize < fieldSize.y)
                                {
                                    var changeEntity=commanBuffer.CreateEntity(entityInQueryIndex);
                                    commanBuffer.AddComponent(entityInQueryIndex, changeEntity,new GridStackHeightChangeComp { 
                                        GridIndex= gridIndexComp.Value,
                                        height=1,
                                        oldHeight= oldHeight
                                    });
                                }
                                else
                                {
                                    commanBuffer.AddComponent(entityInQueryIndex, resource,new DeadStateComp { DeathTimer=0.2f});
                                }
                            }
                        }
                    }
                }
            }).ScheduleParallel(Dependency);
        gridArrayOG.Dispose(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
