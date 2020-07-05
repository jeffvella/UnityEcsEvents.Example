using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Providers.Grid;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Vella.SimpleBurstCollision;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

[Serializable]
public struct VolumeModificationData
{
    public NodeFlags AddFlags;
    public NodeFlags RemoveFlags;
    public NodeFlags PresentRestriction;
    public NodeFlags AbsentRestriction;
}

[ExecuteInEditMode]
public unsafe class GridVolumeModifier : MonoBehaviour
{
    private readonly List<Vector3> _colliderPoints = new List<Vector3>();

    private GridNode _closestNodeToCenter;

    private void* _colliderValidationPtr;
    private List<GridNode> _nodesInRange = new List<GridNode>();
    private Stopwatch _stopwatch = new Stopwatch();
    private List<GridNode> _touchedNodes = new List<GridNode>();
    private int[] _trackedIndices;

    private BurstBoxCollider Box;

    public Color DebugColor = Color.yellow;

    public bool ExecuteInEditMode;
    //public NodeFlagSettingGroup NodeFlagsSettings;
    //public bool ShowNodeFlags;

    [Header("Setup")] public GridManager GridManager;

    public bool LogPerformance;

    public VolumeModificationData ModificationData;
    public bool ShowClosestNode;

    [Header("Debug Options")] public bool ShowPointsOnCollider;

    public bool ShowScannedNodes;

    public BlobAssetReference<Collider> NativeCollider { get; private set; }

    private void Awake()
    {
    }

    public BlobAssetReference<Collider> GenerateNativeCollider()
    {
        BlobAssetReference<Collider> result = default;

        var shapeAuthoring = GetComponent<PhysicsShapeAuthoring>();
        if (shapeAuthoring == null)
            return default;

        var t = transform;
        quaternion rot = t.rotation;
        float3 pos = t.position;

        switch (shapeAuthoring.ShapeType)
        {
            case ShapeType.Box:
            {
                var shape = shapeAuthoring.GetBoxProperties();
                shape.Center += pos; //transform.TransformPoint(shape.Center);
                shape.Orientation = math.mul(rot, shape.Orientation);
                shape.Size *= transform.localScale;
                result = BoxCollider.Create(shape);

                // {
                //     Center = transform.localPosition,
                //     Orientation = transform.localRotation,
                //     Size = transform.localScale,
                // });
                break;
            }
            case ShapeType.Sphere:
            {
                var shape = shapeAuthoring.GetSphereProperties(out var orientation);
                shape.Center += pos;
                shape.Radius = shape.Radius / 0.5f * math.cmax(transform.localScale) * 0.5f;
                result = SphereCollider.Create(shape);
                // {
                //     Center = transform.localPosition,
                //     Radius = math.cmax(transform.localScale)
                // });
                break;
            }
            case ShapeType.Capsule:
            {
                var capsuleAuthoring = shapeAuthoring.GetCapsuleProperties();
                var props = capsuleAuthoring.ToRuntime();
                props.Vertex0 = transform.TransformPoint(props.Vertex0); //math.mul(rot, props.Vertex0) + pos;
                props.Vertex1 = transform.TransformPoint(props.Vertex1); //math.mul(rot, props.Vertex1) + pos;
                props.Radius = props.Radius / 0.5f * math.cmax(transform.localScale) * 0.5f;
                result = CapsuleCollider.Create(props);
                break;
            }
        }

        return result;
    }

    private bool IsColliderValid()
    {
        if (_colliderValidationPtr == null) return false;
        return (long) _colliderValidationPtr == Unsafe.ReadUnaligned<long>(_colliderValidationPtr);
    }

