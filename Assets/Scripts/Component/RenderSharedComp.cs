using System;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct RenderSharedComp : ISharedComponentData,IEquatable<RenderSharedComp>
{
    public Material Value;
    public Mesh Value2;

    public bool Equals(RenderSharedComp other)
    {
        return Value.Equals(other);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
