using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GrabTool.Math;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace GrabTool.Mesh
{
    [RequireComponent(typeof(Models.MeshHistory))]
    public class MouseMeshDragger : MonoBehaviour
    {
        [SerializeField] private float size = 0.2f;
        [SerializeField] private GameObject mouseIndicator;
        private MouseIndicatorState _mouseIndicatorState;
        private Camera _camera;
        private readonly TrackingState _trackingState = new();
        private Models.MeshHistory _history;
        [SerializeField] private MeshFilter[] meshesToCheckCollision;
        public UnityEvent onDragComplete;
        private bool _enabled;

        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve falloffCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        private void Start()
        {
            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity));
            _history = GetComponent<Models.MeshHistory>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_enabled) return;

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

                    _history.AddMesh(_trackingState.LastMesh);

                    onDragComplete.Invoke();
                }
            }
            else
            {
                CheckForMouseOverAndStart(ray);


#if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Z))
#else
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
#endif
                {
                    _history.Undo();

                    _trackingState.UpdateMeshes(_history.CurrentMesh.vertices);
                }
            }
        }

        private void CheckForMouseOverAndStart(Ray ray)
        {
            if (MattMath.Raycast(ray, meshesToCheckCollision.First(), out var mouseHit))
            {
                var hitObject = mouseHit.Transform.gameObject;
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, size);

                if (Input.GetMouseButtonDown(0))
                {
                    _trackingState.StartTracking(worldSpacePosition, hitObject, size, falloffCurve);

                    Debug.Log("Starting history");
                    _history.SetInitialMesh(hitObject.GetComponent<MeshFilter>().sharedMesh);
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

        public void SetEnabled(bool value)
        {
            _enabled = value;
        }

        private class MouseIndicatorState
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