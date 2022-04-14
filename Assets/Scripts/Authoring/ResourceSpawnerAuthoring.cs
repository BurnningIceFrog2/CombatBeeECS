using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ResourceSpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject ResourcePrefab;
    public int ResourceCount;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(ResourcePrefab);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
       var prefab= conversionSystem.GetPrimaryEntity(ResourcePrefab);
        var spawnerData = new ResourceSpawnerComp
        {
            Prefab=prefab,
            Count= ResourceCount
        };
        dstManager.AddComponentData(entity,spawnerData);
    }

}
