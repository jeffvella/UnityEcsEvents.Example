using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Components.Events
{
    public enum ScoreUpdateType
    {
        None = 0,
        AttackerDeath
    }

    public struct ScoreUpdatedEvent : IComponentData
    {
        public ScoreUpdateType Type;
        public int CurrentScore;
        public int ChangedAmount;
    }
}
