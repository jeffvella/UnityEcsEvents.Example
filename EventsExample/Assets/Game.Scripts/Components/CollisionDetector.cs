using Unity.Entities;

namespace Assets.Scripts.Components
{
    [GenerateAuthoringComponent]
    public struct CollisionDetector : IComponentData
    {
        public float Range;
    }
}