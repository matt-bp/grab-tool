using System;
using System.Linq;
using GrabTool.Math;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prototypes.VectorField.TwoDimensional
{
    [RequireComponent(typeof(Grid))]
    public class VVisualizer : MonoBehaviour
    {
        private Grid _grid;

        [Tooltip("The desired transformation that this vector field will apply when advected through.")]
        [SerializeField]
        private Vector2 desiredTransformation;

        public Vector2 DesiredTransformation
        {
            get => desiredTransformation;
            set => desiredTransformation = value;
        }

        [Tooltip("Current center of the vector field")] [SerializeField]
        private Vector2 c;

        public Vector2 C
        {
            get => c;
            set => c = value;
        }
        
        private Vector2 Norm => desiredTransformation.normalized;

        [Tooltip("The outer loop cutoff")] [SerializeField]
        private float rO;

        [Tooltip("The inner loop cutoff")] [SerializeField]
        private float rI;

        [Tooltip("Multiplier for underlying r function")] [SerializeField] [Range(0.1f, 4.0f)]
        private float rMultiplier = 0.1f;
        
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

        private float DeDy()
        {
            return -Norm.U();
        }

        private float DeDx()
        {
            return Norm.V();
        }

        private float DrDx(float x, float y)
        {
            return 4 * x;
        }
        
        private float DrDy(float x, float y)
        {
            return 4 * y;
        }

        private float DbDr(float r)
        {
            return Bernstein.PolynomialDt(r, 4, 3) + Bernstein.PolynomialDt(r, 4, 4);
        }

        private float DbrDx(float r, float x, float y)
        {
            return DbDr(r) * DrDx(x, y);
        }

        private float DbrDy(float r, float x, float y)
        {
            return DbDr(r) * DrDy(x, y);
        }

        private float DpDx(float r, float x, float y)
        {
            return (1 - B(r)) * DeDx() - DbrDx(r, x, y) * E(x, y);
        }
        
        private float DpDy(float r, float x, float y)
        {
            return (1 - B(r)) * DeDy() - DbrDy(r, x, y) * E(x, y);
        }

        private (float, float, Color) GetVValue(float x, float y)
        {
            var vec = new Vector2(x, y);

            var r = rMultiplier * Vector2.Distance(vec, c);
            
            if (r >= rO) // Outside the outer loop
            {
                return (0, 0, Color.white);
            }

            if (r < rI) // Inside the inner loop
            {
                return (-DeDy(), DeDx(), Color.green);
            }

            // Do blending here eventually
            return (-DpDy(r, x - c.x, y - c.y), DpDx(r, x - c.x, y - c.y), Color.red);
        }
    }
}