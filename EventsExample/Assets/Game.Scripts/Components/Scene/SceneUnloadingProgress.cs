using Unity.Entities;

namespace Assets.Game.Scripts.Components
{
    public struct SceneUnloadingProgress : IComponentData
    {
        public SceneId Id;
        public float PercentComplete;
    }
}