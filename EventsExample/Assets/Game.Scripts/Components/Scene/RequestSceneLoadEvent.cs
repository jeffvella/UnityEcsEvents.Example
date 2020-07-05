using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct RequestSceneLoadEvent : IComponentData
    {
        public SceneId Id;
    }
}