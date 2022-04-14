using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

public partial class RandomSystem : SystemBase
{
    public NativeArray<Random> randomTLS;
    protected override void OnCreate()
    {
        randomTLS = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
        for (int i = 0; i < randomTLS.Length; i++)
        {
            randomTLS[i] = Random.CreateFromIndex((uint)i);
        }
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < randomTLS.Length; i++)
        {
            randomTLS[i].NextInt();
        }
    }
    protected override void OnStopRunning()
    {
        randomTLS.Dispose();
    }
    
}
