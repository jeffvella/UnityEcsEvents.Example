using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using System;
using System.Collections.Generic;
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
        private EntityQuery _collectionQuery;
        private Events _events;
        private EventQueue<PlayAudioEvent> _playAudioEvents;

        protected override void OnCreate()
        {
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            var eventSystem = World.GetOrCreateSystem<EntityEventSystem>();
            _events.AttackerDeath = eventSystem.GetQueue<ActorDeathEvent>();
            _events.SpawnEffect = eventSystem.GetQueue<SpawnEffectEvent>();
            _events.PlaySound = eventSystem.GetQueue<PlayAudioEvent>();

            _playAudioEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<PlayAudioEvent>();
        }

        private struct Events
        {
            public EventQueue<ActorDeathEvent> AttackerDeath;
            public EventQueue<SpawnEffectEvent> SpawnEffect;
            public EventQueue<PlayAudioEvent> PlaySound;
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _events;

            Entities.ForEach((int nativeThreadIndex, in CollisionEvent e) =>
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
                    Category = EffectCategory.Collision,
                });

                events.PlaySound.Enqueue(new PlayAudioEvent
                {
                    Sound = SoundCategory.Collision,
                    SpawnPosition = e.Hit.Position,
                    AssociatedEntity = e.Hit.Target
                });

                commands.DestroyEntity(e.Hit.Target);

            }).WithStoreEntityQueryInField(ref _collectionQuery).Run();
        }
    }
}
