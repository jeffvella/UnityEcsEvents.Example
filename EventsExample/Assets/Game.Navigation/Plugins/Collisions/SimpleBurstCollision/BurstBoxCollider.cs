﻿using Unity.Mathematics;
using UnityEngine;

namespace Vella.SimpleBurstCollision
{
    public struct BurstBoxCollider : IBurstCollider
    {
        public float4x4 ToLocalMatrix;
        public float4x4 ToWorldMatrix;

        public Bounds Bounds;

        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public Vector3 Center;
        public Vector3 Max;
        public Vector3 Min;

        public Vector3 V1;
        public Vector3 V2;
        public Vector3 V3;
        public Vector3 V4;
        public Vector3 V5;
        public Vector3 V6;
        public Vector3 V7;
        public Vector3 V8;

        public static readonly BurstBoxCollider Empty = new BurstBoxCollider();
    }

    public static class BurstBoxColliderExtensions
    {
        public static void UpdateBounds(this BurstBoxCollider box)
        {
            box.Bounds = new Bounds();
            box.Bounds.Encapsulate(box.V1);
            box.Bounds.Encapsulate(box.V2);
            box.Bounds.Encapsulate(box.V3);
            box.Bounds.Encapsulate(box.V4);
            box.Bounds.Encapsulate(box.V5);
            box.Bounds.Encapsulate(box.V6);
            box.Bounds.Encapsulate(box.V7);
            box.Bounds.Encapsulate(box.V8);
        }

        public static Vector3 ClosestPosition(this BurstBoxCollider box, Vector3 worldPosition)
        {
            return ClosestPosition(box, worldPosition, box.ToLocalMatrix, box.ToWorldMatrix);
        }

        public static Vector3 ClosestPosition(this BurstBoxCollider box, Vector3 worldPosition, float4x4 toLocal, float4x4 toWorld)
        {
            var localSphereCenter = math.transform(toLocal, worldPosition);
            var closest = box.ClosestLocalPosition(localSphereCenter);
            return math.transform(toWorld, closest);
        }

        public static Vector3 ClosestPosition(this BurstBoxCollider box, Vector3 worldPosition, float4x4 toLocal)
        {
            var localPosition = math.transform(toLocal, worldPosition);
            var closest = box.ClosestLocalPosition(localPosition);
            return math.transform(math.inverse(box.ToWorldMatrix), closest);
        }

        private static Vector3 ClosestLocalPosition(this BurstBoxCollider box, float3 localPosition)
        {
            var closestX = Mathf.Max(box.Min.x, Mathf.Min(localPosition.x, box.Max.x));
            var closestY = Mathf.Max(box.Min.y, Mathf.Min(localPosition.y, box.Max.y));
            var closestZ = Mathf.Max(box.Min.z, Mathf.Min(localPosition.z, box.Max.z));
            var closest = new Vector3(closestX, closestY, closestZ);
            return closest;
        }

        public static bool Contains(this BurstBoxCollider box, Vector3 worldPosition)
        {
            var localPosition = box.GetLocalPosition(worldPosition);
            return localPosition.x > box.Min.x && localPosition.x < box.Max.x && localPosition.y > box.Min.y && localPosition.y < box.Max.y && localPosition.z > box.Min.z && localPosition.z < box.Max.z;
        }

        public static bool Contains(this BurstBoxCollider box, Vector3 worldPosition, float4x4 matrix)
        {
            var localPosition = math.transform(matrix, worldPosition);
            return localPosition.x > box.Min.x && localPosition.x < box.Max.x && localPosition.y > box.Min.y && localPosition.y < box.Max.y && localPosition.z > box.Min.z && localPosition.z < box.Max.z;
        }

        public static Vector3 GetWorldPosition(this BurstBoxCollider box, Vector3 localPosition)
        {
            return math.transform(box.ToWorldMatrix, localPosition);
        }

        public static Vector3 GetLocalPosition(this BurstBoxCollider box, Vector3 worldPosition)
        {
            return math.transform(box.ToLocalMatrix, worldPosition);
        }

        public static BurstBaseCollider ToBaseCollider(this BurstBoxCollider box)
        {
            return new BurstBaseCollider {Type = BurstColliderType.Box, Box = box};
        }
    }
}

/*
 *https://github.com/jeffvella/UnityNativeCollision/blob/master/UnityNativeCollision/Assets/Plugins/Vella.UnityNativeHull/Runtime/HullCollision.cs
 * 
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution. 
* https://en.wikipedia.org/wiki/Zlib_License
*/

