using UnityEngine;

namespace GrabTool.Math
{
    public class VectorField2D
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
        public Vector2 C { get; set; }
        public Vector2 DesiredTranslation { get; set; }

        // Does this need to be normalized?
        private Vector2 TranslationNorm => DesiredTranslation.normalized;
        
        public Vector2 GetVelocity(float x, float y)
        {
            var vec = new Vector2(x, y);

            var r = RMultiplier * Vector2.Distance(vec, C);
            
            if (r >= Ro) // Outside the outer loop
            {
                return new Vector2(0, 0);
            }

            if (r < Ri) // Inside the inner loop
            {
                return new Vector2(-DeDy(), DeDx());
            }

            return new Vector2(-DpDy(r, x - C.x, y - C.y), DpDx(r, x - C.x, y - C.y));
        }
        
        private float E(float x, float y)
        {
            return TranslationNorm.V() * x - TranslationNorm.U() * y;
        }

        private float B(float r)
        {
            var ratio = (r - Ri) / (Ro - Ri);

            return Bernstein.Polynomial(ratio, 4, 3) + Bernstein.Polynomial(ratio, 4, 4);
        }

        private float DeDy()
        {
            return -TranslationNorm.U();
        }

        private float DeDx()
        {
            return TranslationNorm.V();
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
    }
}