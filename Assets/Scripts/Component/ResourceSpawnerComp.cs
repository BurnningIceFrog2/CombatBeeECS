using Unity.Entities;

public struct ResourceSpawnerComp : IComponentData
{
    public Entity Prefab;
    public int Count;
}
