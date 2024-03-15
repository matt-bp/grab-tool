using System;
using System.Linq;
using GrabTool.Math;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GrabTool.Mesh
{
    public class MouseMeshDragger : MonoBehaviour
    {
        private const float KMouseSensitivityMultiplier = 0.01f;
        
        [Tooltip("This is the minimum radius used for selecting the mesh.")] [SerializeField]
        private float minimumRadius = 0.1f;

        [SerializeField] private float constantUpperLimitMultiplier = 0.25f;
        [SerializeField] private Material constantIndicatorMaterial;
        [SerializeField] private GameObject mouseIndicator;
        [SerializeField] private Models.MeshHistory history;

        [SerializeField] private OnClickDrag onClickDrag;

        public Vector3 PlaneNormal => onClickDrag switch
        {
            OnClickDrag.PlaneScreen => -_camera.transform.forward,
            OnClickDrag.PlaneXY => Vector3.forward,
            OnClickDrag.PlaneYZ => Vector3.right,
            OnClickDrag.PlaneXZ => Vector3.up,
            OnClickDrag.PlaneSurface => _trackingState.SurfaceNormal,
            _ => throw new ArgumentOutOfRangeException()
        };

        public void SetPlaneNormalType(int value)
        {
            onClickDrag = (OnClickDrag)value;
        }

        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve falloffCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        [SerializeField] private MeshFilter[] meshesToCheckCollision;
        public UnityEvent onDragComplete;

        private MouseIndicatorState _mouseIndicatorState;
        private MouseIndicatorState _constantMouseIndicator;
        private Camera _camera;
        private readonly TrackingState _trackingState = new();
        private bool _disabled;
        private bool _stoppedByCameraMovement;
        private float _currentRadius;
        private float ConstantRadius => constantUpperLimitMultiplier * _currentRadius;
        public int[] ConstantIndices => _trackingState.ConstantIndices;

        public Vector3 CurrentSurfaceNormal => _trackingState.SurfaceNormal;
        
        [Tooltip("Multiplier for the slide sensitivity for moving a mesh along a vector.")]
        public float slideSensitivity = 60.0f;
        
#if ENABLE_INPUT_SYSTEM
        private InputAction _mouseAction;
#endif

        private void Start()
        {
            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity), null);
            _constantMouseIndicator =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity),
                    constantIndicatorMaterial);

            _currentRadius = minimumRadius;
            
#if ENABLE_INPUT_SYSTEM
            var map = new InputActionMap("Unity Camera Controller");

            _mouseAction = map.AddAction("look", binding: "<Mouse>/delta");

            _mouseAction.Enable();
#endif
        }

        // Update is called once per frame
        private void Update()
        {
            if (_disabled) return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (_trackingState.CurrentlyTracking)
            {
                // If the mouse button is still down and we're tracking
                if (Input.GetMouseButton(0))
                {
                    _mouseIndicatorState.Hide();
                    _constantMouseIndicator.Hide();

                    if (onClickDrag is OnClickDrag.VectorSurface or OnClickDrag.VectorCamera)
                    {
                        UpdateIndicesBasedOnVector(ray);
                    }
                    else
                    {
                        UpdateIndicesBasedOnPlane(ray);
                    }
                }

                if (Input.GetMouseButtonUp(0) || _stoppedByCameraMovement)
                {
                    _trackingState.StopTracking();

                    history.AddMesh(_trackingState.LastMesh);

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
                    history.Undo();

                    _trackingState.UpdateMeshes(history.CurrentMesh.vertices);
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
                _mouseIndicatorState.UpdatePosition(worldSpacePosition, _currentRadius);

                _constantMouseIndicator.Show();
                _constantMouseIndicator.UpdatePosition(worldSpacePosition, ConstantRadius);

                // If we haven't clicked the mouse button while over the mesh, we won't start!
                if (!Input.GetMouseButtonDown(0)) return;

                _t = 0.0f;

                _trackingState.StartTracking(worldSpacePosition, hitObject, _currentRadius,
                    constantUpperLimitMultiplier, falloffCurve, mouseHit.Normal);

                if (!history.NeedsCreated) return;

                Debug.Log("Starting history");
                history.SetInitialMesh(hitObject.GetComponent<MeshFilter>().sharedMesh);
            }
            else
            {
                // If we're not over the cloth, we for sure wont see anything
                _mouseIndicatorState.Hide();
                _constantMouseIndicator.Hide();
            }
        }

        // private Vector3 _rayRayDifference = Vector3.zero;
        private float _t;
        private void UpdateIndicesBasedOnVector(Ray screenRay)
        {
            if (onClickDrag == OnClickDrag.VectorCamera)
            {
                var normal = _camera.transform.position - _trackingState.InitialPosition;
                var surfaceRay = new Ray(_trackingState.InitialPosition, normal);

                var mouseMovement = GetMouseMovement() * (KMouseSensitivityMultiplier * slideSensitivity);
                var delta = -(mouseMovement.y - mouseMovement.x);
                _t += delta;

                var worldPositionAlongSurfaceNormal = surfaceRay.GetPoint(_t);
            
                _trackingState.UpdateIndices(worldPositionAlongSurfaceNormal);
            }
            else
            {
                var surfaceRay = new Ray(_trackingState.InitialPosition, _trackingState.SurfaceNormal);
                Debug.DrawRay(surfaceRay.origin, surfaceRay.direction, Color.red);

                var (_, t) = ClosestPoint.RayRay(screenRay, surfaceRay);

                var worldPositionAlongSurfaceNormal = surfaceRay.GetPoint(t);
            
                _trackingState.UpdateIndices(worldPositionAlongSurfaceNormal);
            }
        }
        
        private Vector2 GetMouseMovement()
        {
            // try to compensate the diff between the two input systems by multiplying with empirical values
#if ENABLE_INPUT_SYSTEM
            var delta = _mouseAction.ReadValue<Vector2>();
            delta *= 0.5f; // Account for scaling applied directly in Windows code by old input system.
            delta *= 0.1f; // Account for sensitivity setting on old Mouse X and Y axes.
            return delta;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        private void UpdateIndicesBasedOnPlane(Ray screenRay)
        {
            var point = _trackingState.InitialPosition;

            Debug.DrawRay(point, PlaneNormal * 2, Color.green);

            if (Intersections.RayPlane(screenRay, point, PlaneNormal, out var hit))
            {
                _trackingState.UpdateIndices(hit.Point);
            }
        }

        public void OnSizeChanged(float value)
        {
            _currentRadius = Mathf.Max(value, minimumRadius);
        }

        public void SetDisabled(bool value)
        {
            _disabled = value;
        }

        public void SetStoppedByCameraMovement(bool value)
        {
            _stoppedByCameraMovement = value;
        }

        private class MouseIndicatorState
        {
            private readonly GameObject _instance;
            private readonly LineRenderer _lineRenderer;

            public MouseIndicatorState(GameObject instance, [CanBeNull] Material material)
            {
                _instance = instance;
                _lineRenderer = instance.GetComponent<LineRenderer>();

                if (material != null)
                {
                    _lineRenderer.material = material;
                }
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

        private enum OnClickDrag
        {
            PlaneScreen,
            PlaneXY,
            PlaneYZ,
            PlaneXZ,
            PlaneSurface,
            VectorSurface,
            VectorCamera
        }
    }
}