using System;
using Providers.Grid;
using Unity.Entities;
using Unity.Mathematics;
using Vella.Common.Game.Navigation;

namespace Unity.Collections
{
    public struct NativeArray3D<T> : IDisposable where T : unmanaged
    {
        public BlobAssetReference<BlobGrid3D<T, NodeFlags>> Data;

        public NativeArray3D(int3 size, Allocator allocator) : this(size.x, size.y, size.z, allocator)
        {
        }

        public NativeArray3D(int x, int y, int z, Allocator allocator)
        {
            Data = BlobGrid3D<T, NodeFlags>.Build(x, y, z, allocator);

            // var length = x * y * z;
            // var builder = new BlobBuilder(Allocator.Temp);
            // ref var root = ref builder.ConstructRoot<BlobGrid3D<T>>();
            // root.Size = new int4(x, y, z, y * z);
            // var nodeBuilder = builder.Allocate(ref root.Nodes, length);
            // Data = builder.CreateBlobAssetReference<BlobGrid3D<T>>(allocator);
            // builder.Dispose();
        }

        public ref T this[int i] => ref Data.Value[i];
        public ref T this[int x, int y, int z] => ref Data.Value[x, y, z];

        public int3 Size => Data.Value.Size.xyz;
        public bool IsCreated => Data.IsCreated;

        public void Dispose()
        {
            Data.Dispose();
        }

        public int Length => Data.Value.Nodes.Length;

        [Obsolete("use Size instead")]
        public int GetLength(int dimensionIndex)
        {
            return Data.Value.Size[dimensionIndex];
        }

        // todo: remove managed members
        //public List<T> ToList() => Data.Value.Nodes.ToArray().ToList();
        //public IEnumerator<T> GetEnumerator() => ToList().GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}