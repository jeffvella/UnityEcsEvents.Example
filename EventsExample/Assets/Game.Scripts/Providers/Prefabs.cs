using System;
using Assets.Scripts.Components;
using Assets.Scripts.Support;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Providers
{
    public struct PrefabRef : IComponentData, IEquatable<PrefabRef>
    {
        public Entity Entity;
        public ActorDefinition Definition;
        internal float3 SpawnOffset;

        public bool Equals(PrefabRef other)
        {
            return Definition.AssetId == other.Definition.AssetId;
        }
    }

    public struct Prefabs : INativeProvider
    {
        private UnsafeMultiHashMap<int, PrefabRef> _prefabsByTeam;
        private UnsafeHashMap<int, PrefabRef> _prefabsByAssetId;

        public int Length { get; private set; }

        public void Allocate(SystemBase owner, Allocator allocator)
        {
            _prefabsByTeam = new UnsafeMultiHashMap<int, PrefabRef>(1, allocator);
            _prefabsByAssetId = new UnsafeHashMap<int, PrefabRef>(1, allocator);
        }

        public void Clear(PrefabRef prefab)
        {
            var teamKey = (int) prefab.Definition.Team;
            var idKey = (int) prefab.Definition.AssetId;
            if (_prefabsByAssetId.ContainsKey(idKey))
            {
                Length -= _prefabsByTeam.CountValuesForKey(teamKey); // todo this doesnt seem right.
                _prefabsByTeam.Remove(teamKey, prefab);
                _prefabsByAssetId.Remove(idKey);
            }
        }

        public void AddOrReplace(PrefabRef prefab)
        {
            var teamKey = (int) prefab.Definition.Team;
            var idKey = (int) prefab.Definition.AssetId;
            if (_prefabsByAssetId.ContainsKey(idKey))
            {
                _prefabsByTeam.Remove(teamKey, prefab);
                _prefabsByAssetId.Remove(idKey);
            }
            else
            {
                Length++;
            }

            _prefabsByTeam.Add(teamKey, prefab);
            _prefabsByAssetId.Add(idKey, prefab);
        }

        public void Add(PrefabRef prefab)
        {
            var teamKey = (int) prefab.Definition.Team;
            var idKey = (int) prefab.Definition.AssetId;
            _prefabsByTeam.Add(teamKey, prefab);
            _prefabsByAssetId.Add(idKey, prefab);
            Length++;
        }

        public bool TryGetFirst(ActorCategory team, out PrefabRef item)
        {
            return _prefabsByTeam.TryGetFirstValue((int) team, out item, out var it);
        }

        public bool TryGet(ActorAssetId id, out PrefabRef item)
        {
            return _prefabsByAssetId.TryGetValue((int) id, out item);
        }

        public UnsafeEnumerator<int, PrefabRef> GetEnumerator()
        {
            return new UnsafeEnumerator<int, PrefabRef>(_prefabsByTeam.GetKeyValueArrays(Allocator.Temp));
        }

        public void Dispose()
        {
            _prefabsByTeam.Dispose();
            _prefabsByAssetId.Dispose();
        }
    }
}