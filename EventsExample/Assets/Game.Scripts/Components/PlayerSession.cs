using Unity.Entities;

namespace Assets.Scripts.Components
{
    public struct PlayerSession : IComponentData
    {
        public int PlayerId;
        public int Score;
    }
}