using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Unity.Entities;
using Vella.Events;

namespace Assets.Scripts.Systems
{
    public class PlayerSessionSystem : SystemBase
    {
        private EventQueue<ScoreUpdatedEvent> _scoreUpdatedEvents;

        protected override void OnCreate()
        {
            _scoreUpdatedEvents = World.GetExistingSystem<EntityEventSystem>().GetQueue<ScoreUpdatedEvent>();
        }

        protected override void OnUpdate()
        {
            var events = _scoreUpdatedEvents;

            Entities.ForEach((ref ActorDeathEvent e) =>
            {
                if (e.Definition.Team == ActorCategory.Attacker)
                {
                    var deathValue = 41;

                    var session = GetComponent<PlayerSession>(e.AttributedTo.Entity);
                    session.Score += deathValue;
                    SetComponent(e.AttributedTo.Entity, session);

                    events.Enqueue(new ScoreUpdatedEvent // todo pool into single event with buffer?
                    {
                        Type = ScoreUpdateType.AttackerDeath, ChangedAmount = deathValue, CurrentScore = session.Score
                    });
                }
            }).Run();
        }
    }
}