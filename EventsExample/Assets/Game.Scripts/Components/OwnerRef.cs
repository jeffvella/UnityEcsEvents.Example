using Unity.Entities;

namespace Assets.Scripts.Components
{
    public struct OwnerRef : IComponentData
    {
        public int Id;
        public Entity Entity;
    }
}