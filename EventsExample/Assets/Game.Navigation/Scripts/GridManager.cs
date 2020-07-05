using System;
using System.Collections.Generic;
using System.Linq;
using Providers.Grid;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    // public struct NodeHandle
    // {
    //     private GridNode _node;
    //
    //     public NodeHandle(GridNode node)
    //     {
    //         _node = node;
    //     }
    //
    //     public Vector3 Position => _node.NavigableCenter;
    // }

    //public List<NodeHandle> NodeHandles = new List<NodeHandle>();
    //public HashSet<NodeHandle> SelectedHandles = new HashSet<NodeHandle>();

    private Vector3 _handlePos;

    private float3 _lastPosition;
    private Quaternion _lastRotation;
    private float3 _lastScale;
    public float BoxSize = 2.5f;

    [Header("Grid Options")] 
    public NodeFlags DefaultFlags = NodeFlags.None;
    public NodeFlags WalkableFlag = NodeFlags.AllowWalk;
    public NodeFlags NearEdgeFlag = NodeFlags.NearEdge;
    

    public bool DrawGridPoints = true;
    public float EdgeWidth = 0.2f;

    public Vector3 HandleOffset = new Vector3(0, 3, 0);

    public List<GridVolumeModifier> Obstacles;

    [Header("NavMeshTracing Options")] public float RequiredProximity = 0.5f;

    public bool RequireVerticalAlign = true;
    public bool ShowGridBounds = true;
    public Vector3 Size;

    public NavigationGrid Grid { get; set; }

    public bool IsValid => Grid != null && Grid.NodeCount > 0 && Grid.InnerGrid.IsCreated;

    private void Awake()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        if (BoxSize < 0.1f)
            BoxSize = 1;

        Debug.Log("Building Grid");

        Grid?.Dispose();
        
        Grid = NavigationGrid.Create((int) Size.x, (int) Size.y, (int) Size.z, transform.position, transform.rotation, transform.localScale, BoxSize);

        UpdateWalkableAreas();

        //RebuildNodeHandles();
    }

    private void RebuildNodeHandles()
    {
        /*if (DrawGridPoints && Grid != null)
        {
            NodeHandles.Clear();

            foreach (GridNode node in Grid.InnerGrid)
            {
                if (ShouldDraw(node, out Color color))
                {
                    var worldPosition = Grid.ToWorldPosition(node.NavigableCenter);
                    NodeHandles.Add(new NodeHandle(node, worldPosition));
                    DebugExtension.DebugPoint(worldPosition, color, 0.1f);
                }
            }
        }*/
    }

    // public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    // {
    //     return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    // }

    public void UpdateWalkableAreas()
    {
        _data = new GridData(this);
        
        GridNodeJobs.AllNodes.TraceNavMesh.Execute(_data, RequiredProximity, EdgeWidth, RequireVerticalAlign, WalkableFlag, NearEdgeFlag, DefaultFlags);
    }

    private unsafe void Update()
    {
        if (DrawGridPoints && Grid != null)
        {
            //var previouslySelected = new HashSet<NodeHandle>(SelectedHandles);
            //NodeHandles.Clear();
            //SelectedHandles.Clear();

            if (!Grid.InnerGrid.IsCreated)
                return;

            var flags = (NodeFlags*)Grid.InnerGrid.Data.Value.Flags.GetUnsafePtr();
            
            for (var i = 0; i < Grid.InnerGrid.Length; i++)
            {
                ref var node = ref Grid.InnerGrid[i];
                var flag = flags[i];
                
                DrawNode(node, flag, Grid.Transform._toWorldMatrix);
            }
        }

        if (Grid == null || math.any(_lastPosition != (float3) transform.position) || _lastRotation != transform.rotation || math.any(_lastScale != (float3) transform.localScale))
        {
            BuildGrid();
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
            _lastScale = transform.localScale;
        }
    }

    public NodeFlags TileNodeMask = NodeFlags.NearEdge | NodeFlags.AllowWalk;
    
    private GridData _data;
    
    private unsafe void DrawNode(GridNode node, NodeFlags flag, float4x4 toWorldMatrix)
    {
        Color color = UnityColors.Transparent;
        
        var worldPosition = math.transform(toWorldMatrix, node.Center);

        if (flag.HasFlag(NodeFlags.NearEdge))
        {
            color = UnityColors.Orange;
        }
        else if (flag.HasFlag(NodeFlags.AllowWalk))
        {
            color = UnityColors.DodgerBlue;
        }
        
        if (GridDebugDrawer.HasAnyFlag(ref flag, TileNodeMask))
        {
            GridDebug<TileNode>.Draw(node, worldPosition, color);
            return;
        }
        
        if (flag.HasFlag(NodeFlags.Avoidance))
        {
            color = UnityColors.Orange;
        }
        else if (flag.HasFlag(NodeFlags.Obstacle))
        {
            color = UnityColors.Black;
        }
        else if (flag.HasFlag(NodeFlags.NearEdge))
        {
            color = UnityColors.MintCream;
        }
        else if (flag.HasFlag(NodeFlags.Selected))
        {
            color = UnityColors.YellowGreen;
        }
        else return;
        
        GridDebug<DefaultNodeDrawer>.Draw(node, worldPosition, color);
    }

    public class GridDebug<T> where T : struct, IGridDrawer
    {
        private static T _instance;

        public static void Draw(GridNode node, float3 worldPosition, Color color)
        {
            _instance.Draw(node, worldPosition, color);
        }
    }
    
    public interface IGridDrawer
    {
        void Draw(GridNode node, float3 worldPosition, Color color);
    }
    
    public struct TileNode: IGridDrawer
    {
        public void Draw(GridNode node, float3 worldPosition, Color color)
        {
            DebugExtension.DebugCircle(worldPosition, color, 0.1f);
        }
    }

    public struct DefaultNodeDrawer : IGridDrawer
    {
        public void Draw(GridNode node, float3 worldPosition, Color color)
        {
            DebugExtension.DebugPoint(worldPosition, color, 0.05f);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (Grid != null)
        {
            Gizmos.matrix = Grid.Transform.ToWorldMatrix;

            if (ShowGridBounds)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(Grid.LocalDataBounds.center * Grid.BoxSize + Grid.BoxOffset, Grid.LocalDataBounds.size * Grid.BoxSize);
            }
        }
    }

    /*public struct NodeHandle : IHandleSelectable, IEquatable<NodeHandle>
    {
        public float3 WorldPosition { get; private set; }
        public float3 Position;
        public Color Color;
        
        public NodeHandle(GridNode node, Vector3 worldPosition, Color color) : this()
        {
            WorldPosition = worldPosition;
            Position = node.Position;
            Color = color;
            
            //Debug.Log($"Ctor [{Node.X},{Node.Y},{Node.Z}]");
        }

        public ref GridNode GetNode(NavigationGrid grid) => ref grid[Position];
        
        public void SelectNode(NavigationGrid grid) => grid[Position].AddFlags(NodeFlags.Selected);
        
        public void DeselectNode(NavigationGrid grid) => grid[Position].AddFlags(NodeFlags.Selected);
        
        public void Draw(NavigationGrid grid)
        {
            ref var nodeRef = ref grid[Position];

            var isSelected = nodeRef.HasFlag(NodeFlags.Selected);
            
            //MyHandles.DragHandle(WorldPosition, 0.1f, Handles.SphereHandleCap, isSelected, Color.green, Color, out var dhResult);
            
            // switch (dhResult)
            // {
            //     case MyHandles.DragHandleResult.LMBClick:
            //
            //         nodeRef.ToggleFlag(NodeFlags.Selected);
            //
            //         Debug.Log($"Clicked {nodeRef.HasFlag(NodeFlags.Selected)}");
            //         
            //         break;
            // }
        }

        //public float3 WorldPosition => WorldPosition;
        public bool Equals(NodeHandle other)
        {
            return WorldPosition.Equals(other.WorldPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return WorldPosition.GetHashCode();
        }
    }*/


    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void EditorApplication_playModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
            case PlayModeStateChange.ExitingPlayMode:
                EnsureDestroyed();
                break;
        }
    }
