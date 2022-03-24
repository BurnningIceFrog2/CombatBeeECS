using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BeeSpawnerAuthoring : MonoBehaviour,IConvertGameObjectToEntity,IDeclareReferencedPrefabs
{
    /*public Mesh mesh;
    public Material material;*/
    public GameObject BluePrefab;
    public GameObject YellowPrefab;
    public int beeCount;
    public float maxBeeSize;
    public float minBeeSize;
    public float initVelocity;
    public float teamAttraction;
    public float teamRepulsion;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(BluePrefab);
        referencedPrefabs.Add(YellowPrefab);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
       var bluePrefab= conversionSystem.GetPrimaryEntity(BluePrefab);
        var yellowPrefab= conversionSystem.GetPrimaryEntity(YellowPrefab);
        BeeSpawnComp spawnData = new BeeSpawnComp
        {
            BlueBeePrefab= bluePrefab,
            YellowBeePrefab=yellowPrefab,
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
