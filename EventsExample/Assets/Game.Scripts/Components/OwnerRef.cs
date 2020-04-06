using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components
{
    public struct OwnerRef : IComponentData
    {
        public int Id;
        public Entity Entity;
    }
}
