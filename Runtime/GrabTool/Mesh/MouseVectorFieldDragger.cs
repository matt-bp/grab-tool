using System.Collections.Generic;
using System.Linq;
using GrabTool.Math;
using GrabTool.Math.Integrators;
using UnityEngine;
using UnityEngine.Assertions;

namespace GrabTool.Mesh
{
    [AddComponentMenu("Grab Tool/Dragger/Mouse Vector Field")]
    public class MouseVectorFieldDragger : MonoBehaviour
    {
        [SerializeField] private GameObject mouseIndicator;
        private MouseIndicatorState _mouseIndicatorState;
        private Camera _camera;
        
        private MeshHistory _history;
        private readonly InputState _inputState = new();

        [Header("Vector Field Settings")]
        [SerializeField] private MeshFilter meshToCheckCollision;
        private VectorField3DIntegrator _integrator;
        [SerializeField] private float ri;
        [SerializeField] private float ro;
        // private UnityEngine.Mesh currentMesh => meshToCheckCollision.sharedMesh;
        // private MeshCollider meshCollider => meshToCheckCollision.gameObject.GetComponent<MeshCollider>();

        private void Start()
        {
            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity));
            _integrator = new VectorField3DIntegrator(ri, ro);
        }

        // Update is called once per frame
        private void Update()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_inputState.CurrentlyTracking)
            {
                // If the mouse button is still down and we're tracking
                if (Input.GetMouseButton(0))
                {
                    _mouseIndicatorState.Hide();

                    // I need that point in a plane parallel to the camera XY plane.
                    // - Get the camera normal, and reverse it
                    var planeNormal = -_camera.transform.forward;
                    // - Get the point that we started at (1st mouse down)
                    var point = _inputState.InitialPosition;

                    Debug.DrawRay(point, planeNormal * 2);

                    if (Intersections.RayPlane(ray, point, planeNormal, out var hit))
                    {
                        _integrator.C = hit.Point;
                        _integrator.DesiredTranslation = _inputState.GetDesiredTranslation(hit.Point);
                        
                        // Might need to convert to world space
                        var oldPositions = meshToCheckCollision.sharedMesh.vertices;
                        var newPositions = _integrator.Integrate(oldPositions);
                        Assert.IsTrue(oldPositions.Length == newPositions.Length);
                        MeshUpdater.UpdateMeshes(meshToCheckCollision.sharedMesh, null, newPositions);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _inputState.StopTracking();

                    _history.AddMesh(meshToCheckCollision.sharedMesh);
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
                    
                    MeshUpdater.UpdateMeshes(meshToCheckCollision.sharedMesh, null, _history.CurrentMesh.vertices);
                }
            }
        }

        private void CheckForMouseOverAndStart(Ray ray)
        {
            if (MattMath.Raycast(ray, meshToCheckCollision, out var mouseHit))
            {
                var hitObject = mouseHit.Transform.gameObject;
                var worldSpacePosition = mouseHit.Point;

                _mouseIndicatorState.Show();
                _mouseIndicatorState.UpdatePositionAndSize(worldSpacePosition, _integrator.Ro);

                if (Input.GetMouseButtonDown(0))
                {
                    _inputState.StartTracking(worldSpacePosition);
                    
                    //_trackingState.StartTracking(worldSpacePosition, hitObject, size, falloffCurve);

                    if (_history is null)
                    {
                        Debug.Log("Starting history");
                        _history = new MeshHistory(hitObject.GetComponent<MeshFilter>().sharedMesh);
                    }
                }
            }
            else
            {
                // If we're not over the cloth, we for sure wont see anything
                _mouseIndicatorState.Hide();
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

        private class MouseIndicatorState
        {
            private readonly GameObject _instance;
            private readonly LineRenderer _lineRenderer;

            public MouseIndicatorState(GameObject instance)
            {
                _instance = instance;
                _lineRenderer = instance.GetComponent<LineRenderer>();
            }

            public void UpdatePositionAndSize(Vector3 position, float size)
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