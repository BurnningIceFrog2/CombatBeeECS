using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Unity.Collections.LowLevel.Unsafe;

public partial class BeeMovementSystem : SystemBase
{
    EntityQuery beeQuery;
    EntityQuery team0beeQuery;
    EntityQuery team1beeQuery;
    BlobAssetReference<PropertiesBlob> Blob;
    RandomSystem randomSystem;

    protected override void OnCreate()
    {
        beeQuery = GetEntityQuery(ComponentType.ReadOnly<BeeTagComp>(),
            ComponentType.ReadOnly<BeeTeamComp>(),
           ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<VelocityComp>()
           );
        team0beeQuery = GetEntityQuery(
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadOnly<Team0TagComp>()
            );
        team1beeQuery = GetEntityQuery(
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadOnly<Team1TagComp>()
            );
        Blob = PropertiesBlob.CreatePropertiesBlob();
        randomSystem = World.GetOrCreateSystem<RandomSystem>();
    }
    partial struct BeeMoveJob : IJobEntityBatch
    {
        public float DeltaTime;
        [ReadOnly]
        public uint Seed;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> Team0Array;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> Team1Array;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Random> randomTLS;
        [ReadOnly] public BlobAssetReference<PropertiesBlob> property;
        public ComponentTypeHandle<Translation> TranslationHandler;
        public ComponentTypeHandle<VelocityComp> VelocityHandler;
        public ComponentTypeHandle<SmoothRotationComp> SmoothHandler;
        [NativeSetThreadIndex]
        private int threadIndex;
        [ReadOnly]
        public ComponentTypeHandle<BeeTagComp> BeeHandler;
        [ReadOnly]
        public ComponentTypeHandle<BeeTeamComp> TeamHandler;
        [ReadOnly]
        public ComponentTypeHandle<DeadStateComp> DeadStateHandler;
        [ReadOnly]
        public ComponentTypeHandle<EnemyTargetComp> EnemyHandler;
        [ReadOnly]
        public ComponentTypeHandle<ResourceTargetComp> ResourceHandler;
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<Translation> transArray = batchInChunk.GetNativeArray(TranslationHandler);
            NativeArray<VelocityComp> velocityArray = batchInChunk.GetNativeArray(VelocityHandler);
            NativeArray<EnemyTargetComp> enemyArray = batchInChunk.GetNativeArray(EnemyHandler);
            NativeArray<ResourceTargetComp> resourceArray = batchInChunk.GetNativeArray(ResourceHandler);
            NativeArray<SmoothRotationComp> smoothArray = batchInChunk.GetNativeArray(SmoothHandler);
            NativeArray<BeeTeamComp> teamArray = batchInChunk.GetNativeArray(TeamHandler);
            bool hasDead = batchInChunk.Has(DeadStateHandler);
            bool hasEnemyTarget = batchInChunk.Has(EnemyHandler);
            bool hasResourceTarget = batchInChunk.Has(ResourceHandler);
            if (hasDead)
            {
                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var velocity = velocityArray[i];
                    var trans = transArray[i];
                    velocity.Value.y += property.Value.Gravity * DeltaTime;
                    DealWithPosition(ref trans,ref velocity);
                    var smooth = smoothArray[i];
                    DealWithSmoothRotation(ref trans, ref smooth, false);
                    transArray[i] = new Translation { Value = trans.Value };
                }
            }
            else 
            {
                Random r = randomTLS[threadIndex];
                r.InitState((uint)(Seed+ batchIndex));
                float flightJitter = property.Value.FlightJitter;
                float damping = property.Value.Damping;
                float3 sphereFloat3 = new float3(1,1,1);
                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var velocity = velocityArray[i];
                    var trans = transArray[i];
                    //Debug.Log("update==satrt=="+velocity.Value);
                    velocity.Value+= r.NextFloat3(-sphereFloat3,sphereFloat3) * (flightJitter * DeltaTime);
                    velocity.Value *= (1f - damping);
                    //Debug.Log("update==satrt2=="+velocity.Value);
                    int teamCode = teamArray[i].TeamCode;
                    if (teamArray[i].TeamCode == 0)
                    {
                        MoveInfluencedByTeamMate(ref trans, ref velocity, Team0Array, r);
                    }
                    else 
                    {
                        MoveInfluencedByTeamMate(ref trans, ref velocity, Team1Array, r);
                    }
                    bool isAttacking = false;
                    if (hasEnemyTarget)
                    {
                        isAttacking = enemyArray[i].IsAttacking;
                        MoveInfluencedByEnemy(ref trans, ref velocity, i, enemyArray);
                    }
                    else if(hasResourceTarget)
                    {
                        //Debug.Log("batchIndex=" + batchIndex + ": MoveInfluencedByResource==beforetrans==" + velocity.Value);
                        MoveInfluencedByResource(ref trans,ref velocity,i,teamCode,resourceArray);
                        //Debug.Log("batchIndex=" + batchIndex + ": MoveInfluencedByResource==aftertrans==" + velocity.Value);
                    }
                    if (hasResourceTarget)
                    {
                        //Debug.Log("batchIndex=" + batchIndex + ": hasResourceTarget==beforetrans==" + trans.Value);
                        DealWithPosition(ref trans, ref velocity, resourceArray[i]);
                        //Debug.Log("batchIndex=" + batchIndex + ": hasResourceTarget==aftertrans==" + trans.Value);
                    }
                    else 
                    {
                        DealWithPosition(ref trans,ref velocity);
                    }
                    //Debug.Log("batchIndex=" + batchIndex + "after update=="+velocity.Value);
                    var smooth = smoothArray[i];
                    DealWithSmoothRotation(ref trans,ref smooth , isAttacking);
                    transArray[i] = new Translation { Value = trans.Value };
                    velocityArray[i] = new VelocityComp { Value = velocity.Value };
                    smoothArray[i] = new SmoothRotationComp { SmoothPosition = smooth.SmoothPosition, smoothDirection = smooth.smoothDirection };
                }
            }
        }
        private void DealWithPosition(ref Translation trans, ref VelocityComp velocity)
        {
            trans.Value += DeltaTime * velocity.Value;
            if (math.abs(trans.Value.x) > property.Value.FieldSize.x * 0.5f)
            {
                trans.Value.x = property.Value.FieldSize.x * 0.5f * math.sign(trans.Value.x);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
            if (math.abs(trans.Value.z) > property.Value.FieldSize.z * 0.5f)
            {
                trans.Value.z = property.Value.FieldSize.z * 0.5f * math.sign(trans.Value.z);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
            if (math.abs(trans.Value.y) > property.Value.FieldSize.y * 0.5f)
            {
                trans.Value.y = property.Value.FieldSize.y * 0.5f * math.sign(trans.Value.y);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
        }
        private void DealWithPosition(ref Translation trans,ref VelocityComp velocity,ResourceTargetComp resourceComp) 
        {
            trans.Value += DeltaTime * velocity.Value;
            if (math.abs(trans.Value.x) > property.Value.FieldSize.x * 0.5f) 
            {
                trans.Value.x = property.Value.FieldSize.x * 0.5f * math.sign(trans.Value.x);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
            if (math.abs(trans.Value.z) > property.Value.FieldSize.z * 0.5f)
            {
                trans.Value.z = property.Value.FieldSize.z * 0.5f * math.sign(trans.Value.z);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
            float resourceModifier = 0f;
            if (resourceComp.IsHoldingBySelf) 
            {
                resourceModifier = property.Value.ResourceSize;
            }
            if (math.abs(trans.Value.y) > (property.Value.FieldSize.y * 0.5f- resourceModifier))
            {
                trans.Value.y = (property.Value.FieldSize.y * 0.5f- resourceModifier) * math.sign(trans.Value.y);
                velocity.Value.x *= -0.5f;
                velocity.Value.y *= 0.8f;
                velocity.Value.z *= 0.8f;
            }
        }
        private void DealWithSmoothRotation(ref Translation trans,ref SmoothRotationComp smoothCom,bool isAttacking) 
        {
            float3 oldSmooth = smoothCom.SmoothPosition;
            if (isAttacking)
            {
                float rotationStiffness = property.Value.RotationStiffness;
                smoothCom.SmoothPosition = math.lerp(smoothCom.SmoothPosition,trans.Value, rotationStiffness*DeltaTime);
            }
            else 
            {
                smoothCom.SmoothPosition = trans.Value;
            }
            smoothCom.smoothDirection = smoothCom.SmoothPosition - oldSmooth;
        }
        private void MoveInfluencedByTeamMate(ref Translation trans,ref VelocityComp velocity, NativeArray<Translation> teamArray,Random r) 
        {
            float teamAttraction = property.Value.TeamAttraction;
            float teamRepulsion = property.Value.TeamRepulsion;
            int attractiveFriendIndex = r.NextInt(teamArray.Length);
            int repellentFriendIndex = r.NextInt(teamArray.Length);
            var attractiveFriendTrans = teamArray[attractiveFriendIndex];
            var repellentFriendTrans = teamArray[repellentFriendIndex];
            float3 delta = attractiveFriendTrans.Value - trans.Value;
            float dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
            if (dist > 0f)
            {
                velocity.Value += delta * (teamAttraction * DeltaTime / dist);
            }
            delta = repellentFriendTrans.Value - trans.Value;
            dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
            if (dist > 0f)
            {
                velocity.Value -= delta * (teamRepulsion * DeltaTime / dist);
            }
        }
        private void MoveInfluencedByEnemy(ref Translation trans, ref VelocityComp velocity,int index, NativeArray<EnemyTargetComp> enemyArray) 
        {
            var enemyTrans = enemyArray[index].EnemyTrans;
            float3 delta = enemyTrans.Value - trans.Value;
            float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
            float attackDistance = property.Value.AttackDistance;
            if (sqrDist > attackDistance * attackDistance)
            {
                float chaseForce = property.Value.ChaseForce;
                velocity.Value+= delta * (chaseForce * DeltaTime / math.sqrt(sqrDist));
            }
            else 
            {
                float attackForce = property.Value.AttackForce;
                float hitDistance = property.Value.HitDistance;
                velocity.Value += delta * (attackForce * DeltaTime / math.sqrt(sqrDist));
                if (sqrDist < hitDistance * hitDistance) 
                {
                    velocity.Value *= 0.5f;
                }
            }
        }
        private void MoveInfluencedByResource(ref Translation trans,ref VelocityComp velocity, int index,int teamCode, NativeArray<ResourceTargetComp> resourceArray) 
        {
            var resourceTrans = resourceArray[index].ResourceTrans;
            if (resourceArray[index].IsHoldingBySelf)
            {
                float fieldSizeX = property.Value.FieldSize.x;
                float3 targetPos = new float3(fieldSizeX*-0.45f+fieldSizeX*0.9f* teamCode, 0,trans.Value.z);
                float3 delta = targetPos - trans.Value;
                float dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                float carryForce = property.Value.CarryForce;
                velocity.Value += delta * (carryForce * DeltaTime / dist);
            }
            else 
            {
                float3 delta = resourceTrans.Value - trans.Value;
                float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                float grabDistance = property.Value.GrabDistance;
                if (sqrDist > grabDistance * grabDistance)
                {
                    float chaseForce = property.Value.ChaseForce;
                    velocity.Value += delta * (chaseForce * DeltaTime / math.sqrt(sqrDist));
                }
            }
        }
    }
    protected override void OnUpdate()
    {
        var team0Array = team0beeQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var team1Array = team1beeQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var randomTLS = new NativeArray<Random>(randomSystem.randomTLS,Allocator.TempJob);
        uint seed = (uint)(UnityEngine.Random.Range(0.1f, 0.8f) * uint.MaxValue);
        BeeMoveJob job0 = new BeeMoveJob
        {
            TranslationHandler = GetComponentTypeHandle<Translation>(),
            VelocityHandler = GetComponentTypeHandle<VelocityComp>(),
            BeeHandler = GetComponentTypeHandle<BeeTagComp>(),
            DeadStateHandler = GetComponentTypeHandle<DeadStateComp>(),
            EnemyHandler = GetComponentTypeHandle<EnemyTargetComp>(),
            SmoothHandler = GetComponentTypeHandle<SmoothRotationComp>(),
            ResourceHandler = GetComponentTypeHandle<ResourceTargetComp>(),
            TeamHandler = GetComponentTypeHandle<BeeTeamComp>(),
            randomTLS = randomTLS,
            property = Blob,
            Team0Array = team0Array,
            Team1Array = team1Array,
            DeltaTime = Time.DeltaTime,
            Seed=seed
        };
        Dependency = job0.ScheduleParallel(beeQuery, Dependency);
    }
}
