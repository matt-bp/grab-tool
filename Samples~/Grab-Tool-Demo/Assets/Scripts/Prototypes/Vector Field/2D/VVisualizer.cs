using System;
using System.Linq;
using GrabTool.Math;
using UnityEngine;
using UnityEngine.Assertions;
using GrabTool.Math;

namespace Prototypes.Vector_Field._2D
{
    [RequireComponent(typeof(Grid))]
    public class VVisualizer : MonoBehaviour
    {
        private Grid _grid;

        [Tooltip("The desired transformation that this vector field will apply when advected through.")]
        [SerializeField]
        private Vector2 desiredTransformation;

        private Vector2 Norm => desiredTransformation.normalized;

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
                var value = GetVValue(v.v.x, v.v.y);
                _grid.Velocities[v.i] = new Vector3(value.Item1, value.Item2, 0);
                _grid.Colors[v.i] = value.Item3;
            }
        }

        private float E(float x, float y)
        {
            return Norm.V() * x - Norm.U() * y;
        }

        private float B(float r)
        {
            var ratio = (r - rI) / (rO - rI);

            return Bernstein.Polynomial(ratio, 4, 3) + Bernstein.Polynomial(ratio, 4, 4);
        }

        private float GetEDy()
        {
            return -Norm.U();
        }

        private float GetEDx()
        {
            return Norm.V();
        }

        private (float, float, Color) GetVValue(float x, float y)
        {
            var r = MathF.Pow(x, 2) + MathF.Pow(y, 2);

            if (r >= rO) // Outside the outer loop
            {
                return (0, 0, Color.white);
            }

            if (r < rI) // Inside the inner loop
            {
                return (-GetEDy(), GetEDx(), Color.blue);
            }

            // Do blending here eventually
            return (0, 0, Color.red);
        }
    }
}