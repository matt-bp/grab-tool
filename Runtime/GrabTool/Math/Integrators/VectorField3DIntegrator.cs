using System.Collections.Generic;
using UnityEngine;

namespace GrabTool.Math.Integrators
{
    public class VectorField3DIntegrator
    {
        private readonly MeshFilter _meshFilter;
        private readonly VectorField3D _vectorField;
        private readonly float _rI;
        private readonly float _rO;
        public Vector3 C
        {
            set => _vectorField.C = value;
        }

        public Vector3 DesiredTranslation
        {
            set => _vectorField.DesiredTranslation = value;
        }

        public VectorField3DIntegrator(float ri, float ro)
        {
            _vectorField = new VectorField3D();
            _rI = ri;
            _rO = ro;
        }

        public Vector3[] Integrate(Vector3[] positions)
        {
            var results = new List<Vector3>();
            
            foreach (var position in positions)
            {
                var v = _vectorField.GetVelocity(position);

                if (v.magnitude == 0) continue;

                var d = Vector3.Magnitude(_vectorField.DesiredTranslation);
                var t = d / v.magnitude;

                // Debug.Log($"Time update is {t}");

                var newPosition = position + t * v;

                results.Add(newPosition);
            }

            return results.ToArray();
        }
    }
}