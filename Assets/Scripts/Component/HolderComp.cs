using Unity.Entities;
using Unity.Transforms;

public struct HolderComp : IComponentData
{
    public Entity Holder;
    public Translation HolderTrans;
    public VelocityComp HolerVelocity;
    public int HolderTeamCode;
}
