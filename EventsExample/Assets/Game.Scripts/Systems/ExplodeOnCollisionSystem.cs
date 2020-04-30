using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
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
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            var eventSystem = World.GetOrCreateSystem<EntityEventSystem>();
            _events.AttackerDeath = eventSystem.GetQueue<ActorDeathEvent>();
            _events.SpawnEffect = eventSystem.GetQueue<SpawnEffectEvent>();
            _events.PlaySound = eventSystem.GetQueue<PlayAudioEvent>();
            _uem = EntityManager.Unsafe;
        }

        private struct Events
        {
            public EventQueue<ActorDeathEvent> AttackerDeath;
            public EventQueue<SpawnEffectEvent> SpawnEffect;
            public EventQueue<PlayAudioEvent> PlaySound;
        }

        protected override void OnUpdate()
        {
            var uem = _uem;
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _events;

            Entities.ForEach((in CollisionEvent e) =>
            {
                CheckEntityExistsOrThrow(uem, e.Hit.Target);

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
                    Category = EffectCategory.Collision,
                });

                events.PlaySound.Enqueue(new PlayAudioEvent
                {
                    Sound = SoundCategory.Collision,
                    SpawnPosition = e.Hit.Position,
                    AssociatedEntity = e.Hit.Target
                });

                commands.DestroyEntity(e.Hit.Target);

            }).Run();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckEntityExistsOrThrow(UnsafeEntityManager uem, Entity entity)
        {
            // Calling SystemBase.GetComponent<T>() in Burst on Entity.Null will crash the editor.

            if (!uem.Exists(entity))
                throw new ArgumentException($"Entity doesn't exist");
        }

    }
}
