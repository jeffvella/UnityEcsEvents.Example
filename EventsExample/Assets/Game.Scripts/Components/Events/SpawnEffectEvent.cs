using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components.Events
{
    public enum EffectCategory
    {
        None = 0,
        Collision,
        ScoreGained,
        Spawn
    }

    public struct SpawnEffectEvent : IComponentData
    {
        public float3 SpawnPosition;
        public Entity AssociatedEntity;
        public EffectCategory Category;
    }
}