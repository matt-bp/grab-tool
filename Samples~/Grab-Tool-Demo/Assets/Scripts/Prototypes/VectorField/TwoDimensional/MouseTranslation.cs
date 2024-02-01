using System;
using GrabTool.Math;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Prototypes.VectorField.TwoDimensional
{
    public class MouseTranslation : MonoBehaviour
    {
        [Header("Intersection Placeholders")]
        [Tooltip("This is where the click interaction would've started.")]
        [SerializeField]
        private Vector3 point;

        [SerializeField] private GameObject positionIndicator;

        [Header("Model/Presenter")] [SerializeField]
        private VectorFieldIntegrator fieldIntegrator;

        private Camera _camera;

        private Vector3? previousPosition;
        private Vector3? direction;

        public UnityEvent<(Vector3, Vector3)> OnMouseUpdate;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            var planeNormal = Vector3.back;

            if (Intersections.RayPlane(ray, point, planeNormal, out var hit))
            {
                var current = hit.Point;
                positionIndicator.transform.position = current;

                if (previousPosition != null)
                {
                    var diff = current - previousPosition.Value;

                    direction = diff != Vector3.zero ? diff : null;

                    OnMouseUpdate.Invoke((diff, previousPosition.Value));
                }

                previousPosition = hit.Point;
            }
            else
            {
                // Or if we didn't click. Or some other way to stop this current "session"
                previousPosition = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (previousPosition != null && direction != null)
            {
                Handles.color = Color.magenta;
                var rotation = Quaternion.LookRotation(direction.Value);
                Handles.ArrowHandleCap(1, previousPosition.Value, rotation, 0.2f, EventType.Repaint);
            }
        }
    }
}