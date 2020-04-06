using Assets.Scripts.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct ActorCreatedEvent : IComponentData
    {
        public ActorAssetId AssetId;
    }
}