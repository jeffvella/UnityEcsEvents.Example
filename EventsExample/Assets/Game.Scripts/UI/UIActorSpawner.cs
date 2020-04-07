using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Systems;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIActorSpawner : MonoBehaviour, IEventObserver<SpawnActorEvent>, IEventObserver<PlayerCreatedEvent>
    {
        public EventRouter EventSource;

        private int _invocations;
        private int CurrentPlayerId;

        private void Start()
        {
            EventSource.AddListener<UIActorSpawner, SpawnActorEvent>(this);
            EventSource.AddListener<UIActorSpawner, PlayerCreatedEvent>(this);
        }
        private void OnDestroy()
        {
            EventSource.RemoveListener<UIActorSpawner, SpawnActorEvent>(this);
            EventSource.RemoveListener<UIActorSpawner, PlayerCreatedEvent>(this);
        }

        public void SpawnAttacker()
        {
            ++_invocations;
            EventSource.FireEvent(new SpawnActorEvent
            {
                
                Amount = UnityEngine.Random.Range(_invocations, _invocations / 2),
                Catetory = ActorCategory.Attacker,
            }); ;
        }

        public void SpawnPlayer()
        {
            EventSource.FireEvent(new SpawnActorEvent
            {
                Amount = 1,
                Catetory = ActorCategory.HumanPlayer,
            });
        }

        public void SpawnDefender()
        {
            EventSource.FireEvent(new SpawnActorEvent
            {
                Amount = 1,
                Catetory = ActorCategory.Defender,
                OwnerId = CurrentPlayerId,
            });
        }

        public void OnEvent(PlayerCreatedEvent e)
        {
            CurrentPlayerId = e.Id;
        }

        public void OnEvent(SpawnActorEvent e)
        {
            Debug.Log($"{nameof(SpawnActorEvent)}! Amount={e.Amount} Team={e.Catetory}");
        }

    }
}