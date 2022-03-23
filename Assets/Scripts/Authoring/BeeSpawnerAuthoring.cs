using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BeeSpawnerAuthoring : MonoBehaviour,IConvertGameObjectToEntity,IDeclareReferencedPrefabs
{
    /*public Mesh mesh;
    public Material material;*/
    public GameObject Prefab;
    public int beeCount;
    public float maxBeeSize;
    public float minBeeSize;
    public float initVelocity;
    public float teamAttraction;
    public float teamRepulsion;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
       var entityPrefab= conversionSystem.GetPrimaryEntity(Prefab);
        BeeSpawnComp spawnData = new BeeSpawnComp
        {
            BeePrefab= entityPrefab,
            BeeCount = beeCount,
            MaxBeeSize = maxBeeSize,
            MinBeeSize = minBeeSize,
            InitVelocity = initVelocity,
            TeamAttraction=teamAttraction,
            TeamRepulsion=teamRepulsion
        };
        /*RenderSharedComp renderData = new RenderSharedComp
        {
            MaterialValue=material,
            MeshValue=mesh
        };*/
        dstManager.AddComponentData(entity,spawnData);
        //dstManager.AddSharedComponentData(entity,renderData);
    }
}
