using Unity.Entities;

namespace Assets.Game.Scripts.Components
{
    public struct SceneLoadingProgress : IComponentData
    {
        public SceneId Id;
        public float PercentComplete;
    }
}