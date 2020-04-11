using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities.Unsafe
{
    public static unsafe class ArchetypeChunkImposterExtensions
    {
        public static ref ArchetypeChunkImposter AsImposter(this ArchetypeChunk archetypeChunk)
        {
            return ref UnsafeUtilityEx.AsRef<ArchetypeChunkImposter>(UnsafeUtility.AddressOf(ref archetypeChunk));
        }


        //public static UnsafeArray GetComponents(this ArchetypeChunkImposter archetypeChunk, int typeIndex, bool isReadOnly = false)
        //{
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(typeIndex);
        //    if (typeIndexInArchetype == -1)
        //        return default;

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    return new UnsafeArray
        //    {
        //        Ptr = archetypeChunk.Chunk->Buffer + offset,
        //        Length = archetypeChunk.Chunk->Count
        //    };
        //}

        //public static UnsafeArray<T> GetComponents<T>(this ArchetypeChunkImposter archetypeChunk) where T : unmanaged
        //{
        //    var typeIndex = TypeManager.GetTypeInfo<T>().TypeIndex;
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(typeIndex);
        //    if (typeIndexInArchetype == -1)
        //        return default;

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    return new UnsafeArray<T>
        //    {
        //        Ptr = (T*)archetypeChunk.Chunk->Buffer + offset,
        //        Length = archetypeChunk.Chunk->Count
        //    };
        //}

        //public static ref T GetComponent<T>(this ArchetypeChunkImposter archetypeChunk, ComponentType type, int entityIndex) where T : unmanaged
        //{
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(type.TypeIndex);
        //    if (typeIndexInArchetype == -1)
        //        throw new InvalidOperationException("Component type is not in the chunk");

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    var ptr = archetypeChunk.Chunk->Buffer + offset;
        //    return ref UnsafeUtilityEx.ArrayElementAsRef<T>(ptr, entityIndex);
        //    //return *((T*)ptr + index * UnsafeUtility.SizeOf<T>());
        //}

        ///// <summary>
        ///// Pointer to the first component in the chunk's area allocated for the specified <see cref="ComponentType"/>
        ///// </summary>
        //public static void* GetComponentPtr(this ArchetypeChunkImposter archetypeChunk, ComponentType type)
        //{
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(type.TypeIndex);
        //    if (typeIndexInArchetype == -1)
        //        throw new InvalidOperationException("Component type is not in the chunk");

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    return archetypeChunk.Chunk->Buffer + offset;
        //}

        ///// <summary>
        ///// Pointer to a specific component in the chunk's area allocated for the specified <see cref="ComponentType"/>
        ///// </summary>
        //public static void* GetComponentPtr(this ArchetypeChunkImposter archetypeChunk, ComponentType type, int entityIndex)
        //{
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(type.TypeIndex);
        //    if (typeIndexInArchetype == -1)
        //        throw new InvalidOperationException("Component type is not in the chunk");

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    var ptr = archetypeChunk.Chunk->Buffer + offset;
        //    return ptr + entityIndex * TypeManager.GetTypeInfo(type.TypeIndex).SizeInChunk;
        //}

        //public static void* GetComponentPtr(this ArchetypeChunkImposter archetypeChunk, ComponentType type, int index)
        //{
        //    var typeIndexInArchetype = archetypeChunk.GetTypeIndexInChunk(type.TypeIndex);
        //    if (typeIndexInArchetype == -1)
        //        throw new InvalidOperationException("Component type is not in the chunk");

        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    var sizeOf = archetypeChunk.Chunk->Archetype->SizeOfs[typeIndexInArchetype];

        //    return archetypeChunk.Chunk->Buffer + offset + index * sizeOf;
        //}

        //public static void* GetComponentPtr(this ArchetypeChunkImposter archetypeChunk, int typeIndexInArchetype, int index)
        //{
        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[typeIndexInArchetype];
        //    var sizeOf = archetypeChunk.Chunk->Archetype->SizeOfs[typeIndexInArchetype];
        //    return archetypeChunk.Chunk->Buffer + offset + index * sizeOf;
        //}

        //public static Entity* GetEntityPtr(this ArchetypeChunkImposter archetypeChunk)
        //{
        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[0];
        //    return (Entity*)archetypeChunk.Chunk->Buffer + offset;
        //}

        //public static Entity* GetEntityPtr(this ArchetypeChunkImposter archetypeChunk, int index)
        //{
        //    var offset = archetypeChunk.Chunk->Archetype->Offsets[0];
        //    var startPtr = archetypeChunk.Chunk->Buffer + offset;
        //    var offsetToItem = index * UnsafeUtility.SizeOf<Entity>();
        //    return (Entity*)(startPtr + offsetToItem);
        //}

        public static bool HasComponent(this ArchetypeChunkImposter archetypeChunk, ComponentType type)
        {
            return archetypeChunk.GetTypeIndexInChunk(type.TypeIndex) != -1;
        }

        public static bool HasComponent<T>(this ArchetypeChunkImposter archetypeChunk) where T : struct
        {
            return archetypeChunk.GetTypeIndexInChunk(TypeManager.GetTypeInfo<T>().TypeIndex) != -1;
        }

        public static bool HasComponent<T>(this ArchetypeChunkImposter archetypeChunk, ArchetypeChunkComponentType<T> type) where T : unmanaged, IComponentData
        {
            return archetypeChunk.GetTypeIndexInChunk(UnsafeUtilityEx.AsRef<int>(UnsafeUtility.AddressOf(ref type))) != -1;
        }

        public static bool HasComponent<T>(this ArchetypeChunkImposter archetypeChunk, ArchetypeChunkBufferType<T> type) where T : unmanaged, IBufferElementData
        {
            return archetypeChunk.GetTypeIndexInChunk(UnsafeUtilityEx.AsRef<int>(UnsafeUtility.AddressOf(ref type))) != -1;
        }

        public static bool HasComponent(this ArchetypeChunkImposter archetypeChunk, int typeIndex)
        {
            return archetypeChunk.GetTypeIndexInChunk(typeIndex) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTypeIndexInChunk<T>(this ArchetypeChunkImposter archetypeChunk) where T : struct
        {
            return GetTypeIndexInChunk(archetypeChunk, TypeManager.GetTypeInfo<T>().TypeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTypeIndexInChunk(this ArchetypeChunkImposter archetypeChunk, int typeIndexToFind)
        {
            var types = archetypeChunk.Chunk->Archetype->Types;
            var typeCount = archetypeChunk.Chunk->Archetype->TypesCount;

            for (var i = 0; i != typeCount; i++)
                if (typeIndexToFind == types[i])
                    return i;

            return -1;
        }
    }

}
