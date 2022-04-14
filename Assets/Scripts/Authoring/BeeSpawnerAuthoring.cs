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
            //BeeCount = beeCount,
            MaxBeeSize = maxBeeSize,
            MinBeeSize = minBeeSize,
            InitVelocity = initVelocity,
            //TeamCode=-1,
            TeamAttraction =teamAttraction,
            TeamRepulsion=teamRepulsion
        };
        BeeGenerateComp beeGenerate = new BeeGenerateComp
        {
            BeeCount=beeCount,
            TeamCode = -1
        };
        var generateEntity=conversionSystem.CreateAdditionalEntity(GetComponent<BeeSpawnerAuthoring>());
        dstManager.AddComponentData(entity,spawnData);
        dstManager.AddComponentData(generateEntity, beeGenerate);
        //dstManager.AddSharedComponentData(entity,renderData);
    }
}
