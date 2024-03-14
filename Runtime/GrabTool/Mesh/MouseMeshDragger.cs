using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GrabTool.Math;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GrabTool.Mesh
{
    public class MouseMeshDragger : MonoBehaviour
    {
        [Tooltip("This is the minimum radius used for selecting the mesh.")] [SerializeField]
        private float minimumRadius = 0.1f;

        [SerializeField] private float constantUpperLimitMultiplier = 0.25f;
        [SerializeField] private Material constantIndicatorMaterial;
        [SerializeField] private GameObject mouseIndicator;
        [SerializeField] private Models.MeshHistory history;

        [SerializeField] private OnClickDragPlaneNormal onClickDragPlaneNormal;

        public Vector3 PlaneNormal => onClickDragPlaneNormal switch
        {
            OnClickDragPlaneNormal.Screen => -_camera.transform.forward,
            OnClickDragPlaneNormal.XY => Vector3.forward,
            OnClickDragPlaneNormal.YZ => Vector3.right,
            OnClickDragPlaneNormal.XZ => Vector3.up,
            OnClickDragPlaneNormal.Surface => _trackingState.SurfaceNormal,
            _ => throw new ArgumentOutOfRangeException()
        };

        public void SetPlaneNormalType(int value)
        {
            onClickDragPlaneNormal = (OnClickDragPlaneNormal)value;
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

        private void Start()
        {
            _camera = Camera.main;
            _mouseIndicatorState =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity), null);
            _constantMouseIndicator =
                new MouseIndicatorState(Instantiate(mouseIndicator, Vector3.zero, Quaternion.identity),
                    constantIndicatorMaterial);

            _currentRadius = minimumRadius;
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

                    // Find closest point on a ray to another ray.

                    var point = _trackingState.InitialPosition;

                    Debug.DrawRay(point, PlaneNormal * 2, Color.green);

                    if (Intersections.RayPlane(ray, point, PlaneNormal, out var hit))
                    {
                        _trackingState.UpdateIndices(hit.Point);
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

        public enum OnClickDragPlaneNormal
        {
            Screen,
            XY,
            YZ,
            XZ,
            Surface
        }
    }
}