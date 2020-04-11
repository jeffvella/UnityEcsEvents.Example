using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Unity.Entities.Unsafe
{

    #region Auto-Generated Content

        // Entities: 0.9.0-preview.6

        public struct ArchetypeChunkOffsets
        {           
            public const int _0000_ChunkPtr_m_Chunk_8 = 0;
            public const int _0008_EntityComponentStorePtr_entityComponentStore_8 = 8;
        }

        public struct ChunkOffsets
        {           
            public const int _0000_ArchetypePtr_Archetype_8 = 0;
            public const int _0008_Entity_metaChunkEntity_Index_4 = 8;
            public const int _0012_Entity_metaChunkEntity_Version_4 = 12;
            public const int _0016_Int32_Count_4 = 16;
            public const int _0020_Int32_Capacity_4 = 20;
            public const int _0024_Int32_ListIndex_4 = 24;
            public const int _0028_Int32_ListWithEmptySlotsIndex_4 = 28;
            public const int _0032_UInt32_Flags_4 = 32;
            public const int _0036_UInt64_SequenceNumber_8 = 36;
            public const int _0064_Byte_Buffer_m_value_1 = 64;
            public const int _0065_Byte_Buffer_m_value_1 = 65;
            public const int _0066_Byte_Buffer_m_value_1 = 66;
            public const int _0067_Byte_Buffer_m_value_1 = 67;
        }

        public struct ByteOffsets
        {           
            public const int _0000_Byte_m_value_1 = 0;
        }

        public struct EntityComponentStoreOffsets
        {           
            public const int _0000_Int32Ptr_m_VersionByEntity_8 = 0;
            public const int _0008_ArchetypePtrPtr_m_ArchetypeByEntity_8 = 8;
            public const int _0016_EntityInChunkPtr_m_EntityInChunkByEntity_8 = 16;
            public const int _0024_Int32Ptr_m_ComponentTypeOrderVersion_8 = 24;
            public const int _0032_BlockAllocator_m_ArchetypeChunkAllocator_m_FirstBlock_8 = 32;
            public const int _0040_BlockAllocator_m_ArchetypeChunkAllocator_m_LastBlock_8 = 40;
            public const int _0048_BlockAllocator_m_ArchetypeChunkAllocator_m_LastBlockUsedSize_4 = 48;
            public const int _0052_BlockAllocator_m_ArchetypeChunkAllocator_totalSizeInBytes_4 = 52;
            public const int _0056_UnsafeChunkPtrList_m_EmptyChunks_Ptr_8 = 56;
            public const int _0064_UnsafeChunkPtrList_m_EmptyChunks_Length_4 = 64;
            public const int _0068_UnsafeChunkPtrList_m_EmptyChunks_Capacity_4 = 68;
            public const int _0072_UnsafeChunkPtrList_m_EmptyChunks_Allocator_4 = 72;
            public const int _0080_UnsafeArchetypePtrList_m_Archetypes_Ptr_8 = 80;
            public const int _0088_UnsafeArchetypePtrList_m_Archetypes_Length_4 = 88;
            public const int _0092_UnsafeArchetypePtrList_m_Archetypes_Capacity_4 = 92;
            public const int _0096_UnsafeArchetypePtrList_m_Archetypes_Allocator_4 = 96;
            public const int _0104_UnsafeChunkPtrList_m_DeleteChunks_Ptr_8 = 104;
            public const int _0112_UnsafeChunkPtrList_m_DeleteChunks_Length_4 = 112;
            public const int _0116_UnsafeChunkPtrList_m_DeleteChunks_Capacity_4 = 116;
            public const int _0120_UnsafeChunkPtrList_m_DeleteChunks_Allocator_4 = 120;
            public const int _0128_UnsafeUintList_hashes_Ptr_8 = 128;
            public const int _0136_UnsafeUintList_hashes_Length_4 = 136;
            public const int _0140_UnsafeUintList_hashes_Capacity_4 = 140;
            public const int _0144_UnsafeUintList_hashes_Allocator_4 = 144;
            public const int _0152_UnsafeArchetypePtrList_archetypes_Ptr_8 = 152;
            public const int _0160_UnsafeArchetypePtrList_archetypes_Length_4 = 160;
            public const int _0164_UnsafeArchetypePtrList_archetypes_Capacity_4 = 164;
            public const int _0168_UnsafeArchetypePtrList_archetypes_Allocator_4 = 168;
            public const int _0176_ArchetypeListMap_m_TypeLookup_emptyNodes_4 = 176;
            public const int _0180_ArchetypeListMap_m_TypeLookup_skipNodes_4 = 180;
            public const int _0184_Int32_m_ManagedComponentIndex_4 = 184;
            public const int _0188_Int32_m_ManagedComponentIndexCapacity_4 = 188;
            public const int _0192_UnsafeAppendBuffer_m_ManagedComponentFreeIndex_Ptr_8 = 192;
            public const int _0200_UnsafeAppendBuffer_m_ManagedComponentFreeIndex_Length_4 = 200;
            public const int _0204_UnsafeAppendBuffer_m_ManagedComponentFreeIndex_Capacity_4 = 204;
            public const int _0208_UnsafeAppendBuffer_m_ManagedComponentFreeIndex_Allocator_4 = 208;
            public const int _0212_UnsafeAppendBuffer_m_ManagedComponentFreeIndex_Alignment_4 = 212;
            public const int _0216_UnsafeAppendBuffer_CommandBuffer_Ptr_8 = 216;
            public const int _0224_UnsafeAppendBuffer_CommandBuffer_Length_4 = 224;
            public const int _0228_UnsafeAppendBuffer_CommandBuffer_Capacity_4 = 228;
            public const int _0232_UnsafeAppendBuffer_CommandBuffer_Allocator_4 = 232;
            public const int _0236_UnsafeAppendBuffer_CommandBuffer_Alignment_4 = 236;
            public const int _0240_UInt64_m_NextChunkSequenceNumber_8 = 240;
            public const int _0248_Int32_m_NextFreeEntityIndex_4 = 248;
            public const int _0252_UInt32_m_GlobalSystemVersion_4 = 252;
            public const int _0256_Int32_m_EntitiesCapacity_4 = 256;
            public const int _0260_UInt32_m_ArchetypeTrackingVersion_4 = 260;
            public const int _0264_Int32_m_LinkedGroupType_4 = 264;
            public const int _0268_Int32_m_ChunkHeaderType_4 = 268;
            public const int _0272_Int32_m_PrefabType_4 = 272;
            public const int _0276_Int32_m_CleanupEntityType_4 = 276;
            public const int _0280_Int32_m_DisabledType_4 = 280;
            public const int _0284_Int32_m_EntityType_4 = 284;
            public const int _0288_ComponentType_m_ChunkHeaderComponentType_TypeIndex_4 = 288;
            public const int _0292_ComponentType_m_ChunkHeaderComponentType_AccessModeType_4 = 292;
            public const int _0296_ComponentType_m_EntityComponentType_TypeIndex_4 = 296;
            public const int _0300_ComponentType_m_EntityComponentType_AccessModeType_4 = 300;
            public const int _0304_TypeInfoPtr_m_TypeInfos_8 = 304;
            public const int _0312_EntityOffsetInfoPtr_m_EntityOffsetInfos_8 = 312;
            public const int _0320_Byte_memoryInitPattern_1 = 320;
            public const int _0321_Byte_useMemoryInitPattern_1 = 321;
            public const int _0328_NumberedWordsPtr_m_NameByEntity_8 = 328;
        }

        public struct UnsafeArchetypePtrListOffsets
        {           
            public const int _0000_ArchetypePtrPtr_Ptr_8 = 0;
            public const int _0008_Int32_Length_4 = 8;
            public const int _0012_Int32_Capacity_4 = 12;
            public const int _0016_Allocator_Allocator_4 = 16;
        }

        public struct UnsafeChunkPtrListOffsets
        {           
            public const int _0000_ChunkPtrPtr_Ptr_8 = 0;
            public const int _0008_Int32_Length_4 = 8;
            public const int _0012_Int32_Capacity_4 = 12;
            public const int _0016_Allocator_Allocator_4 = 16;
        }

        public struct UnsafeUintListOffsets
        {           
            public const int _0000_UInt32Ptr_Ptr_8 = 0;
            public const int _0008_Int32_Length_4 = 8;
            public const int _0012_Int32_Capacity_4 = 12;
            public const int _0016_Allocator_Allocator_4 = 16;
        }

        public struct ArchetypeListMapOffsets
        {           
            public const int _0000_UnsafeUintList_hashes_Ptr_8 = 0;
            public const int _0008_UnsafeUintList_hashes_Length_4 = 8;
            public const int _0012_UnsafeUintList_hashes_Capacity_4 = 12;
            public const int _0016_UnsafeUintList_hashes_Allocator_4 = 16;
            public const int _0024_UnsafeArchetypePtrList_archetypes_Ptr_8 = 24;
            public const int _0032_UnsafeArchetypePtrList_archetypes_Length_4 = 32;
            public const int _0036_UnsafeArchetypePtrList_archetypes_Capacity_4 = 36;
            public const int _0040_UnsafeArchetypePtrList_archetypes_Allocator_4 = 40;
            public const int _0048_Int32_emptyNodes_4 = 48;
            public const int _0052_Int32_skipNodes_4 = 52;
        }

        public struct BlockAllocatorOffsets
        {           
            public const int _0000_BytePtr_m_FirstBlock_8 = 0;
            public const int _0008_BytePtr_m_LastBlock_8 = 8;
            public const int _0016_Int32_m_LastBlockUsedSize_4 = 16;
            public const int _0020_Int32_totalSizeInBytes_4 = 20;
        }

        public struct UnsafeAppendBufferOffsets
        {           
            public const int _0000_BytePtr_Ptr_8 = 0;
            public const int _0008_Int32_Length_4 = 8;
            public const int _0012_Int32_Capacity_4 = 12;
            public const int _0016_Allocator_Allocator_4 = 16;
            public const int _0020_Int32_Alignment_4 = 20;
        }

        public struct ArchetypeOffsets
        {           
            public const int _0000_ArchetypeChunkData_Chunks_p_8 = 0;
            public const int _0008_ArchetypeChunkData_Chunks_data_8 = 8;
            public const int _0016_ArchetypeChunkData_Chunks_Capacity_4 = 16;
            public const int _0020_ArchetypeChunkData_Chunks_Count_4 = 20;
            public const int _0024_ArchetypeChunkData_Chunks_SharedComponentCount_4 = 24;
            public const int _0028_ArchetypeChunkData_Chunks_EntityCountIndex_4 = 28;
            public const int _0032_ArchetypeChunkData_Chunks_Channels_4 = 32;
            public const int _0040_UnsafeChunkPtrList_ChunksWithEmptySlots_Ptr_8 = 40;
            public const int _0048_UnsafeChunkPtrList_ChunksWithEmptySlots_Length_4 = 48;
            public const int _0052_UnsafeChunkPtrList_ChunksWithEmptySlots_Capacity_4 = 52;
            public const int _0056_UnsafeChunkPtrList_ChunksWithEmptySlots_Allocator_4 = 56;
            public const int _0064_UnsafeUintList_hashes_Ptr_8 = 64;
            public const int _0072_UnsafeUintList_hashes_Length_4 = 72;
            public const int _0076_UnsafeUintList_hashes_Capacity_4 = 76;
            public const int _0080_UnsafeUintList_hashes_Allocator_4 = 80;
            public const int _0088_UnsafeChunkPtrList_chunks_Ptr_8 = 88;
            public const int _0096_UnsafeChunkPtrList_chunks_Length_4 = 96;
            public const int _0100_UnsafeChunkPtrList_chunks_Capacity_4 = 100;
            public const int _0104_UnsafeChunkPtrList_chunks_Allocator_4 = 104;
            public const int _0112_ChunkListMap_FreeChunksBySharedComponents_emptyNodes_4 = 112;
            public const int _0116_ChunkListMap_FreeChunksBySharedComponents_skipNodes_4 = 116;
            public const int _0120_ComponentTypeInArchetypePtr_Types_8 = 120;
            public const int _0128_Int32_EntityCount_4 = 128;
            public const int _0132_Int32_ChunkCapacity_4 = 132;
            public const int _0136_Int32_TypesCount_4 = 136;
            public const int _0140_Int32_InstanceSize_4 = 140;
            public const int _0144_Int32_InstanceSizeWithOverhead_4 = 144;
            public const int _0148_Int32_ManagedEntityPatchCount_4 = 148;
            public const int _0152_Int32_ScalarEntityPatchCount_4 = 152;
            public const int _0156_Int32_BufferEntityPatchCount_4 = 156;
            public const int _0160_Int32Ptr_Offsets_8 = 160;
            public const int _0168_Int32Ptr_SizeOfs_8 = 168;
            public const int _0176_Int32Ptr_BufferCapacities_8 = 176;
            public const int _0184_Int32Ptr_TypeMemoryOrder_8 = 184;
            public const int _0192_Int16_FirstBufferComponent_2 = 192;
            public const int _0194_Int16_FirstManagedComponent_2 = 194;
            public const int _0196_Int16_FirstTagComponent_2 = 196;
            public const int _0198_Int16_FirstSharedComponent_2 = 198;
            public const int _0200_Int16_FirstChunkComponent_2 = 200;
            public const int _0202_ArchetypeFlags_Flags_2 = 202;
            public const int _0208_ArchetypePtr_CopyArchetype_8 = 208;
            public const int _0216_ArchetypePtr_InstantiateArchetype_8 = 216;
            public const int _0224_ArchetypePtr_SystemStateResidueArchetype_8 = 224;
            public const int _0232_ArchetypePtr_MetaChunkArchetype_8 = 232;
            public const int _0240_EntityPatchInfoPtr_ScalarEntityPatches_8 = 240;
            public const int _0248_BufferEntityPatchInfoPtr_BufferEntityPatches_8 = 248;
            public const int _0256_ManagedEntityPatchInfoPtr_ManagedEntityPatches_8 = 256;
            public const int _0264_Byte_QueryMaskArray_m_value_1 = 264;
            public const int _0265_Byte_QueryMaskArray_m_value_1 = 265;
            public const int _0266_Byte_QueryMaskArray_m_value_1 = 266;
            public const int _0267_Byte_QueryMaskArray_m_value_1 = 267;
            public const int _0268_Byte_QueryMaskArray_m_value_1 = 268;
            public const int _0269_Byte_QueryMaskArray_m_value_1 = 269;
            public const int _0270_Byte_QueryMaskArray_m_value_1 = 270;
            public const int _0271_Byte_QueryMaskArray_m_value_1 = 271;
            public const int _0272_Byte_QueryMaskArray_m_value_1 = 272;
            public const int _0273_Byte_QueryMaskArray_m_value_1 = 273;
            public const int _0274_Byte_QueryMaskArray_m_value_1 = 274;
            public const int _0275_Byte_QueryMaskArray_m_value_1 = 275;
            public const int _0276_Byte_QueryMaskArray_m_value_1 = 276;
            public const int _0277_Byte_QueryMaskArray_m_value_1 = 277;
            public const int _0278_Byte_QueryMaskArray_m_value_1 = 278;
            public const int _0279_Byte_QueryMaskArray_m_value_1 = 279;
            public const int _0280_Byte_QueryMaskArray_m_value_1 = 280;
            public const int _0281_Byte_QueryMaskArray_m_value_1 = 281;
            public const int _0282_Byte_QueryMaskArray_m_value_1 = 282;
            public const int _0283_Byte_QueryMaskArray_m_value_1 = 283;
            public const int _0284_Byte_QueryMaskArray_m_value_1 = 284;
            public const int _0285_Byte_QueryMaskArray_m_value_1 = 285;
            public const int _0286_Byte_QueryMaskArray_m_value_1 = 286;
            public const int _0287_Byte_QueryMaskArray_m_value_1 = 287;
            public const int _0288_Byte_QueryMaskArray_m_value_1 = 288;
            public const int _0289_Byte_QueryMaskArray_m_value_1 = 289;
            public const int _0290_Byte_QueryMaskArray_m_value_1 = 290;
            public const int _0291_Byte_QueryMaskArray_m_value_1 = 291;
            public const int _0292_Byte_QueryMaskArray_m_value_1 = 292;
            public const int _0293_Byte_QueryMaskArray_m_value_1 = 293;
            public const int _0294_Byte_QueryMaskArray_m_value_1 = 294;
            public const int _0295_Byte_QueryMaskArray_m_value_1 = 295;
            public const int _0296_Byte_QueryMaskArray_m_value_1 = 296;
            public const int _0297_Byte_QueryMaskArray_m_value_1 = 297;
            public const int _0298_Byte_QueryMaskArray_m_value_1 = 298;
            public const int _0299_Byte_QueryMaskArray_m_value_1 = 299;
            public const int _0300_Byte_QueryMaskArray_m_value_1 = 300;
            public const int _0301_Byte_QueryMaskArray_m_value_1 = 301;
            public const int _0302_Byte_QueryMaskArray_m_value_1 = 302;
            public const int _0303_Byte_QueryMaskArray_m_value_1 = 303;
            public const int _0304_Byte_QueryMaskArray_m_value_1 = 304;
            public const int _0305_Byte_QueryMaskArray_m_value_1 = 305;
            public const int _0306_Byte_QueryMaskArray_m_value_1 = 306;
            public const int _0307_Byte_QueryMaskArray_m_value_1 = 307;
            public const int _0308_Byte_QueryMaskArray_m_value_1 = 308;
            public const int _0309_Byte_QueryMaskArray_m_value_1 = 309;
            public const int _0310_Byte_QueryMaskArray_m_value_1 = 310;
            public const int _0311_Byte_QueryMaskArray_m_value_1 = 311;
            public const int _0312_Byte_QueryMaskArray_m_value_1 = 312;
            public const int _0313_Byte_QueryMaskArray_m_value_1 = 313;
            public const int _0314_Byte_QueryMaskArray_m_value_1 = 314;
            public const int _0315_Byte_QueryMaskArray_m_value_1 = 315;
            public const int _0316_Byte_QueryMaskArray_m_value_1 = 316;
            public const int _0317_Byte_QueryMaskArray_m_value_1 = 317;
            public const int _0318_Byte_QueryMaskArray_m_value_1 = 318;
            public const int _0319_Byte_QueryMaskArray_m_value_1 = 319;
            public const int _0320_Byte_QueryMaskArray_m_value_1 = 320;
            public const int _0321_Byte_QueryMaskArray_m_value_1 = 321;
            public const int _0322_Byte_QueryMaskArray_m_value_1 = 322;
            public const int _0323_Byte_QueryMaskArray_m_value_1 = 323;
            public const int _0324_Byte_QueryMaskArray_m_value_1 = 324;
            public const int _0325_Byte_QueryMaskArray_m_value_1 = 325;
            public const int _0326_Byte_QueryMaskArray_m_value_1 = 326;
            public const int _0327_Byte_QueryMaskArray_m_value_1 = 327;
            public const int _0328_Byte_QueryMaskArray_m_value_1 = 328;
            public const int _0329_Byte_QueryMaskArray_m_value_1 = 329;
            public const int _0330_Byte_QueryMaskArray_m_value_1 = 330;
            public const int _0331_Byte_QueryMaskArray_m_value_1 = 331;
            public const int _0332_Byte_QueryMaskArray_m_value_1 = 332;
            public const int _0333_Byte_QueryMaskArray_m_value_1 = 333;
            public const int _0334_Byte_QueryMaskArray_m_value_1 = 334;
            public const int _0335_Byte_QueryMaskArray_m_value_1 = 335;
            public const int _0336_Byte_QueryMaskArray_m_value_1 = 336;
            public const int _0337_Byte_QueryMaskArray_m_value_1 = 337;
            public const int _0338_Byte_QueryMaskArray_m_value_1 = 338;
            public const int _0339_Byte_QueryMaskArray_m_value_1 = 339;
            public const int _0340_Byte_QueryMaskArray_m_value_1 = 340;
            public const int _0341_Byte_QueryMaskArray_m_value_1 = 341;
            public const int _0342_Byte_QueryMaskArray_m_value_1 = 342;
            public const int _0343_Byte_QueryMaskArray_m_value_1 = 343;
            public const int _0344_Byte_QueryMaskArray_m_value_1 = 344;
            public const int _0345_Byte_QueryMaskArray_m_value_1 = 345;
            public const int _0346_Byte_QueryMaskArray_m_value_1 = 346;
            public const int _0347_Byte_QueryMaskArray_m_value_1 = 347;
            public const int _0348_Byte_QueryMaskArray_m_value_1 = 348;
            public const int _0349_Byte_QueryMaskArray_m_value_1 = 349;
            public const int _0350_Byte_QueryMaskArray_m_value_1 = 350;
            public const int _0351_Byte_QueryMaskArray_m_value_1 = 351;
            public const int _0352_Byte_QueryMaskArray_m_value_1 = 352;
            public const int _0353_Byte_QueryMaskArray_m_value_1 = 353;
            public const int _0354_Byte_QueryMaskArray_m_value_1 = 354;
            public const int _0355_Byte_QueryMaskArray_m_value_1 = 355;
            public const int _0356_Byte_QueryMaskArray_m_value_1 = 356;
            public const int _0357_Byte_QueryMaskArray_m_value_1 = 357;
            public const int _0358_Byte_QueryMaskArray_m_value_1 = 358;
            public const int _0359_Byte_QueryMaskArray_m_value_1 = 359;
            public const int _0360_Byte_QueryMaskArray_m_value_1 = 360;
            public const int _0361_Byte_QueryMaskArray_m_value_1 = 361;
            public const int _0362_Byte_QueryMaskArray_m_value_1 = 362;
            public const int _0363_Byte_QueryMaskArray_m_value_1 = 363;
            public const int _0364_Byte_QueryMaskArray_m_value_1 = 364;
            public const int _0365_Byte_QueryMaskArray_m_value_1 = 365;
            public const int _0366_Byte_QueryMaskArray_m_value_1 = 366;
            public const int _0367_Byte_QueryMaskArray_m_value_1 = 367;
            public const int _0368_Byte_QueryMaskArray_m_value_1 = 368;
            public const int _0369_Byte_QueryMaskArray_m_value_1 = 369;
            public const int _0370_Byte_QueryMaskArray_m_value_1 = 370;
            public const int _0371_Byte_QueryMaskArray_m_value_1 = 371;
            public const int _0372_Byte_QueryMaskArray_m_value_1 = 372;
            public const int _0373_Byte_QueryMaskArray_m_value_1 = 373;
            public const int _0374_Byte_QueryMaskArray_m_value_1 = 374;
            public const int _0375_Byte_QueryMaskArray_m_value_1 = 375;
            public const int _0376_Byte_QueryMaskArray_m_value_1 = 376;
            public const int _0377_Byte_QueryMaskArray_m_value_1 = 377;
            public const int _0378_Byte_QueryMaskArray_m_value_1 = 378;
            public const int _0379_Byte_QueryMaskArray_m_value_1 = 379;
            public const int _0380_Byte_QueryMaskArray_m_value_1 = 380;
            public const int _0381_Byte_QueryMaskArray_m_value_1 = 381;
            public const int _0382_Byte_QueryMaskArray_m_value_1 = 382;
            public const int _0383_Byte_QueryMaskArray_m_value_1 = 383;
            public const int _0384_Byte_QueryMaskArray_m_value_1 = 384;
            public const int _0385_Byte_QueryMaskArray_m_value_1 = 385;
            public const int _0386_Byte_QueryMaskArray_m_value_1 = 386;
            public const int _0387_Byte_QueryMaskArray_m_value_1 = 387;
            public const int _0388_Byte_QueryMaskArray_m_value_1 = 388;
            public const int _0389_Byte_QueryMaskArray_m_value_1 = 389;
            public const int _0390_Byte_QueryMaskArray_m_value_1 = 390;
            public const int _0391_Byte_QueryMaskArray_m_value_1 = 391;
        }

        public struct ChunkListMapOffsets
        {           
            public const int _0000_UnsafeUintList_hashes_Ptr_8 = 0;
            public const int _0008_UnsafeUintList_hashes_Length_4 = 8;
            public const int _0012_UnsafeUintList_hashes_Capacity_4 = 12;
            public const int _0016_UnsafeUintList_hashes_Allocator_4 = 16;
            public const int _0024_UnsafeChunkPtrList_chunks_Ptr_8 = 24;
            public const int _0032_UnsafeChunkPtrList_chunks_Length_4 = 32;
            public const int _0036_UnsafeChunkPtrList_chunks_Capacity_4 = 36;
            public const int _0040_UnsafeChunkPtrList_chunks_Allocator_4 = 40;
            public const int _0048_Int32_emptyNodes_4 = 48;
            public const int _0052_Int32_skipNodes_4 = 52;
        }

        public struct ArchetypeChunkDataOffsets
        {           
            public const int _0000_ChunkPtrPtr_p_8 = 0;
            public const int _0008_Int32Ptr_data_8 = 8;
            public const int _0016_Int32_Capacity_4 = 16;
            public const int _0020_Int32_Count_4 = 20;
            public const int _0024_Int32_SharedComponentCount_4 = 24;
            public const int _0028_Int32_EntityCountIndex_4 = 28;
            public const int _0032_Int32_Channels_4 = 32;
        }

        public struct EntityOffsets
        {           
            public const int _0000_Int32_Index_4 = 0;
            public const int _0004_Int32_Version_4 = 4;
        }

        public struct ComponentTypeOffsets
        {           
            public const int _0000_Int32_TypeIndex_4 = 0;
            public const int _0004_AccessMode_AccessModeType_4 = 4;
        }

        public struct ComponentTypesOffsets
        {           
            public const int _0000_FixedListInt64_m_sorted_length_2 = 0;
            public const int _0002_FixedBytes16_offset0000_byte0000_1 = 2;
            public const int _0003_FixedBytes16_offset0000_byte0001_1 = 3;
            public const int _0004_FixedBytes16_offset0000_byte0002_1 = 4;
            public const int _0005_FixedBytes16_offset0000_byte0003_1 = 5;
            public const int _0006_FixedBytes16_offset0000_byte0004_1 = 6;
            public const int _0007_FixedBytes16_offset0000_byte0005_1 = 7;
            public const int _0008_FixedBytes16_offset0000_byte0006_1 = 8;
            public const int _0009_FixedBytes16_offset0000_byte0007_1 = 9;
            public const int _0010_FixedBytes16_offset0000_byte0008_1 = 10;
            public const int _0011_FixedBytes16_offset0000_byte0009_1 = 11;
            public const int _0012_FixedBytes16_offset0000_byte0010_1 = 12;
            public const int _0013_FixedBytes16_offset0000_byte0011_1 = 13;
            public const int _0014_FixedBytes16_offset0000_byte0012_1 = 14;
            public const int _0015_FixedBytes16_offset0000_byte0013_1 = 15;
            public const int _0016_FixedBytes16_offset0000_byte0014_1 = 16;
            public const int _0017_FixedBytes16_offset0000_byte0015_1 = 17;
            public const int _0018_FixedBytes16_offset0016_byte0000_1 = 18;
            public const int _0019_FixedBytes16_offset0016_byte0001_1 = 19;
            public const int _0020_FixedBytes16_offset0016_byte0002_1 = 20;
            public const int _0021_FixedBytes16_offset0016_byte0003_1 = 21;
            public const int _0022_FixedBytes16_offset0016_byte0004_1 = 22;
            public const int _0023_FixedBytes16_offset0016_byte0005_1 = 23;
            public const int _0024_FixedBytes16_offset0016_byte0006_1 = 24;
            public const int _0025_FixedBytes16_offset0016_byte0007_1 = 25;
            public const int _0026_FixedBytes16_offset0016_byte0008_1 = 26;
            public const int _0027_FixedBytes16_offset0016_byte0009_1 = 27;
            public const int _0028_FixedBytes16_offset0016_byte0010_1 = 28;
            public const int _0029_FixedBytes16_offset0016_byte0011_1 = 29;
            public const int _0030_FixedBytes16_offset0016_byte0012_1 = 30;
            public const int _0031_FixedBytes16_offset0016_byte0013_1 = 31;
            public const int _0032_FixedBytes16_offset0016_byte0014_1 = 32;
            public const int _0033_FixedBytes16_offset0016_byte0015_1 = 33;
            public const int _0034_FixedBytes16_offset0032_byte0000_1 = 34;
            public const int _0035_FixedBytes16_offset0032_byte0001_1 = 35;
            public const int _0036_FixedBytes16_offset0032_byte0002_1 = 36;
            public const int _0037_FixedBytes16_offset0032_byte0003_1 = 37;
            public const int _0038_FixedBytes16_offset0032_byte0004_1 = 38;
            public const int _0039_FixedBytes16_offset0032_byte0005_1 = 39;
            public const int _0040_FixedBytes16_offset0032_byte0006_1 = 40;
            public const int _0041_FixedBytes16_offset0032_byte0007_1 = 41;
            public const int _0042_FixedBytes16_offset0032_byte0008_1 = 42;
            public const int _0043_FixedBytes16_offset0032_byte0009_1 = 43;
            public const int _0044_FixedBytes16_offset0032_byte0010_1 = 44;
            public const int _0045_FixedBytes16_offset0032_byte0011_1 = 45;
            public const int _0046_FixedBytes16_offset0032_byte0012_1 = 46;
            public const int _0047_FixedBytes16_offset0032_byte0013_1 = 47;
            public const int _0048_FixedBytes16_offset0032_byte0014_1 = 48;
            public const int _0049_FixedBytes16_offset0032_byte0015_1 = 49;
            public const int _0050_FixedBytes62_buffer_byte0048_1 = 50;
            public const int _0051_FixedBytes62_buffer_byte0049_1 = 51;
            public const int _0052_FixedBytes62_buffer_byte0050_1 = 52;
            public const int _0053_FixedBytes62_buffer_byte0051_1 = 53;
            public const int _0054_FixedBytes62_buffer_byte0052_1 = 54;
            public const int _0055_FixedBytes62_buffer_byte0053_1 = 55;
            public const int _0056_FixedBytes62_buffer_byte0054_1 = 56;
            public const int _0057_FixedBytes62_buffer_byte0055_1 = 57;
            public const int _0058_FixedBytes62_buffer_byte0056_1 = 58;
            public const int _0059_FixedBytes62_buffer_byte0057_1 = 59;
            public const int _0060_FixedBytes62_buffer_byte0058_1 = 60;
            public const int _0061_FixedBytes62_buffer_byte0059_1 = 61;
            public const int _0062_FixedBytes62_buffer_byte0060_1 = 62;
            public const int _0063_FixedBytes62_buffer_byte0061_1 = 63;
            public const int _0064_Masks_m_masks_m_BufferMask_2 = 64;
            public const int _0066_Masks_m_masks_m_SystemStateComponentMask_2 = 66;
            public const int _0068_Masks_m_masks_m_SharedComponentMask_2 = 68;
            public const int _0070_Masks_m_masks_m_ZeroSizedMask_2 = 70;
        }

        public struct EntityCommandBufferOffsets
        {           
            public const int _0000_EntityCommandBufferDataPtr_m_Data_8 = 0;
            public const int _0008_AtomicSafetyHandle_m_Safety0_versionNode_8 = 8;
            public const int _0016_AtomicSafetyHandle_m_Safety0_version_4 = 16;
            public const int _0020_AtomicSafetyHandle_m_Safety0_staticSafetyId_4 = 20;
            public const int _0024_AtomicSafetyHandle_m_BufferSafety_versionNode_8 = 24;
            public const int _0032_AtomicSafetyHandle_m_BufferSafety_version_4 = 32;
            public const int _0036_AtomicSafetyHandle_m_BufferSafety_staticSafetyId_4 = 36;
            public const int _0040_AtomicSafetyHandle_m_ArrayInvalidationSafety_versionNode_8 = 40;
            public const int _0048_AtomicSafetyHandle_m_ArrayInvalidationSafety_version_4 = 48;
            public const int _0052_AtomicSafetyHandle_m_ArrayInvalidationSafety_staticSafetyId_4 = 52;
            public const int _0056_Int32_m_SafetyReadOnlyCount_4 = 56;
            public const int _0060_Int32_m_SafetyReadWriteCount_4 = 60;
            public const int _0064_DisposeSentinel_m_DisposeSentinel_8 = 64;
            public const int _0072_Int32_SystemID_4 = 72;
        }

        public struct EntityQueryDataOffsets
        {           
            public const int _0000_ComponentTypePtr_RequiredComponents_8 = 0;
            public const int _0008_Int32_RequiredComponentsCount_4 = 8;
            public const int _0016_Int32Ptr_ReaderTypes_8 = 16;
            public const int _0024_Int32_ReaderTypesCount_4 = 24;
            public const int _0032_Int32Ptr_WriterTypes_8 = 32;
            public const int _0040_Int32_WriterTypesCount_4 = 40;
            public const int _0048_ArchetypeQueryPtr_ArchetypeQuery_8 = 48;
            public const int _0056_Int32_ArchetypeQueryCount_4 = 56;
            public const int _0064_EntityQueryMask_EntityQueryMask_Index_1 = 64;
            public const int _0065_EntityQueryMask_EntityQueryMask_Mask_1 = 65;
            public const int _0072_EntityQueryMask_EntityQueryMask_EntityComponentStore_8 = 72;
            public const int _0080_UnsafeMatchingArchetypePtrList_MatchingArchetypes_ListData_8 = 80;
            public const int _0088_UnsafeMatchingArchetypePtrList_MatchingArchetypes_entityComponentStore_8 = 88;
        }

        public struct EntityQueryMaskOffsets
        {           
            public const int _0000_Byte_Index_1 = 0;
            public const int _0001_Byte_Mask_1 = 1;
            public const int _0008_EntityComponentStorePtr_EntityComponentStore_8 = 8;
        }

        public struct UnsafeMatchingArchetypePtrListOffsets
        {           
            public const int _0000_UnsafePtrListPtr_ListData_8 = 0;
            public const int _0008_EntityComponentStorePtr_entityComponentStore_8 = 8;
        }

        public struct EntityQueryFilterOffsets
        {           
            public const int _0000_UInt32_RequiredChangeVersion_4 = 0;
            public const int _0004_SharedComponentData_Shared_Count_4 = 4;
            public const int _0008_Int32_IndexInEntityQuery_m_value_4 = 8;
            public const int _0012_Int32_IndexInEntityQuery_m_value_4 = 12;
            public const int _0016_Int32_SharedComponentIndex_m_value_4 = 16;
            public const int _0020_Int32_SharedComponentIndex_m_value_4 = 20;
            public const int _0024_ChangedFilter_Changed_Count_4 = 24;
            public const int _0028_Int32_IndexInEntityQuery_m_value_4 = 28;
            public const int _0032_Int32_IndexInEntityQuery_m_value_4 = 32;
        }

    #endregion Auto-Generated Content

#if UNITY_EDITOR


    /// <summary>
    /// This file re-writes itself with the current offsets when the Editor menu option 'DOTS->Update Offsets' is used.
    /// Note: this file can be placed anywhere in your project, and be called any file name.
    /// </summary>
    public static class OffsetsGenerator
    {
        private static void Update(bool forceUpdate = false, [CallerFilePath] string executingFilePath = "")
        {
            var items = new Dictionary<Type, TypeLayoutUtility.TypeInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var versions = new List<string>();
            var hash = 0;

            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == "Unity.Entities")
                {
                    var id = assembly.ManifestModule.ModuleVersionId;
                    var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
                    versions.Add($"// Entities: {packageInfo.version}");

                    hash = packageInfo.version.GetHashCode();
                    if (!forceUpdate && EditorPrefs.GetInt(SettingsKey) == hash)
                        return;

                    GetOffsets(items, assembly.GetType("Unity.Entities.ArchetypeChunk"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.Chunk"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.EntityComponentStore"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.Archetype"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.Entity"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.ComponentType"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.ComponentTypes"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.EntityCommandBuffer"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.EntityQueryData"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.EntityQueryFilter"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.ExclusiveTransation"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.DynamicBuffer"));
                    GetOffsets(items, assembly.GetType("Unity.Entities.BufferFromEntity"));


                    break;
                }
            }

            GenerateFile(items.Values.ToList(), versions, hash, executingFilePath);
        }

        private static void GetOffsets(Dictionary<Type, TypeLayoutUtility.TypeInfo> items, Type type, bool expandPointers = true, bool expandLayouts = true)
        {
            if (type == null || items.ContainsKey(type))
                return;

            var typeInfo = TypeLayoutUtility.CreateTypeInfo(type);
            items.Add(type, typeInfo);

            if (expandLayouts)
            {
                foreach (var layout in typeInfo.Layouts)
                {
                    if (layout.Type != null && !items.ContainsKey(layout.Type))
                    {
                        items.Add(layout.Type, TypeLayoutUtility.CreateTypeInfo(layout.Type));
                    }
                }
            }
            if (expandPointers)
            {
                foreach (var pointerType in typeInfo.PointerTypes)
                {
                    if (pointerType != null && !items.ContainsKey(pointerType))
                    {
                        items.Add(pointerType, TypeLayoutUtility.CreateTypeInfo(pointerType));
                    }
                }
            }
        }

        private static void GenerateFile(List<TypeLayoutUtility.TypeInfo> typeInfos, List<string> versions, int hash, string outputPath)
        {
            var allFileLines = File.ReadAllLines(outputPath);

            List<(string Name, string ReplacementLines)> replacements = new List<(string Name, string replacementLines)>();

            for (var i = 0; i < typeInfos.Count; i++)
            {
                var typeInfo = typeInfos[i];
                var name = typeInfo.Type.Name + "Offsets";
                var membersBuilder = new StringBuilder();
                var isLastTypeInfo = i == typeInfos.Count - 1;

                for (var j = 0; j < typeInfo.Fields.Length; j++)
                {
                    var fieldData = typeInfo.Fields[j];
                    var str = ConstantTemplate.Replace(MemberTypeToken, "int");
                    str = str.Replace(MemberNameToken, fieldData.CreateMemberName());
                    str = str.Replace(MemberValueToken, fieldData.Offset.ToString());

                    var isLast = j == typeInfo.Fields.Length - 1;
                    if (!isLast) str += Environment.NewLine + Indent2;

                    membersBuilder.Append(str);
                }

                var newLines = StructTemplate.Replace(MembersToken, membersBuilder.ToString());
                newLines = newLines.Replace(StructNameToken, name);

                replacements.Add((name, newLines));
            }

            var output = ReplaceAutoGeneratedRegion(replacements, versions, allFileLines);
            if (output != null)
            {
                CreateScriptAssetWithContent(outputPath, output);
                EditorPrefs.SetInt(SettingsKey, hash);
                //AssetDatabase.Refresh();
            }
        }

        private const string MembersToken = "#Member";
        private const string MemberNameToken = "#MemberName";
        private const string MemberTypeToken = "#MemberType";
        private const string MemberValueToken = "#MemberValue";
        private const string StructNameToken = "#StructName";
        private const string SettingsKey = "AutoGeneratedInternalOffsetsKey";
        private const string StartRegionShortToken = "#region";
        private const string StartRegionToken = "#region Auto-Generated Content";
        private const string EndRegionShortToken = "#endregion";
        private const string EndRegionToken = "#endregion Auto-Generated Content";
        private const string Indent1 = "        ";
        private const string Indent2 = "            ";

        private static readonly string StructTemplate =
        $@"        public struct {StructNameToken}
        {{           
            {MembersToken}
        }}";

        private static readonly string ConstantTemplate =
        $@"public const {MemberTypeToken} {MemberNameToken} = {MemberValueToken};";


        private static string ReplaceAutoGeneratedRegion(List<(string Name, string ReplacementLines)> replacements, List<string> versions, string[] fileLines)
        {
            var result = new StringBuilder();
            int i = 0, j = 0;

            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];

                if (!line.TrimStart().StartsWith(StartRegionShortToken) || !line.Contains(StartRegionToken))
                {
                    result.AppendLine(line);
                    continue;
                }

                result.AppendLine(line);
                result.AppendLine();

                for (int k = 0; k < versions.Count; k++)
                {
                    result.AppendLine(Indent1 + versions[k]);
                }

                for (j = i; j < fileLines.Length; j++)
                {
                    line = fileLines[j];

                    if (line.TrimStart().StartsWith(EndRegionShortToken) && line.Contains(EndRegionToken))
                    {
                        break;
                    }
                }

                var endNotFound = j == fileLines.Length;
                if (endNotFound)
                {
                    Debug.LogError("The required #endregion tag was not found");
                    return default;
                }

                foreach (var replacement in replacements)
                {
                    result.AppendLine();
                    result.Append(replacement.ReplacementLines);
                    result.AppendLine();
                }
                break;
            }

            result.AppendLine();
            return result + string.Join(Environment.NewLine, fileLines.Slice(j, fileLines.Length));
        }

        /// <summary>
        /// Create a new script asset.
        /// UnityEditor.ProjectWindowUtil.CreateScriptAssetWithContent (2019.1)
        /// </summary>
        /// <param name="pathName">the path to where the new file should be created</param>
        /// <param name="templateContent">the text to put inside</param>
        /// <returns></returns>
        private static void CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            templateContent = SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
            string fullPath = Path.GetFullPath(pathName);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding(true);
            File.WriteAllText(fullPath, templateContent, encoding);

            // From 2020.1 ImportAsset requires a relative path.
            var dataUri = new Uri(Application.dataPath);
            var fullUri = new Uri(pathName);
            var relativePath = dataUri.MakeRelativeUri(fullUri).ToString();
            AssetDatabase.ImportAsset(relativePath);
            AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object)); 
        }

        public static T[] Slice<T>(this T[] arr, int indexFrom, int indexTo)
        {
            uint length = (uint)(indexTo - indexFrom);
            T[] result = new T[length];
            Array.Copy(arr, indexFrom, result, 0, length);
            return result;
        }

        /// <summary>
        /// Ensure correct OS specific line endings for saving file content.
        /// UnityEditor.ProjectWindowUtil.SetLineEndings (2019.1)
        /// </summary>
        /// <param name="content">a string to have line endings checked</param>
        /// <param name="lineEndingsMode">the type of line endings to use</param>
        /// <returns>a cleaned string</returns>
        private static string SetLineEndings(string content, LineEndingsMode lineEndingsMode)
        {
            string replacement;
            switch (lineEndingsMode)
            {
                case LineEndingsMode.OSNative:
                    replacement = Application.platform == RuntimePlatform.WindowsEditor ? "\r\n" : "\n";
                    break;
                case LineEndingsMode.Unix:
                    replacement = "\n";
                    break;
                case LineEndingsMode.Windows:
                    replacement = "\r\n";
                    break;
                default:
                    replacement = "\n";
                    break;
            }
            content = System.Text.RegularExpressions.Regex.Replace(content, "\\r\\n?|\\n", replacement);
            return content;
        }


        ///// <summary>
        ///// Hook that runs the enum generator whenever assets are saved.
        ///// </summary>
        //private class UpdateOnAssetModification : UnityEditor.AssetModificationProcessor
        //{
        //    public static string[] OnWillSaveAssets(string[] paths)
        //    {
        //        Update();
        //        return paths;
        //    }
        //}

        ///// <summary>
        ///// Hook that runs the enum generator whenever scripts are compiled.
        ///// </summary>
        //[UnityEditor.Callbacks.DidReloadScripts]
        //private static void UpdateOnScriptCompile()
        //{
        //    Update();
        //}

        /// <summary>
        /// Enables manually running the enum generator from the menus.
        /// </summary>
        [MenuItem("DOTS/Update Offsets File")]
        private static void UpdateOnMenuCommand()
        {
            Update(true);
        }

    }

    public static class TypeLayoutUtility
    {
        public static TypeInfo CreateTypeInfo<T>() where T : struct
        {
            return CreateTypeInfoBlittable(typeof(T));
        }

        public static TypeInfo CreateTypeInfo(Type type)
        {
            return CreateTypeInfoBlittable(type);
        }

        private static TypeInfo CreateTypeInfoBlittable(Type type)
        {
            var begin = 0;
            var end = 0;
            var hash = 0;

            var layouts = new List<Layout>();
            var fields = new List<FieldData>();
            var pointerTypes = new List<Type>();

            CreateLayoutRecurse(type, null, 0, layouts, fields, pointerTypes, ref begin, ref end, ref hash);

            if (begin != end)
            {
                layouts.Add(new Layout
                {
                    Offset = begin,
                    Count = end - begin,
                    Size = end - begin,
                    Aligned4 = false
                });
            }

            var layoutsArray = layouts.ToArray();

            for (var i = 0; i != layoutsArray.Length; i++)
            {
                if (layoutsArray[i].Count % 4 == 0 && layoutsArray[i].Offset % 4 == 0)
                {
                    layoutsArray[i].Count /= 4;
                    layoutsArray[i].Aligned4 = true;
                }
            }

            return new TypeInfo
            {
                Type = type,
                Hash = hash,
                Layouts = layoutsArray,
                Fields = fields.ToArray(),
                PointerTypes = pointerTypes.ToArray(),
            };
        }

        public struct Layout
        {
            public int Offset;
            public int Count;
            public bool Aligned4;
            public int Size;
            public FieldData[] Fields;
            public Type Type;

            public override string ToString()
            {
                return $"{Offset}: {Type.Name} Size: {Size}, Fields={Fields.Length}";
            }
        }

        public struct TypeInfo
        {
            public int Hash;
            public Layout[] Layouts;
            public FieldData[] Fields;
            public Type[] PointerTypes;
            public Type Type;

            public static TypeInfo Null => new TypeInfo();

        }

        public unsafe struct PointerSize
        {
            private void* pter;
        }

        public struct FieldData
        {
            public int Offset;
            public int Size;
            public FieldInfo Field;
            public Type ParentType;
            public FieldInfo ParentField;

            public string CreateTypeString() => ParentField != null ? GetCleanTypeName(ParentType) : GetCleanTypeName(Field.FieldType);

            public string CreateNameString() => ParentField != null ? $"{ParentField.Name}_{Field.Name}" : Field.Name;

            public string CreateMemberName() => $"_{Offset:D4}_{CreateTypeString().Replace("*", "Ptr")}_{CreateNameString()}_{Size:X}";

            public static string GetCleanTypeName(Type type)
            {
                if (type.IsGenericType)
                {
                    var simpleName = type.Name.Substring(0, type.Name.IndexOf('`'));
                    string genericTypeParams = string.Empty;
                    var args = !type.IsGenericTypeDefinition
                        ? type.GetGenericArguments()
                        : Type.EmptyTypes;

                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i > 0) genericTypeParams += ",";
                        genericTypeParams += GetCleanTypeName(args[i]);
                    }
                    return string.Format("{0}_t{1}_", simpleName, genericTypeParams);
                }
                return type.Name;
            }

            public override string ToString() => CreateMemberName();
        }

        public static FixedBufferAttribute GetFixedBufferAttribute(FieldInfo field)
        {
            foreach (var attribute in field.GetCustomAttributes(typeof(FixedBufferAttribute)))
            {
                return (FixedBufferAttribute)attribute;
            }

            return null;
        }

        public static void CombineHash(ref int hash, params int[] values)
        {
            foreach (var value in values)
            {
                hash *= FNV_32_PRIME;
                hash ^= value;
            }
        }

        private static void CreateLayoutRecurse(Type type, FieldInfo parentField, int baseOffset, List<Layout> layouts, List<FieldData> fieldsOutput, List<Type> pointerTypes, ref int begin, ref int end, ref int typeHash)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var fieldsWithOffset = new FieldData[fields.Length];
            for (int i = 0; i != fields.Length; i++)
            {
                fieldsWithOffset[i].Offset = UnsafeUtility.GetFieldOffset(fields[i]);
                fieldsWithOffset[i].Field = fields[i];
            }

            Array.Sort(fieldsWithOffset, (a, b) => a.Offset - b.Offset);

            for (var i = 0; i < fieldsWithOffset.Length; i++)
            {
                var sizeOf = 0;
                var fieldWithOffset = fieldsWithOffset[i];
                var field = fieldWithOffset.Field;
                var fixedBuffer = GetFixedBufferAttribute(field);

                if (fixedBuffer != null)
                {
                    var stride = UnsafeUtility.SizeOf(fixedBuffer.ElementType);
                    sizeOf = stride * fixedBuffer.Length;
                }
                else if (field.FieldType.IsPrimitive || field.FieldType.IsPointer || field.FieldType.IsClass || field.FieldType.IsEnum)
                {
                    if (field.FieldType.IsPointer)
                    {
                        sizeOf = UnsafeUtility.SizeOf<PointerSize>();

                        // Record discovered pointers so they can be expanded separately.
                        pointerTypes.Add(field.ReflectedType);
                    }
                    else if (field.FieldType.IsClass)
                    {
                        sizeOf = UnsafeUtility.SizeOf<PointerSize>();
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        // Workaround IL2CPP bug
                        sizeOf = UnsafeUtility.SizeOf(field.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0].FieldType);
                    }
                    else
                    {
                        sizeOf = UnsafeUtility.SizeOf(field.FieldType);
                    }
                }
                else
                {
                    sizeOf = UnsafeUtility.SizeOf(field.FieldType);
                }
                fieldsWithOffset[i].Size = sizeOf;
            }

            for (var i = 0; i < fieldsWithOffset.Length; i++)
            {
                var fieldWithOffset = fieldsWithOffset[i];
                var field = fieldWithOffset.Field;
                var fixedBuffer = GetFixedBufferAttribute(field);
                var offset = baseOffset + fieldWithOffset.Offset;
                var size = fieldsWithOffset[i].Size;

                if (fixedBuffer != null)
                {
                    var stride = UnsafeUtility.SizeOf(fixedBuffer.ElementType);
                    for (int j = 0; j < fixedBuffer.Length; ++j)
                    {
                        CreateLayoutRecurse(fixedBuffer.ElementType, field, offset + j * stride, layouts, fieldsOutput, pointerTypes, ref begin, ref end, ref typeHash);
                    }
                }
                else if (field.FieldType.IsPrimitive || field.FieldType.IsPointer || field.FieldType.IsClass || field.FieldType.IsEnum)
                {
                    CombineHash(ref typeHash, offset, (int)Type.GetTypeCode(field.FieldType));

                    if (end != offset)
                    {
                        layouts.Add(new Layout
                        {
                            Type = type,
                            Offset = begin,
                            Count = end - begin,
                            Size = end - begin,
                            Aligned4 = false,
                            Fields = fieldsWithOffset
                        });
                        begin = offset;
                        end = offset + size;
                    }
                    else
                    {
                        end += size;
                    }

                    // A copy with absolute offset;
                    fieldWithOffset.Offset = offset;
                    fieldWithOffset.ParentType = type;
                    fieldWithOffset.ParentField = parentField;
                    fieldsOutput.Add(fieldWithOffset);
                }
                else
                {
                    CreateLayoutRecurse(field.FieldType, field, offset, layouts, fieldsOutput, pointerTypes, ref begin, ref end, ref typeHash);
                }
            }
        }

        public const int FNV_32_PRIME = 0x01000193;
    }

#endif



}