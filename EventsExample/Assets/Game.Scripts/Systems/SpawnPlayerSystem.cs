using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Components.Tags;
using Assets.Scripts.Providers;
using Assets.Scripts.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Vella.Events;

namespace Assets.Scripts.Systems
{
    /// <summary>
    /// Creates player Entities when the <see cref="SpawnActorEvent"/> fires.
    /// Fires <see cref="PlayerCreatedEvent"/>
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SpawnPlayerSystem : SystemBase
    {
        private IProvider<Players> _players;
        private EntityArchetype _playerArchetype;
        private EntityQuery _playerQuery;
        private BeginPresentationEntityCommandBufferSystem _commandSystem;
        private IdGenerator _idGenerator;

        protected override void OnCreate()
        {
            var components = new[]
            {
                ComponentType.ReadWrite<PlayerTag>(),
                ComponentType.ReadWrite<PlayerSession>(),
            };

            _players = World.GetOrCreateProvider<Players>();
            _playerArchetype = EntityManager.CreateArchetype(components);
            _playerQuery = EntityManager.CreateEntityQuery(components);
            _commandSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            _idGenerator = new IdGenerator();
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var archetype = _playerArchetype;
            var idGenerator = _idGenerator;
            var players = _players;

            Entities.ForEach((ref SpawnActorEvent e) =>
            {
                if(e.Team == ActorCategory.Player)
                {
                    for (int i = 0; i < e.Amount; i++)
                    {
                        var playerEntity = commands.CreateEntity(archetype);
                        var id = idGenerator.NewId();

                        commands.SetComponent(playerEntity, new PlayerSession
                        {
                            PlayerId = id,
                            Score = 0,
                        });

                        commands.AddComponent<UnprocessedTag>(playerEntity);
                    }
                }

            }).Run();
        }

        public struct IdGenerator
        {
            public int _next;
            public int NewId() => _next++;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class InitializePlayerSystem : SystemBase
    {
        private IProvider<Players> _players;
        private EventQueue<PlayerCreatedEvent> _playerCreatedEvents;
        private BeginPresentationEntityCommandBufferSystem _commandSystem;

        protected override void OnCreate()
        {
            _players = World.GetOrCreateProvider<Players>();
            _playerCreatedEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<PlayerCreatedEvent>();
            _commandSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _playerCreatedEvents;
            var players = _players.Data;

            Entities.WithAll<PlayerTag, UnprocessedTag>().ForEach((Entity entity, ref PlayerSession session) =>
            {
                players.Add(new PlayerRef
                {
                    Id = session.PlayerId,
                    Entity = entity,
                });

                commands.RemoveComponent<UnprocessedTag>(entity);

                events.Enqueue(new PlayerCreatedEvent
                {
                    Id = session.PlayerId,
                });

            }).Run();
        }

    }

}
