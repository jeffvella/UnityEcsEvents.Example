using Assets.Scripts.Components;
using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct ActorCreatedEvent : IComponentData
    {
        public ActorAssetId AssetId;
    }
}