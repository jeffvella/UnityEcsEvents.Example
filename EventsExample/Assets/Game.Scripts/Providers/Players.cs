using Assets.Scripts.Support;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Assets.Scripts.Providers
{
    public struct PlayerRef
    {
        public int Id;
        public Entity Entity;
    }

    /// <summary>
    ///     A burstable lookup for <see cref="PlayerRef" />; maintained by <see cref="SpawnPlayerSystem" />
    /// </summary>
    public struct Players : INativeProvider
    {
        private UnsafeMultiHashMap<int, PlayerRef> _playerById;

        public int Length { get; private set; }

        public void Allocate(SystemBase owner, Allocator allocator)
        {
            _playerById = new UnsafeMultiHashMap<int, PlayerRef>(1, allocator);
        }

        public void Add(PlayerRef player)
        {
            _playerById.Add(player.Id, player);
            Length++;
        }

        public bool TryGet(int playerId, out PlayerRef item)
        {
            return _playerById.TryGetFirstValue(playerId, out item, out var it);
        }

        public UnsafeEnumerator<int, PlayerRef> GetEnumerator()
        {
            return new UnsafeEnumerator<int, PlayerRef>(_playerById.GetKeyValueArrays(Allocator.Temp));
        }

        public void Dispose()
        {
            _playerById.Dispose();
        }
    }
}