    private void Update()
    {
        if (!ExecuteInEditMode && !Application.isPlaying)
            return;

        if (GridManager == null || !GridManager.IsValid)
            return;

        if (!isActiveAndEnabled || !transform.hasChanged)
            return;

        // Physics authoring can invalidate this blob if bad data is entered in the inspector;
        // Is will still show as created, but throw exceptions on internal validation.
        if (NativeCollider.IsCreated && IsColliderValid())
        {
            NativeCollider.GetUnsafePtr();
            NativeCollider.Dispose();
        }

        NativeCollider = GenerateNativeCollider();
        _colliderValidationPtr = NativeCollider.GetUnsafePtr();

        //var color = Selection.activeTransform == transform ? Color.clear : DebugColor;
        GridDebugDrawer.DrawCollider(NativeCollider, DebugColor);

        var gridComponent = new GridComponent {GridData = GridManager.Grid.InnerGrid.Data, Transform = new HeavyTransform(GridManager.transform), Size = new GridSize {BoxSize = GridManager.Grid.BoxSize, HalfBoxSize = GridManager.Grid.BoxSize / 2, BoxSizeQuotient = 1 / GridManager.Grid.BoxSize}};

        //GridDebugDrawer.DrawNodesInCollider(ref gridComponent, NativeCollider);

        GridDebugDrawer.ModifyFlagsInVolume(ref gridComponent, NativeCollider, ModificationData);

        // Box = BurstColliderFactory.CreateBox(transform);
        //
        // if (LogPerformance)
        // {
        //     _stopwatch.Restart();
        // }

        //var box = BurstColliderFactory.CreateBox(Collider as BoxCollider);

        // _closestNodeToCenter = GridManager.Grid.FindClosestNode(Box.Center);
        //
        // _nodesInRange.Clear();
        // _touchedNodes.Clear();
        //
        // if (GridManager.Grid.CollidesWithNode(Collider, _closestNodeToCenter, out float gapDistance, out Vector3 pointOnCollider))
        // {
        //     _trackedIndices = GridNodeJobs.AllNodes.IntersectionDiff.SetFlagsInBoxDiff.Run(GridManager.Grid, (ulong)AddFlags, box, _trackedIndices);
        // }
    }

    private void OnEnable()
    {
        NativeCollider = GenerateNativeCollider();
    }

    private void OnDisable()
    {
        if (GridManager == null)
            return;

        if (NativeCollider.IsCreated)
            NativeCollider.Dispose();

        //RestoreNodes(_touchedNodes);
    }

    // private void RestoreNodes(IList<GridNode> previousSet, IList<GridNode> newSet = null)
    // {
    //     var orphans = newSet != null ? previousSet.Except(newSet, GridPointComparer.Instance) : previousSet;
    //
    //     foreach (var node in orphans.ToList())
    //     {
    //         node.AddFlags(NodeFlags.AllowWalk);
    //         previousSet.Remove(node);
    //     }
    // }

    private void OnDrawGizmos()
    {
        if (!ExecuteInEditMode && !Application.isPlaying)
            return;

        // foreach (var index in _trackedIndices)
        // {
        //     var node = GridManager.Grid.InnerGrid[index];
        //     if (node.HasFlag(NodeFlags.Avoidance))
        //     {
        //         Gizmos.DrawWireSphere(node.Center, 0.1f);
        //     }
        // }

        DrawingHelpers.DrawWireFrame(Box);
    }

    /*public class GridPointComparer : IEqualityComparer<GridNode>
    {
        public static GridPointComparer Instance { get; } = new GridPointComparer();

        public bool Equals(GridNode x, GridNode y)
        {
            return x.GridPoint == y.GridPoint;
        }

        public int GetHashCode(GridNode obj)
        {
            unchecked
            {
                return obj.GridPoint.X.GetHashCode() + obj.GridPoint.Y.GetHashCode() + obj.GridPoint.Z.GetHashCode();
            }
        }
    }*/

//    void OnEnable()
//    {
//#if UNITY_EDITOR
//        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
//#endif
//    }

//#if UNITY_EDITOR
//    private void EditorApplication_playModeStateChanged(PlayModeStateChange state)
//    {
//        switch (state)
//        {
//            case PlayModeStateChange.ExitingEditMode:
//            case PlayModeStateChange.ExitingPlayMode:
//                EnsureDestroyed();
//                break;
//        }
//    }
//#endif

//    void OnDisable()
//    {
//#if UNITY_EDITOR
//        EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
//#endif
//        GridManager = null;
//    }
}