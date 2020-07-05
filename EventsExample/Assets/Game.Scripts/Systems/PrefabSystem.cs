using System.Diagnostics;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Tags;
using Assets.Scripts.Providers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Vella.Events.Extensions;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Systems
{
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class PrefabSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _commandSystem;
        private IProvider<Prefabs> _prefabs;
        private EntityQuery _unprocessedPrefabs;

        protected override void OnCreate()
        {
            _prefabs = World.GetOrCreateProvider<Prefabs>();
            _commandSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            LoadNewPrefabs();
        }

        private void LoadNewPrefabs()
        {
            var prefabs = _prefabs.Data;
            var commands = _commandSystem.CreateCommandBuffer();

            Debug.Log($"Processing: {_unprocessedPrefabs.CalculateEntityCount()} Prefabs.");

            Entities.ForEach((Entity entity, in ActorDefinition def) =>
            {
                float3 spawnOffset = default;
                if (HasComponent<Translation>(entity) && def.Flags.IsFlagSet(ActorFlags.AddTransformAsOffset))
                    spawnOffset = GetComponent<Translation>(entity).Value;

                prefabs.Ref.AddOrReplace(new PrefabRef {Entity = entity, Definition = def, SpawnOffset = spawnOffset});

                commands.AddComponent<OwnerRef>(entity);
                commands.AddComponent<UninitializedTag>(entity);
                commands.RemoveComponent<UnprocessedTag>(entity);
            }).WithAll<Prefab, UnprocessedTag>().WithStoreEntityQueryInField(ref _unprocessedPrefabs).WithEntityQueryOptions(EntityQueryOptions.IncludePrefab).Run();

            SetEntityNames();
        }


        [Conditional("UNITY_EDITOR")]
        public void SetEntityNames()
        {
#if UNITY_EDITOR
            foreach (var pair in _prefabs.Data.Ref)
            {
                // Fix for entities not being named when a sub-scene is not live-linked.}
                EntityManager.SetName(pair.Value.Entity, pair.Value.Definition.AssetId.ToString());
            }
#endif              
        }
      
    }
}