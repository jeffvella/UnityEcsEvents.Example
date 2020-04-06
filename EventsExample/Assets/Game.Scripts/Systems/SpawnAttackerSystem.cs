using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Assets.Scripts.Extensions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using Assets.Scripts.Components.Tags;
using Assets.Scripts.Providers;
using Vella.Events;
using Assets.Game.Scripts.Components.Events;

namespace Assets.Scripts.Systems
{
    public class SpawnAttackerSystem : SystemBase
    {
        private IProvider<Prefabs> _prefabs;
        private NativeList<Entity> _buffer;
        private EventQueue<PlayAudioEvent> _playAudioEvents;

        protected override void OnCreate()
        {
            _prefabs = World.GetOrCreateProvider<Prefabs>();
            _buffer = new NativeList<Entity>(1, Allocator.Persistent);
            _playAudioEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<PlayAudioEvent>();
        }

        protected override void OnUpdate()
        {
            var entities = _buffer;
            var center = float3.zero;
            var radius = 9f;
            var prefabData = _prefabs.Data;
            var events = _playAudioEvents;

            events.Enqueue(new PlayAudioEvent
            {
                Sound = SoundCategory.Spawn
            });

            Entities.ForEach((ref SpawnActorEvent e) =>
            {
                if (e.Team != ActorCategory.Attacker)
                    return;

                if (prefabData.TryGetFirst(e.Team, out var prefab))
                {
                    entities.Resize(e.Amount, NativeArrayOptions.UninitializedMemory);

                    EntityManager.Instantiate(prefab.Entity, entities);

                    SetSpawnPositions(entities, center, radius);

                    EntityManager.AddComponent<AttackerTag>(entities);
                }

            }).WithStructuralChanges().Run();
        }

        private unsafe void SetSpawnPositions(NativeList<Entity> entities, float3 center, float radius)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                EntityManager.SetComponentData(entities[i], new Translation
                {
                    Value = GetRandomCirclePosition(center, radius)
                });
            }
        }

        private float3 GetRandomCirclePosition(float3 center, float radius)
        {
            float ang = UnityEngine.Random.value * 360;
            float3 pos;
            pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.y = center.y;
            pos.z = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            return pos;
        }

        protected override void OnDestroy()
        {
            _buffer.Dispose();
        }
    }

    public class InitializeAttackerSystem : SystemBase
    {
        private EventQueue<ActorCreatedEvent> _createdEvents;
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private EntityQuery _newAttackerQuery;

        protected override void OnCreate()
        {
            _createdEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<ActorCreatedEvent>();
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _createdEvents;

            Entities.WithAll<AttackerTag, UnprocessedTag>().ForEach((Entity entity, ref ActorDefinition def) =>
            {
                commands.RemoveComponent<UnprocessedTag>(entity);

                events.Enqueue(new ActorCreatedEvent
                {
                    AssetId = def.AssetId,
                });

            }).WithStoreEntityQueryInField(ref _newAttackerQuery).Run();
        }
    }
}