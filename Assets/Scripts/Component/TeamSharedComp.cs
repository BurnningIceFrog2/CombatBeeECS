using System;
using Unity.Entities;

[Serializable]
public struct TeamSharedComp : ISharedComponentData
{
    public int TeamCode;
}
