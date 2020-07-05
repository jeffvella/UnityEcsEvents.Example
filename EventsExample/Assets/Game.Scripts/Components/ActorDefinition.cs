using System;
using Unity.Entities;

namespace Assets.Scripts.Components
{
    public enum ActorType
    {
        None = 0,
        Unit,
        Player,
        Environment,
        Lighting,
        Effect,
        Sound
    }

    public enum ActorCategory
    {
        None = 0,
        Attacker,
        Defender,
        HumanPlayer,
        AIPlayer
    }

    public enum ActorAssetId
    {
        None = 0,
        PlayerPawn,
        DefenderSphere,
        AttackerCube,
        Adventurer
    }

    [Flags]
    [Serializable]
    public enum ActorFlags
    {
        None = 0,
        AddTransformAsOffset = 1 << 0,
        Unused = 1 << 1
    }

    [GenerateAuthoringComponent]
    public struct ActorDefinition : IComponentData
    {
        public ActorAssetId AssetId;
        public ActorType ActorType;
        public ActorCategory Team;
        public ActorFlags Flags;
    }
}