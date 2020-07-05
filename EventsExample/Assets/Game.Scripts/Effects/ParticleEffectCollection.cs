using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Events;
using SubjectNerd.Utilities;
using UnityEngine;

namespace Assets.Game.Scripts.Effects
{
    public class ParticleEffectCollection : MonoBehaviour
    {
        public ILookup<EffectCategory, EffectEntry> ByCategory;

        [Reorderable] public List<EffectEntry> Effects = new List<EffectEntry>();

        private void Start()
        {
            ByCategory = Effects.ToLookup(k => k.Category, v => v);
        }

        [Serializable]
        public struct EffectEntry
        {
            public EffectCategory Category;
            public ParticleEffect Effect;
        }
    }
}