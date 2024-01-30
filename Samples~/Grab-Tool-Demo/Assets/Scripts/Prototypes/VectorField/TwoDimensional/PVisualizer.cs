using System;
using System.Linq;
using GrabTool.Math;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prototypes.VectorField.TwoDimensional
{
    [RequireComponent(typeof(Grid))]
    public class PVisualizer : MonoBehaviour
    {
        private Grid _grid;

        [Tooltip("The desired transformation that this vector field will apply when advected through.")]
        [SerializeField]
        private Vector2 desiredTransformation;

        private Vector2 DesiredTransformationNorm => desiredTransformation.normalized;

        [Tooltip("The outer loop cutoff")] [SerializeField]
        private float rO;

        [Tooltip("The inner loop cutoff")] [SerializeField]
        private float rI;

        private void Start()
        {
            _grid = GetComponent<Grid>();

            Assert.IsTrue(rI < rO);
        }

        private void Update()
        {
            foreach (var v in _grid.Points.Select((v, i) => new { v, i }))
            {
                var value = GetPValue(v.v.x, v.v.y);
                _grid.Velocities[v.i] = new Vector3(0, 0, value.Item1);
                _grid.Colors[v.i] = value.Item2;
            }
        }

        private float E(float x, float y)
        {
            return DesiredTransformationNorm.y * x - desiredTransformation.x * y;
        }

        private float B(float r)
        {
            var ratio = (r - rI) / (rO - rI);

            return Bernstein.Polynomial(ratio, 4, 3) + Bernstein.Polynomial(ratio, 4, 4);
        }

        private (float, Color) GetPValue(float x, float y)
        {
            var r = MathF.Pow(x, 2) + MathF.Pow(y, 2);

            if (r >= rO) // Outside the outer loop
            {
                return (0, Color.white);
            }

            if (r < rI) // Inside the inner loop
            {
                return (E(x, y), Color.blue);
            }

            // Do blending here eventually
            return ((1 - B(r)) * E(x, y), Color.red);
            // return 0;
        }
    }
}