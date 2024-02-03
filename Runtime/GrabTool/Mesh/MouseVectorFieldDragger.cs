using System;
using System.Collections.Generic;
using System.Linq;
using GrabTool.Math;
using GrabTool.Math.Integrators;
using GrabTool.Visualization;
using UnityEngine;
using UnityEngine.Assertions;

namespace GrabTool.Mesh
{
    [AddComponentMenu("Grab Tool/Dragger/Mouse Vector Field")]
    public class MouseVectorFieldDragger : MonoBehaviour
    {
        [Tooltip("The outer loop cutoff")] [SerializeField]
        private float rO;

        [Tooltip("The inner loop cutoff")] [SerializeField]
        private float rI;

        [Header("Vector grid visualization")] [SerializeField]
        private Grid3D grid;

        [SerializeField] private bool showGridVisualization;
        [SerializeField] private bool updateGridWithMouse;

        private readonly VectorField3D _vectorField3D = new();
        [SerializeField] private MeshFilter meshToCheckCollision;

        /// <summary>
        /// <para>I made this because with using the velocity update when Dot(v, desiredDir) is less than zero, it clumps meshes together.</para>
        /// <para>I could've implemented this wrong, but from what I'm seeing, ignoring the negative dot product velocities helps usability.</para>
        /// </summary>
        [Tooltip("When true, this makes it so the mesh is integrated with the whole vector field.")]
        [SerializeField] private bool useNegativeHemisphere;
        private readonly InputState _inputState = new();
        private Camera _camera;

        [Header("Mouse Input")] private MouseIndicatorState _mouseIndicatorState;
        [SerializeField] private GameObject mouseIndicatorPrefab;
        [SerializeField] private GameObject selectionMouseIndicator;

