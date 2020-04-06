using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components.Events
{
    public struct ActorDeathEvent : IComponentData
    {
        public ActorDefinition Definition;

        public OwnerRef AttributedTo;

        public float3 DeathPosition;
    }
}
