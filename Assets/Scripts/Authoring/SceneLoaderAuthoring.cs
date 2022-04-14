using Unity.Entities;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SceneLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public SceneAsset Scene;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var path = AssetDatabase.GetAssetPath(Scene);
        var guid = AssetDatabase.GUIDFromAssetPath(path);
        dstManager.AddComponentData(entity,new SceneLoaderComp { Guid=guid});
    }
}
#endif
