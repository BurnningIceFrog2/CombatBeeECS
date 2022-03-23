using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class BeeSpawnerSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    //EntityArchetype beeBaseArchetype;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        /*beeBaseArchetype = EntityManager.CreateArchetype(typeof(PositionComp), typeof(ScaleComp),
            typeof(VelocityComp), typeof(ColorComp),typeof(RenderSharedComp),
            typeof(BeeTagComp), typeof(MeshMatrixComp), typeof(TeamSharedComp),
            typeof(PropertiesBlobReferenceComp)
            );*/
    }

    partial struct BeeSpawnJob : IJobEntity 
    {
        //public EntityArchetype beeBaseArchetype;
        public EntityCommandBuffer.ParallelWriter commandBuffer;
        public void Execute(ref BeeSpawnComp spawnData) 
        {
            BlobAssetReference<PropertiesBlob> blob = PropertiesBlob.CreatePropertiesBlob();
            for (int i = 0; i < spawnData.BeeCount; i++)
            {
                var bee = commandBuffer.Instantiate(i,spawnData.BeePrefab);
                //var bee = commandBuffer.CreateEntity(i, beeBaseArchetype);
                commandBuffer.SetComponent(i, bee, new PositionComp { Value=new float3(0,0,0)});
                commandBuffer.SetComponent(i, bee, new ScaleComp { Value = new float3(1, 1, 1) });
                commandBuffer.SetComponent(i, bee, new VelocityComp { Value = new float3(0, 0, 0) });
                commandBuffer.SetComponent(i, bee, new ColorComp { Value=new float4(1,1,1,1)});
                commandBuffer.SetComponent(i, bee, new BeeTagComp { });
                //commandBuffer.SetComponent(i, bee, new MeshMatrixComp { });
                commandBuffer.SetComponent(i, bee, new PropertiesBlobReferenceComp { PropertyBlob=blob});
                int teamCode = i % 2;
                commandBuffer.SetSharedComponent(i, bee, new TeamSharedComp {
                    TeamCode=teamCode, 
                    TeamAttraction=spawnData.TeamAttraction,
                    TeamRepulsion=spawnData.TeamRepulsion
                });
                //commandBuffer.SetSharedComponent(i, bee, renderData);
            }
        }
    }
    protected override void OnUpdate()
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        BlobAssetReference<PropertiesBlob> blob = PropertiesBlob.CreatePropertiesBlob();
        //commandBuffer.CreateEntity(0,beeBaseArchetype);
        //commandBuffer.CreateEntity
        Entities.WithName("BeeSpawnerSystem")
            .WithoutBurst()
            .ForEach((Entity entity, int entityInQueryIndex, ref BeeSpawnComp spawnerData) =>
            {
                for (int i = 0; i < spawnerData.BeeCount; i++)
                {
                    var bee = commandBuffer.Instantiate(entityInQueryIndex, spawnerData.BeePrefab);
                    //var bee = commandBuffer.CreateEntity(i, beeBaseArchetype);
                    commandBuffer.AddComponent(i, bee, new Translation { Value = new float3(0, 0, 0) });
                    commandBuffer.AddComponent(i, bee, new NonUniformScale { Value = new float3(1, 1, 1) });
                    commandBuffer.AddComponent(i, bee, new VelocityComp { Value = new float3(0, 0, 0) });
                    commandBuffer.AddComponent(i, bee, new ColorComp { Value = new float4(1, 1, 1, 1) });
                    commandBuffer.AddComponent(i, bee, new BeeTagComp { });
                    //commandBuffer.SetComponent(i, bee, new MeshMatrixComp { });
                    commandBuffer.AddComponent(i, bee, new PropertiesBlobReferenceComp { PropertyBlob = blob });
                    int teamCode = i % 2;
                    commandBuffer.AddSharedComponent(i, bee, new TeamSharedComp
                    {
                        TeamCode = teamCode,
                        TeamAttraction = spawnerData.TeamAttraction,
                        TeamRepulsion = spawnerData.TeamRepulsion
                    });
                    //commandBuffer.SetSharedComponent(i, bee, renderData);
                }
                spawnerData.BeeCount = 0;
                //commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();
        /*BeeSpawnJob job = new BeeSpawnJob { 
            commandBuffer=commandBuffer,
            //beeBaseArchetype=beeBaseArchetype
        };*/
        //job.ScheduleParallel();
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