#endif

    private void OnDestroy()
    {
        EnsureDestroyed();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
#endif        
        EnsureDestroyed();
    }

    private void EnsureDestroyed()
    {
        Grid?.Dispose();
        NavMeshCustomExtensions.Dispose();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(GridManager))]
[CanEditMultipleObjects]
public class GridManager_Editor : Editor
{/*
    void OnSceneGUI()
    {
        // get the chosen game object
        GridManager t = target as GridManager;

        if( t == null || t.Handles == null )
            return;

        foreach(var handle in t.Handles)
        Handles.DrawLine(handle.Position, (float3)0);
        */
        // grab the center of the parent
        // Vector3 center = t.transform.position;
        //
        // // iterate over game objects added to the array...
        // for( int i = 0; i < t.GameObjects.Length; i++ )
        // {
        //     // ... and draw a line between them
        //     if( t.GameObjects[i] != null )
        //         Handles.DrawLine( center, t.GameObjects[i].transform.position );
        // }
    //}

    /*void OnSceneGUI()
    {
        // get the chosen game object
        DrawLine t = target as DrawLine;

        if( t == null || t.GameObjects == null )
            return;

        // grab the center of the parent
        Vector3 center = t.transform.position;

        // iterate over game objects added to the array...
        for( int i = 0; i < t.GameObjects.Length; i++ )
        {
            // ... and draw a line between them
            if( t.GameObjects[i] != null )
                Handles.DrawLine( center, t.GameObjects[i].transform.position );
        }
    }#1#
*/
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        foreach (var targ in targets.Cast<GridManager>())
        {
            var grid = targ.Grid;

            if (GUILayout.Button("Generate Grid") || grid == null)
                targ.BuildGrid();

            if (GUILayout.Button("Update Edges") || grid == null)
            {
                NavMeshCustomExtensions.CalculateEdges(NavMesh.CalculateTriangulation());
                SceneView.RepaintAll();
            }

            if (grid != null && grid.InnerGrid.IsCreated)
            {
                GUILayout.Label($"Nodes: {grid.InnerGrid.Length}");
                GUILayout.Label($"InnerArraySize: {grid.InnerGrid.Length}");
            }
        }
    }
}

