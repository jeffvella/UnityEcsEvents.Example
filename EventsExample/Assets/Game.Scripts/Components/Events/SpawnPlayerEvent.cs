using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct SpawnPlayerEvent : IComponentData
    {
        public int InputPlayerId;
        public ActorCategory Category;
    }
}
