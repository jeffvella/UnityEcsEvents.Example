using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Game.Scripts.Components.Events
{
    public struct RequestSceneUnloadEvent : IComponentData
    {
        public SceneId Id;
        public SceneId ThenLoad;
    }
}