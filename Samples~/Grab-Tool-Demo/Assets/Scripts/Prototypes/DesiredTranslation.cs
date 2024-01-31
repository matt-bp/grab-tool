using System;
using GrabTool.Math;
using UnityEngine;

namespace Prototypes
{
    public class DesiredTranslation : MonoBehaviour
    {
        [Header("Intersection Placeholders")] [SerializeField]
        private Vector3 point;
        [SerializeField] private GameObject positionIndicator;

        private Camera _camera;

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
                positionIndicator.transform.position = hit.Point;
            }
        }
    }
}