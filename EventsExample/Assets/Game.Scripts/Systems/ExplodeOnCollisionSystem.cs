using System;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Unity.Entities;
using Vella.Events;

namespace Assets.Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class ExplodeOnCollisionSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private Events _events;
        private UnsafeEntityManager _uem;

        protected override void OnCreate()
        {
            var eventSystem = World.GetOrCreateSystem<EntityEventSystem>();
            _events.AttackerDeath = eventSystem.GetQueue<ActorDeathEvent>();
            _events.SpawnEffect = eventSystem.GetQueue<SpawnEffectEvent>();
            _events.PlaySound = eventSystem.GetQueue<PlayAudioEvent>();
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _uem = EntityManager.Unsafe;
        }

        protected override void OnUpdate()
        {
            var uem = _uem;
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _events;

            Entities.ForEach((in CollisionEvent e) =>
            {
                if (uem.Exists(e.Hit.Target))
                {
                    events.AttackerDeath.Enqueue(new ActorDeathEvent
                    {
                        Definition = GetComponent<ActorDefinition>(e.Hit.Target),
                        DeathPosition = e.Hit.Position,
                        AttributedTo = e.Source.Owner
                    });

                    events.SpawnEffect.Enqueue(new SpawnEffectEvent
                    {
                        SpawnPosition = e.Hit.Position,
                        AssociatedEntity = e.Hit.Target,
                        Category = EffectCategory.Collision
                    });

                    events.PlaySound.Enqueue(new PlayAudioEvent
                    {
                        Sound = SoundCategory.Collision,
                        SpawnPosition = e.Hit.Position,
                        AssociatedEntity = e.Hit.Target
                    });

                    commands.AddComponent(e.Hit.Target, new PendingDestruction
                    {
                        QueuedTime = DateTime.UtcNow,
                        DestroyTime = DateTime.UtcNow
                    });

                    commands.AddComponent<Disabled>(e.Hit.Target);
                }

            }).WithoutBurst().Run();
        }

        private struct Events
        {
            public EventQueue<ActorDeathEvent> AttackerDeath;
            public EventQueue<PlayAudioEvent> PlaySound;
            public EventQueue<SpawnEffectEvent> SpawnEffect;
        }
    }
}
