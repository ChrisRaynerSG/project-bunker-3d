using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class NoiseGenerationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<WorldTag>())
        {
            UnityEngine.Debug.Log("No WorldTag singleton yet; skipping NoiseGenerationSystem.");
            return;
        }

        var worldEntity = SystemAPI.GetSingletonEntity<WorldTag>();
        if (SystemAPI.HasComponent<NoiseGeneratedTag>(worldEntity))
        {
            Enabled = false;
            UnityEngine.Debug.Log("Noise has already been generated for this world. Disabling NoiseGenerationSystem.");
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

                float baseNoise = heightNoise.GetNoise(x, z);
                // normalize the noise value to a range suitable for height
                int heightValue = (int)((baseNoise + 1f) * 0.5f * WorldConstants.WORLD_HEIGHT);
                
                buffer[index++] = new HeightNoise
                {
                    Value = heightValue
                };
            }
        }

        // After generating noise, we mark the world as having noise generated
        EntityManager.AddComponent<NoiseGeneratedTag>(worldEntity);

        // we now have a 1d array of height noise values for the world indicating where the surface terrain is located
        // now I need to generate the surface terrain in the chunks based on this noise data
        // we need some sort of way of deciding what the blocks are going to be, the blocks on top will be grass blocks, then dirt underneath and then stone finally
        UnityEngine.Debug.Log($"Noise generation complete for world with seed {seed}. Noise values generated for {bufferSize} positions.");
    }
}