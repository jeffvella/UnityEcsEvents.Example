using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using UnityEngine;
using Vella.Events;

namespace Assets.Scripts.UI
{
    public class UIActorSpawner : MonoBehaviour, IEventObserver<SpawnActorEvent>, IEventObserver<PlayerCreatedEvent>
    {
        private int _invocations;
        private int CurrentPlayerId;
        public EventRouter EventSource;

        public void OnEvent(PlayerCreatedEvent e) => CurrentPlayerId = e.Id;

        public void OnEvent(SpawnActorEvent e) => Debug.Log($"{nameof(SpawnActorEvent)}! Amount={e.Amount} Team={e.Catetory}");

        private void Start()
        {
            EventSource.Subscribe<SpawnActorEvent>(this);
            EventSource.Subscribe<PlayerCreatedEvent>(this);
        }

        private void OnDestroy()
        {
            EventSource.Unsubscribe<SpawnActorEvent>(this);
            EventSource.Unsubscribe<PlayerCreatedEvent>(this);
        }

        public void SpawnAttacker()
        {
            ++_invocations;
            EventSource.FireEvent(new SpawnActorEvent
            {
                Amount = Random.Range(_invocations, _invocations / 2),
                Catetory = ActorCategory.Attacker
            });
        }

        public void SpawnPlayer() => EventSource.FireEvent(new SpawnActorEvent
        {
            Amount = 1,
            Catetory = ActorCategory.HumanPlayer
        });

        public void SpawnDefender() => EventSource.FireEvent(new SpawnActorEvent
        {
            Amount = 1,
            Catetory = ActorCategory.Defender,
            OwnerId = CurrentPlayerId
        });
    }
}
