using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace GrabTool.Mesh
{
    public class VRMeshDragger : MonoBehaviour
    {
        private readonly VRHoverStatus _hoverStatus = new();
        private readonly TrackingState _trackingState = new();
        private MeshHistory _history;
        private float _currentRadius;
        public float CurrentRadius => _currentRadius;

        [Header("Settings")] [SerializeField] private float minimumRadius = 0.1f;

        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve falloffCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        [Header("View")] [SerializeField] private GameObject colliderVisualization;

        [SerializeField] [Tooltip("The Input System Action that will be used to signify a grab.")]
        private InputActionProperty grabAction = new(new InputAction("Grab Action"));

        [SerializeField] [Tooltip("The Input System Action that will be used to signify undo.")]
        private InputActionProperty undoAction = new(new InputAction("Undo Action"));

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
            // _vrIndicatorState = new VRIndicatorState(colliderVisualization);
            _currentRadius = minimumRadius;
            UpdateRadiusUsages();
        }

        private void Update()
        {
            if (!_trackingState.CurrentlyTracking)
            {
                CheckForHoverAndStart();

                if (undoAction.action.WasPressedThisFrame() && undoAction.action.IsPressed() && _history != null)
                {
                    _history.Undo();

                    _trackingState.UpdateMeshes(_history.CurrentMesh.vertices);
                }

                return;
            }

            if (grabAction.action.IsPressed())
            {
                Debug.Log("Updating!");
                // _vrIndicatorState.Hide();

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
                // _vrIndicatorState.Hide();
                return;
            }

            // _vrIndicatorState.Show();

            // Check if user has initiated tracking by pressing the grab button on the controller.
            if (!(grabAction.action.WasPressedThisFrame() && grabAction.action.IsPressed())) return;

            Debug.Log("Start tracking!");

            if (_hoverStatus.HoveredGameObject == null || _hoverStatus.InteractorGameObject == null)
            {
                Debug.Log("No collider or interactor game object.");
                return;
            }

            Debug.Log("Started tracking, on the hunt...");

            _trackingState.StartTracking(_hoverStatus.InteractorGameObject.transform.position,
                _hoverStatus.HoveredGameObject, _currentRadius, falloffCurve);

            // Do history things
            if (_history is null)
            {
                Debug.Log("Starting history");
                _history = new MeshHistory(_hoverStatus.HoveredGameObject.GetComponent<MeshFilter>().sharedMesh);
            }
        }

        private void StopTracking()
        {
            Debug.Log("Stopped tracking");

            _trackingState.StopTracking();

            _history.AddMesh(_trackingState.LastMesh);
        }

        public void OnRadiusChanged(float value)
        {
            _currentRadius = System.Math.Max(value, minimumRadius);
            UpdateRadiusUsages();
        }

        private void UpdateRadiusUsages()
        {
            Debug.Log($"VRMeshDragger Radius: {_currentRadius}");
            _hoverStatus.InteractorGameObject.GetComponent<SphereCollider>().radius = _currentRadius;
            colliderVisualization.transform.localScale =
                2.0f * new Vector3(_currentRadius, _currentRadius, _currentRadius);
        }

        #region Event Listeners

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            Debug.Log("ClothInteractionPresenter.OnHoverEnter()");

            // Debug.Log($"Hit: {args.interactableObject.transform.gameObject.name}");
            // Debug.Log($"Hitter: {args.interactorObject.transform.gameObject.name}");
            Debug.Assert(args.interactableObject.colliders.Count == 1);

            _hoverStatus.StartHover(args.interactableObject.transform.gameObject,
                args.interactorObject.transform.gameObject);
        }

        public void OnHoverExit()
        {
            Debug.Log("ClothInteractionPresenter.OnHoverExit()");

            _hoverStatus.EndHover();
        }

        #endregion
    }

    // internal class VRIndicatorState
    // {
    //     private readonly GameObject _indicator;
    //
    //     public VRIndicatorState(GameObject indicator)
    //     {
    //         _indicator = indicator;
    //     }
    //
    //     public void Hide()
    //     {
    //         if (_indicator.activeSelf)
    //         {
    //             _indicator.SetActive(false);
    //         }
    //     }
    //
    //     public void Show()
    //     {
    //         _indicator.SetActive(true);
    //     }
    // }

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