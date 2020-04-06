using Assets.Scripts.Components.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Game.Scripts.Effects
{
    public class ParticleEffectCollection : MonoBehaviour
    {
        [Serializable]
        public struct EffectEntry
        {
            public EffectCategory Category;
            public ParticleEffect Effect;
        }

        public List<EffectEntry> Effects = new List<EffectEntry>();
        public ILookup<EffectCategory, EffectEntry> ByCategory;


        private void Start()
        {
            ByCategory = Effects.ToLookup(k => k.Category, v => v);
        }
    }
}
