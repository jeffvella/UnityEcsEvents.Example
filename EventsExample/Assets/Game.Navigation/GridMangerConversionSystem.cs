using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Providers.Grid;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Vella.Common.Game.Navigation;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using Debug = UnityEngine.Debug;
using Math = Unity.Physics.Math;
using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;
using SphereCollider = Unity.Physics.SphereCollider;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Compilation;

#endif

namespace Vella.Common.Game.Navigation
{
    public struct BlobArray_
    {
        internal int Length;
        internal int Offset;
        // number of T, not number of bytes

        // Generic accessor
        public unsafe struct Accessor<T> where T : struct
        {
            public readonly int* m_OffsetPtr;

            public Accessor(ref BlobArray_ blobArray)
            {
                fixed (BlobArray_* ptr = &blobArray)
                {
                    m_OffsetPtr = &ptr->Offset;
                    Length = ptr->Length;
                }
            }

            public int Length { get; }

            public ref T this[int index]
            {
                get
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if ((uint)index >= (uint)Length)
                        throw new IndexOutOfRangeException(string.Format("Index {0} is out of range Length {1}", index, Length));
#endif
                    return ref UnsafeUtilityEx.ArrayElementAsRef<T>((byte*)m_OffsetPtr + *m_OffsetPtr, index);
                }
            }
        }
    }

    public struct BlobGrid3D
    {
        public UntypedBlobArray Flags;
        public int3 MaxIndices;
        public UntypedBlobArray Nodes;
        public int4 Size; // x,y,z + yz

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3x2 GetIndices(Aabb colliderWorldBounds, float4x4 toLocalMatrix, int3 maxIndices, float halfBoxSize, float boxSizeQuotient)
        {
            return new int3x2
            {
                c0 = math.clamp((int3)math.round((math.transform(toLocalMatrix, colliderWorldBounds.Min) - halfBoxSize) * boxSizeQuotient), 0, maxIndices),
                c1 = math.clamp((int3)math.round((math.transform(toLocalMatrix, colliderWorldBounds.Max) - halfBoxSize) * boxSizeQuotient), 0, maxIndices)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3x2 GetIndicesUnclamped(Aabb wouldBounds, float4x4 toLocalMatrix, float halfBoxSize, float boxSizeQuotient)
        {
            return new int3x2
            {
                c0 = (int3)math.round((math.transform(toLocalMatrix, wouldBounds.Min) - halfBoxSize) * boxSizeQuotient),
                c1 = (int3)math.round((math.transform(toLocalMatrix, wouldBounds.Max) - halfBoxSize) * boxSizeQuotient)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y, int z) => x * Size.w + y * Size.z + z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetNode<T>(int i) where T : struct => ref new UntypedBlobArray.Accessor<T>(ref Nodes)[i];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetNode<T>(int x, int y, int z) where T : struct => ref new UntypedBlobArray.Accessor<T>(ref Nodes)[GetIndex(x, y, z)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* GetPtr<T>(int x, int y, int z) where T : unmanaged => (T*)UnsafeUtility.AddressOf(ref Nodes) + sizeof(T) * GetIndex(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(int x, int y, int z) => x >= 0 && y >= 0 && z >= 0 && x < Size.x && y < Size.y && z < Size.z;
    }

    public struct BlobGrid3D<T, TFlags> where T : struct where TFlags : struct, Enum
    {
        public BlobArray<TFlags> Flags;
        public int3 MaxIndices;
        public BlobArray<T> Nodes;
        public int4 Size; // x,y,z + yz
        private const int vectorEndPadding = 4;
        public ref T this[int i] => ref Nodes[i];
        public ref T this[int x, int y, int z] => ref Nodes[GetIndex(x, y, z)];

        public static BlobAssetReference<BlobGrid3D<T, TFlags>> Build(int x, int y, int z, Allocator allocator)
        {
            int length = x * y * z;
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobGrid3D<T, TFlags>>();
            root.Size = new int4(x, y, z, y * z);
            root.MaxIndices = root.Size.xyz - 1;
            builder.Allocate(ref root.Nodes, length + vectorEndPadding);
            builder.Allocate(ref root.Flags, length + vectorEndPadding);
            var result = builder.CreateBlobAssetReference<BlobGrid3D<T, TFlags>>(allocator);
            builder.Dispose();
            return result;
        }

        public static unsafe BlobAssetReference<BlobGrid3D<T, TFlags>> Clone(BlobAssetReference<BlobGrid3D<T, TFlags>> source, Allocator allocator)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var dst = ref builder.ConstructRoot<BlobGrid3D<T, TFlags>>();
            ref var src = ref source.Value;

            void* srcNodesPtr = src.Nodes.GetUnsafePtr();
            void* srcFlagsPtr = src.Flags.GetUnsafePtr();

            dst.Size = src.Size;
            dst.MaxIndices = src.MaxIndices;

            int nodeSize = UnsafeUtility.SizeOf<T>();
            int flagSize = UnsafeUtility.SizeOf<TFlags>();

            void* dstNodesPtr = builder.Allocate(ref dst.Nodes, src.Nodes.Length + vectorEndPadding).GetUnsafePtr();
            void* dstFlagsPtr = builder.Allocate(ref dst.Flags, src.Flags.Length + vectorEndPadding).GetUnsafePtr();

            UnsafeUtility.MemCpy(dstNodesPtr, srcNodesPtr, (src.Nodes.Length + vectorEndPadding) * nodeSize);
            UnsafeUtility.MemCpy(dstFlagsPtr, srcFlagsPtr, (src.Flags.Length + vectorEndPadding) * flagSize);

            var result = builder.CreateBlobAssetReference<BlobGrid3D<T, TFlags>>(allocator);
            builder.Dispose();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y, int z) => x * Size.w + y * Size.z + z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int3 GetIndexes(int idx)
        {
            int x = idx / Size.w;
            idx -= x * Size.w;
            return new int3(x, idx / Size.z, idx % Size.z);
        }

        public bool IsValid(int x, int y, int z) => x >= 0 && y >= 0 && z >= 0 && x < Size.x && y < Size.y && z < Size.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetFlags(NodeFlags flags)
        {
            var flag4 = (int4)(int)flags;
            var nodeFlags = (int4*)Flags.GetUnsafePtr();
            int length = Nodes.Length / 4;
            SetFlags(nodeFlags, ref flag4, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetFlags(int4* flags, ref int4 flagToSet, int length)
        {
            int i = 0;
            do
            {
                flags[i++] |= flagToSet;
            } while (i != length);
        }
    }

    public struct Collider_
    {
        public ConvexHull_ ConvexHull;
        public ConvexColliderHeader m_Header;
        private unsafe fixed byte m_FaceLinks[4 * 24];
        private unsafe fixed byte m_FacePlanes[sizeof(float) * 4 * 6];

        // Plane[6]
        private unsafe fixed byte m_Faces[4 * 6];

        // ConvexHull.Face[6]
        private unsafe fixed byte m_FaceVertexIndices[sizeof(byte) * 24];

        // byte[24]
        private unsafe fixed byte m_VertexEdges[4 * 8];

        private unsafe fixed byte m_Vertices[sizeof(float) * 3 * 8]; // float3[8]
                                                                     // ConvexHull.Edge[8]
                                                                     // ConvexHull.Edge[24]

        public CollisionType CollisionType => m_Header.CollisionType;
        public ColliderType Type => m_Header.Type;
    }

    public struct ConvexHull_
    {
        // A distance by which to inflate the surface of the hull for collision detection.
        // This helps to keep the actual hulls from overlapping during simulation, which avoids more costly algorithms.
        // For spheres and capsules, this is the radius of the primitive.
        // For other convex hulls, this is typically a small value.
        // For polygons in a static mesh, this is typically zero.
        public float ConvexRadius;

        internal BlobArray_ FaceLinksBlob;

        internal BlobArray_ FacePlanesBlob;

        internal BlobArray_ FacesBlob;

        internal BlobArray_ FaceVertexIndicesBlob;

        internal BlobArray_ VertexEdgesBlob;

        // Relative arrays of convex hull data
        internal BlobArray_ VerticesBlob;

        public BlobArray_.Accessor<Edge_> FaceLinks => new BlobArray_.Accessor<Edge_>(ref FaceLinksBlob);
        public BlobArray_.Accessor<Face_> Faces => new BlobArray_.Accessor<Face_>(ref FacesBlob);
        public BlobArray_.Accessor<byte> FaceVertexIndices => new BlobArray_.Accessor<byte>(ref FaceVertexIndicesBlob);
        public unsafe byte* FaceVertexIndicesPtr => (byte*)UnsafeUtility.AddressOf(ref FaceVertexIndicesBlob.Offset) + FaceVertexIndicesBlob.Offset;
        public int NumFaces => FacesBlob.Length;
        public int NumVertices => VerticesBlob.Length;
        public BlobArray_.Accessor<Plane_> Planes => new BlobArray_.Accessor<Plane_>(ref FacePlanesBlob);

        public BlobArray_.Accessor<Edge_> VertexEdges => new BlobArray_.Accessor<Edge_>(ref VertexEdgesBlob);

        // Indexers for the data
        public BlobArray_.Accessor<float3> Vertices => new BlobArray_.Accessor<float3>(ref VerticesBlob);

        public unsafe float3* VerticesPtr => (float3*)((byte*)UnsafeUtility.AddressOf(ref VerticesBlob.Offset) + VerticesBlob.Offset);

        // Returns the index of the face with maximum normal dot direction
        public int GetSupportingFace(float3 direction)
        {
            int bestIndex = 0;
            float bestDot = math.dot(direction, Planes[0].Normal);
            for (int i = 1; i < NumFaces; i++)
            {
                float dot = math.dot(direction, Planes[i].Normal);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        // Returns the index of the best supporting face that contains supportingVertex
        public int GetSupportingFace(float3 direction, int supportingVertexIndex)
        {
            // Special case for for polygons or colliders without connectivity.
            // Polygons don't need to search edges because both faces contain all vertices.
            if (Faces.Length == 2 || VertexEdges.Length == 0 || FaceLinks.Length == 0)
                return GetSupportingFace(direction);

            // Search the edges that contain supportingVertexIndex for the one that is most perpendicular to direction
            int bestEdgeIndex = -1;
            {
                float bestEdgeDot = float.MaxValue;
                var supportingVertex = Vertices[supportingVertexIndex];
                var edge = VertexEdges[supportingVertexIndex];
                int firstFaceIndex = edge.FaceIndex;
                var face = Faces[firstFaceIndex];
                while (true)
                {
                    // Get the linked edge and test it against the support direction
                    int linkedEdgeIndex = face.FirstIndex + edge.EdgeIndex;
                    edge = FaceLinks[linkedEdgeIndex];
                    face = Faces[edge.FaceIndex];
                    var linkedVertex = Vertices[FaceVertexIndices[face.FirstIndex + edge.EdgeIndex]];
                    var edgeDirection = linkedVertex - supportingVertex;
                    float dot = math.abs(math.dot(direction, edgeDirection)) * math.rsqrt(math.lengthsq(edgeDirection));
                    bestEdgeIndex = math.select(bestEdgeIndex, linkedEdgeIndex, dot < bestEdgeDot);
                    bestEdgeDot = math.min(bestEdgeDot, dot);

                    // Quit after looping back to the first face
                    if (edge.FaceIndex == firstFaceIndex)
                        break;

                    // Get the next edge
                    edge.EdgeIndex = (byte)((edge.EdgeIndex + 1) % face.NumVertices);
                }

                // If no suitable face is found then return the first face containing the supportingVertex
                if (-1 == bestEdgeIndex)
                    return firstFaceIndex;
            }

            // Choose the face containing the best edge that is most parallel to the support direction
            var bestEdge = FaceLinks[bestEdgeIndex];
            int faceIndex0 = bestEdge.FaceIndex;
            int faceIndex1 = FaceLinks[Faces[faceIndex0].FirstIndex + bestEdge.EdgeIndex].FaceIndex;
            var normal0 = Planes[faceIndex0].Normal;
            var normal1 = Planes[faceIndex1].Normal;
            return math.select(faceIndex0, faceIndex1, math.dot(direction, normal1) > math.dot(direction, normal0));
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)] // extra 1 byte padding to match Havok
    public struct Edge_ : IEquatable<Edge_>
    {
        public short FaceIndex; // index into Faces array
        public byte EdgeIndex; // edge index within the face

        public bool Equals(Edge_ other) => FaceIndex.Equals(other.FaceIndex) && EdgeIndex.Equals(other.EdgeIndex);

        public override int GetHashCode() => unchecked((ushort)FaceIndex | (EdgeIndex << 16));
    }

    public struct Face_ : IEquatable<Face_>
    {
        public short FirstIndex; // index into FaceVertexIndices array
        public byte MinHalfAngleCompressed;
        public byte NumVertices; // number of vertex indices in the FaceVertexIndices array
                                 // 0-255 = 0-90 degrees

        private const float k_CompressionFactor = 255.0f / (math.PI * 0.5f);

        public float MinHalfAngle
        {
            set => MinHalfAngleCompressed = (byte)math.min(value * k_CompressionFactor, 255);
        }

        public bool Equals(Face_ other) => FirstIndex.Equals(other.FirstIndex) && NumVertices.Equals(other.NumVertices) && MinHalfAngleCompressed.Equals(other.MinHalfAngleCompressed);
    }

    public struct GridVolumeModifierComponent : IComponentData
    {
        public BlobAssetReference<Collider> Collider;
        public Color Color;
        public VolumeModificationData ModificationData;
    }

    // A plane described by a normal and a distance from the origin
    [DebuggerDisplay("{Normal}, {Distance}")]
    public struct Plane_
    {
        public float4 m_NormalAndDistance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane_(float3 normal, float distance) => m_NormalAndDistance = new float4(normal, distance);

        public float Distance
        {
            get => m_NormalAndDistance.w;
            set => m_NormalAndDistance.w = value;
        }

        public Plane_ Flipped => new Plane_ { m_NormalAndDistance = -m_NormalAndDistance };

        public float3 Normal
        {
            get => m_NormalAndDistance.xyz;
            set => m_NormalAndDistance.xyz = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float4(Plane_ plane) => plane.m_NormalAndDistance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane_ PlaneFromDirection(float3 origin, float3 direction)
        {
            var normal = math.normalize(direction);
            return new Plane_(normal, -math.dot(normal, origin));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane_ PlaneFromTwoEdges(float3 origin, float3 edgeA, float3 edgeB) => PlaneFromDirection(origin, math.cross(edgeA, edgeB));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane_ TransformPlane(RigidTransform transform, Plane_ plane)
        {
            var normal = math.rotate(transform.rot, plane.Normal);
            return new Plane_(normal, plane.Distance - math.dot(normal, transform.pos));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane_ TransformPlane(Math.MTransform transform, Plane_ plane)
        {
            var normal = math.mul(transform.Rotation, plane.Normal);
            return new Plane_(normal, plane.Distance - math.dot(normal, transform.Translation));
        }

        // Returns the closest point on the plane to the input point.
        public float3 Projection(float3 point) => point - Normal * SignedDistanceToPoint(point);

        // Returns the distance from the point to the plane, positive if the point is on the side of
        // the plane on which the plane normal points, zero if the point is on the plane, negative otherwise.
        public float SignedDistanceToPoint(float3 point) => Math.Dotxyz1(m_NormalAndDistance, point);
    }

    public unsafe struct UntypedBlobArray
    {
        public int Length;
        public int Offset;

        public unsafe struct Accessor<T> where T : struct
        {
            private readonly int* m_OffsetPtr;

            public Accessor(ref UntypedBlobArray blobArray)
            {
                fixed (UntypedBlobArray* ptr = &blobArray)
                {
                    m_OffsetPtr = &ptr->Offset;
                    Length = ptr->Length;
                }
            }

            public int Length { get; private set; }

            public ref T this[int index]
            {
                get
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if ((uint)index >= (uint)Length)
                        throw new IndexOutOfRangeException(string.Format("Index {0} is out of range Length {1}", index, Length));
#endif
                    return ref UnsafeUtilityEx.ArrayElementAsRef<T>((byte*)m_OffsetPtr + *m_OffsetPtr, index);
                }
            }

            public Enumerator GetEnumerator() => new Enumerator(m_OffsetPtr, Length);

            public struct Enumerator
            {
                private readonly int m_Length;
                private readonly int* m_OffsetPtr;
                private int m_Index;

                public Enumerator(int* offsetPtr, int length)
                {
                    m_OffsetPtr = offsetPtr;
                    m_Length = length;
                    m_Index = -1;
                }

                public T Current => UnsafeUtilityEx.ArrayElementAsRef<T>((byte*)m_OffsetPtr + *m_OffsetPtr, m_Index);

                public bool MoveNext() => ++m_Index < m_Length;
            }
        }
    }

    public class GridMangerConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GridManager gridManager) =>
            {
                var entity = GetPrimaryEntity(gridManager);
                if (!gridManager.IsValid)
                    return;

                var transform = new HeavyTransform(gridManager.transform);
                var blobAsset = gridManager.Grid.InnerGrid.Data;
                var clone = BlobGrid3D<GridNode, NodeFlags>.Clone(blobAsset, Allocator.Persistent);

                var gridComponent = new GridComponent
                {
                    GridData = clone,
                    Transform = transform,
                    Size = new GridSize
                    {
                        BoxSize = gridManager.Grid.BoxSize,
                        HalfBoxSize = gridManager.Grid.BoxSize / 2,
                        BoxSizeQuotient = 1 / gridManager.Grid.BoxSize
                    },
                    //WorldBounds = default, // todo
                };

                DstEntityManager.AddComponentData(entity, gridComponent);

                DstEntityManager.AddComponentData(entity, new GridWorldBounds { Value = new Aabb { Min = transform.GetWorldPoint(0), Max = transform.GetWorldPoint(blobAsset.Value.Size.xyz) } });
            });
        }
    }

    [UpdateAfter(typeof(PhysicsShapeConversionSystem))]
    public class GridVolumeModifierConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            //BlobAssetReference<Collider> collider = default;

            Entities.ForEach((GridVolumeModifier gridVolumeModifier) =>
            {
                var entity = GetPrimaryEntity(gridVolumeModifier);

                var shapeAuthoring = gridVolumeModifier.GetComponent<PhysicsShapeAuthoring>();
                if (shapeAuthoring == null)
                    throw new InvalidOperationException();

                var colliderComponent = DstEntityManager.GetComponentData<PhysicsCollider>(entity);

                var componentData = new GridVolumeModifierComponent
                {
                    ModificationData = gridVolumeModifier.ModificationData,
                    //Bounds = colliderComponent.Value.Value.CalculateAabb(),
                    Collider = colliderComponent.Value
                    //Transform = new HeavyTransform(gridVolumeModifier.transform)
                };

                // if (!Application.isPlaying)
                // {
                //     GridDebugDrawer.DrawCollider(componentData.Collider, gridVolumeModifier.DebugColor);
                //     //GridDebugDrawer.DrawNodesInCollider(ref grid, volumeModifier.Collider);
                // }

                DstEntityManager.AddComponentData(entity, componentData);
            });

            /*Entities.ForEach((ref GridComponent gridComponent) =>
            {
                ref var blobValue = ref gridComponent.GridData.Value;
                for (int i = 0; i < blobValue.Nodes.Length; i++)
                {
                    var node = blobValue.Nodes[i];
                    GridDebugDrawer.DrawNode(node, ref gridComponent.Transform, gridComponent.BoxSize);
                }

            }).WithoutBurst().Run();

            var grid = GetSingleton<GridComponent>();

            Entities.ForEach((ref GridVolumeModifierComponent volumeModifier) =>
            {
                GridDebugDrawer.DrawCollider(volumeModifier.Collider);

                GridDebugDrawer.DrawNodesInCollider(ref grid, volumeModifier.Collider);

            }).WithoutBurst().Run();

            Entities.ForEach((ref GridWorldBounds bounds) =>
            {
                GridDebugDrawer.DrawBounds(bounds.Value);

            }).WithoutBurst().Run();     */
        }
    }
}

public struct GridComponent : IComponentData
{
    public BlobAssetReference<BlobGrid3D<GridNode, NodeFlags>> GridData;
    public GridSize Size;
    public HeavyTransform Transform;
    //public Aabb WorldBounds;

    //    public int ConvertToGridDistance(float value)
    //=> (int)math.round((value - HalfBoxSize) * BoxSizeQuotient);

    public int ConvertToGridDistance(float value) => (int)math.round((value - Size.HalfBoxSize) * Size.BoxSizeQuotient);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int3 GetGridIndex(float3 localPosition) => math.clamp(GetGridIndexUnclamped(localPosition), 0, GridData.Value.MaxIndices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int3 GetGridIndex(float3 localPosition, int3 maxIndices, float halfBoxSize, float boxSizeQuotient) => math.clamp(GetGridIndexUnclamped(localPosition, halfBoxSize, boxSizeQuotient), 0, maxIndices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int3 GetGridIndexUnclamped(float3 localPosition) => (int3)math.round((localPosition - Size.HalfBoxSize) * Size.BoxSizeQuotient);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int3 GetGridIndexUnclamped(float3 localPosition, float halfBoxSize, float boxSizeQuotient) => (int3)math.round((localPosition - Size.HalfBoxSize) * Size.BoxSizeQuotient);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int3x2 GetIndices(Aabb localBounds) => new int3x2 { c0 = GetGridIndex(localBounds.Min), c1 = GetGridIndex(localBounds.Max) };

    public void GetIndices(Aabb colliderWorldBounds, out int3 min, out int3 max)
    {
        var localBounds = GetLocalBounds(colliderWorldBounds);
        min = GetGridIndex(localBounds.Min);
        max = GetGridIndex(localBounds.Max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Aabb GetLocalBounds() => new Aabb { Max = GridData.Value.MaxIndices };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Aabb GetLocalBounds(Aabb worldBounds) => new Aabb { Min = math.transform(Transform.ToLocalMatrix, worldBounds.Min), Max = math.transform(Transform.ToLocalMatrix, worldBounds.Max) };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Aabb GetLocalBounds(Aabb worldBounds, float4x4 toLocalMatrix) => new Aabb { Min = math.transform(toLocalMatrix, worldBounds.Min), Max = math.transform(toLocalMatrix, worldBounds.Max) };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Aabb GetWorldBounds() => new Aabb { Min = Transform.GetWorldPoint(0), Max = Transform.GetWorldPoint(GridData.Value.MaxIndices) };

    /*public unsafe void CopyNodes(ref UnsafeList<GridNode> destination, BlobAssetReference<Collider> collider, int maxDistance = 100)
    {
        var colliderPtr = (Collider*)collider.GetUnsafePtr();
        var colliderWorldBounds = colliderPtr->CalculateAabb();
        var blobGrid = (BlobGrid3D*)GridData.GetUnsafePtr();
        var nodes = (GridNode*)GridData.Value.Nodes.GetUnsafePtr();

        var distanceInput = new PointDistanceInput
        {
            MaxDistance = math.cmax(colliderWorldBounds.Extents)
        };

        //var results = new UnsafeCollector<GridNodeResult>();

        // Restrict to processing only the nodes inside collider AABB
        GetIndices(colliderWorldBounds, out var min, out var max);
        for (var x = min.x; x <= max.x; x++)
        for (var y = min.y; y <= max.y; y++)
        for (var z = min.z; z <= max.z; z++)
        {
            var node = nodes[blobGrid->GetIndex(x, y, z)];
            var nodeWorldPosition = Transform.GetWorldPoint(node.NavigableCenter);

            //var collector = new AnyHitCollector<DistanceHit>(distanceInput.MaxDistance);
            //var hits = colliderPtr->CalculateDistance(distanceInput, ref collector);

            var result = PhysicsQueries.Distance.FastPointCollider(nodeWorldPosition, colliderPtr);
            if (result.Distance > 0)
            {
            }

        }
    }*/
}

public readonly unsafe struct GridData
{
    [NativeDisableUnsafePtrRestriction]
    public readonly BlobGrid3D* BlobGrid;

    [NativeDisableUnsafePtrRestriction]
    public readonly NodeFlags* Flags;

    public readonly GridSize GridSize;

    [NativeDisableUnsafePtrRestriction]
    public readonly GridNode* Nodes;

    public readonly float4x4 ToLocalMatrix;

    public readonly float4x4 ToWorldMatrix;

    public GridData(ref GridComponent gridComponent)
    {
        BlobGrid = (BlobGrid3D*)gridComponent.GridData.GetUnsafePtr();
        Nodes = (GridNode*)gridComponent.GridData.Value.Nodes.GetUnsafePtr();
        Flags = (NodeFlags*)gridComponent.GridData.Value.Flags.GetUnsafePtr();
        GridSize = gridComponent.Size;
        ToLocalMatrix = gridComponent.Transform.ToLocalMatrix;
        ToWorldMatrix = gridComponent.Transform.ToLocalMatrix;
        //Bounds = gridComponent.WorldBounds;
    }

    public GridData(GridManager gridManager)
    {
        BlobGrid = (BlobGrid3D*)gridManager.Grid.InnerGrid.Data.GetUnsafePtr();
        Nodes = (GridNode*)gridManager.Grid.InnerGrid.Data.Value.Nodes.GetUnsafePtr();
        Flags = (NodeFlags*)gridManager.Grid.InnerGrid.Data.Value.Flags.GetUnsafePtr();
        ToLocalMatrix = gridManager.transform.worldToLocalMatrix;
        ToWorldMatrix = gridManager.transform.localToWorldMatrix;
        GridSize = new GridSize
        {
            BoxSize = gridManager.Grid.BoxSize,
            HalfBoxSize = gridManager.Grid.BoxSize / 2,
            BoxSizeQuotient = 1 / gridManager.Grid.BoxSize,
            DimensionSize = gridManager.Grid.InnerGrid.Size,
            Length = gridManager.Grid.InnerGrid.Length
        };
    }

    //public readonly Aabb Bounds;
}

public struct GridSize
{
    public float BoxSize;
    public float BoxSizeQuotient;
    public int3 DimensionSize;
    public float HalfBoxSize;
    public int Length;
}

public struct GridWorldBounds : IComponentData
{
    public Aabb Value;
}

internal struct GridUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetFlags(int* flagsPtr, int length, ref NodeFlags addFlags)
    {
        var flagToSet = (int4)(int)addFlags;
        var batchFlags = (int4*)flagsPtr;

        // array length must be 4x aligned for overrun on last batch
        int batchLen = length / 4 + math.select(1, 0, length % 4 != 0);

        for (int i = 0; i < batchLen; i++)
            batchFlags[i] |= flagToSet;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetFlags(ref GridComponent grid, ref NodeFlags addFlags)
    {
        ref var blobValue = ref grid.GridData.Value;
        SetFlags((int*)blobValue.Flags.GetUnsafePtr(), blobValue.Flags.Length, ref addFlags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetFlags(ref GridComponent grid, int3 min, int3 max, ref NodeFlags addFlags)
    {
        // untested

        ref var blobValue = ref grid.GridData.Value;
        int* batchFlags = (int*)blobValue.Flags.GetUnsafePtr();
        int flagToSet = (int)addFlags;

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                    batchFlags[blobValue.GetIndex(x, y, z)] |= flagToSet;
            }
        }
    }

    [BurstCompile]
    internal struct Managed
    {
        public static SetFlagsDelegate SetFlags;

        public static SetFlagsRawDelegate SetFlagsRaw;

        public delegate void SetFlagsDelegate(ref GridComponent grid, ref NodeFlags addFlags);

        public unsafe delegate void SetFlagsRawDelegate(int* flagsPtr, int length, ref NodeFlags addFlags);

        [RuntimeInitializeOnLoadMethod]
        public static unsafe void Initialize()
        {
            if (SetFlags != null)
                return;

            SetFlags = BurstCompiler.CompileFunctionPointer<SetFlagsDelegate>(SetFlagsExecute).Invoke;
            SetFlagsRaw = BurstCompiler.CompileFunctionPointer<SetFlagsRawDelegate>(SetFlagsRawExecute).Invoke;
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SetFlagsDelegate))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetFlagsExecute(ref GridComponent grid, ref NodeFlags addFlags) => GridUtility.SetFlags(ref grid, ref addFlags);

        [BurstCompile]
        [MonoPInvokeCallback(typeof(SetFlagsRawDelegate))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void SetFlagsRawExecute(int* flagsPtr, int length, ref NodeFlags addFlags) => GridUtility.SetFlags(flagsPtr, length, ref addFlags);
    }
}

public static class DefaultColors
{
    public static Color Bounds = Color.blue;
}

public static class GridDebugDrawer
{
    // public static void DrawNode(GridNode node, ref HeavyTransform transform, float size, bool alwaysDraw = false)
    // {
    //     if (ShouldDraw(node, out var color) || alwaysDraw)
    //     {
    //         var worldPosition = transform.GetWorldPoint(node.NavigableCenter);
    //         DrawPoint(worldPosition, color, size);
    //     }
    // }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AddFlags(NodeFlags* flagsArray, int index, NodeFlags flags) => flagsArray[index] |= flags;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AddFlags(ref NodeFlags source, NodeFlags flags) => source |= flags;

    public static void DrawBounds(Aabb bounds, Color color = default)
    {
        var c = color == default ? DefaultColors.Bounds : color;
        DrawBoxPoints(bounds.Center, bounds.Extents * 0.5f, c);
    }

    public static unsafe void DrawBoxPoints(float3 worldCenter, float3 halfExtents, Color color)
    {
        var points = stackalloc float3[8];

        points[0] = worldCenter + new float3(halfExtents.x, halfExtents.y, halfExtents.z);
        points[1] = worldCenter + new float3(-halfExtents.x, halfExtents.y, halfExtents.z);
        points[2] = worldCenter + new float3(halfExtents.x, -halfExtents.y, halfExtents.z);
        points[3] = worldCenter + new float3(-halfExtents.x, -halfExtents.y, halfExtents.z);
        points[4] = worldCenter + new float3(halfExtents.x, halfExtents.y, -halfExtents.z);
        points[5] = worldCenter + new float3(-halfExtents.x, halfExtents.y, -halfExtents.z);
        points[6] = worldCenter + new float3(halfExtents.x, -halfExtents.y, -halfExtents.z);
        points[7] = worldCenter + new float3(-halfExtents.x, -halfExtents.y, -halfExtents.z);

        Debug.DrawLine(points[0], points[1], color, 0);
        Debug.DrawLine(points[1], points[3], color, 0);
        Debug.DrawLine(points[2], points[3], color, 0);
        Debug.DrawLine(points[2], points[0], color, 0);
        Debug.DrawLine(points[4], points[5], color, 0);
        Debug.DrawLine(points[5], points[7], color, 0);
        Debug.DrawLine(points[6], points[7], color, 0);
        Debug.DrawLine(points[6], points[4], color, 0);
        Debug.DrawLine(points[0], points[4], color, 0);
        Debug.DrawLine(points[1], points[5], color, 0);
        Debug.DrawLine(points[2], points[6], color, 0);
        Debug.DrawLine(points[3], points[7], color, 0);
    }

    public static unsafe void DrawCollider(BlobAssetReference<Collider> collider, Color color)
    {
        if (!collider.IsCreated)
            return;

        var colliderPtr = (Collider_*)collider.GetUnsafePtr();
        var ctx = SceneViewDrawingQueue.GetContext(1);
        ctx.Begin(0);

        switch (collider.Value.Type)
        {
            case ColliderType.Box:
                {
                    var box = (BoxCollider*)colliderPtr;
                    ctx.Box(box->Size, box->Center, box->Orientation, color);
                    break;
                }

            case ColliderType.Sphere:
                {
                    var sphere = (SphereCollider*)colliderPtr;
                    ctx.Sphere(sphere->Center, Quaternion.identity, sphere->Radius, color);
                    break;
                }

            case ColliderType.Capsule:
                {
                    var capsule = (CapsuleCollider*)colliderPtr;
                    var direction = capsule->Vertex0 - capsule->Vertex1;
                    ctx.Capsule(capsule->Vertex0, capsule->Vertex1, capsule->Radius, color);

                    break;
                }
        }

        ctx.End();
    }

    /*
    public unsafe struct BoxDrawingFromPoints : ISceneViewDrawing
    {
        public FixedList128<float3> Points;
        public Color Color;

        public int UniqueHash { get; set; }

        public void Draw(Camera cam, Handles.DrawingScope scope)
        {
            Handles.color = Color;
            HandleDrawingUtility.HandleBox((float3*)UnsafeUtility.AddressOf(ref Points));
        }
    }

    public struct SphereDrawing : ISceneViewDrawing
    {
        public float3 Position;
        public float Radius;
        public Color Color;

        public int UniqueHash { get; set; }

        public void Draw(Camera cam, Handles.DrawingScope scope)
        {
            Handles.color = Color;
            HandleDrawingUtility.Capsule(Position, Quaternion.identity, Radius, Radius*2);
            var directionToCamera = cam.transform.position - (Vector3)Position;
            HandleDrawingUtility.Circle(Position, directionToCamera.normalized, Radius);
        }
    }
*/
    // public static void DrawCircle(Vector3 position, Vector3 up, Color color = default, float radius = 1.0f)
    // {
    //     SceneViewDrawingQueue.EnqueueManaged(new SphereDrawing
    //     {
    //         Position = position,
    //         Radius = radius,
    //         Color = color,
    //     });
    // }

    /*
    public static unsafe byte* GetUnsafePtr<T>(this FixedList128<T> list) where T : unmanaged
    {
        return (byte*)UnsafeUtility.AddressOf(ref list) + sizeof(int);
    }

    public static unsafe void AddRange<T>(ref FixedList128<T> list, T* items, int length) where T : unmanaged
    {
        for (int i = 0; i < length; i++)
            list.Add(items[i]);
        //UnsafeUtility.MemCpy(list.GetUnsafePtr(), items, sizeof(T) * length);
        //list.Length = length;
    }

    public static unsafe void DrawBox(float3* points, Color color = default)
    {
        BoxDrawingFromPoints boxDrawing = default;
        AddRange(ref boxDrawing.Points, points, 8);
        boxDrawing.Color = color;
        //SceneViewDrawingQueue.EnqueueManaged(boxDrawing);

        //SceneViewDrawingQueue.GetContext(Thread.CurrentThread.ManagedThreadId).Box();
    }

    public static void DebugWireSphere(Vector3 position, Color color, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        const float angle = 10.0f;
        var x = new Vector3(position.x, position.y + radius * Mathf.Sin(0), position.z + radius * Mathf.Cos(0));
        var y = new Vector3(position.x + radius * Mathf.Cos(0), position.y, position.z + radius * Mathf.Sin(0));
        var z = new Vector3(position.x + radius * Mathf.Cos(0), position.y + radius * Mathf.Sin(0), position.z);

        for (var i = 1; i < 37; i++)
        {
            var newX = new Vector3(position.x, position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad));
            var newY = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y, position.z + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad));
            var newZ = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z);

            Debug.DrawLine(x, newX, color, duration, depthTest);
            Debug.DrawLine(y, newY, color, duration, depthTest);
            Debug.DrawLine(z, newZ, color, duration, depthTest);

            x = newX;
            y = newY;
            z = newZ;
        }
    }

    public static Aabb Transform(float4x4 matrix, Aabb bounds)
    {
        return new Aabb
        {
            Min = math.transform(matrix, bounds.Min),
            Max = math.transform(matrix, bounds.Max),
        };
    }
    */

    /*public static unsafe void DrawNodesInCollider(ref GridComponent gridComponent, BlobAssetReference<Collider> collider)
    {
        var blobGrid = (BlobGrid3D*) gridComponent.GridData.GetUnsafePtr();
        var nodes = (GridNode*) gridComponent.GridData.Value.Nodes.GetUnsafePtr();
        var colliderPtr = (Collider*) collider.GetUnsafePtr();

        // translate the collider AABB into grid space and pull
        // out the range of indices that could be inside.
        gridComponent.GetIndices(colliderPtr->CalculateAabb(), out var min, out var max);

        for (var x = min.x; x <= max.x; x++)
        for (var y = min.y; y <= max.y; y++)
        for (var z = min.z; z <= max.z; z++)
        {
            var node = nodes[blobGrid->GetIndex(x, y, z)];
            var nodeWorldPosition = gridComponent.Transform.GetWorldPoint(node.NavigableCenter);
            var result = PhysicsQueries.Distance.FastPointCollider(nodeWorldPosition, colliderPtr);

            if (result.Distance <= 0)
                DrawPoint(nodeWorldPosition, Color.black, 0.3f);
        }
    }*/

    public static void DrawCube(Vector3 center, Vector3 extents)
    {
        var points = GetBoxPoints(center, extents);
        Debug.DrawLine(points[0], points[1]);
        Debug.DrawLine(points[1], points[3]);
        Debug.DrawLine(points[2], points[3]);
        Debug.DrawLine(points[2], points[0]);
        Debug.DrawLine(points[4], points[5]);
        Debug.DrawLine(points[5], points[7]);
        Debug.DrawLine(points[6], points[7]);
        Debug.DrawLine(points[6], points[4]);
        Debug.DrawLine(points[0], points[4]);
        Debug.DrawLine(points[1], points[5]);
        Debug.DrawLine(points[2], points[6]);
        Debug.DrawLine(points[3], points[7]);
    }

    public static void DrawPoint(Vector3 position, Color color, float size, float duration = 0, bool depthTest = true)
    {
        color = color == default ? Color.white : color;
        //Debug.DrawRay(position+(Vector3.up*(size*0.5f)), -Vector3.up*size, color, duration, depthTest);
        Debug.DrawRay(position + Vector3.right * (size * 0.5f), -Vector3.right * size, color, duration, depthTest);
        Debug.DrawRay(position + Vector3.forward * (size * 0.5f), -Vector3.forward * size, color, duration, depthTest);
    }

    public static float3[] GetBoxPoints(float3 worldCenter, float3 he) => new[] { worldCenter + new float3(he.x, he.y, he.z), worldCenter + new float3(he.x, he.y, -he.z), worldCenter + new float3(he.x, -he.y, he.z), worldCenter + new float3(he.x, -he.y, -he.z), worldCenter + new float3(-he.x, he.y, he.z), worldCenter + new float3(-he.x, he.y, -he.z), worldCenter + new float3(-he.x, -he.y, he.z), worldCenter + new float3(-he.x, -he.y, -he.z) };

    public static NativeArray<float3> GetPoints(BoxCollider boxCollider, float inflation = 0)
    {
        var transform = new RigidTransform(boxCollider.Orientation, float3.zero);

        var toWorld = new float4x4(new RigidTransform(quaternion.identity, boxCollider.Center));

        var inverse = math.inverse(transform);

        var he = math.max(0, boxCollider.Size * 0.5f - inflation); // half extents

        //math.transform(transform, new float3(he.x, he.y, he.z))

        var vertices = new NativeArray<float3>(8, Allocator.Temp);
        vertices[0] = math.transform(transform, new float3(he.x, he.y, he.z)) + boxCollider.Center;
        vertices[1] = math.transform(transform, new float3(-he.x, he.y, he.z)) + boxCollider.Center;
        vertices[2] = math.transform(transform, new float3(he.x, -he.y, he.z)) + boxCollider.Center;
        vertices[3] = math.transform(transform, new float3(-he.x, -he.y, he.z)) + boxCollider.Center;
        vertices[4] = math.transform(transform, new float3(he.x, he.y, -he.z)) + boxCollider.Center;
        vertices[5] = math.transform(transform, new float3(-he.x, he.y, -he.z)) + boxCollider.Center;
        vertices[6] = math.transform(transform, new float3(he.x, -he.y, -he.z)) + boxCollider.Center;
        vertices[7] = math.transform(transform, new float3(-he.x, -he.y, -he.z)) + boxCollider.Center;

        // vertices[0] = math.transform(toWorld, new float3(he.x, he.y, he.z));
        // vertices[1] = math.transform(toWorld, new float3(-he.x, he.y, he.z));
        // vertices[2] = math.transform(toWorld, new float3(he.x, -he.y, he.z));
        // vertices[3] = math.transform(toWorld, new float3(-he.x, -he.y, he.z));
        // vertices[4] = math.transform(toWorld, new float3(he.x, he.y, -he.z));
        // vertices[5] = math.transform(toWorld, new float3(-he.x, he.y, -he.z));
        // vertices[6] = math.transform(toWorld, new float3(he.x, -he.y, -he.z));
        // vertices[7] = math.transform(toWorld, new float3(-he.x, -he.y, -he.z));
        return vertices;
    }

    public static float3[] GetTransformedPoints(Vector3 center, Vector3 extents, float4x4 matrix) => new[] { math.transform(matrix, center + new Vector3(extents.x, extents.y, extents.z)), math.transform(matrix, center + new Vector3(extents.x, extents.y, -extents.z)), math.transform(matrix, center + new Vector3(extents.x, -extents.y, extents.z)), math.transform(matrix, center + new Vector3(extents.x, -extents.y, -extents.z)), math.transform(matrix, center + new Vector3(-extents.x, extents.y, extents.z)), math.transform(matrix, center + new Vector3(-extents.x, extents.y, -extents.z)), math.transform(matrix, center + new Vector3(-extents.x, -extents.y, extents.z)), math.transform(matrix, center + new Vector3(-extents.x, -extents.y, -extents.z)) };

    public static float3[] GetTransformedPoints(Vector3 center, Vector3 extents, RigidTransform rt) => new[] { math.transform(rt, center + new Vector3(extents.x, extents.y, extents.z)), math.transform(rt, center + new Vector3(extents.x, extents.y, -extents.z)), math.transform(rt, center + new Vector3(extents.x, -extents.y, extents.z)), math.transform(rt, center + new Vector3(extents.x, -extents.y, -extents.z)), math.transform(rt, center + new Vector3(-extents.x, extents.y, extents.z)), math.transform(rt, center + new Vector3(-extents.x, extents.y, -extents.z)), math.transform(rt, center + new Vector3(-extents.x, -extents.y, extents.z)), math.transform(rt, center + new Vector3(-extents.x, -extents.y, -extents.z)) };

    public static float3[] GetTransformedPoints(Vector3 center, Vector3 extents, quaternion rot) => new[] { math.rotate(rot, center + new Vector3(extents.x, extents.y, extents.z)), math.rotate(rot, center + new Vector3(extents.x, extents.y, -extents.z)), math.rotate(rot, center + new Vector3(extents.x, -extents.y, extents.z)), math.rotate(rot, center + new Vector3(extents.x, -extents.y, -extents.z)), math.rotate(rot, center + new Vector3(-extents.x, extents.y, extents.z)), math.rotate(rot, center + new Vector3(-extents.x, extents.y, -extents.z)), math.rotate(rot, center + new Vector3(-extents.x, -extents.y, extents.z)), math.rotate(rot, center + new Vector3(-extents.x, -extents.y, -extents.z)) };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasAnyFlag(NodeFlags* flagsArray, int index, NodeFlags flags) => (flagsArray[index] & flags) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasAnyFlag(ref NodeFlags source, NodeFlags flag) => (source & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlag(NodeFlags* flagsArray, int index, NodeFlags flag) => (flagsArray[index] & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlag(ref NodeFlags source, NodeFlags flag) => (source & flag) == flag;

    /// <summary>
    /// Set flags at random-access indices;
    /// </summary>
    public static unsafe void ModifyFlagsInBatch(NodeFlags* flags, int* indices, int indicesCount, VolumeModificationData volumeData)
    {
        for (int i = 0; i < indicesCount; i++)
        {
            int idx = indices[i];

            if (volumeData.PresentRestriction != NodeFlags.None && !HasFlag(flags, idx, volumeData.PresentRestriction))
                continue;

            if (volumeData.AbsentRestriction != NodeFlags.None && HasFlag(flags, idx, volumeData.PresentRestriction))
                continue;

            RemoveFlags(flags, idx, volumeData.RemoveFlags);
            AddFlags(flags, idx, volumeData.AddFlags);
        }
    }

    public static unsafe void ModifyFlagsInVolume(ref GridComponent gridComponent, BlobAssetReference<Collider> collider, VolumeModificationData volumeData)
    {
        var blobGrid = (BlobGrid3D*)gridComponent.GridData.GetUnsafePtr();
        var nodes = (GridNode*)gridComponent.GridData.Value.Nodes.GetUnsafePtr();
        var flags = (NodeFlags*)gridComponent.GridData.Value.Flags.GetUnsafePtr();
        var colliderPtr = (Collider*)collider.GetUnsafePtr();

        // Convert the collider AABB into grid space and pull out the corresponding range of array indices.
        gridComponent.GetIndices(colliderPtr->CalculateAabb(), out var min, out var max);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    int idx = blobGrid->GetIndex(x, y, z);
                    ref var node = ref nodes[idx];
                    var nodeWorldPosition = gridComponent.Transform.GetWorldPoint(node.NavigableCenter);
                    var result = PhysicsQueries.Distance.FastPointCollider(nodeWorldPosition, colliderPtr);
                    if (result.Distance > 0)
                        continue;

                    if (volumeData.PresentRestriction != NodeFlags.None && !HasFlag(flags, idx, volumeData.PresentRestriction))
                        continue;

                    if (volumeData.AbsentRestriction != NodeFlags.None && HasFlag(flags, idx, volumeData.PresentRestriction))
                        continue;

                    RemoveFlags(flags, idx, volumeData.RemoveFlags);
                    AddFlags(flags, idx, volumeData.AddFlags);
                }
            }
        }
    }

    /// <summary>
    /// Optimized version of conditionally modifying flags in a volume with re-used/pre-calculated arguments.
    /// </summary>
    public static unsafe void ModifyFlagsInVolume(GridData gridData, Collider* collider, VolumeModificationData volumeData)
    {
        // Assumes the collider is already checked to be within the grid area.
        var indices = BlobGrid3D.GetIndicesUnclamped(collider->CalculateAabb(), gridData.ToLocalMatrix, gridData.GridSize.HalfBoxSize, gridData.GridSize.BoxSizeQuotient);

        for (int x = indices.c0.x; x <= indices.c1.x; x++)
        {
            for (int y = indices.c0.y; y <= indices.c1.y; y++)
            {
                for (int z = indices.c0.z; z <= indices.c1.z; z++)
                {
                    int idx = gridData.BlobGrid->GetIndex(x, y, z);
                    ref var node = ref gridData.Nodes[idx];
                    var nodeWorldPosition = math.transform(gridData.ToWorldMatrix, node.NavigableCenter);

                    var collisionResult = PhysicsQueries.Distance.FastPointCollider(nodeWorldPosition, collider);
                    if (collisionResult.Distance > 0)
                        continue;

                    if (volumeData.PresentRestriction != NodeFlags.None && !HasFlag(gridData.Flags, idx, volumeData.PresentRestriction))
                        continue;

                    if (volumeData.AbsentRestriction != NodeFlags.None && HasFlag(gridData.Flags, idx, volumeData.PresentRestriction))
                        continue;

                    RemoveFlags(gridData.Flags, idx, volumeData.RemoveFlags);
                    AddFlags(gridData.Flags, idx, volumeData.AddFlags);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void RemoveFlags(NodeFlags* flagsArray, int index, NodeFlags flags) => flagsArray[index] &= ~flags;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void RemoveFlags(ref NodeFlags source, NodeFlags flags) => source &= ~flags;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void ToggleFlag(NodeFlags* flagsArray, int index, NodeFlags flags) => flagsArray[index] ^= flags;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void ToggleFlag(ref NodeFlags source, NodeFlags flags) => source ^= flags;

    /*public static unsafe void ModifyFlagsInVolume(ref GridComponent gridComponent, BlobAssetReference<Collider> collider, VolumeModificationData volumeData, NativeList<int> previousIndices, NativeList<int> modifiedIndices)
    {
        var blobGrid = (BlobGrid3D*) gridComponent.GridData.GetUnsafePtr();
        var nodes = (GridNode*) gridComponent.GridData.Value.Nodes.GetUnsafePtr();
        var colliderPtr = (Collider*) collider.GetUnsafePtr();

        // translate the collider AABB into grid space and pull
        // out the range of indices that could be inside.
        gridComponent.GetIndices(colliderPtr->CalculateAabb(), out var min, out var max);

        for (var x = min.x; x <= max.x; x++)
        for (var y = min.y; y <= max.y; y++)
        for (var z = min.z; z <= max.z; z++)
        {
            ref var node = ref nodes[blobGrid->GetIndex(x, y, z)];
            var nodeWorldPosition = gridComponent.Transform.GetWorldPoint(node.NavigableCenter);
            var result = PhysicsQueries.Distance.FastPointCollider(nodeWorldPosition, colliderPtr);

            if (result.Distance > 0)
                continue;

            if (volumeData.PresentRestriction != NodeFlags.None && !node.HasFlag(volumeData.PresentRestriction))
                continue;

            if (volumeData.AbsentRestriction != NodeFlags.None && node.HasFlag(volumeData.PresentRestriction))
                continue;

            node.RemoveFlags(volumeData.RemoveFlags);
            node.AddFlags(volumeData.AddFlags);
        }
    }    */
    /*private static bool ShouldDraw(GridNode node, out Color color)
    {
        color = default;

        if (node.HasFlag(NodeFlags.Avoidance))
        {
            color = UnityColors.OrangeRed;
            return true;
        }

        if (node.HasFlag(NodeFlags.NearEdge))
        {
            color = UnityColors.Grey;
            return true;
        }

        if (node.HasFlag(NodeFlags.Navigation))
        {
            color = UnityColors.Blue;
            return true;
        }

        if (node.HasFlag(NodeFlags.Selected))
            return true;
        return false;
    }*/
    /*public static void DrawCube(Vector3 center, Vector3 extents, float4x4 matrix)
    {
        DrawBoxPoints(GetTransformedPoints(center, extents, matrix));
    }

    public static void DrawCube(Vector3 center, Vector3 extents, RigidTransform rt)
    {
        DrawBoxPoints(GetTransformedPoints(center, extents, rt));
    }

    public static void DrawCube(Vector3 center, Vector3 extents, quaternion rot)
    {
        DrawBoxPoints(GetTransformedPoints(center, extents, rot));
    }*/

    //public static void DrawCube(BoxCollider box) => DrawCubePoints(GetPoints(box));

    /*private static void DrawBoxPoints(NativeArray<float3> points)
    {
        Debug.DrawLine(points[0], points[1]);
        Debug.DrawLine(points[1], points[3]);
        Debug.DrawLine(points[2], points[3]);
        Debug.DrawLine(points[2], points[0]);
        Debug.DrawLine(points[4], points[5]);
        Debug.DrawLine(points[5], points[7]);
        Debug.DrawLine(points[6], points[7]);
        Debug.DrawLine(points[6], points[4]);
        Debug.DrawLine(points[0], points[4]);
        Debug.DrawLine(points[1], points[5]);
        Debug.DrawLine(points[2], points[6]);
        Debug.DrawLine(points[3], points[7]);
    }*/

    private static void DrawBoxPoints(float3[] points)
    {
        Debug.DrawLine(points[0], points[1]);
        Debug.DrawLine(points[1], points[3]);
        Debug.DrawLine(points[2], points[3]);
        Debug.DrawLine(points[2], points[0]);
        Debug.DrawLine(points[4], points[5]);
        Debug.DrawLine(points[5], points[7]);
        Debug.DrawLine(points[6], points[7]);
        Debug.DrawLine(points[6], points[4]);
        Debug.DrawLine(points[0], points[4]);
        Debug.DrawLine(points[1], points[5]);
        Debug.DrawLine(points[2], points[6]);
        Debug.DrawLine(points[3], points[7]);
    }

    // #if UNITY_EDITOR
    //     public interface ISceneViewDrawing
    //     {
    //         int UniqueHash { get; set; }
    //         void Draw(Camera camera, Handles.DrawingScope scope);
    //     }
    // #endif

    public static class HandleDrawingUtility
    {
        public static void Capsule(Vector3 position, Quaternion rotation, float radius, float height)
        {
            var angleMatrix = Matrix4x4.TRS(position, rotation * Quaternion.AngleAxis(45f, Vector3.up), Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                float pointOffset = (height - radius * 2) / 2;

                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
                Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);

                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
                Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);

                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);
            }
        }

        public static void Circle(Vector3 position, Vector3 up, float radius = 1.0f)
        {
            var _up = up.normalized * radius;
            var _forward = Vector3.Slerp(_up, -_up, 0.5f);
            var _right = Vector3.Cross(_up, _forward).normalized * radius;
            var matrix = new Matrix4x4();

            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;
            matrix[4] = _up.x;
            matrix[5] = _up.y;
            matrix[6] = _up.z;
            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;

            var _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            var _nextPoint = Vector3.zero;

            for (int i = 0; i < 91; i++)
            {
                _nextPoint.x = Mathf.Cos(i * 4 * Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin(i * 4 * Mathf.Deg2Rad);
                _nextPoint.y = 0;
                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);
                Handles.DrawLine(_lastPoint, _nextPoint);
                _lastPoint = _nextPoint;
            }
        }

        public static void HandleArrow(Vector3 position, Vector3 direction)
        {
            HandleRay(position, direction);
            HandleCone(position + direction, -direction * 0.333f, 15);
        }

        public static unsafe void HandleBox(float3* points)
        {
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[1], points[3]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawLine(points[2], points[0]);
            Handles.DrawLine(points[4], points[5]);
            Handles.DrawLine(points[5], points[7]);
            Handles.DrawLine(points[6], points[7]);
            Handles.DrawLine(points[6], points[4]);
            Handles.DrawLine(points[0], points[4]);
            Handles.DrawLine(points[1], points[5]);
            Handles.DrawLine(points[2], points[6]);
            Handles.DrawLine(points[3], points[7]);
        }

        public static void HandleCone(Vector3 position, Vector3 direction, float angle = 45)
        {
            float length = direction.magnitude;
            var _forward = direction;
            var _up = Vector3.Slerp(_forward, -_forward, 0.5f);
            var _right = Vector3.Cross(_forward, _up).normalized * length;
            direction = direction.normalized;
            var slerpedVector = Vector3.Slerp(_forward, _up, angle / 90.0f);
            var farPlane = new Plane(-direction, position + _forward);
            var distRay = new Ray(position, slerpedVector);
            farPlane.Raycast(distRay, out float dist);
            HandleRay(position, slerpedVector.normalized * dist);
            HandleRay(position, Vector3.Slerp(_forward, -_up, angle / 90.0f).normalized * dist);
            HandleRay(position, Vector3.Slerp(_forward, _right, angle / 90.0f).normalized * dist);
            HandleRay(position, Vector3.Slerp(_forward, -_right, angle / 90.0f).normalized * dist);
            Circle(position + _forward, direction, (_forward - slerpedVector.normalized * dist).magnitude);
            Circle(position + _forward * 0.5f, direction, (_forward * 0.5f - slerpedVector.normalized * (dist * 0.5f)).magnitude);
        }

        public static void HandleRay(Vector3 start, Vector3 dir, float thickness = 0f) => Handles.DrawLine(start, start + dir);
    }

    public static class SceneViewDrawingQueue
    {
        private static readonly DebugStream _stream;

        //private static readonly Dictionary<int, ISceneViewDrawing> Drawings = new Dictionary<int, ISceneViewDrawing>();
        private static int _lastFrame;

        static SceneViewDrawingQueue()
        {
            _stream = new DebugStream();
            SceneView.duringSceneGui += Draw;
            CompilationPipeline.assemblyCompilationStarted += obj => _stream.Clear();
        }

        public static DebugStream.Context GetContext(int size)
        {
            if (HasFrameChanged())
                _stream.Clear();

            return _stream.GetContext(size);
        }

        /*
        public static unsafe void EnqueueManaged<T>(T item) where T : struct, ISceneViewDrawing
        {
            var hasFrameChanged = HasFrameChanged();
            if (hasFrameChanged)
            {
                Drawings.Clear();
                _stream.Clear();
            }

            if (item.UniqueHash == default)
                item.UniqueHash = (int) CollectionHelper.Hash(UnsafeUtility.AddressOf(ref item), UnsafeUtility.SizeOf<T>());

            if (!Drawings.ContainsKey(item.UniqueHash))
                Drawings.Add(item.UniqueHash, item);
        }
        */

        private static void Draw(SceneView view)
        {
            var cam = view.camera;
            using (var scope = new Handles.DrawingScope())
            {
                // foreach (var drawing in Drawings.Values)
                //     drawing.Draw(cam, scope);
                _stream.Draw(cam.transform.position);
            }

            SceneView.RepaintAll();
        }

        private static bool HasFrameChanged()
        {
            int t = Time.frameCount;
            if (_lastFrame == t)
                return false;
            _lastFrame = t;
            return true;
        }

        public class DebugStream
        {
            internal readonly List<NativeStream> m_DebugStreams = new List<NativeStream>();

            public enum Type
            {
                Point,
                Line,
                Arrow,
                Plane,
                Circle,
                Arc,
                Cone,
                Text,
                Box,
                Capsule,
                Sphere,
                CapsuleFromTwoPoints
            }

            public void Clear()
            {
                // Reset
                for (int i = 0; i < m_DebugStreams.Count; i++)
                    m_DebugStreams[i].Dispose();
                m_DebugStreams.Clear();

                // // Set up component to draw
                // if (m_DrawComponent == null)
                // {
                //     GameObject drawObject = new GameObject();
                //     m_DrawComponent = drawObject.AddComponent<DrawComponent>();
                //     m_DrawComponent.name = "DebugStream.DrawComponent";
                //     m_DrawComponent.DebugDraw = this;
                // }
            }

            public void Draw(float3 cameraPosition)
            {
                for (int i = 0; i < m_DebugStreams.Count; i++)
                {
                    var reader = m_DebugStreams[i].AsReader();
                    for (int j = 0; j != reader.ForEachCount; j++)
                    {
                        reader.BeginForEachIndex(j);
                        while (reader.RemainingItemCount != 0)
                        {
                            switch (reader.Read<Type>())
                            {
                                case Type.Point:
                                    reader.Read<Point>().Draw();
                                    break;

                                case Type.Line:
                                    reader.Read<Line>().Draw();
                                    break;

                                case Type.Arrow:
                                    reader.Read<Line>().DrawArrow();
                                    break;

                                case Type.Plane:
                                    reader.Read<Line>().DrawPlane();
                                    break;

                                case Type.Circle:
                                    reader.Read<Line>().DrawCircle();
                                    break;

                                case Type.Arc:
                                    reader.Read<Arc>().Draw();
                                    break;

                                case Type.Cone:
                                    reader.Read<Cone>().Draw();
                                    break;

                                case Type.Text:
                                    reader.Read<Text>().Draw(ref reader);
                                    break;

                                case Type.Box:
                                    reader.Read<Box>().Draw();
                                    break;

                                case Type.Capsule:
                                    reader.Read<Capsule>().Draw();
                                    break;

                                case Type.Sphere:
                                    reader.Read<Sphere>().Draw(cameraPosition);
                                    break;

                                case Type.CapsuleFromTwoPoints:
                                    reader.Read<CapsuleFromTwoPoints>().Draw();
                                    break;

                                default:
                                    return; // unknown type
                            }
                        }

                        reader.EndForEachIndex();
                    }
                }
            }

            public Context GetContext(int foreachCount)
            {
                var stream = new NativeStream(foreachCount, Allocator.TempJob);
                m_DebugStreams.Add(stream);
                return new Context
                {
                    Writer = stream.AsWriter()
                };
            }

            //private class DrawComponent : MonoBehaviour
            //{
            //    public DebugStream DebugDraw;

            // public void OnDrawGizmos()
            // {
            //     if (DebugDraw != null)
            //     {
            //         DebugDraw.Draw();
            //     }
            // }
            //}
            public void OnDestroy()
            {
                for (int i = 0; i < m_DebugStreams.Count; i++)
                    m_DebugStreams[i].Dispose();
            }

            public struct Arc
            {
                public float Angle;
                public float3 Arm;
                public float3 Center;
                public Color Color;
                public float3 Normal;

                public void Draw()
                {
#if UNITY_EDITOR
                    Handles.color = Color;

                    const int res = 16;
                    var q = quaternion.AxisAngle(Normal, Angle / res);
                    var currentArm = Arm;
                    Handles.DrawLine(Center, Center + currentArm);
                    for (int i = 0; i < res; i++)
                    {
                        var nextArm = math.mul(q, currentArm);
                        Handles.DrawLine(Center + currentArm, Center + nextArm);
                        currentArm = nextArm;
                    }

                    Handles.DrawLine(Center, Center + currentArm);
#endif
                }
            }

            public struct Cone
            {
                public float Angle;
                public float3 Axis;
                public Color Color;
                public float3 Point;

                public void Draw()
                {
#if UNITY_EDITOR
                    Handles.color = Color;

                    float scale = Math.NormalizeWithLength(Axis, out var dir);

                    float3 arm;
                    {
                        Math.CalculatePerpendicularNormalized(dir, out var perp1, out var perp2);
                        arm = math.mul(quaternion.AxisAngle(perp1, Angle), dir) * scale;
                    }

                    const int res = 16;
                    var q = quaternion.AxisAngle(dir, 2.0f * math.PI / res);
                    for (int i = 0; i < res; i++)
                    {
                        var nextArm = math.mul(q, arm);
                        Handles.DrawLine(Point, Point + arm);
                        Handles.DrawLine(Point + arm, Point + nextArm);
                        arm = nextArm;
                    }
#endif
                }
            }

            public ref struct Context
            {
                internal NativeStream.Writer Writer;

                public void Arc(float3 center, float3 normal, float3 arm, float angle, Color color)
                {
                    Writer.Write(Type.Arc);
                    Writer.Write(new Arc
                    {
                        Center = center,
                        Normal = normal,
                        Arm = arm,
                        Angle = angle,
                        Color = color
                    });
                }

                public void Arrow(float3 x, float3 v, Color color)
                {
                    Writer.Write(Type.Arrow);
                    Writer.Write(new Line { X0 = x, X1 = x + v, Color = color });
                }

                public void Begin(int index) => Writer.BeginForEachIndex(index);

                public void Box(float3 size, float3 center, quaternion orientation, Color color)
                {
                    Writer.Write(Type.Box);
                    Writer.Write(new Box { Size = size, Center = center, Orientation = orientation, Color = color });
                }

                public void Capsule(Vector3 center, Quaternion orientation, float radius, float height, Color color)
                {
                    Writer.Write(Type.Capsule);
                    Writer.Write(new Capsule
                    {
                        Center = center,
                        Orientation = orientation,
                        Radius = radius,
                        Height = height,
                        Color = color
                    });
                }

                public void Capsule(Vector3 point0, Vector3 point1, float radius, Color color)
                {
                    Writer.Write(Type.CapsuleFromTwoPoints);
                    Writer.Write(new CapsuleFromTwoPoints { PointA = point0, PointB = point1, Radius = radius, Color = color });
                }

                public void Circle(float3 x, float3 v, Color color)
                {
                    Writer.Write(Type.Circle);
                    Writer.Write(new Line { X0 = x, X1 = x + v, Color = color });
                }

                public void Cone(float3 point, float3 axis, float angle, Color color)
                {
                    Writer.Write(Type.Cone);
                    Writer.Write(new Cone { Point = point, Axis = axis, Angle = angle, Color = color });
                }

                public void End() => Writer.EndForEachIndex();

                public void Line(float3 x0, float3 x1, Color color)
                {
                    Writer.Write(Type.Line);
                    Writer.Write(new Line { X0 = x0, X1 = x1, Color = color });
                }

                public void Plane(float3 x, float3 v, Color color)
                {
                    Writer.Write(Type.Plane);
                    Writer.Write(new Line { X0 = x, X1 = x + v, Color = color });
                }

                public void Point(float3 x, float size, Color color)
                {
                    Writer.Write(Type.Point);
                    Writer.Write(new Point { X = x, Size = size, Color = color });
                }

                public void Sphere(Vector3 position, Quaternion rotation, float radius, Color color)
                {
                    Writer.Write(Type.Sphere);
                    Writer.Write(new Sphere
                    {
                        Capsule = new Capsule
                        {
                            Center = position,
                            Orientation = rotation,
                            Radius = radius,
                            Height = radius * 2,
                            Color = color
                        }
                    });
                }

                public void Text(char[] text, float3 x, Color color)
                {
                    Writer.Write(Type.Text);
                    Writer.Write(new Text { X = x, Color = color, Length = text.Length });

                    foreach (char c in text)
                        Writer.Write(c);
                }
            }

            public struct Line
            {
                public Color Color;
                public float3 X0;
                public float3 X1;

                public void Draw()
                {
#if UNITY_EDITOR
                    Handles.color = Color;
                    Handles.DrawLine(X0, X1);
#endif
                }

                public void DrawArrow()
                {
                    if (!math.all(X0 == X1))
                    {
#if UNITY_EDITOR
                        Handles.color = Color;

                        Handles.DrawLine(X0, X1);
                        var v = X1 - X0;
                        float length = Math.NormalizeWithLength(v, out var dir);
                        Math.CalculatePerpendicularNormalized(dir, out var perp, out var perp2);
                        float3 scale = length * 0.2f;

                        Handles.DrawLine(X1, X1 + (perp - dir) * scale);
                        Handles.DrawLine(X1, X1 - (perp + dir) * scale);
                        Handles.DrawLine(X1, X1 + (perp2 - dir) * scale);
                        Handles.DrawLine(X1, X1 - (perp2 + dir) * scale);
#endif
                    }
                }

                public void DrawCircle()
                {
                    if (!math.all(X0 == X1))
                    {
#if UNITY_EDITOR
                        Handles.color = Color;

                        var v = X1 - X0;
                        float length = Math.NormalizeWithLength(v, out var dir);
                        Math.CalculatePerpendicularNormalized(dir, out var perp, out var perp2);
                        float3 scale = length * 0.2f;

                        const int res = 16;
                        var q = quaternion.AxisAngle(dir, 2.0f * math.PI / res);
                        var arm = perp * length;
                        for (int i = 0; i < res; i++)
                        {
                            var nextArm = math.mul(q, arm);
                            Handles.DrawLine(X0 + arm, X0 + nextArm);
                            arm = nextArm;
                        }
#endif
                    }
                }

                public void DrawPlane()
                {
                    if (!math.all(X0 == X1))
                    {
#if UNITY_EDITOR
                        Handles.color = Color;

                        Handles.DrawLine(X0, X1);
                        var v = X1 - X0;
                        float length = Math.NormalizeWithLength(v, out var dir);
                        Math.CalculatePerpendicularNormalized(dir, out var perp, out var perp2);
                        float3 scale = length * 0.2f;

                        Handles.DrawLine(X1, X1 + (perp - dir) * scale);
                        Handles.DrawLine(X1, X1 - (perp + dir) * scale);
                        Handles.DrawLine(X1, X1 + (perp2 - dir) * scale);
                        Handles.DrawLine(X1, X1 - (perp2 + dir) * scale);

                        perp *= length;
                        perp2 *= length;
                        Handles.DrawLine(X0 + perp + perp2, X0 + perp - perp2);
                        Handles.DrawLine(X0 + perp - perp2, X0 - perp - perp2);
                        Handles.DrawLine(X0 - perp - perp2, X0 - perp + perp2);
                        Handles.DrawLine(X0 - perp + perp2, X0 + perp + perp2);
#endif
                    }
                }
            }

            public struct Point
            {
                public Color Color;
                public float Size;
                public float3 X;

                public void Draw()
                {
#if UNITY_EDITOR
                    Handles.color = Color;
                    Handles.DrawLine(X - new float3(Size, 0, 0), X + new float3(Size, 0, 0));
                    Handles.DrawLine(X - new float3(0, Size, 0), X + new float3(0, Size, 0));
                    Handles.DrawLine(X - new float3(0, 0, Size), X + new float3(0, 0, Size));
#endif
                }
            }

            public struct Vector
            {
                public static readonly float3 Back = new float3(0.0f, 0.0f, -1f);
                public static readonly float3 Down = new float3(0.0f, -1f, 0.0f);
                public static readonly float3 Forward = new float3(0.0f, 0.0f, 1f);
                public static readonly float3 Left = new float3(-1f, 0.0f, 0.0f);
                public static readonly float3 One = new float3(1f, 1f, 1f);
                public static readonly float3 Right = new float3(1f, 0.0f, 0.0f);
                public static readonly float3 Up = new float3(0.0f, 1f, 0.0f);
                public static readonly float3 Zero = new float3(0.0f, 0.0f, 0.0f);
            }

            private struct Box
            {
                public float3 Center;
                public Color Color;
                public quaternion Orientation;
                public float3 Size;

                public void Draw()
                {
#if UNITY_EDITOR
                    var orig = Handles.matrix;
                    var mat = Matrix4x4.TRS(Center, Orientation, Vector3.one);
                    Handles.matrix = mat;
                    Handles.color = Color;
                    Handles.DrawWireCube(Vector3.zero, new Vector3(Size.x, Size.y, Size.z));
                    Handles.matrix = orig;
#endif
                }
            }

            private struct Capsule
            {
                public Vector3 Center;
                public Color Color;
                public float Height;
                public Quaternion Orientation;
                public float Radius;

                public void Draw()
                {
#if UNITY_EDITOR
                    Handles.color = Color;
                    var angleMatrix = Matrix4x4.TRS(Center, Orientation * Quaternion.AngleAxis(45f, Vector3.up), Handles.matrix.lossyScale);
                    using (new Handles.DrawingScope(angleMatrix))
                    {
                        float pointOffset = (Height - Radius * 2) / 2;
                        Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, Radius);
                        Handles.DrawLine(new Vector3(0, pointOffset, -Radius), new Vector3(0, -pointOffset, -Radius));
                        Handles.DrawLine(new Vector3(0, pointOffset, Radius), new Vector3(0, -pointOffset, Radius));
                        Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, Radius);
                        Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, Radius);
                        Handles.DrawLine(new Vector3(-Radius, pointOffset, 0), new Vector3(-Radius, -pointOffset, 0));
                        Handles.DrawLine(new Vector3(Radius, pointOffset, 0), new Vector3(Radius, -pointOffset, 0));
                        Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, Radius);
                        Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, Radius);
                        Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, Radius);
                    }
#endif
                }
            }

            private struct CapsuleFromTwoPoints
            {
                public Color Color;
                public Vector3 PointA;
                public Vector3 PointB;
                public float Radius;

                public void Draw()
                {
#if UNITY_EDITOR
                    var auth = new CapsuleGeometry { Vertex0 = PointA, Vertex1 = PointB, Radius = Radius }.ToAuthoring();

                    Handles.color = Color;

                    var toB = PointB - PointA;
                    var inverseOrientation = math.inverse(auth.Orientation);
                    var localA = math.rotate(inverseOrientation, (float3)PointA - auth.Center);
                    var localB = math.rotate(inverseOrientation, (float3)PointB - auth.Center);
                    var bFromA = localB - localA;

                    var orig = Handles.matrix;
                    var mat = Matrix4x4.TRS(auth.Center, auth.Orientation, Vector3.one);
                    Handles.matrix = mat;

                    //new Point { Color = Color, Size = 0.1f, X = float3.zero }.Draw();
                    //new Point { Color = Color, Size = 0.1f, X = localA }.Draw();
                    //new Point { Color = Color, Size = 0.1f, X = localB }.Draw();

                    Handles.DrawWireDisc(localA, bFromA, Radius);
                    Handles.DrawWireDisc(localB, bFromA, Radius);

                    Handles.DrawWireArc(localA, Vector3.right, Vector3.down, 180, Radius);
                    Handles.DrawWireArc(localB, Vector3.left, Vector3.down, 180, Radius);

                    Handles.DrawWireArc(localA, Vector3.down, Vector3.left, 180, Radius);
                    Handles.DrawWireArc(localB, Vector3.up, Vector3.left, 180, Radius);

                    var up = Vector.Up * Radius;
                    var down = Vector.Down * Radius;
                    var left = Vector.Left * Radius;
                    var right = Vector.Right * Radius;

                    Handles.DrawLine(localA + up, localB + up);
                    Handles.DrawLine(localA + down, localB + down);
                    Handles.DrawLine(localA + left, localB + left);
                    Handles.DrawLine(localA + right, localB + right);

                    Handles.matrix = orig;
#endif
                }
            }

            private struct Sphere
            {
                public Capsule Capsule;

                public void Draw(float3 cameraPosition)
                {
#if UNITY_EDITOR
                    Handles.color = Capsule.Color;
                    Capsule.Draw();
                    var directionToCamera = cameraPosition - (float3)Capsule.Center;
                    HandleDrawingUtility.Circle(Capsule.Center, math.normalize(directionToCamera), Capsule.Radius);
#endif
                }
            }

            private struct Text
            {
                public Color Color;
                public int Length;
                public float3 X;

                public void Draw(ref NativeStream.Reader reader)
                {
                    // Read string data.
                    char[] stringBuf = new char[Length];
                    for (int i = 0; i < Length; i++)
                        stringBuf[i] = reader.Read<char>();

                    var style = new GUIStyle();
                    style.normal.textColor = Color;
#if UNITY_EDITOR
                    Handles.Label(X, new string(stringBuf), style);
#endif
                }
            }

            /*
            public static void DrawWireHalfDiscAlongNormal(Vector3 center, Vector3 normal, float radius)
            {
                normal = Vector3.Cross(normal, Vector3.left);
                Vector3 from = Vector3.Cross(normal, Vector3.back);
                Handles.DrawWireArc(center, normal, from, 180f, radius, 0);
            }

            public static void DrawWireHalfDiscAlongNormal2(Vector3 center, Vector3 normal, float radius)
            {
                normal = Vector3.Cross(normal, Vector3.up);
                Vector3 from = Vector3.Cross(normal, Vector3.back);
                Handles.DrawWireArc(center, normal, from, 180f, radius, 0);
            }
            */
        }

#if UNITY_EDITOR
#endif

#if UNITY_EDITOR
#endif
    }

#if UNITY_EDITOR
#endif

}

public class GridDebugDrawingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref GridComponent gridComponent) =>
        {
            // ref var blobValue = ref gridComponent.GridData.Value;
            // for (var i = 0; i < blobValue.Nodes.Length; i++)
            // {
            //     var node = blobValue.Nodes[i];
            //     GridDebugDrawer.DrawNode(node, ref gridComponent.Transform, gridComponent.Size.BoxSize);
            // }

        }).WithoutBurst().Run();

        var grid = GetSingleton<GridComponent>();

        Entities.ForEach((ref GridVolumeModifierComponent volumeModifier) =>
        {
            GridDebugDrawer.DrawCollider(volumeModifier.Collider, volumeModifier.Color);
            //GridDebugDrawer.DrawNodesInCollider(ref grid, volumeModifier.Collider);

        }).WithoutBurst().Run();

        Entities.ForEach((ref GridWorldBounds bounds) =>
        {
            GridDebugDrawer.DrawBounds(bounds.Value);

        }).WithoutBurst().Run();
    }
}

/*public static class GridUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetAllFlags(ref GridComponent grid, NodeFlags flag)
    {
        var flag4 = (int4)(int)flag;
        var flags = (int4*)grid.GridData.Value.Flags.GetUnsafePtr();
        var length = grid.GridData.Value.Nodes.Length;
        for (var i = 0; i < length; i++)
        {
            flags[i] |= flag4;
        }
    }
}*/

public class GridVolumeNodeUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var grid = GetSingleton<GridComponent>();
        var flags = NodeFlags.Avoidance;
        GridUtility.SetFlags(ref grid, ref flags);
    }
}
