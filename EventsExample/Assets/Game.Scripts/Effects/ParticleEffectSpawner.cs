using Assets.Scripts.Components.Events;
using Assets.Scripts.UI;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Game.Scripts.Effects
{

    public class ParticleEffectSpawner : MonoBehaviour, IEventObserver<SpawnEffectEvent>
    {
        public EventRouter EventSource;
        public ParticleEffectManager Manager;
        public ParticleEffectCollection Effects;

        private void Start()
        {
            EventSource.AddListener<ParticleEffectSpawner, SpawnEffectEvent>(this);
        }

        public void OnEvent(SpawnEffectEvent e)
        {
            if(Effects.ByCategory.Contains(e.Category))
            {
                var effects = Effects.ByCategory[e.Category];
                var first = effects.FirstOrDefault();
                Manager.Spawn(first.Effect, e.SpawnPosition);
            }
        }
    }
}
