using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components.Events
{
    public struct TargetAcquiredEvent : IComponentData
    {
        public Entity Source;
        public float3 SourcePosition;
        public Entity Target;
        public float3 TargetPosition;
    }
}
