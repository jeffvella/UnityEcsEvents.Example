using System;
using Providers.Grid;
using SimpleBurstCollision;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Vella.SimpleBurstCollision;

public static class GridNodeJobs
{
    public static class AllNodes
    {
        /*[BurstCompile]
        public struct SetFlags : IJob
        {
            [NativeDisableUnsafePtrRestriction] public IntPtr BaseAddress;
            public NodeFlags Flags;
            public int Stride;
            public int Length;

            public unsafe void Execute()
            {
                for (var i = 0; i < Length; i++)
                    ((GridNode*) (BaseAddress + i * Stride))->Flags = Flags;
            }

            public static unsafe JobHandle Schedule(NativeArray<GridNode> array, NodeFlags defaultFlags)
            {
                return new SetFlags {Flags = defaultFlags, BaseAddress = (IntPtr) array.GetUnsafePtr(), Stride = UnsafeUtility.SizeOf<GridNode>(), Length = array.Length}.Schedule();
            }
        }*/

        /*[BurstCompile]
        public struct SetFlagsInNodeRange : IJob
        {
            //public NativeResultList<GridNode> Results;
            public NativeArray3D<GridNode> Grid;
            public int3 LocalGridMax;
            public int3 LocalGridMin;
            public NodeFlags Flags;

            public void Execute()
            {
                for (var x = LocalGridMin.x; x <= LocalGridMax.x; x++)
                for (var y = LocalGridMin.y; y <= LocalGridMax.y; y++)
                for (var z = LocalGridMin.z; z <= LocalGridMax.z; z++)
                    Grid[x, y, z].Flags |= Flags;
            }

            public static JobHandle Schedule(NativeArray3D<GridNode> grid, NodeFlags flags, int3 min, int3 max)
            {
                return new SetFlagsInNodeRange {Flags = flags, Grid = grid, LocalGridMin = min, LocalGridMax = max}.Schedule();
            }
        }*/

        /*[BurstCompile]
        public struct SetFlagsInBox : IJob
        {
            public NativeArray3D<GridNode> Array;
            public float4x4 ToGridLocalMatrix;
            public float4x4 ToGridWorldMatrix;
            public GridViewData ViewData;
            public BurstBoxCollider Box;
            public NodeFlags Flags;

            public void Execute()
            {
                // Translate the World space AABB into grid coordinates.
                var gridLocalMin = math.transform(ToGridLocalMatrix, Box.Bounds.min);
                var gridLocalMax = math.transform(ToGridLocalMatrix, Box.Bounds.max);
                var gridPointMin = GetNearestNodeIndices(Array, gridLocalMin, ViewData);
                var gridPointMax = GetNearestNodeIndices(Array, gridLocalMax, ViewData);

                for (var x = gridPointMin.x; x <= gridPointMax.x; x++)
                for (var y = gridPointMin.y; y <= gridPointMax.y; y++)
                for (var z = gridPointMin.z; z <= gridPointMax.z; z++)
                {
                    ref var node = ref Array[x, y, z];

                    // Move GridLocal point into world space for comparison
                    var worldNavigableCenter = math.transform(ToGridWorldMatrix, node.NavigableCenter);
                    if (Box.Contains(worldNavigableCenter))
                        node.Flags |= Flags;
                }
            }

            public int3 GetNearestNodeIndices(NativeArray3D<GridNode> array, Vector3 position, GridViewData viewData)
            {
                var gridX = ToGridDistance(position.x, viewData.BoxSize);
                var gridY = ToGridDistance(position.y, viewData.BoxSize);
                var gridZ = ToGridDistance(position.z, viewData.BoxSize);
                if (gridX < 0) gridX = 0;
                if (gridY < 0) gridY = 0;
                if (gridZ < 0) gridZ = 0;
                var xLen = array.Size.x;
                var yLen = array.Size.y;
                var zLen = array.Size.z;
                if (gridX > xLen) gridX = xLen;
                if (gridY > yLen) gridY = yLen;
                if (gridZ > zLen) gridZ = zLen;
                return new int3(gridX, gridY, gridZ);
            }

            public int ToGridDistance(float value, float boxSize)
            {
                return (int) math.round((value - boxSize / 2) / boxSize);
            }

            public static JobHandle Schedule(NavigationGrid grid, NodeFlags flags, BurstBoxCollider box)
            {
                return new SetFlagsInBox
                {
                    Flags = flags,
                    Array = grid.InnerGrid,
                    ViewData = grid.Data,
                    ToGridLocalMatrix = grid.Transform.ToLocalMatrix,
                    ToGridWorldMatrix = grid.Transform.ToWorldMatrix,
                    Box = box
                }.Schedule();
            }
        }*/

