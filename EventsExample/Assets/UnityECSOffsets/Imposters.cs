using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities.Unsafe
{

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ArchetypeChunkImposter
    {
        [FieldOffset(ArchetypeChunkOffsets._0000_ChunkPtr_m_Chunk_8)]
        [NativeDisableUnsafePtrRestriction]
        public ChunkImposter* Chunk;

        [FieldOffset(ArchetypeChunkOffsets._0008_EntityComponentStorePtr_entityComponentStore_8)]
        [NativeDisableUnsafePtrRestriction]
        public EntityComponentStoreImposter* EntityComponentStore;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct EntityComponentStoreImposter
    {
        [FieldOffset(EntityComponentStoreOffsets._0000_Int32Ptr_m_VersionByEntity_8)]
        [NativeDisableUnsafePtrRestriction]
        public int* VersionByEntity;

        [FieldOffset(EntityComponentStoreOffsets._0008_ArchetypePtrPtr_m_ArchetypeByEntity_8)]
        [NativeDisableUnsafePtrRestriction]
        public ArchetypeImposter** ArchetypeByEntity;

        //[FieldOffset(EntityComponentStoreOffsets._0016_EntityInChunkPtr_m_EntityInChunkByEntity_8)]
        //[NativeDisableUnsafePtrRestriction]
        //public EntityInChunk ArchetypeByEntity;
    }

    //public unsafe struct EntityInChunkImposter
    //{
    //    [FieldOffset(8)]
    //    [NativeDisableUnsafePtrRestriction]
    //    public EntityInChunk* ArchetypeByEntity;
    //}

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ChunkImposter
    {
        [FieldOffset(ChunkOffsets._0000_ArchetypePtr_Archetype_8)]
        [NativeDisableUnsafePtrRestriction]
        public ArchetypeImposter* Archetype;

        [FieldOffset(ChunkOffsets._0008_Entity_metaChunkEntity_Index_4)]
        public Entity metaChunkEntity;

        [FieldOffset(ChunkOffsets._0016_Int32_Count_4)]
        public int Count;

        [FieldOffset(ChunkOffsets._0020_Int32_Capacity_4)]
        public int Capacity;

        //[FieldOffset(ChunkOffsets._0024_Int32_ManagedArrayIndex_4)]
        //public int ManagedArrayIndex;

        //[FieldOffset(ChunkOffsets._0028_Int32_ListIndex_4)]
        //public int ListIndex;

        //[FieldOffset(ChunkOffsets._0032_Int32_ListWithEmptySlotsIndex_4)]
        //public int ListWithEmptySlotsIndex;

        //[FieldOffset(ChunkOffsets._0036_UInt32_Flags_4)]
        //public uint Flags;

        //[FieldOffset(ChunkOffsets._0040_UInt64_SequenceNumber_8)]
        //public ulong SequenceNumber;

        //[FieldOffset(ChunkOffsets._0048_Byte_Buffer_m_value_1)]
        //public fixed byte Buffer[4];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct ArchetypeImposter
    {
        [FieldOffset(ArchetypeOffsets._0120_ComponentTypeInArchetypePtr_Types_8)]
        [NativeDisableUnsafePtrRestriction]
        public int* Types;

        [FieldOffset(ArchetypeOffsets._0136_Int32_TypesCount_4)]
        public int TypesCount;

        [FieldOffset(ArchetypeOffsets._0160_Int32Ptr_Offsets_8)]
        [NativeDisableUnsafePtrRestriction]
        public int* Offsets;

        [FieldOffset(ArchetypeOffsets._0168_Int32Ptr_SizeOfs_8)]
        [NativeDisableUnsafePtrRestriction]
        public int* SizeOfs;
    }



}
