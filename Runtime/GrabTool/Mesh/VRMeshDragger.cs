using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace GrabTool.Mesh
{
    public class VRMeshDragger : MonoBehaviour
    {
        private readonly VRHoverStatus _hoverStatus = new();
        private readonly TrackingState _trackingState = new();
        private bool _disabled;
        private float _currentRadius;

        [Obsolete("Don't use this to increment the radius.")]
        public float CurrentRadius => _currentRadius;

        public int[] ConstantIndices => _trackingState.ConstantIndices;

        [Header("Models")] [SerializeField] private Models.MeshHistory history;

        [Header("Settings")] [SerializeField] private float minimumRadius = 0.1f;

        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve falloffCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        [SerializeField] private float constantUpperLimitMultiplier = 0.25f;

        [Header("View")] [SerializeField] private SphereCollider sphereCollider;

        public SphereCollider SphereCollider
        {
            get => sphereCollider;
            set => sphereCollider = value;
        }

        [SerializeField] private GameObject colliderVisualization;
        [SerializeField] private GameObject constantColliderVisualization;

        [SerializeField] [Tooltip("The Input System Action that will be used to signify a grab.")]
        private InputActionProperty grabAction = new(new InputAction("Grab Action"));

        [SerializeField] [Tooltip("The Input System Action that will be used to signify undo.")]
        private InputActionProperty undoAction = new(new InputAction("Undo Action"));

        public UnityEvent onDragComplete;

        protected void OnEnable()
        {
            grabAction.EnableDirectAction();
            undoAction.EnableDirectAction();
        }

        protected void OnDisable()
        {
            grabAction.DisableDirectAction();
            undoAction.DisableDirectAction();
        }

        private void Start()
        {
            _currentRadius = minimumRadius;
            UpdateRadiusUsages();
        }

        private void Update()
        {
            if (_disabled) return;

            if (!_trackingState.CurrentlyTracking)
            {
                CheckForHoverAndStart();

                if (undoAction.action.WasPressedThisFrame() && undoAction.action.IsPressed())
                {
                    history.Undo();

                    _trackingState.UpdateMeshes(history.CurrentMesh.vertices);
                }

                return;
            }

            if (grabAction.action.IsPressed())
            {
                Debug.Assert(_hoverStatus.InteractorGameObject != null,
                    "Need to have an interactor object (controller)");

                _trackingState.UpdateIndices(_hoverStatus.InteractorGameObject.transform.position);
            }
            else
            {
                StopTracking();
            }
        }

        private void CheckForHoverAndStart()
        {
            if (!_hoverStatus.Hovering)
            {
                return;
            }

            // Check if user has initiated tracking by pressing the grab button on the controller.
            if (!(grabAction.action.WasPressedThisFrame() && grabAction.action.IsPressed())) return;

            if (_hoverStatus.HoveredGameObject == null || _hoverStatus.InteractorGameObject == null)
            {
                Debug.Log("No collider or interactor game object.");
                return;
            }

            Debug.Log("Started tracking, on the hunt...");

            _trackingState.StartTracking(_hoverStatus.InteractorGameObject.transform.position,
                _hoverStatus.HoveredGameObject, _currentRadius, constantUpperLimitMultiplier, falloffCurve,
                Vector3.zero);

            // Do history things
            if (!history.NeedsCreated) return;

            Debug.Log("Starting history");
            history.SetInitialMesh(_hoverStatus.HoveredGameObject.GetComponent<MeshFilter>().sharedMesh);
        }

        private void StopTracking()
        {
            Debug.Log("Stopped tracking");

            _trackingState.StopTracking();

            history.AddMesh(_trackingState.LastMesh);

            onDragComplete.Invoke();
        }

        public void OnRadiusChanged(float value)
        {
            _currentRadius = System.Math.Max(value, minimumRadius);

            UpdateRadiusUsages();
        }

        private void UpdateRadiusUsages()
        {
            sphereCollider.radius = _currentRadius;

            colliderVisualization.transform.localScale =
                2.0f * new Vector3(_currentRadius, _currentRadius, _currentRadius);

            constantColliderVisualization.transform.localScale =
                constantUpperLimitMultiplier * colliderVisualization.transform.localScale;
        }

        public void SetDisabled(bool value)
        {
            _disabled = value;
        }

        #region Event Listeners

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            var interactor = args.interactorObject.transform.gameObject;
            // If we're tracking and this gets called again, that means another interactor has hovered over the mesh. No good!
            if (_trackingState.CurrentlyTracking && interactor != _hoverStatus.InteractorGameObject) return;

            // Debug.Log($"Hit: {args.interactableObject.transform.gameObject.name}");
            // Debug.Log($"Hitter: {args.interactorObject.transform.gameObject.name}");
            Debug.Assert(args.interactableObject.colliders.Count == 1);

            _hoverStatus.StartHover(args.interactableObject.transform.gameObject, interactor);
        }

        public void OnHoverExit()
        {
            _hoverStatus.EndHover();
        }

        #endregion
    }

    internal class VRHoverStatus
    {
        public bool Hovering { get; private set; }
        [CanBeNull] public GameObject HoveredGameObject { get; private set; }
        [CanBeNull] public GameObject InteractorGameObject { get; private set; }

        public void StartHover(GameObject hoveredObject, GameObject interactor)
        {
            Hovering = true;
            HoveredGameObject = hoveredObject;
            InteractorGameObject = interactor;
        }

        public void EndHover()
        {
            Hovering = false;
            HoveredGameObject = null;
        }
    }
}