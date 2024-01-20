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

    private Vector3 _currentPointOnPlane;
    private bool _pointInTriangle;
    
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
            new( 1, 0, 0),
            new( 1, 1, 0),
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
        // var ray = _camera.ScreenPointToRay(Input.mousePosition);
        _pointInTriangle = false;

        var pointInQuestion = controllerProxy.transform.position;
        
        // Get point from controller proxy to plane defined by mesh
        var mesh = meshVisualization.sharedMesh;

        for (var i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v0 = mesh.vertices[mesh.triangles[i]];
            var v1 = mesh.vertices[mesh.triangles[i + 1]];
            var v2 = mesh.vertices[mesh.triangles[i + 2]];

            var temp = pointInQuestion - v0;
            
            Debug.DrawRay(v0, temp);

            var planeNormal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
            
            Debug.DrawRay(v0, planeNormal);

            var distance = Vector3.Dot(temp, planeNormal);

            var pointOnPlane = pointInQuestion - distance * planeNormal;

            _currentPointOnPlane = pointOnPlane;
            
            Debug.DrawLine(v0, pointInQuestion, Color.cyan);

            var pv0 = pointOnPlane - v0;
            var v1v0 = v1 - v0;
            var pv0xv1v0 = Vector3.Cross(pv0, v1v0);
            Debug.DrawRay(v0, pv0xv1v0, Color.magenta);
            
            var pv1 = pointOnPlane - v1;
            var v2v1 = v2 - v1;
            var pv1xv2v1 = Vector3.Cross(pv1, v2v1);
            Debug.DrawRay(v1, pv1xv2v1, Color.magenta);
            
            var pv2 = pointOnPlane - v2;
            var v0v2 = v0 - v2;
            var pv2xv0v2 = Vector3.Cross(pv2, v0v2);
            Debug.DrawRay(v2, pv2xv0v2, Color.magenta);
            
            var firstTwoParallel = Vector3.Dot(pv0xv1v0, pv1xv2v1) >= 0;
            var secondAndThirdParallel = Vector3.Dot(pv1xv2v1, pv2xv0v2) >= 0;

            // _pointInTriangle |= Intersections.PointInTriangle(pointInQuestion, v0, v1, v2);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _pointInTriangle ? Color.green : Color.red;
        Gizmos.DrawSphere(_currentPointOnPlane, 0.1f);
    }
}
