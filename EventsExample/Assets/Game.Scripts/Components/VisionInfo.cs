using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components
{
    [Flags, Serializable]
    public enum VisionFlags
    {
        None = 0,
        Blinded = 1 << 0,
        Perceptive = 1 << 0,
    }

    [GenerateAuthoringComponent]
    public struct VisionInfo : IComponentData
    {
        public float Distance;
        public float ArcDegrees;
        public VisionFlags Flags;
    }
}