/* Acknowledgments:
 * This work is derived from BounceLite by Irlan Robson (zLib License): 
 * https://github.com/irlanrobson/bounce_lite 
 * The optimized SAT and clipping is based on the 2013 GDC presentation by Dirk Gregorius 
 * and his forum posts about Valve's Rubikon physics engine:
 * https://www.gdcvault.com/play/1017646/Physics-for-Game-Programmers-The
 * https://www.gamedev.net/forums/topic/692141-collision-detection-why-gjk/?do=findComment&comment=5356490 
 * http://www.gamedev.net/topic/667499-3d-sat-problem/ 
 */

/*
using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Vella.Common;
using Debug = UnityEngine.Debug;

namespace Vella.UnityNativeHull
{
    public struct FaceQueryResult
    {
        public int Index;
        public float Distance;
    };

    public struct EdgeQueryResult
    {
        public int Index1;
        public int Index2;
        public float Distance;
    };

    public struct CollisionInfo
    {
        public bool IsColliding;
        public FaceQueryResult Face1;
        public FaceQueryResult Face2;
        public EdgeQueryResult Edge;
    }

    public class HullCollision
    {
        public static bool IsColliding(RigidTransform transform1, NativeHull hull1, RigidTransform transform2, NativeHull hull2)
        {
            FaceQueryResult faceQuery;

            QueryFaceDistance(out faceQuery, transform1, hull1, transform2, hull2);
            if (faceQuery.Distance > 0)
                return false;

            QueryFaceDistance(out faceQuery, transform2, hull2, transform1, hull1);
            if (faceQuery.Distance > 0)
                return false;

            QueryEdgeDistance(out EdgeQueryResult edgeQuery, transform1, hull1, transform2, hull2);
            if (edgeQuery.Distance > 0)
                return false;

            return true;
        }

        public static CollisionInfo GetDebugCollisionInfo(RigidTransform transform1, NativeHull hull1, RigidTransform transform2, NativeHull hull2)
        {
            CollisionInfo result = default;
            QueryFaceDistance(out result.Face1, transform1, hull1, transform2, hull2);
            QueryFaceDistance(out result.Face2, transform2, hull2, transform1, hull1);
            QueryEdgeDistance(out result.Edge, transform1, hull1, transform2, hull2);
            result.IsColliding = result.Face1.Distance < 0 && result.Face2.Distance < 0 && result.Edge.Distance < 0;
            return result;
        }

        public static unsafe void QueryFaceDistance(out FaceQueryResult result, RigidTransform transform1, NativeHull hull1, RigidTransform transform2, NativeHull hull2)
        {
            // Perform computations in the local space of the second hull.
            RigidTransform transform = math.mul(math.inverse(transform2), transform1);

            result.Distance = -float.MaxValue;
            result.Index = -1;

            for (int i = 0; i < hull1.FaceCount; ++i)
            {
                NativePlane plane = transform * hull1.GetPlane(i);
                float3 support = hull2.GetSupport(-plane.Normal);
                float distance = plane.Distance(support);

                if (distance > result.Distance)
                {
                    result.Distance = distance;
                    result.Index = i;
                }
            }
        }

        public static unsafe void QueryEdgeDistance(out EdgeQueryResult result, RigidTransform transform1, NativeHull hull1, RigidTransform transform2, NativeHull hull2)
        {
            // Perform computations in the local space of the second hull.
            RigidTransform transform = math.mul(math.inverse(transform2), transform1);

            float3 C1 = transform.pos;

            result.Distance = -float.MaxValue;
            result.Index1 = -1;
            result.Index2 = -1;

            for (int i = 0; i < hull1.EdgeCount; i += 2)
            {
                NativeHalfEdge* edge1 = hull1.GetEdgePtr(i);
                NativeHalfEdge* twin1 = hull1.GetEdgePtr(i + 1);

                Debug.Assert(edge1->Twin == i + 1 && twin1->Twin == i);

                float3 P1 = math.transform(transform, hull1.GetVertex(edge1->Origin));
                float3 Q1 = math.transform(transform, hull1.GetVertex(twin1->Origin));
                float3 E1 = Q1 - P1;

                float3 U1 = math.rotate(transform, hull1.GetPlane(edge1->Face).Normal);
                float3 V1 = math.rotate(transform, hull1.GetPlane(twin1->Face).Normal);

                for (int j = 0; j < hull2.EdgeCount; j += 2)
                {
                    NativeHalfEdge* edge2 = hull2.GetEdgePtr(j);
                    NativeHalfEdge* twin2 = hull2.GetEdgePtr(j + 1);

                    Debug.Assert(edge2->Twin == j + 1 && twin2->Twin == j);

                    float3 P2 = hull2.GetVertex(edge2->Origin);
                    float3 Q2 = hull2.GetVertex(twin2->Origin);
                    float3 E2 = Q2 - P2;
                   
                    float3 U2 = hull2.GetPlane(edge2->Face).Normal;
                    float3 V2 = hull2.GetPlane(twin2->Face).Normal;

                    if (IsMinkowskiFace(U1, V1, -E1, -U2, -V2, -E2))
                    {
                        float distance = Project(P1, E1, P2, E2, C1);
                        if (distance > result.Distance)
                        {
                            result.Index1 = i;
                            result.Index2 = j;
                            result.Distance = distance;
                        }
                    }
                }
            }
        }

        
        public static bool IsMinkowskiFace(float3 A, float3 B, float3 B_x_A, float3 C, float3 D, float3 D_x_C)
        {
            // If an edge pair doesn't build a face on the MD then it isn't a supporting edge.

            // Test if arcs AB and CD intersect on the unit sphere 
            float CBA = math.dot(C, B_x_A);
            float DBA = math.dot(D, B_x_A);
            float ADC = math.dot(A, D_x_C);
            float BDC = math.dot(B, D_x_C);

            return CBA * DBA < 0 &&
                   ADC * BDC < 0 &&
                   CBA * BDC > 0;
        }

        public static float Project(float3 P1, float3 E1, float3 P2, float3 E2, float3 C1)
        {
            // The given edge pair must create a face on the MD.

            // Compute search direction.
            float3 E1_x_E2 = math.cross(E1, E2);

            // Skip if the edges are significantly parallel to each other.
            float kTol = 0.005f;
            float L = math.length(E1_x_E2);
            if (L < kTol * math.sqrt(math.lengthsq(E1) * math.lengthsq(E2)))
            {
                return -float.MaxValue;
            }

            // Assure the normal points from hull1 to hull2.
            float3 N = (1 / L) * E1_x_E2;
            if (math.dot(N, P1 - C1) < 0)
            {
                N = -N;
            }

            // Return the signed distance.
            return math.dot(N, P2 - P1);
        }

        /// <summary>
        /// Determines if a world point is contained within a hull
        /// </summary>
        public static bool Contains(RigidTransform t, NativeHull hull, float3 point)
        {
            float maxDistance = -float.MaxValue;
            for (int i = 0; i < hull.FaceCount; ++i)
            {
                NativePlane plane = t * hull.GetPlane(i);
                float d = plane.Distance(point);
                if (d > maxDistance)
                {
                    maxDistance = d;
                }
            }
            return maxDistance < 0;
        }

        /// <summary>
        /// Finds the point on the surface of a hull closest to a world point.
        /// </summary>
        public static float3 ClosestPoint(RigidTransform t, NativeHull hull, float3 point)
        {
            float distance = -float.MaxValue;
            int closestFaceIndex = -1;
            NativePlane closestPlane = default;

            // Find the closest face plane.
            for (int i = 0; i < hull.FaceCount; ++i)
            {
                NativePlane plane = t * hull.GetPlane(i);
                float d = plane.Distance(point);
                if (d > distance)
                {
                    distance = d;
                    closestFaceIndex = i;
                    closestPlane = plane;
                }
            }

            var closestPlanePoint = closestPlane.ClosestPoint(point);
            if (distance > 0)
            {
                // Use a point along the closest edge if the plane point would be outside the face bounds.
                ref NativeFace face = ref hull.GetFaceRef(closestFaceIndex);
                ref NativeHalfEdge start = ref hull.GetEdgeRef(face.Edge);
                ref NativeHalfEdge current = ref start;
                do
                {
                    var v1 = math.transform(t, hull.GetVertex(current.Origin));
                    var v2 = math.transform(t, hull.GetVertex(hull.GetEdge(current.Twin).Origin));

                    var signedDistance = math.dot(math.cross(v2, v1), closestPlanePoint);
                    if (signedDistance < 0)
                    {
                        return MathUtility.ProjectPointOnLineSegment(v1, v2, point);
                    }
                    current = ref hull.GetEdgeRef(current.Next);
                }
                while (current.Origin != start.Origin);
            }
            return closestPlanePoint;
        }
    }

}
*/