using System.Linq;
using Assets.Scripts.Components.Events;
using UnityEngine;
using Vella.Events;

namespace Assets.Game.Scripts.Effects
{
    public class ParticleEffectSpawner : MonoBehaviour, IEventObserver<SpawnEffectEvent>
    {
        public ParticleEffectCollection Effects;
        public EventRouter EventSource;
        public ParticleEffectManager Manager;

        public void OnEvent(SpawnEffectEvent e)
        {
            if (Effects.ByCategory.Contains(e.Category))
            {
                var effects = Effects.ByCategory[e.Category];
                var first = effects.FirstOrDefault();
                Manager.Spawn(first.Effect, e.SpawnPosition);
            }
        }

        private void Start() => EventSource.Subscribe<SpawnEffectEvent>(this);
    }
}
