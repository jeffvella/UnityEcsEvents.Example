using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct SpawnActorEvent : IComponentData
    {
        public int Amount;
        public ActorCategory Team;
        public int OwnerId;
    }
}
