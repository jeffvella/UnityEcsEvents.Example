using Assets.Scripts.Systems;
using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct CollisionEvent : IComponentData
    {
        public DetectorInfo Source;
        public Hit Hit;
    }
}