#endif

public interface IHandleSelectable
{
    float3 WorldPosition { get; }
}

/*
public static class SelectionHandler<T> where T : IHandleSelectable, IEquatable<T>
{
    public delegate void SelectionEvent();

    private static int _selectionHandleHash = nameof(SelectionHandler<T>).GetHashCode();

    private static Rect _rect;
    public static bool IsSelecting { get; private set; }
    public static Vector2 StartPosition { get; private set; }
    public static Vector2 CurrentPosition { get; private set; }
    public static HashSet<T> Selections { get; } = new HashSet<T>();

    public static HashSet<T> Scan(GameObject source, IEnumerable<T> selectables)
    {
        if (Selection.activeGameObject != source)
            return Selections;

        var id = GUIUtility.GetControlID(_selectionHandleHash, FocusType.Passive);
        var eventType = Event.current.GetTypeForControl(id);

        switch (eventType)
        {
            case EventType.MouseDown:

                if (!IsSelecting && Event.current.button == 0)
                {
                    GUIUtility.hotControl = id;
                    Event.current.Use();
                    StartPosition = Event.current.mousePosition;
                    CurrentPosition = StartPosition;
                    IsSelecting = true;
                    Selections.Clear();
                }

                break;

            case EventType.MouseUp:

                if (GUIUtility.hotControl == id && Event.current.button == 0)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    IsSelecting = false;
                    _rect = default;
                }

                break;

            case EventType.MouseDrag:

                if (IsSelecting)
                {
                    CurrentPosition += new Vector2(Event.current.delta.x, Event.current.delta.y);

                    var screenDelta = CurrentPosition - StartPosition;
                    _rect = new Rect(StartPosition, screenDelta);

                    if (_rect.height < 0)
                    {
                        _rect.height = -_rect.height;
                        _rect.y -= _rect.height;
                    }

                    if (_rect.width < 0)
                    {
                        _rect.width = -_rect.width;
                        _rect.x -= _rect.width;
                    }

                    Select(selectables, _rect);

                    RefreshSceneViews();
                }

                break;

            case EventType.Repaint:

                if (IsSelecting)
                {
                    Handles.BeginGUI();
                    var prev = GUI.color;
                    GUI.color = Color.green;
                    GUI.Box(_rect, GUIContent.none);
                    GUI.color = prev;
                    Handles.EndGUI();
                    RefreshSceneViews();
                }

                break;
        }

        /*
        if (eventType == EventType.MouseDown && Event.current.button == 0 && !IsSelecting)
        {
            GUILayout
            StartPosition = Event.current.mousePosition;
            CurrentPosition = StartPosition;
            IsSelecting = true;
            Selections.Clear();
        }

        if (!IsSelecting)
            return Selections;
        
       var isMouseUp = GUIUtility.hotControl == 0;
       if (Event.current.isMouse && isMouseUp)
       {
           IsSelecting = false;
       }
       else
       {
           CurrentPosition += new Vector2(Event.current.delta.x, Event.current.delta.y);

           var screenDelta = CurrentPosition - StartPosition;
           var rect = new Rect(StartPosition, screenDelta);

           for (var i = 0; i < SceneView.sceneViews.Count; i++)
           {
               ((SceneView)SceneView.sceneViews[i]).Repaint();
           }

           if (rect.height < 0)
           {
               rect.height = -rect.height;
               rect.y -= rect.height;
           }
           if (rect.width < 0)
           {
               rect.width = -rect.width;
               rect.x -= rect.width;
           }
           
           Select(selectables, rect);
       }#1#

        return Selections;
    }

    /*private static void RefreshSceneViews()
    {
        //var currentId = EditorWindow.focusedWindow.GetInstanceID();
        for (var i = 0; i < SceneView.sceneViews.Count; i++)
        {
            var thisScene = (SceneView) SceneView.sceneViews[i];
            //if (thisScene.GetInstanceID() != currentId)
            thisScene.Repaint();
        }
    }

    private static void Select(IEnumerable<T> selectables, Rect rect)
    {
        foreach (var item in selectables)
        {
            var screenPoint = HandleUtility.WorldToGUIPoint(item.WorldPosition);

            if (rect.Contains(screenPoint))
            {
                if (!Selections.Contains(item))
                    Selections.Add(item);
            }
            else if (Selections.Contains(item))
            {
                Selections.Remove(item);
            }
        }
    }

    private static void DrawColoredScreenRect(Vector2 screenPoint, Vector2 size, Color color)
    {
        var rect = new Rect(screenPoint, size);
        Handles.BeginGUI();
        GUI.color = color;
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;
        Handles.EndGUI();
    }

    private static void DrawColoredScreenRect(Rect rect, Color color)
    {
        Handles.BeginGUI();
        GUI.color = color;
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;
        Handles.EndGUI();
    }#1#
}
*/

