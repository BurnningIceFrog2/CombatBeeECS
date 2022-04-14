using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct PropertiesBlob
{
    public float Aggression;
    public float CarryForce;
    public float AttackDistance;
    public float ChaseForce;
    public float AttackForce;
    public float HitDistance;
    public float GrabDistance;
    public float Damping;
    public float FlightJitter;
    public float BeeSpeedStretch;
    public float ParticalSpeedStretch;
    public float RotationStiffness;
    public float SnapStiffness;
    public float CarryStiffness;
    public int BeesPerResource;
    public float ResourceSpawnRate;
    public float3 FieldSize;
    public float Gravity;
    public float TeamAttraction;
    public float TeamRepulsion;
    public float ResourceSize;

    public static BlobAssetReference<PropertiesBlob> CreatePropertiesBlob() 
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref PropertiesBlob blob = ref builder.ConstructRoot<PropertiesBlob>();
        blob.Aggression = 0.2f;
        blob.CarryForce = 25f;
        blob.AttackDistance = 4f;
        blob.ChaseForce = 50f;
        blob.AttackForce = 500f;
        blob.HitDistance = 0.5f;
        blob.GrabDistance = 0.5f;
        blob.Damping = 0.1f;
        blob.FlightJitter = 200;
        blob.BeeSpeedStretch = 0.2f;
        blob.ParticalSpeedStretch = 0.25f;
        blob.RotationStiffness = 5f;
        blob.SnapStiffness = 2f;
        blob.CarryStiffness = 15f;
        blob.FieldSize = new float3(100,20,30);
        blob.Gravity = -20f;
        blob.TeamAttraction = 5f;
        blob.TeamRepulsion = 4f;
        blob.ResourceSize = 0.75f;
        blob.BeesPerResource = 6;
        var result = builder.CreateBlobAssetReference<PropertiesBlob>(Allocator.Persistent);
        builder.Dispose();
        return result;
    }
}

