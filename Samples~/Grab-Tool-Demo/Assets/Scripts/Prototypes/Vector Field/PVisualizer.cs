using System;
using System.Linq;
using UnityEngine;

namespace Prototypes.Vector_Field
{
    [RequireComponent(typeof(Grid))]
    public class PVisualizer : MonoBehaviour
    {
        private Grid _grid;

        [Tooltip("The desired transformation that this vector field will apply when advected through.")]
        [SerializeField] private Vector2 desiredTransformation;
        private Vector2 DesiredTransformationNorm => desiredTransformation.normalized;

        [Tooltip("The outer loop cutoff")]
        [SerializeField] private float rO;

        [Tooltip("The inner loop cutoff")]
        [SerializeField] private float rI;
        
        private void Start()
        {
            _grid = GetComponent<Grid>();
        }

        private void Update()
        {
            foreach (var v in _grid.Points.Select((v, i) => new {v, i}))
            {
                _grid.Velocities[v.i] = new Vector3(0, 0, GetPValue(v.v.x, v.v.y));
            }
        }

        private float E(float x, float y)
        {
            return DesiredTransformationNorm.y * x - desiredTransformation.x * y;
        }

        private float GetPValue(float x, float y)
        {
            var r = MathF.Pow(x, 2) + MathF.Pow(y, 2);

            if (r >= rO) // Outside the outer loop
            {
                return 0;
            }

            if (r < rI) // Inside the inner loop
            {
                return E(x, y);
            }
            
            // Do blending here eventually
            return 0;
        }
    }
}