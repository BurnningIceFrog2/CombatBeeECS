using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class BeeRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithName("BeeRotationSystem")
            .WithAll<BeeTagComp>()
            .ForEach((Entity bee, int entityInQueryIndex, ref Rotation rotation, in SmoothRotationComp smooth) =>
            {
                quaternion r=quaternion.identity;
                if (!smooth.smoothDirection.Equals(float3.zero)) 
                {
                    r = quaternion.LookRotation(smooth.smoothDirection,new float3(0,1,0));
                }
                rotation.Value = r;
            }).ScheduleParallel();
    }
}
