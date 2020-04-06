using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

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
        Sound,
    }

    public enum ActorCategory
    {
        None = 0,
        Attacker,
        Defender,
        Player,
    }

    public enum ActorAssetId
    {
        None = 0,
        PlayerPawn,
        DefenderSphere,
        AttackerCube,
        Adventurer,
    }

    [GenerateAuthoringComponent]
    public struct ActorDefinition : IComponentData
    {
        public ActorAssetId AssetId;
        public ActorType ActorType;
        public ActorCategory Team;
    }
}
