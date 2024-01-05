using System.Collections.Generic;
using System.Linq;
using GrabTool.Math;
using UnityEngine;

namespace GrabTool.Mesh
{
    public class MouseMeshDragger : MonoBehaviour
    {
        [SerializeField] private float size = 0.2f;
        [SerializeField] private GameObject mouseIndicator;
        private MouseIndicatorState _mouseIndicatorState;
        private Camera _camera;
        private readonly TrackingState _trackingState = new();
        [SerializeField] private MeshFilter[] meshesToCheckCollision;

        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve dragSensitivityCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        private void Start()
        {
            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity));
        }

        // Update is called once per frame
        private void Update()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_trackingState.CurrentlyTracking)
            {
                // If the mouse button is still down and we're tracking
                if (Input.GetMouseButton(0))
                {
                    _mouseIndicatorState.Hide();

                    // I need that point in a plane parallel to the camera XY plane.
                    // - Get the camera normal, and reverse it
                    var planeNormal = -_camera.transform.forward;
                    // - Get the point that we started at (1st mouse down)
                    var point = _trackingState.InitialPosition;

                    Debug.DrawRay(point, planeNormal * 2);

                    if (Intersections.RayPlane(ray, point, planeNormal, out var hit))
                    {
                        _trackingState.UpdateIndices(hit.Point);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _trackingState.StopTracking();
                }
            }
            else
            {
                CheckForMouseOverAndStart(ray);
            }
        }

        private void CheckForMouseOverAndStart(Ray ray)
        {
            // // Will probably use something like this for VR
            // if (Physics.Raycast(ray, out var mouseHit, 1000, LayerMask.GetMask("MovableCloth")))
            // {
            //     var hitObject = mouseHit.transform.gameObject;
            //
            //     // Debug.Log($"Mouse is over: {hitObject.name}");
            //
            //     var worldSpacePosition = mouseHit.point;
            //
            //     _mouseIndicatorState.Show();
            //     _mouseIndicatorState.UpdatePosition(worldSpacePosition, size);
            //     
            //     if (Input.GetMouseButtonDown(0))
            //     {
            //         _trackingState.StartTracking(worldSpacePosition, hitObject, size);
            //         _startInstance.transform.position = worldSpacePosition;
            //     }
            // }
            // else
            // {
            //     // If we're not over the cloth, we for sure wont see anything
            //     _mouseIndicatorState.Hide();
            // }

            if (MattMath.Raycast(ray, meshesToCheckCollision.First(), out var mouseHit))
            {
                var hitObject = mouseHit.Transform.gameObject;
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, size);

                if (Input.GetMouseButtonDown(0))
                {
                    _trackingState.StartTracking(worldSpacePosition, hitObject, size, dragSensitivityCurve);
                }
            }
            else
            {
                // If we're not over the cloth, we for sure wont see anything
                _mouseIndicatorState.Hide();
            }
        }

        public void OnSizeChanged(float value)
        {
            size = value;
        }

        class TrackingState
        {
            public bool CurrentlyTracking { get; private set; }
            public Vector3 InitialPosition { get; private set; }
            private GameObject _hitObject;
            private UnityEngine.Mesh _meshToUpdate;
            private MeshCollider _meshCollider;
            private Dictionary<int, (Vector3 LocalPoint, float CloseRatio)> _indicesAndOriginalPositions;
            private AnimationCurve _falloff;

            public void StartTracking(Vector3 initialHitPosition, GameObject hitObject, float radius,
                AnimationCurve falloff)
            {
                CurrentlyTracking = true;
                InitialPosition = initialHitPosition;
                _hitObject = hitObject;
                _meshToUpdate = hitObject.GetComponent<MeshFilter>().sharedMesh;
                _meshCollider = hitObject.GetComponent<MeshCollider>();
                _falloff = falloff;

                // Make sure we have the valid range of [0, 1].
                Debug.Assert(_falloff.keys.Any(k => k.time >= 1));
                Debug.Assert(_falloff.keys.Any(k => k.time <= 0));

                float GetWorldSpaceDistance(Vector3 p) =>
                    Vector3.Distance(hitObject.transform.TransformPoint(p), initialHitPosition);

                _indicesAndOriginalPositions = _meshToUpdate.vertices
                    .Select((v, i) => new { v, i, closeRatio = GetWorldSpaceDistance(v) / radius })
                    .Where(x => x.closeRatio is >= 0 and <= 1)
                    .ToDictionary(x => x.i, x => (x.v, x.closeRatio));

                Debug.Log($"Finished starting tracking! Got {_indicesAndOriginalPositions.Count} indices.");
            }

            public void UpdateIndices(Vector3 worldMousePosition)
            {
                var localDelta = _hitObject.transform.InverseTransformVector(worldMousePosition - InitialPosition);
                var newPositions = _meshToUpdate.vertices;

                if (!_indicesAndOriginalPositions.Any()) return;

                foreach (var contents in _indicesAndOriginalPositions)
                {
                    Debug.Assert(contents.Value.CloseRatio is >= 0 and <= 1);

                    newPositions[contents.Key] = contents.Value.LocalPoint +
                                                 localDelta * _falloff.Evaluate(contents.Value.CloseRatio);
                }

                UpdateMeshes(newPositions);
            }

            private void UpdateMeshes(Vector3[] newPositions)
            {
                _meshToUpdate.vertices = newPositions;
                _meshToUpdate.RecalculateBounds();
                _meshToUpdate.RecalculateNormals();

                // Need to assign the mesh every frame to get intersections happening correctly.
                // See: https://forum.unity.com/threads/how-to-update-a-mesh-collider.32467/
                _meshCollider.sharedMesh = _meshToUpdate;
            }

            public void StopTracking()
            {
                CurrentlyTracking = false;
            }
        }

        class MouseIndicatorState
        {
            private readonly GameObject _instance;
            private readonly LineRenderer _lineRenderer;

            public MouseIndicatorState(GameObject instance)
            {
                _instance = instance;
                _lineRenderer = instance.GetComponent<LineRenderer>();
            }

            public void UpdatePosition(Vector3 position, float size)
            {
                _instance.transform.position = position;
                _instance.transform.localScale = new Vector3(size, size, size);
            }

            public void Hide()
            {
                if (_lineRenderer.enabled)
                {
                    _lineRenderer.enabled = false;
                }
            }

            public void Show()
            {
                _lineRenderer.enabled = true;
            }
        }
    }
}