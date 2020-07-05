using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct RequestSceneUnloadEvent : IComponentData
    {
        public SceneId Id;
        public SceneId ThenLoad;
    }
}