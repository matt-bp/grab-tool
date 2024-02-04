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

        public float AdjustedRo => rO / rMultiplier;

        [Tooltip("The inner loop cutoff")] [SerializeField]
        private float rI;

        [Tooltip("Value to scale r by. Ri and Ro are based on the underlying function, where this values scales that function.")]
        [SerializeField] private float rMultiplier = 1.0f;
        
        private readonly VectorField3D _vectorField3D = new();
        [SerializeField] private MeshFilter meshToCheckCollision;
        private MeshHistory _history;

        /// <summary>
        /// <para>This makes velocity updates on points "behind" (-dot product) from our desired translation weaker.</para>
        /// <para>I made this because with using the velocity update when Dot(v, desiredDir) is less than zero, it clumps meshes together.</para>
        /// <para>I could've implemented this wrong, but from what I'm seeing in the planar mesh, the divergence free comes at a cost.</para>
        /// </summary>
        [SerializeField] private bool taperVelocityOnNegativeHemisphere;

        private readonly InputState _inputState = new();
        private Camera _camera;

        [Header("Mouse Input")] private MouseIndicatorState _mouseIndicatorState;
        [SerializeField] private GameObject mouseIndicatorPrefab;

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
            _vectorField3D.RMultiplier = rMultiplier;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);


            if (_inputState.CurrentlyTracking)
            {
                StillInTracking(ray);
            }
            else
            {
                CheckForMouseOverAndStart(ray);

                CheckForUndo();
            }
        }

        private void CheckForMouseOverAndStart(Ray ray)
        {
            if (MattMath.Raycast(ray, meshToCheckCollision, out var mouseHit))
            {
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, AdjustedRo);

                // We press the mouse button to start updating the mesh
                if (!Input.GetMouseButtonDown(0)) return;

                _inputState.StartTracking(worldSpacePosition);

                _history ??= new MeshHistory(meshToCheckCollision.sharedMesh);
            }
            else
            {
                // If we're not over the cloth, we for sure won't see anything
                _mouseIndicatorState.Hide();
            }
        }

        private void CheckForUndo()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
#else
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
#endif
            {
                _history.Undo();

                MeshUpdater.UpdateMeshes(meshToCheckCollision.sharedMesh, null, _history.CurrentMesh.vertices);
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

            // If we don't let go of the mouse button, we are still tracking!
            if (!Input.GetMouseButtonUp(0)) return;

            _inputState.StopTracking();
            _history.AddMesh(meshToCheckCollision.sharedMesh);
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
            
            // grid.transform.position = updateGridWithMouse ? point : Vector3.zero;
            // UpdateGridVisualization();

            // UpdateThingsToUpdate();

            UpdateMesh();
        }

        // [Header("Vector grid visualization")] [SerializeField]
        // private Grid3D grid;
        // [SerializeField] private bool showGridVisualization;
        // [SerializeField] private bool updateGridWithMouse;
        // private void UpdateGridVisualization()
        // {
        //     if (showGridVisualization)
        //     {
        //         grid.enabled = true;
        //         foreach (var v in grid.Points.Select((v, i) => new { v, i }))
        //         {
        //             var newPoint = grid.transform.TransformPoint(v.v);
        //             var value = _vectorField3D.GetVelocity(newPoint);
        //             grid.Velocities[v.i] = value;
        //             grid.Colors[v.i] = _vectorField3D.wasInner ? Color.green : Color.red;
        //         }
        //     }
        //     else
        //     {
        //         grid.enabled = false;
        //     }
        // }

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

                // Options:
                // - Let everything go through
                // - Let only points in front of you get updated
                //   - var directionToCurrentPoint = worldSpacePosition - _vectorField3D.C;
                // - Only velocities that aren't going away from the desired translation get updated
                //   - Use Dot(v, ...)
                // - Apply a scaling on velocities for points the other direction of where we want to translate.
                //   This would make them smaller. Or linearly interpolate between a min and max, so velocity is 0 
                //   when the point is "behind" us.
                // if (!useNegativeHemisphere && Vector3.Dot(v, _vectorField3D.DesiredTranslation) < 0)
                // {
                //     newWorldSpacePositions.Add(worldSpacePosition);
                //     continue;
                // }
                // if (!use)

                var modifier = 1.0f;
                if (taperVelocityOnNegativeHemisphere)
                {
                    if (!_vectorField3D.wasInner)
                    {
                        var directionToCurrentPoint = worldSpacePosition - _vectorField3D.C;
                        var dot = Vector3.Dot(directionToCurrentPoint, _vectorField3D.DesiredTranslation);

                        if (dot < 0.0f)
                        {
                            var modifiedDot = Mathf.Min(1.0f, Mathf.Abs(dot));
                            modifier = (1.0f - modifiedDot) / 8;
                        }
                    }
                    else
                    {
                        var directionToCurrentPoint = Vector3.Distance(worldSpacePosition, _vectorField3D.C);
                        

                    }
                }
                
                var distanceToGo = Vector3.Magnitude(_vectorField3D.DesiredTranslation);
                var dt = Mathf.Min(2.0f, distanceToGo / v.magnitude);

                worldSpacePosition += dt * modifier * v;

                newWorldSpacePositions.Add(worldSpacePosition);
            }

            var newPositions = newWorldSpacePositions.Select(meshTransform.InverseTransformPoint).ToArray();

            Assert.IsTrue(mesh.vertices.Length == newPositions.Length);

            MeshUpdater.UpdateMeshes(mesh, null, newPositions);
        }

        public void OnMultiplierChanged(float value)
        {
            rMultiplier = 1.0f / value;
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