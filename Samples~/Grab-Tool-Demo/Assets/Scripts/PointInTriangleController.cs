using System;
using System.Collections;
using System.Collections.Generic;
using GrabTool.Math;
using UnityEngine;

public class PointInTriangleController : MonoBehaviour
{
    [SerializeField] private MeshFilter meshVisualization;
    private Camera _camera;
    [SerializeField] private GameObject controllerProxy;

    // private Vector3 _currentPointOnPlane;
    // private bool _pointInTriangle;
    private List<(bool, Vector3)> info = new();

    // Start is called before the first frame update
    private void Start()
    {
        _camera = Camera.main;
        InitMesh();
    }

    private void InitMesh()
    {
        var mesh = new Mesh();
        var vertices = new Vector3[4]
        {
            new(-1, 0, 0),
            new(0, 1, 0),
            new(1, 0, 0),
            new(1, 1, 1),
        };
        mesh.vertices = vertices;

        var triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshVisualization.sharedMesh = mesh;
    }

    // Update is called once per frame
    private void Update()
    {
        info = new();
        var pointInQuestion = controllerProxy.transform.position;

        // Get point from controller proxy to plane defined by mesh
        var mesh = meshVisualization.sharedMesh;

        for (var i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v0 = mesh.vertices[mesh.triangles[i]];
            var v1 = mesh.vertices[mesh.triangles[i + 1]];
            var v2 = mesh.vertices[mesh.triangles[i + 2]];
            
            var closestPoint = Collisions.ClosestPointInTriangleToPoint(pointInQuestion, v0, v1, v2);
            
            info.Add((true, closestPoint));
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var i in info)
        {
            Gizmos.color = i.Item1 ? Color.green : Color.red;
            Gizmos.DrawSphere(i.Item2, 0.1f);
        }
    }
}