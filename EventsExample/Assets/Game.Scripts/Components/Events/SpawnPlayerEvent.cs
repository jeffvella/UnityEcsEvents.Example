using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct SpawnPlayerEvent : IComponentData
    {
        public int InputPlayerId;
        public ActorCategory Category;
    }
}