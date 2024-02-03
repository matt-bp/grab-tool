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
        [SerializeField] private GameObject[] thingsToUpdate;

        private readonly InputState _inputState = new();
        private Camera _camera;

        [Header("Mouse Input")] private MouseIndicatorState _mouseIndicatorState;
        [SerializeField] private GameObject mouseIndicatorPrefab;
        [SerializeField] private GameObject selectionMouseIndicator;
        private Vector3? _previousMousePosition;

        private bool doFixedUpdate;

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
                // var hitObject = mouseHit.Transform.gameObject;
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, rO);

                if (Input.GetMouseButtonDown(0))
                {
                    _inputState.StartTracking(worldSpacePosition);

                    Debug.Log("Start!");
                    // _trackingState.StartTracking(worldSpacePosition, hitObject, size, falloffCurve);

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
            if (!_previousMousePosition.HasValue)
            {
                _previousMousePosition = point;
                return;
            }

            _vectorField3D.C = point;
            _vectorField3D.DesiredTranslation = point - _previousMousePosition.Value;
            selectionMouseIndicator.transform.position = point;
            grid.transform.position = updateGridWithMouse ? point : Vector3.zero;
            doFixedUpdate = true;

            _previousMousePosition = point;
        }

        private void FixedUpdate()
        {
            if (!doFixedUpdate) return;

            UpdateGridVisualization();

            UpdateThingsToUpdate();

            doFixedUpdate = false;
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

        private void UpdateThingsToUpdate()
        {
            foreach (var particle in thingsToUpdate)
            {
                var position = particle.transform.position;
                var v = _vectorField3D.GetVelocity(position);

                if (v.magnitude == 0) continue;

                var d = Vector3.Magnitude(_vectorField3D.DesiredTranslation);
                var t = d / v.magnitude;

                // Debug.Log($"Time update is {t}");

                position += t * new Vector3(v.x, v.y, 0);

                particle.transform.position = position;
            }
        }

        private class InputState
        {
            public bool CurrentlyTracking { get; private set; }
            public Vector3 InitialPosition { get; private set; }

            // private readonly Queue<Vector3> _positionHistory = new();
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

            public Vector3 GetDesiredTranslation(Vector3 currentPosition)
            {
                if (_previousPosition == null)
                {
                    _previousPosition = currentPosition;
                    return currentPosition;
                }

                var direction = currentPosition - _previousPosition.Value;
                return direction;
            }
        }
    }
}