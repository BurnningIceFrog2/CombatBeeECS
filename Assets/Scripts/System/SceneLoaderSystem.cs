using Unity.Entities;
using Unity.Scenes;
using Unity.Collections;
using UnityEngine;

public partial class SceneLoaderSystem : SystemBase
{
    SceneSystem sceneSystem;
    //EntityQuery loadQuery;
    protected override void OnCreate()
    {
        sceneSystem = World.GetOrCreateSystem<SceneSystem>();
        RequireSingletonForUpdate<SceneLoaderComp>();
        RequireSingletonForUpdate<LoadSceneTagComp>();
    }
    protected override void OnUpdate()
    {
        var loaderComp = GetSingleton<SceneLoaderComp>();
        var sceneEntity = sceneSystem.GetSceneEntity(loaderComp.Guid);
        sceneSystem.UnloadScene(sceneEntity);
        var entity=GetSingletonEntity<LoadSceneTagComp>();
        EntityManager.DestroyEntity(entity);
        sceneSystem.LoadSceneAsync(loaderComp.Guid);
    }
}
