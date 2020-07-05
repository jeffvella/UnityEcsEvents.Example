using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct PlayerCreatedEvent : IComponentData
    {
        public int Id;
    }
}