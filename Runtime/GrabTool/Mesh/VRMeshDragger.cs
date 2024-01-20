using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GrabTool.Mesh
{
    public class VRMeshDragger : MonoBehaviour
    {
        [Header("View")] [SerializeField] private GameObject leftHand;
        [SerializeField] private GameObject colliderVisualization;
        [SerializeField] private MeshFilter[] meshesToCheckCollision;
        [SerializeField] private GameObject closestPointVisualization;
        
        private VRIndicatorState _vrIndicatorState;
        private readonly VREventStatus _eventStatus = new();
        private readonly VRTrackingState _trackingState = new();
        
        private void Start()
        {
            _vrIndicatorState = new VRIndicatorState(colliderVisualization);
        }

        private void Update()
        {
            if (!_trackingState.CurrentlyTracking)
            {
                CheckForHoverAndStart();
                return;
            }
            
            if (_eventStatus.GrabPressed)
            {
                _vrIndicatorState.Hide();

                // UpdateIndices(leftController.transform.point?);
                // just get the world space position of the "Left Controller"
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
                _vrIndicatorState.Hide();
                return;
            }
            
            _vrIndicatorState.Show();

            if (!_eventStatus.GrabPressed) return;
            
            // Start tracking!
            Debug.Log("Start tracking!");

            if (_eventStatus.HoveredCollider == null || _eventStatus.InteractorGameObject == null)
            {
                Debug.Log("No collider or interactor game object.");
                return;
            }
                
            // Get world space position of collision of sphere and mesh
            // Pickup here! Look for how to get 
            // I have the controller center, I need to find a world space hit point 
            // - Maybe I take the controller center, and find the closest point on the mesh to that center?
            //leftHand.ClosestPoint(colliderVisualization.transform.position); // Uhh, I think this is it? Does it work with a non-convex mesh?
            // Maybe I can get the collider information from the hover event? Then I'd just need to store the leftController main object
            _eventStatus.HoveredCollider.ClosestPoint(_eventStatus.InteractorGameObject.transform.position);    
            // Physics.ComputePenetration()
            // var position = _eventStatus.HoveredCollider.ClosestPoint()
            
            // Start tracking
            _trackingState.StartTracking();
            // Get size
            // Get falloff curve
            // Also need a way to adjust the radius of the collider on the leftController.
                
            // Do history things
        }

        private void StopTracking()
        {
            Debug.Log("Stopped tracking");
            
            _trackingState.StopTracking();
        }
        
        #region Event Listeners

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            Debug.Log("ClothInteractionPresenter.OnHoverEnter()");
            
            // Debug.Log($"Hit: {args.interactableObject.transform.gameObject.name}");
            // Debug.Log($"Hitter: {args.interactorObject.transform.gameObject.name}");
            // args.interactorObject.transform.gameObject.transform.position
            Debug.Assert(args.interactableObject.colliders.Count == 1);
            
            _eventStatus.StartHover(args.interactableObject.colliders[0], args.interactorObject.transform.gameObject);
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
        
        #endregion
    }

    internal class VRIndicatorState
    {
        private readonly GameObject _indicator;

        public VRIndicatorState(GameObject indicator)
        {
            _indicator = indicator;
        }
        
        public void Hide()
        {
            if (_indicator.activeSelf)
            {
                _indicator.SetActive(false);
            }
        }

        public void Show()
        {
            _indicator.SetActive(true);
        }
    }
    
    internal class VREventStatus
    {
        public bool Hovering { get; private set; }
        public bool GrabPressed { get; private set; }
        [CanBeNull] public Collider HoveredCollider { get; private set; }
        [CanBeNull] public GameObject InteractorGameObject { get; private set; }

        public void StartHover(Collider hoveredCollider, GameObject interactor)
        {
            Hovering = true;
            HoveredCollider = hoveredCollider;
            InteractorGameObject = interactor;
        }

        public void EndHover()
        {
            Hovering = false;
            HoveredCollider = null;
        }

        public void PressGrab()
        {
            GrabPressed = true;
        }

        public void ReleaseGrab()
        {
            GrabPressed = false;
        }
    }
    
    internal class VRTrackingState
    {
        public bool CurrentlyTracking { get; private set; }

        public void StartTracking()
        {
            CurrentlyTracking = true;
        }

        public void StopTracking()
        {
            CurrentlyTracking = false;
        }
    }
}