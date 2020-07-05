﻿using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class DrawBoundsGizmo : MonoBehaviour
{
    public Color ActualBoxColor = UnityColors.DodgerBlue;

    [Header("Colors")] public Color BoundingBoxColor = Color.black;

    public bool ShowActualBox = true;

    [Header("Options")] public bool ShowBoundingBox = true;

    public bool ShowDuringPlay = true;
    public bool ShowInEditor = true;

    private void OnDrawGizmos()
    {
        if (ShowDuringPlay && Application.isPlaying || ShowInEditor && !Application.isPlaying)
        {
            var collider = GetComponent<Collider>();
            if (collider == null) return;

            if (ShowBoundingBox)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }

            if (ShowActualBox)
            {
                var boxCollider = collider as BoxCollider;
                if (boxCollider != null)
                {
                    Gizmos.color = BoundingBoxColor;
                    DrawCube(boxCollider.center, boxCollider.size / 2, transform.localToWorldMatrix);

                    //NativeDebugger.Log(0, "Test");
                    //NativeDebugger.Log(0, "Test2");
                }
            }
        }
    }

    public static void DrawCube(Vector3 center, Vector3 extents)
    {
        var points = GetTransformedPoints(center, extents);
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[3]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[2], points[0]);
        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[7]);
        Gizmos.DrawLine(points[6], points[7]);
        Gizmos.DrawLine(points[6], points[4]);
        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[1], points[5]);
        Gizmos.DrawLine(points[2], points[6]);
        Gizmos.DrawLine(points[3], points[7]);
    }

    public static Vector3[] GetTransformedPoints(Vector3 center, Vector3 extents)
    {
        return new[] {center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, extents.y, -extents.z), center + new Vector3(extents.x, -extents.y, extents.z), center + new Vector3(extents.x, -extents.y, -extents.z), center + new Vector3(-extents.x, extents.y, extents.z), center + new Vector3(-extents.x, extents.y, -extents.z), center + new Vector3(-extents.x, -extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z)};
    }

    public static void DrawCube(Vector3 center, Vector3 extents, float4x4 matrix)
    {
        var points = GetTransformedPoints(center, extents, matrix);
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[3]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[2], points[0]);
        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[7]);
        Gizmos.DrawLine(points[6], points[7]);
        Gizmos.DrawLine(points[6], points[4]);
        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[1], points[5]);
        Gizmos.DrawLine(points[2], points[6]);
        Gizmos.DrawLine(points[3], points[7]);
    }

    public static float3[] GetTransformedPoints(Vector3 center, Vector3 extents, float4x4 matrix)
    {
        return new[] {math.transform(matrix, center + new Vector3(extents.x, extents.y, extents.z)), math.transform(matrix, center + new Vector3(extents.x, extents.y, -extents.z)), math.transform(matrix, center + new Vector3(extents.x, -extents.y, extents.z)), math.transform(matrix, center + new Vector3(extents.x, -extents.y, -extents.z)), math.transform(matrix, center + new Vector3(-extents.x, extents.y, extents.z)), math.transform(matrix, center + new Vector3(-extents.x, extents.y, -extents.z)), math.transform(matrix, center + new Vector3(-extents.x, -extents.y, extents.z)), math.transform(matrix, center + new Vector3(-extents.x, -extents.y, -extents.z))};
    }
}