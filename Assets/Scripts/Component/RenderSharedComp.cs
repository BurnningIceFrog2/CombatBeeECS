using System;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct RenderSharedComp : ISharedComponentData,IEquatable<RenderSharedComp>
{
    public Material MaterialValue;
    public Mesh MeshValue;

    public bool Equals(RenderSharedComp other)
    {
        return MaterialValue.Equals(other.MaterialValue)&&MeshValue.Equals(other.MeshValue);
    }
    public override int GetHashCode()
    {
        return MaterialValue.GetHashCode()<<8|MeshValue.GetHashCode();
    }
}
