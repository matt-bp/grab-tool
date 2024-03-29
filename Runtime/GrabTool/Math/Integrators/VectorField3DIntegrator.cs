using System.Collections.Generic;
using UnityEngine;

namespace GrabTool.Math.Integrators
{
    public class VectorField3DIntegrator
    {
        private readonly MeshFilter _meshFilter;
        private readonly VectorField3D _vectorField;
        public Vector3 C
        {
            set => _vectorField.C = value;
        }
        public Vector3 DesiredTranslation
        {
            set => _vectorField.DesiredTranslation = value;
        }

        public float Ro
        {
            get => _vectorField.Ro;
        }

        public bool WasInner => _vectorField.wasInner;

        public VectorField3DIntegrator(float ri, float ro)
        {
            _vectorField = new VectorField3D
            {
                Ri = ri,
                Ro = ro
            };
        }

        public Vector3[] Integrate(Vector3[] positions)
        {
            var results = new List<Vector3>();
            
            foreach (var position in positions)
            {
                results.Add(Integrate(position));
            }

            return results.ToArray();
        }

        public Vector3 Integrate(Vector3 position)
        {
            var v = _vectorField.GetVelocity(position);

            if (v.magnitude == 0)
            {
                return position;
            };

            // Debug.DrawRay(position, v);

            var d = Vector3.Magnitude(_vectorField.DesiredTranslation);
            var t = d / v.magnitude;

            // Debug.Log($"Time update is {t}");

            return position + t * v;
        }
    }
}