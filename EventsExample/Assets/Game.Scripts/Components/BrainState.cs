using System;
using Unity.Entities;

namespace Assets.Scripts.Components
{
    [Flags]
    [Serializable]
    public enum BrainFlags
    {
        None = 0,
        Alerted = 1 << 0,
        Depressed = 1 << 1,
        Disabled = 1 << 2
    }

    public enum BrainActivity
    {
        None = 0,
        Idle,
        InPursuit,
        Wandering,
        Searching
    }

    [GenerateAuthoringComponent]
    public struct BrainState : IComponentData
    {
        public BrainActivity Activity;
        public BrainFlags Flags;
    }
}