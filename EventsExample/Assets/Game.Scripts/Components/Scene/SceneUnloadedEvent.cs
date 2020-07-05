using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct SceneUnloadedEvent : IComponentData
    {
        public SceneId Id;
        public SceneCategory Category;
        public Entity ProgressEntity;
    }
}