        private void Start()
        {
            Assert.IsFalse(System.Math.Abs(rO - rI) < float.Epsilon);
            Assert.IsTrue(rO != 0);
            Assert.IsTrue(rI != 0);

            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicatorPrefab, Vector3.zero, Quaternion.identity));
        }

        private void Update()
        {
            // What is a better way to do this?
            _vectorField3D.Ri = rI;
            _vectorField3D.Ro = rO;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);


            if (_inputState.CurrentlyTracking)
            {
                StillInTracking(ray);
            }
            else
            {
                CheckForMouseOverAndStart(ray);
            }
        }

        private void CheckForMouseOverAndStart(Ray ray)
        {
            if (MattMath.Raycast(ray, meshToCheckCollision, out var mouseHit))
            {
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, rO);

                if (Input.GetMouseButtonDown(0))
                {
                    _inputState.StartTracking(worldSpacePosition);

                    Debug.Log("Start!");

                    // if (_history is null)
                    // {
                    //     Debug.Log("Starting history");
                    //     _history = new MeshHistory(hitObject.GetComponent<MeshFilter>().sharedMesh);
                    // }
                }
            }
            else
            {
                // If we're not over the cloth, we for sure won't see anything
                _mouseIndicatorState.Hide();
            }
        }

        private void StillInTracking(Ray ray)
        {
            if (Input.GetMouseButton(0)) // If we are still pressing the mouse button down
            {
                _mouseIndicatorState.Hide();

                if (TryGetPointOnParallelToCameraPlane(ray, out var point))
                {
                    ProcessMouseMovement(point);
                }
            }

            if (Input.GetMouseButtonUp(0)) // If we stopped pressing the mouse button
            {
                Debug.Log("End!");
                _inputState.StopTracking();
            }
        }

        private bool TryGetPointOnParallelToCameraPlane(Ray ray, out Vector3 point)
        {
            var planeNormal = -_camera.transform.forward;
            var planeOrigin = _inputState.InitialPosition;

            Debug.DrawRay(planeOrigin, planeNormal * 2);

            if (Intersections.RayPlane(ray, planeOrigin, planeNormal, out var hit))
            {
                point = hit.Point;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private void ProcessMouseMovement(Vector3 point)
        {
            if (!_inputState.TryGetDesiredTranslation(point, out var direction))
            {
                return;
            }

            _vectorField3D.C = point;
            _vectorField3D.DesiredTranslation = direction;
            selectionMouseIndicator.transform.position = point;
            grid.transform.position = updateGridWithMouse ? point : Vector3.zero;

            UpdateGridVisualization();

            // UpdateThingsToUpdate();

            UpdateMesh();
        }

        private void UpdateGridVisualization()
        {
            if (showGridVisualization)
            {
                grid.enabled = true;
                foreach (var v in grid.Points.Select((v, i) => new { v, i }))
                {
                    var newPoint = grid.transform.TransformPoint(v.v);
                    var value = _vectorField3D.GetVelocity(newPoint);
                    grid.Velocities[v.i] = value;
                    grid.Colors[v.i] = _vectorField3D.wasInner ? Color.green : Color.red;
                }
            }
            else
            {
                grid.enabled = false;
            }
        }

        // Used to test with floating points.
        // [SerializeField] private GameObject[] thingsToUpdate;
        // private void UpdateThingsToUpdate()
        // {
        //     var total = new List<float>();
        //
        //     foreach (var particle in thingsToUpdate)
        //     {
        //         var position = particle.transform.position;
        //         var v = _vectorField3D.GetVelocity(position);
        //
        //         if (v.magnitude == 0) continue;
        //
        //         // If we know the desired translation, then we know a previous and current position,
        //         // which means we can get the time it took to move from previous to current, and use
        //         // that as our time step?
        //         // This make the speed of the mouse important, do I want that?
        //         // This is the same as getting the length between the current and previous point
        //         var d = Vector3.Magnitude(_vectorField3D.DesiredTranslation);
        //         // Alternatives, do adaptive time stepping, for a set amount of time?
        //         // var t = d / v.magnitude;
        //         var t = Mathf.Min(2, d / v.magnitude);
        //         // var t = Time.deltaTime; // This didn't work, the resulting velocity was too slow
        //         // Debug.Log($"Time update is {t}");
        //         total.Add(t);
        //
        //         position += t * v;
        //
        //         particle.transform.position = position;
        //     }
        //
        //     Debug.Log($"Time Step State: [Avg {total.Average()}], [Max {total.Max()}], [Min {total.Min()}]");
        // }

        private void UpdateMesh()
        {
            var mesh = meshToCheckCollision.sharedMesh;
            var meshTransform = meshToCheckCollision.gameObject.transform;

            var newWorldSpacePositions = new List<Vector3>();

            foreach (var position in mesh.vertices)
            {
                var worldSpacePosition = meshTransform.TransformPoint(position);

                var v = _vectorField3D.GetVelocity(worldSpacePosition);

                if (v.magnitude == 0)
                {
                    newWorldSpacePositions.Add(worldSpacePosition);
                    continue;
                }

                if (!useNegativeHemisphere && Vector3.Dot(v, _vectorField3D.DesiredTranslation) < 0)
                {
                    newWorldSpacePositions.Add(worldSpacePosition);
                    continue;
                }

                var distanceToGo = Vector3.Magnitude(_vectorField3D.DesiredTranslation);
                var dt = Mathf.Min(2.0f, distanceToGo / v.magnitude);

                worldSpacePosition += dt * v;

                newWorldSpacePositions.Add(worldSpacePosition);
            }

            var newPositions = newWorldSpacePositions.Select(meshTransform.InverseTransformPoint).ToArray();

            Assert.IsTrue(mesh.vertices.Length == newPositions.Length);
            
            MeshUpdater.UpdateMeshes(mesh, null, newPositions);
        }

        private class InputState
        {
            public bool CurrentlyTracking { get; private set; }
            public Vector3 InitialPosition { get; private set; }

            private Vector3? _previousPosition;

            public void StartTracking(Vector3 initialPosition)
            {
                CurrentlyTracking = true;
                InitialPosition = initialPosition;
            }

            public void StopTracking()
            {
                _previousPosition = null;
                CurrentlyTracking = false;
            }

            public bool TryGetDesiredTranslation(Vector3 currentPosition, out Vector3 direction)
            {
                direction = Vector3.zero;
                if (!_previousPosition.HasValue)
                {
                    _previousPosition = currentPosition;
                    return false;
                }

                direction = currentPosition - _previousPosition.Value;
                _previousPosition = currentPosition;
                return true;
            }
        }
    }
}