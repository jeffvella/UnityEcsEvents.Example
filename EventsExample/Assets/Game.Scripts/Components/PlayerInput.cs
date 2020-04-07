using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
    public struct PlayerInput : IComponentData
    {
        public float3 PointerPosition;
        public int InputPlayerId;
    }
}
