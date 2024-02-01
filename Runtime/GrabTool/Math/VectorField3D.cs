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
        public float RMultiplier { get; set; }
        public Vector3 C { get; set; }
        public Vector3 DesiredTranslation { get; set; }

        // Does this need to be normalized? Yes on page 3, above equation 6.
        private Vector3 V => DesiredTranslation.normalized;
        private Vector3 U => Vector3Helpers.OrthogonalVector(V);
        private Vector3 W => Vector3.Cross(V, U).normalized;
        
        public Vector3 GetVelocity(Vector3 position)
        {
            var r = R(position);
            
            if (r >= Ro) // Outside the outer loop
            {
                return Vector3.zero;
            }

            if (r < Ri) // Inside the inner loop
            {
                var gradientP = GradientP();
                var gradientQ = GradientQ();

                return Vector3.Cross(gradientP, gradientQ);
            }

            return Vector3.zero;
        }

        private float R(Vector3 pos) => RMultiplier * Vector3.Distance(pos, C);

        private Vector3 GradientP() => U;
        private Vector3 GradientQ() => W;

        // private float E(float x, float y)
        // {
        //     return TranslationNorm.V() * x - TranslationNorm.U() * y;
        // }
        //
        // private float B(float r)
        // {
        //     var ratio = (r - Ri) / (Ro - Ri);
        //
        //     return Bernstein.Polynomial(ratio, 4, 3) + Bernstein.Polynomial(ratio, 4, 4);
        // }

        // private float DeDy()
        // {
        //     return -TranslationNorm.U();
        // }
        //
        // private float DeDx()
        // {
        //     return TranslationNorm.V();
        // }
        //
        // private float DrDx(float x, float y)
        // {
        //     return 4 * x;
        // }
        //
        // private float DrDy(float x, float y)
        // {
        //     return 4 * y;
        // }
        //
        // private float DbDr(float r)
        // {
        //     return Bernstein.PolynomialDt(r, 4, 3) + Bernstein.PolynomialDt(r, 4, 4);
        // }
        //
        // private float DbrDx(float r, float x, float y)
        // {
        //     return DbDr(r) * DrDx(x, y);
        // }
        //
        // private float DbrDy(float r, float x, float y)
        // {
        //     return DbDr(r) * DrDy(x, y);
        // }
        //
        // private float DpDx(float r, float x, float y)
        // {
        //     return (1 - B(r)) * DeDx() - DbrDx(r, x, y) * E(x, y);
        // }
        //
        // private float DpDy(float r, float x, float y)
        // {
        //     return (1 - B(r)) * DeDy() - DbrDy(r, x, y) * E(x, y);
        // }
    }
}