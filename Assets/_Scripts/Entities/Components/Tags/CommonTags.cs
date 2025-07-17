using Unity.Entities;
namespace Bunker.Entities.Components.Tags
{
    namespace Common
    
    {
        // === CAPABILITY TAGS ===

        /// <summary>
        /// Represents entities that can move around the world.
        /// </summary>
        public struct Moveable : IComponentData { }

        /// <summary>
        /// Represents entities that can take damage and be destroyed.
        /// </summary>
        public struct Killable : IComponentData { }

        /// <summary>
        /// Represents entities that can be selected by the player.
        /// </summary>
        public struct Selectable : IComponentData { }

        /// <summary>
        /// Represents entities that can interact with other entities or objects.
        /// </summary>
        public struct Interactive : IComponentData { }

        /// <summary>
        /// Represents entities that can carry items or resources.
        /// </summary>
        public struct CarryCapable : IComponentData { }

        /// <summary>
        /// Represents entities that can perform work tasks.
        /// </summary>
        public struct WorkCapable : IComponentData { }

        // === STATE TAGS ===

        /// <summary>
        /// Represents entities that are currently alive and active.
        /// </summary>
        public struct Alive : IComponentData { }

        /// <summary>
        /// Represents entities that have died but may still exist as corpses.
        /// </summary>
        public struct Dead : IComponentData { }

        /// <summary>
        /// Represents entities that are currently idle with no assigned tasks.
        /// </summary>
        public struct Idle : IComponentData { }

        /// <summary>
        /// Represents entities that are currently busy performing a task.
        /// </summary>
        public struct Busy : IComponentData { }

        /// <summary>
        /// Represents entities that are currently sleeping or resting.
        /// </summary>
        public struct Sleeping : IComponentData { }

        // === BEHAVIOR TAGS ===

        /// <summary>
        /// Represents entities that exhibit aggressive behavior toward threats.
        /// </summary>
        public struct Aggressive : IComponentData { }

        /// <summary>
        /// Represents entities that are peaceful and avoid conflict.
        /// </summary>
        public struct Passive : IComponentData { }

        /// <summary>
        /// Represents entities that are neutral and may defend if attacked.
        /// </summary>
        public struct Neutral : IComponentData { }

        /// <summary>
        /// Represents entities that are currently fleeing from danger.
        /// </summary>
        public struct Fleeing : IComponentData { }

        // === VISIBILITY TAGS ===

        /// <summary>
        /// Represents entities that should be visible to the player.
        /// </summary>
        public struct Visible : IComponentData { }

        /// <summary>
        /// Represents entities that are hidden from normal view.
        /// </summary>
        public struct Hidden : IComponentData { }

        /// <summary>
        /// Represents entities that should be highlighted in the UI.
        /// </summary>
        public struct Highlighted : IComponentData { }

        /// <summary>
        /// Represents entities that are currently selected by the player.
        /// </summary>
        public struct Selected : IComponentData { }
    }
}
