using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Components
{
    public struct MovementTarget : IComponentData
    {
        public Entity Entity;
        public float3 Position;
    }
}