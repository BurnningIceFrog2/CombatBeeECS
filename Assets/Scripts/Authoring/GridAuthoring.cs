using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class GridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<GridSummaryComp>(entity);
        dstManager.AddComponent<GridSpawnTagComp>(entity);
    }
}
