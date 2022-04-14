using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ParticalSpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject WhiteParticalPrefab;
    public GameObject BloodParticalPrefab;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(WhiteParticalPrefab);
        referencedPrefabs.Add(BloodParticalPrefab);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var whitePrefab = conversionSystem.GetPrimaryEntity(WhiteParticalPrefab);
        var bloodPrefab = conversionSystem.GetPrimaryEntity(BloodParticalPrefab);
        var spawnerData = new ParticalSpawnerComp
        {
            WhitePrefab=whitePrefab,
            BloodPrefab=bloodPrefab
        };
        dstManager.AddComponentData(entity, spawnerData);
    }

}
