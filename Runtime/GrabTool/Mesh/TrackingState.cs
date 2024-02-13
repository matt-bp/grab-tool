using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrabTool.Mesh
{
    public class TrackingState
    {
        public bool CurrentlyTracking { get; private set; }
        public Vector3 InitialPosition { get; private set; }
        public int[] ConstantIndices { get; private set; }
        public UnityEngine.Mesh LastMesh { get; private set; }
        private GameObject _hitObject;
        private MeshCollider _meshCollider;
        private Dictionary<int, (Vector3 LocalPoint, float CloseRatio)> _indicesAndOriginalPositions;
        private AnimationCurve _falloff;
        
        public void StartTracking(Vector3 initialHitPosition, GameObject hitObject, float radius, float constantRadius,
            AnimationCurve falloff)
        {
            CurrentlyTracking = true;
            InitialPosition = initialHitPosition;
            _hitObject = hitObject;
            LastMesh = hitObject.GetComponent<MeshFilter>().sharedMesh;
            _meshCollider = hitObject.GetComponent<MeshCollider>();
            _falloff = falloff;

            // Make sure we have the valid range of [0, 1].
            Debug.Assert(_falloff.keys.Any(k => k.time >= 1));
            Debug.Assert(_falloff.keys.Any(k => k.time <= 0));

            float GetWorldSpaceDistance(Vector3 p) =>
                Vector3.Distance(hitObject.transform.TransformPoint(p), initialHitPosition);

            var allIndicesAndPositions = LastMesh.vertices
                .Select((v, i) => new { v, i, closeRatio = GetWorldSpaceDistance(v) / radius })
                .ToList();
            
            _indicesAndOriginalPositions = allIndicesAndPositions.Where(x => x.closeRatio is >= 0 and <= 1)
                .ToDictionary(x => x.i, x => (x.v, x.closeRatio));

            ConstantIndices = allIndicesAndPositions.Where(x => x.closeRatio >= 0 && x.closeRatio <= constantRadius)
                .Select(x => x.i)
                .ToArray();
        }

        public void UpdateIndices(Vector3 worldMousePosition)
        {
            var localDelta = _hitObject.transform.InverseTransformVector(worldMousePosition - InitialPosition);
            var newPositions = LastMesh.vertices;

            if (!_indicesAndOriginalPositions.Any()) return;

            foreach (var contents in _indicesAndOriginalPositions)
            {
                Debug.Assert(contents.Value.CloseRatio is >= 0 and <= 1);

                newPositions[contents.Key] = contents.Value.LocalPoint +
                                             localDelta * _falloff.Evaluate(contents.Value.CloseRatio);
            }

            UpdateMeshes(newPositions);
        }

        public void UpdateMeshes(Vector3[] newPositions)
        {
            LastMesh.vertices = newPositions;
            LastMesh.RecalculateBounds();
            LastMesh.RecalculateNormals();

            // Need to assign the mesh every frame to get intersections happening correctly.
            // See: https://forum.unity.com/threads/how-to-update-a-mesh-collider.32467/
            _meshCollider.sharedMesh = LastMesh;
        }

        public void StopTracking()
        {
            CurrentlyTracking = false;
        }
    }
}