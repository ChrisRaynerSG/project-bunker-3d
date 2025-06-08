using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class NoiseGenerationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if(!SystemAPI.HasSingleton<WorldTag>())
        {
            UnityEngine.Debug.Log("No WorldTag singleton yet; skipping NoiseGenerationSystem.");
            return;
        }

        var worldEntity = SystemAPI.GetSingletonEntity<WorldTag>();
        if (SystemAPI.HasComponent<NoiseGeneratedTag>(worldEntity))
        {
            // If the world already has noise generation, we skip this system
            return;
        }

        // Generate noise for the world

        var seedComponent = SystemAPI.GetComponent<Seed>(worldEntity);
        int seed = seedComponent.Value;

        FastNoise heightNoise = NoiseProvider.CreateNoise(0.006f, seed);

        var buffer = SystemAPI.GetBuffer<HeightNoise>(worldEntity);
        var bufferSize = WorldConstants.WORLD_SIZE * WorldConstants.WORLD_SIZE;

        if (buffer.Length != bufferSize)
        {
            buffer.Clear();
            buffer.ResizeUninitialized(bufferSize);
        }
        else
        {
            return; // If the buffer is already the correct size, we skip further processing
        }

        int index = 0;

        for (int x = 0; x < WorldConstants.WORLD_SIZE; x++)
        {
            for (int z = 0; z < WorldConstants.WORLD_SIZE; z++)
            {
                buffer[index++] = new HeightNoise
                {
                    Value = heightNoise.GetNoise(x, z)
                };
            }
        }

        // After generating noise, we mark the world as having noise generated
        EntityManager.AddComponent<NoiseGeneratedTag>(worldEntity);
    }
}