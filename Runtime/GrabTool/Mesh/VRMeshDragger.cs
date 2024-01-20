using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GrabTool.Mesh
{
    public class VRMeshDragger : MonoBehaviour
    {
        [SerializeField] private GameObject colliderVisualization;

        // private VRIndicatorState _vrIndicatorState;

        private readonly VREventStatus _eventStatus = new();
        private readonly TrackingState _trackingState = new();
        private MeshHistory _history;
        [SerializeField] private float radius = 0.1f;
        public float CurrentRadius => radius;
        
        [Tooltip("X = Radius percentage distance from hit point.\nY = Strength of offset.")]
        public AnimationCurve falloffCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));

        private void Start()
        {
            // _vrIndicatorState = new VRIndicatorState(colliderVisualization);
            
            UpdateRadiusUsages();
        }

        private void Update()
        {
            if (!_trackingState.CurrentlyTracking)
            {
                CheckForHoverAndStart();

                if (_eventStatus.UndoPressedThisFrame && _history != null)
                {
                    // Be sure to "consume" input
                    _eventStatus.UseUndo();
                    
                    _history.Undo();
                    
                    _trackingState.UpdateMeshes(_history.CurrentMesh.vertices);
                }
                
                return;
            }

            if (_eventStatus.GrabPressed)
            {
                Debug.Log("Updating!");
                // _vrIndicatorState.Hide();

                Debug.Assert(_eventStatus.InteractorGameObject != null, "Need to have an interactor object (controller)");
                
                _trackingState.UpdateIndices(_eventStatus.InteractorGameObject.transform.position);
            }
            else
            {
                StopTracking();
            }
        }

        private void CheckForHoverAndStart()
        {
            if (!_eventStatus.Hovering)
            {
                // _vrIndicatorState.Hide();
                return;
            }

            // _vrIndicatorState.Show();


            // Check if user has initiated tracking by pressing the grab button on the controller.
            if (!_eventStatus.GrabPressed) return;
            
            Debug.Log("Start tracking!");

            if (_eventStatus.HoveredGameObject == null || _eventStatus.InteractorGameObject == null)
            {
                Debug.Log("No collider or interactor game object.");
                return;
            }

            Debug.Log("Started tracking, on the hunt...");

            _trackingState.StartTracking(_eventStatus.InteractorGameObject.transform.position,
                _eventStatus.HoveredGameObject, radius, falloffCurve);

            // Do history things
            if (_history is null)
            {
                Debug.Log("Starting history");
                _history = new MeshHistory(_eventStatus.HoveredGameObject.GetComponent<MeshFilter>().sharedMesh);
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
            radius = value;
            UpdateRadiusUsages();
        }

        private void UpdateRadiusUsages()
        {
            // Debug.Log($"Radius: {radius}");
            _eventStatus.InteractorGameObject.GetComponent<SphereCollider>().radius = radius;
            colliderVisualization.transform.localScale = new Vector3(radius * 2.0f, radius * 2.0f, radius * 2.0f);
        }

        #region Event Listeners

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            Debug.Log("ClothInteractionPresenter.OnHoverEnter()");

            // Debug.Log($"Hit: {args.interactableObject.transform.gameObject.name}");
            // Debug.Log($"Hitter: {args.interactorObject.transform.gameObject.name}");
            Debug.Assert(args.interactableObject.colliders.Count == 1);

            _eventStatus.StartHover(args.interactableObject.transform.gameObject,
                args.interactorObject.transform.gameObject);
        }

        public void OnHoverExit()
        {
            Debug.Log("ClothInteractionPresenter.OnHoverExit()");

            _eventStatus.EndHover();
        }

        public void OnGrabPressed()
        {
            Debug.Log("ClothInteractionPresenter.OnGrabPressed()");
            _eventStatus.PressGrab();
        }

        public void OnGrabReleased()
        {
            Debug.Log("ClothInteractionPresenter.OnGrabReleased()");
            _eventStatus.ReleaseGrab();
        }

        public void OnUndoPressed()
        {
            _eventStatus.PressUndo();   
        }

        public void OnUndoReleased()
        {
            _eventStatus.ReleaseUndo();
        }

        public void OnIncreasePressed()
        {
            _eventStatus.PressIncreaseRadius();
        }

        public void OnIncreaseReleased()
        {
            _eventStatus.ReleaseIncreaseRadius();
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

    internal class VREventStatus
    {
        public bool Hovering { get; private set; }
        public bool GrabPressed { get; private set; }
        public bool UndoPressed { get; private set; }
        private bool _usedUndo;
        public bool UndoPressedThisFrame => UndoPressed && !_usedUndo;
        public bool IncreasePressed { get; private set; }
        
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

        public void PressGrab()
        {
            GrabPressed = true;
        }

        public void ReleaseGrab()
        {
            GrabPressed = false;
        }

        public void PressUndo()
        {
            UndoPressed = true;
            _usedUndo = false;
        }

        public void ReleaseUndo()
        {
            UndoPressed = false;
        }

        public void UseUndo()
        {
            _usedUndo = true;
        }

        public void PressIncreaseRadius()
        {
            IncreasePressed = true;
        }
        
        public void ReleaseIncreaseRadius()
        {
            IncreasePressed = false;
        }
    }
}
