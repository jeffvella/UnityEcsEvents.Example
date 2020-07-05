using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Components.Tags;
using Assets.Scripts.Providers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Vella.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Systems
{
    public class SpawnAttackerSystem : SystemBase
    {
        private NativeList<Entity> _buffer;
        private EventQueue<PlayAudioEvent> _playAudioEvents;
        private IProvider<Prefabs> _prefabs;

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

            events.Enqueue(new PlayAudioEvent {Sound = SoundCategory.Spawn});

            Entities.ForEach((ref SpawnActorEvent e) =>
            {
                if (e.Catetory != ActorCategory.Attacker)
                    return;

                if (prefabData.Ref.TryGetFirst(e.Catetory, out var prefab))
                {
                    entities.Resize(e.Amount, NativeArrayOptions.UninitializedMemory);

                    EntityManager.Instantiate(prefab.Entity, entities);

                    SetSpawnPositions(entities, center, radius);

                    EntityManager.AddComponent<AttackerTag>(entities);
                }
            }).WithStructuralChanges().Run();
        }

        private void SetSpawnPositions(NativeList<Entity> entities, float3 center, float radius)
        {
            for (var i = 0; i < entities.Length; i++)
                EntityManager.SetComponentData(entities[i], new Translation {Value = GetRandomCirclePosition(center, radius)});
        }

        private float3 GetRandomCirclePosition(float3 center, float radius)
        {
            var ang = Random.value * 360;
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
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private EventQueue<ActorCreatedEvent> _createdEvents;
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

                events.Enqueue(new ActorCreatedEvent {AssetId = def.AssetId});
            }).WithStoreEntityQueryInField(ref _newAttackerQuery).Run();
        }
    }
}