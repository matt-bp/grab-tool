using System;
using GrabTool.Math;
using UnityEditor;
using UnityEngine;

public class LineLineController : MonoBehaviour
{
    [SerializeField] private Vector3 thingToIntersectWith;
    [SerializeField] private Color thingToIntersectColor = Color.red;
    [SerializeField] private Color resultColor = Color.magenta;
    private Camera _camera;

    private float thingMultiplier = 1;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        var rayA = _camera.ScreenPointToRay(Input.mousePosition);
        var rayB = new Ray { origin = Vector3.zero, direction = thingToIntersectWith };
        
        Debug.DrawRay(Vector3.zero, thingToIntersectWith.normalized, thingToIntersectColor);

        var result = ClosestPoint.RayRay(rayA, rayB);

        thingMultiplier = result.t;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = resultColor;
        Gizmos.DrawSphere(thingToIntersectWith.normalized * thingMultiplier, 0.25f);
    }
}