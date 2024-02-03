using UnityEngine;

namespace GrabTool.Math
{
    public class VectorField3D
    {
        /// <summary>
        /// R value specifying the inner ring, where the vector field is constant.
        /// </summary>
        public float Ri { get; set; }
        /// <summary>
        /// R value on the outer edge, past which the vector field will be zero.
        /// </summary>
        public float Ro { get; set; }
        public Vector3 C { get; set; }
        public Vector3 DesiredTranslation { get; set; }

        // Does this need to be normalized? Yes on page 3, above equation 6.
        private Vector3 V => DesiredTranslation.normalized;

        public bool wasInner;

        public Vector3 GetVelocity(Vector3 position)
        {
            wasInner = false;
            
            var r = R(position);

            if (r >= Ro) // Outside the outer loop
            {
                return Vector3.zero;
            }
            
            var u = Vector3Helpers.OrthogonalVector(V);
            var w = Vector3.Cross(V, u).normalized;

            if (r < Ri) // Inside the inner loop
            {
                wasInner = true;
                return Vector3.Cross(u, w); // Gradient P x Gradient Q
            }

            var e = Vector3.Dot(u, position - C);
            var f = Vector3.Dot(w, position - C);
            
            var gradE = u;
            var gradF = w;
            
            var ratio = (r - Ri) / (Ro - Ri);

            var gradP = (1 - B(ratio)) * gradE + DbrDx(ratio, position) * e;
            var gradQ = (1 - B(ratio)) * gradF + DbrDx(ratio, position) * f;
            
            return Vector3.Cross(gradP, gradQ);
        }

        private float R(Vector3 pos) => Vector3.Distance(pos, C);
        
        private float B(float r)
        {
            return Bernstein.Polynomial(r, 4, 3) + Bernstein.Polynomial(r, 4, 4);
        }

        /// <summary>
        /// <para>Derivative of the Bernstein polynomial sums.</para>
        /// <para>We needed to take the derivative of B(r(x)). We can take the the derivative dBdR, and then dRdX based on the chain rule.</para>
        /// </summary>
        /// <param name="r"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private Vector3 DbrDx(float r, Vector3 x)
        {
            if (r == 0) return Vector3.zero; // DbDr(r) will be zero anyways, seems like a reasonable default.

            var dRdX = (x - C) / r;
            return DbDr(r) * dRdX;
        }
        
        /// <summary>
        /// <para>Derivative of Bernstein polynomial sum with respect to R. This is based on the chain rule. See Db for more details.</para>
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private float DbDr(float r)
        {
            return Bernstein.PolynomialDt(r, 4, 3) + Bernstein.PolynomialDt(r, 4, 4);
        }
    }
}