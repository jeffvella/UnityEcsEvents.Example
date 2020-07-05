using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components.Events
{
    public struct ActorDeathEvent : IComponentData
    {
        public ActorDefinition Definition;

        public OwnerRef AttributedTo;

        public float3 DeathPosition;
    }
}