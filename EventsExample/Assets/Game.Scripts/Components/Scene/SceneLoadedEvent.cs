using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct SceneLoadedEvent : IComponentData
    {
        public SceneId Id;
        public Entity ProgressEntity;
        public SceneCategory SceneCategory;
    }
}