        /*public class IntersectionDiff
        {
            public static int3 GetNearestNodeIndices(NativeArray3D<GridNode> array, Vector3 position, GridViewData viewData)
            {
                var gridX = ToGridDistance(position.x, viewData.BoxSize);
                var gridY = ToGridDistance(position.y, viewData.BoxSize);
                var gridZ = ToGridDistance(position.z, viewData.BoxSize);
                if (gridX < 0) gridX = 0;
                if (gridY < 0) gridY = 0;
                if (gridZ < 0) gridZ = 0;
                var xLen = array.GetLength(0);
                var yLen = array.GetLength(1);
                var zLen = array.GetLength(2);
                if (gridX > xLen) gridX = xLen;
                if (gridY > yLen) gridY = yLen;
                if (gridZ > zLen) gridZ = zLen;
                return new int3(gridX, gridY, gridZ);
            }

            public static int ToGridDistance(float value, float boxSize)
            {
                return (int) math.round((value - boxSize / 2) / boxSize);
            }

            [BurstCompile]
            public struct SetFlagsInBoxDiff : IJob
            {
                public static int[] Run(NavigationGrid grid, NodeFlags flags, BurstBoxCollider boxCollider, int[] trackedIndices)
                {
                    if (trackedIndices == null)
                        trackedIndices = new int[0];

                    using (var previousIndices = new NativeArray<int>(trackedIndices, Allocator.TempJob))
                    using (var resultIndices = new NativeIndexList<GridNode>(grid.InnerGrid.Data.Value.Nodes.ToArray(), Allocator.TempJob))
                    {
                        new SetFlagsInBoxDiff
                        {
                            Flags = flags,
                            SourceGrid = grid.InnerGrid,
                            ViewData = grid.Data,
                            ToGridLocalMatrix = grid.Transform.ToLocalMatrix,
                            ToGridWorldMatrix = grid.Transform.ToWorldMatrix,
                            Box = boxCollider,
                            PreviouslyTrackedIndices = previousIndices,
                            TrackedIndicesResult = resultIndices
                        }.Schedule().Complete();
                        return resultIndices.IndicesToArray();
                    }
                }

                public NativeArray3D<GridNode> SourceGrid;
                public float4x4 ToGridLocalMatrix;
                public float4x4 ToGridWorldMatrix;
                public GridViewData ViewData;
                public BurstBoxCollider Box;
                public NativeArray<int> PreviouslyTrackedIndices;
                public NativeIndexList<GridNode> TrackedIndicesResult;
                public NodeFlags Flags;

                public void Execute()
                {
                    // The tracked nodes should be ones we previously assigned these exact flags to; now undo that work.        
                    for (var i = 0; i < PreviouslyTrackedIndices.Length; i++)
                    {
                        var idx = PreviouslyTrackedIndices[i];
                        ref var node = ref SourceGrid[idx];
                        node.Flags &= ~Flags;
                    }

                    // Translate the World space AABB into grid coordinates.
                    var gridLocalMin = math.transform(ToGridLocalMatrix, Box.Bounds.min);
                    var gridLocalMax = math.transform(ToGridLocalMatrix, Box.Bounds.max);

                    var gridPointMin = GetNearestNodeIndices(SourceGrid, gridLocalMin, ViewData);
                    var gridPointMax = GetNearestNodeIndices(SourceGrid, gridLocalMax, ViewData);

                    // The source grid may be large so to minimize work, only test nodes within the bounding box.
                    for (var x = gridPointMin.x; x <= gridPointMax.x; x++)
                    for (var y = gridPointMin.y; y <= gridPointMax.y; y++)
                    for (var z = gridPointMin.z; z <= gridPointMax.z; z++)
                    {
                        if (!SourceGrid.Data.Value.IsValid(x, y, z))
                            continue;

                        // Each node starts in GridLocal space, is moved to WorldSpace, then be compared with the BoxCollider.    
                        var idx = SourceGrid.Data.Value.GetIndex(x, y, z);
                        ref var node = ref SourceGrid[idx];
                        var worldNavigableCenter = math.transform(ToGridWorldMatrix, node.NavigableCenter);

                        // The BoxCollider internally converts the point into its own local space to make the collision test.
                        if (Box.Contains(worldNavigableCenter))
                        {
                            node.Flags |= Flags;
                            TrackedIndicesResult.AddIndex(idx);
                        }
                    }
                }
            }

            [BurstCompile]
            public struct SetFlagsInSphereDiff : IJob
            {
                public static int[] Complete(NavigationGrid grid, NodeFlags flags, BurstSphereCollider sphereCollider, int[] trackedIndices)
                {
                    if (trackedIndices == null)
                        trackedIndices = new int[0];

                    using (var previousIndices = new NativeArray<int>(trackedIndices, Allocator.TempJob))
                    using (var resultIndices = new NativeIndexList<GridNode>(grid.InnerGrid.Data.Value.Nodes.ToArray(), Allocator.TempJob))
                    {
                        new SetFlagsInSphereDiff
                        {
                            Flags = flags,
                            SourceGrid = grid.InnerGrid,
                            ViewData = grid.Data,
                            ToGridLocalMatrix = grid.Transform.ToLocalMatrix,
                            ToGridWorldMatrix = grid.Transform.ToWorldMatrix,
                            Sphere = sphereCollider,
                            PreviouslyTrackedIndices = previousIndices,
                            TrackedIndicesResult = resultIndices
                        }.Run();
                        return resultIndices.IndicesToArray();
                    }
                }

                public BurstSphereCollider Sphere;
                public NativeArray3D<GridNode> SourceGrid;
                public float4x4 ToGridLocalMatrix;
                public float4x4 ToGridWorldMatrix;
                public GridViewData ViewData;
                public NativeArray<int> PreviouslyTrackedIndices;
                public NativeIndexList<GridNode> TrackedIndicesResult;
                public NodeFlags Flags;

                public void Execute()
                {
                    // The tracked nodes should be ones we previously assigned these exact flags to; now undo that work.        
                    for (var i = 0; i < PreviouslyTrackedIndices.Length; i++)
                    {
                        var idx = PreviouslyTrackedIndices[i];
                        ref var node = ref SourceGrid[idx];
                        node.Flags &= ~Flags;
                    }

                    // Translate the World space AABB into grid coordinates.
                    var bounds = new Bounds(Sphere.Center + Sphere.Offset, (float3) Sphere.Radius * Sphere.Scale * 2);
                    var gridLocalMin = math.transform(ToGridLocalMatrix, bounds.min);
                    var gridLocalMax = math.transform(ToGridLocalMatrix, bounds.max);
                    var gridPointMin = GetNearestNodeIndices(SourceGrid, gridLocalMin, ViewData);
                    var gridPointMax = GetNearestNodeIndices(SourceGrid, gridLocalMax, ViewData);

                    // The source grid may be large so to minimize work, only test nodes within the bounding box.
                    for (var x = gridPointMin.x; x <= gridPointMax.x; x++)
                    for (var y = gridPointMin.y; y <= gridPointMax.y; y++)
                    for (var z = gridPointMin.z; z <= gridPointMax.z; z++)
                    {
                        // Each node starts in GridLocal space, is moved to WorldSpace, then be compared with the BoxCollider.    
                        var idx = SourceGrid.Data.Value.GetIndex(x, y, z);
                        ref var node = ref SourceGrid[idx];
                        var worldNavigableCenter = math.transform(ToGridWorldMatrix, node.NavigableCenter);

                        // The BoxCollider internally converts the point into its own local space to make the collision test.
                        if (Sphere.Contains(worldNavigableCenter))
                        {
                            node.Flags |= Flags;
                            TrackedIndicesResult.AddIndex(idx);
                        }
                    }
                }
            }
        }*/

