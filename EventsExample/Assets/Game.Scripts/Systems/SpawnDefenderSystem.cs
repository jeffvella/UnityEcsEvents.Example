using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Components.Tags;
using Assets.Scripts.Extensions;
using Assets.Scripts.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vella.Events;

namespace Assets.Scripts.Systems
{
    public class SpawnDefenderSystem : SystemBase
    {
        private IProvider<Prefabs> _prefabs;
        private IProvider<Players> _players;
        private BeginSimulationEntityCommandBufferSystem _commandSystem;

        protected override void OnCreate()
        {
            _prefabs = World.GetOrCreateProvider<Prefabs>();
            _players = World.GetOrCreateProvider<Players>();

            // Instantiate next frame because it needs to happen before TransformSystemGroup
            _commandSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var prefabData = _prefabs.Data;
            var playerData = _players.Data;

            Entities.ForEach((ref SpawnActorEvent e) =>
            {
                if (e.Catetory == ActorCategory.Defender)
                {
                    if (!prefabData.TryGetFirst(ActorCategory.Defender, out var prefab))
                        throw new Exception("Prefab not found");

                    if (!playerData.TryGet(e.OwnerId, out var player))
                        throw new Exception("Player not found");

                    for (int i = 0; i < e.Amount; i++)
                    {
                        CreateDefender(ref commands, prefab, player);
                    }
                }

            }).Run();
        }

        private static void CreateDefender(ref EntityCommandBuffer commands, PrefabRef prefab, PlayerRef player, float3 position = default)
        {
            var targetEntity = commands.Instantiate(prefab.Entity);
            commands.AddComponent<DefenderTag>(targetEntity);
            commands.AddComponent(targetEntity, new OwnerRef
            {
                Id = player.Id,
                Entity = player.Entity
            });
            commands.SetComponent(targetEntity, new Translation
            {
                Value = position + prefab.SpawnOffset
            });
        }

    }

    public class InitializeDefenderSystem : SystemBase
    {
        private EventQueue<ActorCreatedEvent> _createdEvents;
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private EntityQuery _newDefendersQuery;

        protected override void OnCreate()
        {
            _createdEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<ActorCreatedEvent>();
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _createdEvents;

            Entities.WithAll<DefenderTag, UnprocessedTag>().ForEach((Entity entity, in ActorDefinition def) =>
            {
                commands.RemoveComponent<UnprocessedTag>(entity);

                events.Enqueue(new ActorCreatedEvent
                {
                    AssetId = def.AssetId,
                });

            }).WithStoreEntityQueryInField(ref _newDefendersQuery).Run();
        }
    }

}
