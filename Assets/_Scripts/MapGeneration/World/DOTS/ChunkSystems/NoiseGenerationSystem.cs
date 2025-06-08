using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public partial struct NoiseGenerationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldTag>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Compute noise for the world before generating chunks
        var worldEntity = SystemAPI.GetSingletonEntity<WorldTag>();
        

    }

}