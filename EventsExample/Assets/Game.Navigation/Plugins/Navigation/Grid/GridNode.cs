using System;
using System.Runtime.CompilerServices;
using Navigation.Scripts.Region;
using Unity.Mathematics;
using UnityEngine;
using Vella.Events.Extensions;

namespace Providers.Grid
{
    public struct GridNode : IEquatable<GridNode>
    {
        // public float X;
        // public float Y;
        // public float Z;
        public Vector3 Center;
        public Vector3 NavigableCenter;
        public Vector3 Normal;
        public GridPoint GridPoint;
        public SimpleBounds Bounds;
        public Vector3 Size;
        //public NodeFlags Flags;

        public float GScore;
        public float HScore;
        public float FScore;
        public GridPoint ParentPoint;
        public int OpenId;
        public int ClosedId;

        //public float3 Position => new float3(X, Y, Z);

        public static bool operator ==(GridNode first, GridNode second) => first.Center == second.Center;

        public static bool operator !=(GridNode first, GridNode second) => first.Center != second.Center;
        
        public bool Equals(GridNode other) => GridPoint == other.GridPoint;

        public override int GetHashCode() => Center.GetHashCode();

        // public bool IsWalkable
        // {
        //     get => (Flags & NodeFlags.AllowWalk) != 0;
        //     //set => SetFlag(NodeFlags.AllowWalk, value);
        // }


 
        /*private void SetFlag(NodeFlags flag, bool value)
        {
            if (value)
                AddFlags(flag);
            else
                RemoveFlags(flag);
        }

        public bool AddFlags(NodeFlags flags)
        {
            if (flags != NodeFlags.None)
            {
                Flags |= flags;
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool HasFlag(NodeFlags flag)
        {
            return (Flags & flag) == flag;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool HasAnyFlag(NodeFlags flags)
        {
            return (Flags & flags) != 0;
        }
        
        public bool RemoveFlags(NodeFlags flags)
        {
            if (flags != NodeFlags.None)
            {
                Flags &= ~flags;
                return true;
            }

            return false;
        }

        public bool RemoveArea(NodeFlags flags)
        {
            if (flags != NodeFlags.None)
            {
                Flags &= ~flags;
                //Flags &= ~(int)flags;
                return true;
            }

            return false;
        }*/




    }
}