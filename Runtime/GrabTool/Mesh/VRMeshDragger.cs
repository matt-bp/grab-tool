using System;
using UnityEngine;

namespace GrabTool.Mesh
{
    public class VRMeshDragger : MonoBehaviour
    {
        [Header("View")] [SerializeField] private SphereCollider leftHand;
        [SerializeField] private GameObject colliderVisualization;
        [SerializeField] private MeshFilter[] meshesToCheckCollision;
        
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
                
            // Get world space position of collision of sphere and mesh
            // Pickup here! Look for how to get 
            // I have the controller center, I need to find a world space hit point 
            // - Maybe I take the controller center, and find the closest point on the mesh to that center?
            leftHand.ClosestPoint(colliderVisualization.transform.position); // Uhh, I think this is it? Does it work with a non-convex mesh?
            // Maybe I can get the collider information from the hover event? Then I'd just need to store the leftController main object
                
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

        public void OnHoverEnter()
        {
            Debug.Log("ClothInteractionPresenter.OnHoverEnter()");
            
            _eventStatus.StartHover();
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

        public void StartHover()
        {
            Hovering = true;
        }

        public void EndHover()
        {
            Hovering = false;
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