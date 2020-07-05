using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct SpawnActorEvent : IComponentData
    {
        public int Amount;
        public ActorCategory Catetory;
        public int OwnerId;
    }
}