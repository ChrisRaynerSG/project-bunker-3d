using Unity.Entities;
using UnityEngine;

public class WorldAuthoring : MonoBehaviour
{
    // This class is used to author the world entity in the Unity Editor.
    // It can be used to set up initial data for the world entity, such as the seed value.

    [SerializeField]
    private int seed = 0;

    public int Seed => seed;

    // You can add more fields here to configure the world entity as needed.

    public class Baker : Baker<WorldAuthoring>
    {
        public override void Bake(WorldAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.WorldSpace);
            AddComponent<WorldTag>(entity); // Add a tag to identify the world entity
            AddComponent(entity, new global::Seed {Value = authoring.seed}); // Add the seed component to the world entity
        }
    }
}