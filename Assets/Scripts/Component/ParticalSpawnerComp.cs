using Unity.Entities;
using Unity.Mathematics;

public struct ParticalSpawnerComp : IComponentData
{
    public Entity WhitePrefab;
    public Entity BloodPrefab;
}
