using Unity.Entities;
using UnityEngine;

public partial class KeyCommandSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponent<LoadSceneTagComp>(entity);
        }
    }
}
