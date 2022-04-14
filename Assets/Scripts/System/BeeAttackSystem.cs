using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial class BeeAttackSystem : SystemBase
{
    EntityQuery gridQuery;
    EntityQuery resourceQuery;
    EntityQuery team0beeQuery;
    EntityQuery team1beeQuery;
    BlobAssetReference<PropertiesBlob> Blob;
    EntityCommandBufferSystem commandBufferSystem;
    RandomSystem randomSystem;


    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        EntityQueryDesc bee0QueryDesc = new EntityQueryDesc();
        bee0QueryDesc.All = new ComponentType[] {ComponentType.ReadOnly<Team0TagComp>()};
        bee0QueryDesc.None = new ComponentType[] { ComponentType.ReadOnly<DeadStateComp>() };
        EntityQueryDesc bee1QueryDesc = new EntityQueryDesc();
        bee1QueryDesc.All = new ComponentType[] {ComponentType.ReadOnly<Team1TagComp>()};
        bee1QueryDesc.None = new ComponentType[] { ComponentType.ReadOnly<DeadStateComp>() };
        team0beeQuery = GetEntityQuery(bee0QueryDesc);
        team1beeQuery = GetEntityQuery(bee1QueryDesc);
        EntityQueryDesc resourcequeryDesc = new EntityQueryDesc();
        resourcequeryDesc.All = new ComponentType[] { ComponentType.ReadOnly<ResourceTagComp>(),
            ComponentType.ReadOnly<GridIndexComp>(),
            ComponentType.ReadOnly<Translation>()
        };
        resourcequeryDesc.None = new ComponentType[] { ComponentType.ReadOnly<HolderComp>(), ComponentType.ReadOnly<DeadStateComp>() };
        resourceQuery = GetEntityQuery(resourcequeryDesc);
        gridQuery = GetEntityQuery(ComponentType.ReadOnly<GridComp>());
        RequireSingletonForUpdate<GridSummaryComp>();
        Blob = PropertiesBlob.CreatePropertiesBlob();
        randomSystem = World.GetOrCreateSystem<RandomSystem>();
    }
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        GridSummaryComp gridInfo = GetSingleton<GridSummaryComp>();
        NativeArray<Entity> team0BeeArrayOG= team0beeQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> team1BeeArrayOG = team1beeQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> resourceArrayOG = resourceQuery.ToEntityArray(Allocator.TempJob);
        NativeArray <GridComp> gridArrayOG = gridQuery.ToComponentDataArray<GridComp>(Allocator.TempJob);
        NativeArray<Random> randomTLS = new NativeArray<Random>(randomSystem.randomTLS,Allocator.TempJob);
        uint seed = (uint)(UnityEngine.Random.Range(0.1f,0.8f) * uint.MaxValue);
        int team0Count = team0BeeArrayOG.Length;
        int team1Count = team1BeeArrayOG.Length;
        var commandBuffer1 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var commandBuffer2 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var commandBuffer3 = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float aggression = Blob.Value.Aggression;
        float attackDistance = Blob.Value.AttackDistance;
        float fieldSizeX = Blob.Value.FieldSize.x;
        float grabDistance = Blob.Value.GrabDistance;
        float hitDistance = Blob.Value.HitDistance;
        Dependency = Entities
            .WithName("BeeNonTargetAttack")
            .WithNone<DeadStateComp, EnemyTargetComp, ResourceTargetComp>()
            .WithAll<Translation, BeeTeamComp>()
            .WithReadOnly(randomTLS)
            .WithReadOnly(gridArrayOG)
            .WithReadOnly(team0BeeArrayOG)
            .WithReadOnly(team1BeeArrayOG)
            .WithReadOnly(resourceArrayOG)
            .ForEach((Entity bee, int entityInQueryIndex,int nativeThreadIndex, in Translation trans, in BeeTeamComp team) =>
            {
                Random r = randomTLS[nativeThreadIndex];
                r.InitState((uint)(entityInQueryIndex+ seed));
                int teamCode = team.TeamCode;
                if (r.NextFloat() < aggression)
                {//Ѱ�ҹ���Ŀ��
                    if (teamCode == 0)
                    {
                        if (team1Count > 0)
                        {
                            int enemyIndex = r.NextInt(team1Count);
                            commandBuffer1.AddComponent(entityInQueryIndex, bee, new EnemyTargetComp
                            {
                                Enemy = team1BeeArrayOG[enemyIndex],
                                EnemyTrans = new Translation { Value = GetComponent<Translation>(team1BeeArrayOG[enemyIndex]).Value },
                                IsAttacking = false
                            });
                        }
                    }
                    else
                    {
                        if (team0Count > 0)
                        {
                            int enemyIndex = r.NextInt(team0Count);
                            commandBuffer1.AddComponent(entityInQueryIndex, bee, new EnemyTargetComp
                            {
                                Enemy = team0BeeArrayOG[enemyIndex],
                                EnemyTrans = new Translation { Value = GetComponent<Translation>(team0BeeArrayOG[enemyIndex]).Value },
                                IsAttacking = false
                            });
                        }
                    }
                }
                else
                {//Ѱ����ԴĿ��
                    int resourceCount = resourceArrayOG.Length;
                    if (resourceCount > 0)
                    {//����п�����Դ�����ȡһ��ΪĿ��
                        int resourceIndex = r.NextInt(resourceCount);
                        var resourceGrid = GetComponent<GridIndexComp>(resourceArrayOG[resourceIndex]);
                        int height=Utility.GetStackHeight(gridArrayOG,resourceGrid.Value);
                        if (height >= resourceGrid.StackHeight)
                        {
                            commandBuffer1.AddComponent(entityInQueryIndex, bee, new ResourceTargetComp
                            {
                                Resource = resourceArrayOG[resourceIndex],
                                IsHoldingBySelf = false,
                                ResourceTrans = new Translation { Value = GetComponent<Translation>(resourceArrayOG[resourceIndex]).Value }
                            });
                        }
                    }
                }
            }).ScheduleParallel(Dependency);

        Dependency = Entities
            .WithName("BeeEnemyTargetAttack")
            .WithNone<DeadStateComp, ResourceTargetComp>()
            .WithAll<BeeTeamComp, EnemyTargetComp>()
            .ForEach((Entity bee, int entityInQueryIndex, ref EnemyTargetComp enemyComp, in Translation trans,in VelocityComp velocity, in BeeTeamComp team) =>
            {
                if (HasComponent<DeadStateComp>(enemyComp.Enemy))
                {
                    commandBuffer2.RemoveComponent<EnemyTargetComp>(entityInQueryIndex, bee);
                }
                else
                {
                    var enemyTrans = GetComponent<Translation>(enemyComp.Enemy);
                    float3 delta = enemyTrans.Value - trans.Value;
                    float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                    if (sqrDist < attackDistance * attackDistance)
                    {//���й���
                        enemyComp.IsAttacking = true;
                        if (sqrDist < hitDistance * hitDistance)
                        {//�ѻ��У��з��۷�����
                            commandBuffer2.RemoveComponent<EnemyTargetComp>(entityInQueryIndex, bee);
                            commandBuffer2.AddComponent(entityInQueryIndex, enemyComp.Enemy, new DeadStateComp { 
                                DeathTimer = 1f
                            });
                            var particalGenerateEntity=commandBuffer2.CreateEntity(entityInQueryIndex);
                            commandBuffer2.AddComponent(entityInQueryIndex,particalGenerateEntity,new ParticalGenerateComp { 
                                Count=6,
                                Type=0,
                                Position= enemyTrans.Value,
                                Velocity=velocity.Value*0.35f,
                                VelocityJitter=2f
                            });
                        }
                    }
                }
            }).ScheduleParallel(Dependency);

        Dependency = Entities
            .WithName("BeeResourceTargetAttack")
            .WithNone<DeadStateComp, EnemyTargetComp>()
            .WithReadOnly(gridArrayOG)
            .WithAll<BeeTeamComp, ResourceTargetComp>()
            .ForEach((Entity bee, int entityInQueryIndex, ref ResourceTargetComp resourceComp, in Translation trans, in BeeTeamComp team) =>
            {
                if (!resourceComp.IsHoldingBySelf)
                {
                    HolderComp holder;

                    if (GetComponentDataFromEntity<HolderComp>(true).TryGetComponent(resourceComp.Resource, out holder))
                    {//������۷�����
                         if (holder.HolderTeamCode != team.TeamCode)
                        {
                            commandBuffer3.AddComponent(entityInQueryIndex, bee, new EnemyTargetComp
                            {
                                Enemy = holder.Holder,
                                IsAttacking = false
                            });
                        }
                        else
                        {
                            commandBuffer3.RemoveComponent<ResourceTargetComp>(entityInQueryIndex, bee);
                        }
                    }
                    else
                    {//û�б��۷�����
                         if (HasComponent<DeadStateComp>(resourceComp.Resource))
                        {
                            commandBuffer3.RemoveComponent<ResourceTargetComp>(entityInQueryIndex, bee);
                        }
                        else if(HasComponent<StackTagComp>(resourceComp.Resource))
                        {
                            GridIndexComp gridIndexComp = GetComponent<GridIndexComp>(resourceComp.Resource);
                            int gridheight = Utility.GetStackHeight(gridArrayOG, gridIndexComp.Value);
                            if (gridIndexComp.StackHeight<gridheight)
                            {//ֻ���Զ��˵���ԴΪĿ��
                                 commandBuffer3.RemoveComponent<ResourceTargetComp>(entityInQueryIndex, bee);
                            }
                            else
                            {
                                float3 delta = GetComponent<Translation>(resourceComp.Resource).Value - trans.Value;
                                float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                                if (sqrDist <= grabDistance * grabDistance)
                                {//ץȡ��Դ
                                     commandBuffer3.AddComponent(entityInQueryIndex, resourceComp.Resource,
                                        new HolderComp
                                        {
                                            Holder = bee,
                                            HolderTrans = new Translation { Value = trans.Value },
                                            HolderTeamCode = team.TeamCode
                                        });
                                    commandBuffer3.RemoveComponent<StackTagComp>(entityInQueryIndex, resourceComp.Resource);
                                    resourceComp.IsHoldingBySelf = true;
                                    var gridChangeEntity = commandBuffer3.CreateEntity(entityInQueryIndex);
                                    var resourceGridIndex = GetComponent<GridIndexComp>(resourceComp.Resource);
                                    commandBuffer3.SetComponent(entityInQueryIndex, resourceComp.Resource,new GridIndexComp { 
                                        Value=resourceGridIndex.Value,
                                        StackHeight=0
                                    });
                                    commandBuffer3.AddComponent(entityInQueryIndex, gridChangeEntity, new GridStackHeightChangeComp
                                    {
                                        GridIndex = resourceGridIndex.Value,
                                        height = -1,
                                        oldHeight= resourceGridIndex.StackHeight
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {//����ǰ���۷�����
                     float3 targetPos = new float3(fieldSizeX * -0.45f + fieldSizeX * 0.9f * team.TeamCode, 0f, trans.Value.z);
                    float3 delta = targetPos - trans.Value;
                    float dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                    if (dist < 1f)
                    {
                        commandBuffer1.RemoveComponent<HolderComp>(entityInQueryIndex, resourceComp.Resource);
                        commandBuffer1.RemoveComponent<ResourceTargetComp>(entityInQueryIndex, bee);
                    }
                }
            }).ScheduleParallel(Dependency);
        team0BeeArrayOG.Dispose(Dependency);
        team1BeeArrayOG.Dispose(Dependency);
        resourceArrayOG.Dispose(Dependency);
        gridArrayOG.Dispose(Dependency);
        randomTLS.Dispose(Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