        [BurstCompile]
        public unsafe struct TraceNavMesh : IJob
        {
            public NativeArray<NavMeshCustomExtensions.Edge> NavMeshEdges;
            public NavMeshQuery Query;
            //public float4x4 ToWorldMatrix;
            //public float4x4 ToLocalMatrix;
            //public NativeArray3D<GridNode> Nodes;
            public NodeFlags NearEdgeFlags;
            public NodeFlags WalkableFlags;
            public float EdgeWidth;
            public int RequireVerticalAlignment;
            public float ProximityThreshold;
            public NodeFlags DefaultFlags;
            
            public GridData GridData;

            public struct EdgeHit
            {
                public NavMeshCustomExtensions.Edge Edge;
                public float Distance;
                public Vector3 Position;
                public NavMeshCustomExtensions.Triangle Polygon;
            }

            public EdgeHit FindClosestEdge(NativeArray<NavMeshCustomExtensions.Edge> edges, Vector3 position)
            {
                var closestDist = float.MaxValue;
                var closestPoint = Vector3.zero;
                var closestEdge = NavMeshCustomExtensions.Edge.Empty;

                for (var i = 0; i < edges.Length; i++)
                {
                    var edge = edges[i];
                    var nearest = Math3d.ProjectPointOnLineSegment(edge.Start, edge.End, position);
                    var edgeDist = FastDistanceXZ(nearest, position);
                    if (edgeDist < closestDist)
                        if (Math.Abs(nearest.y - position.y) <= 0.5f)
                        {
                            closestPoint = nearest;
                            closestDist = edgeDist;
                            closestEdge = edge;
                        }
                }
                
                return new EdgeHit
                {
                    Distance = closestDist, 
                    Position = closestPoint, 
                    Edge = closestEdge
                };
            }

