using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components
{
    [Flags, Serializable]
    public enum MovementFlags
    {
        None = 0,
        Flying = 1 << 0,
        Charges = 1 << 1,
    }

    [GenerateAuthoringComponent]
    public struct MovementInfo : IComponentData
    {
        public float Speed;
        public MovementFlags Flags;
    }
}