/*public class MyHandles
{
    public enum DragHandleResult
    {
        none = 0,

        LMBPress,
        LMBClick,
        LMBDoubleClick,
        LMBDrag,
        LMBRelease,

        RMBPress,
        RMBClick,
        RMBDoubleClick,
        RMBDrag,
        RMBRelease
    }

    // internal state for DragHandle()
    private static int s_DragHandleHash = "DragHandleHash".GetHashCode();
    private static Vector2 s_DragHandleMouseStart;
    private static Vector2 s_DragHandleMouseCurrent;
    private static Vector3 s_DragHandleWorldStart;
    private static float s_DragHandleClickTime;
    private static int s_DragHandleClickID;
    private static float s_DragHandleDoubleClickInterval = 0.5f;
    private static bool s_DragHandleHasMoved;

    // externally accessible to get the ID of the most resently processed DragHandle
    public static int lastDragHandleID;

    private static HashSet<int> _selectedIds = new HashSet<int>();
    private static Selection _currentSelection;

    public static Vector3 DragHandle(Vector3 position, float handleSize, Handles.CapFunction capFunc, bool isSelected, Color colorSelected, Color defaultColor, out DragHandleResult result)
    {
        var id = GUIUtility.GetControlID(s_DragHandleHash, FocusType.Passive);
        lastDragHandleID = id;

        var screenPosition = Handles.matrix.MultiplyPoint(position);
        var cachedMatrix = Handles.matrix;

        result = DragHandleResult.none;

        var eventType = Event.current.GetTypeForControl(id);

        switch (eventType)
        {
            case EventType.MouseDown:

                if (HandleUtility.nearestControl == id && (Event.current.button == 0 || Event.current.button == 1))
                {
                    GUIUtility.hotControl = id;
                    s_DragHandleMouseCurrent = s_DragHandleMouseStart = Event.current.mousePosition;
                    s_DragHandleWorldStart = position;
                    s_DragHandleHasMoved = false;

                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);

                    if (Event.current.button == 0)
                    {
                        result = DragHandleResult.LMBPress;

                        _selectedIds.Clear();
                        _selectedIds.Add(id);
                    }
                    else if (Event.current.button == 1)
                    {
                        result = DragHandleResult.RMBPress;
                    }
                }

                break;

            case EventType.MouseUp:

                if (GUIUtility.hotControl == id && (Event.current.button == 0 || Event.current.button == 1))
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);

                    if (Event.current.button == 0)
                        result = DragHandleResult.LMBRelease;
                    else if (Event.current.button == 1)
                        result = DragHandleResult.RMBRelease;

                    if (Event.current.mousePosition == s_DragHandleMouseStart)
                    {
                        var doubleClick = s_DragHandleClickID == id && Time.realtimeSinceStartup - s_DragHandleClickTime < s_DragHandleDoubleClickInterval;

                        s_DragHandleClickID = id;
                        s_DragHandleClickTime = Time.realtimeSinceStartup;

                        if (Event.current.button == 0)
                            result = doubleClick ? DragHandleResult.LMBDoubleClick : DragHandleResult.LMBClick;
                        else if (Event.current.button == 1)
                            result = doubleClick ? DragHandleResult.RMBDoubleClick : DragHandleResult.RMBClick;
                    }
                }

                break;

            case EventType.MouseDrag:

                if (GUIUtility.hotControl == id)
                {
                    s_DragHandleMouseCurrent += new Vector2(Event.current.delta.x, -Event.current.delta.y);
                    var position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(s_DragHandleWorldStart)) + (Vector3) (s_DragHandleMouseCurrent - s_DragHandleMouseStart);
                    position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2));

                    if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
                        position.z = s_DragHandleWorldStart.z;
                    if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
                        position.y = s_DragHandleWorldStart.y;
                    if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
                        position.x = s_DragHandleWorldStart.x;

                    if (Event.current.button == 0)
                        result = DragHandleResult.LMBDrag;
                    else if (Event.current.button == 1)
                        result = DragHandleResult.RMBDrag;

                    s_DragHandleHasMoved = true;

                    GUI.changed = true;
                    Event.current.Use();
                }

                //Debug.Log("Dragging Selection");

                break;

            case EventType.Repaint:
                var currentColour = Handles.color;

                //if (id == GUIUtility.hotControl) // && s_DragHandleHasMoved)

                if (_selectedIds.Contains(id))
                {
                    Handles.color = colorSelected;
                    Handles.DrawSolidDisc(position, Vector3.up, handleSize * 1.1f);
                    Handles.color = Color.black;
                    Handles.DrawWireDisc(position, Vector3.up, handleSize * 1.13f);
                }

                Handles.color = id == GUIUtility.hotControl ? colorSelected : defaultColor;

                Handles.matrix = Matrix4x4.identity;

                capFunc(id, screenPosition, Quaternion.identity, handleSize, EventType.Repaint);
                Handles.matrix = cachedMatrix;

                Handles.color = currentColour;
                break;

            case EventType.Layout:
                Handles.matrix = Matrix4x4.identity;
                HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, handleSize));
                Handles.matrix = cachedMatrix;
                break;
            case EventType.MouseMove:

                //Debug.Log("MouseMove");

                //Handles.

                break;
            case EventType.KeyDown:
                break;
            case EventType.KeyUp:
                break;
            case EventType.ScrollWheel:
                break;
            case EventType.DragPerform:

                Debug.Log("DragPerform");

                break;
            case EventType.DragExited:

                Debug.Log("DragExited");

                break;
            case EventType.Ignore:
                break;
            case EventType.Used:
                break;
            case EventType.ValidateCommand:
                break;
            case EventType.ExecuteCommand:
                break;
            case EventType.ContextClick:
                break;
            case EventType.MouseEnterWindow:
                break;
            case EventType.MouseLeaveWindow:
                break;
            case EventType.TouchDown:
                break;
            case EventType.TouchUp:
                break;
            case EventType.TouchMove:
                break;
            case EventType.TouchEnter:
                break;
            case EventType.TouchLeave:
                break;
            case EventType.TouchStationary:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return position;
    }
}*/