            public static float FastDistanceXZ(Vector3 a, Vector3 b)
            {
                var xD = a.x - b.x;
                var zD = a.z - b.z;
                return (xD < 0 ? -xD : xD) + (zD < 0 ? -zD : zD);
            }

            public void Execute()
            {
                for (var i = 0; i < GridData.BlobGrid->Nodes.Length; i++)
                {
                    ref var node = ref GridData.Nodes[i];
                    var nodeWorldPos = math.transform(GridData.ToWorldMatrix, (float3)node.Center);

                    if (DefaultFlags != 0)
                        GridData.Flags[i] = DefaultFlags;

                    var hit = Query.MapLocation(nodeWorldPos, node.Bounds.Extents, 0);
                    if (!Query.IsValid(hit) || hit.position == Vector3.zero)
                        continue;
                    
                    if (Math.Abs(hit.position.x - nodeWorldPos.x) < node.Bounds.Extents.x * 0.15 && Math.Abs(hit.position.z - nodeWorldPos.z) < node.Bounds.Extents.x * 0.15)
                    {
                        GridData.Flags[i] |= WalkableFlags;
                        node.NavigableCenter = math.transform(GridData.ToLocalMatrix, hit.position);

                        var edgeHit = FindClosestEdge(NavMeshEdges, hit.position);

                        var gridAdjustedEdgeDistance = edgeHit.Distance < node.Bounds.Extents.x * 0.95 ? edgeHit.Distance - node.Bounds.Extents.x : edgeHit.Distance;

                        if (gridAdjustedEdgeDistance <= EdgeWidth)
                            GridData.Flags[i] |= NearEdgeFlags;
                    }
                }
            }

            public static void Execute(GridData gridData, float proximity, float edgeWidth, bool alignVertical,
                NodeFlags walkableFlags, NodeFlags nearEdgeFlags, NodeFlags defaultFlags)
            {
                using (var query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 100))
                {
                    ref var edges = ref NavMeshCustomExtensions.GetNativeEdges();

                    var job = new TraceNavMesh
                    {
                        Query = query,
                        NavMeshEdges = edges,
                        GridData = gridData,
                        //ToLocalMatrix = grid.ToLocalMatrix,
                        //ToWorldMatrix = gridData.ToWorldMatrix,
                        WalkableFlags = walkableFlags,
                        NearEdgeFlags = nearEdgeFlags,
                        DefaultFlags = defaultFlags,
                        EdgeWidth = edgeWidth,
                        ProximityThreshold = proximity,
                        RequireVerticalAlignment = alignVertical ? 1 : 0,
                        //Nodes = gridData.Nodes
                    };
                    var jobHandle = job.Schedule();
                    NavMeshWorld.GetDefaultWorld().AddDependency(jobHandle);
                    jobHandle.Complete();
                }
            }


        }
    }
}