using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components.Events
{
    public enum SoundCategory
    {
        None = 0,
        Collision,
        ScoreGained,
        Spawn
    }

    public struct PlayAudioEvent : IComponentData
    {
        internal float3 SpawnPosition;
        internal Entity AssociatedEntity;
        internal SoundCategory Sound;
    }
}