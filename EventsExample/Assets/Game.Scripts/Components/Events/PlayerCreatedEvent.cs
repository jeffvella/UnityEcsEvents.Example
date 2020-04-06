using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public struct PlayerCreatedEvent : IComponentData
    {
        public int Id;
    }
}
