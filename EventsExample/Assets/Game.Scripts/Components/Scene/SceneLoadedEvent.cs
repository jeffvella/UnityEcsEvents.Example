using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public enum SceneCategory
    {
        None = 0,
        Level,
        Menu
    }

    public struct SceneLoadedEvent : IComponentData
    {
        public SceneId Id;
        public SceneCategory SceneCategory;
        public Entity ProgressEntity;
    }
}