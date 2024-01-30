using UnityEngine;
using static MathNet.Numerics.Combinatorics;

namespace GrabTool.Math
{
    public static class Bernstein
    {
        public static float Polynomial(float t, int n, int i)
        {
            return (float)Combinations(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
        }

        /// <summary>
        /// Recursive definition of the Bernstein polynomial. See http://www.inf.ufsc.br/~aldo.vw/grafica/apostilas/Bernstein-Polynomials.pdf page 9 for definition.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="n"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static float PolynomialDt(float t, int n, int i)
        {
            // First try, still figuring out why this didn't work
            // return (float)Combinations(n, i) * (Mathf.Pow(t, i) * (n - i) * Mathf.Pow(1 - t, n - i - 1) -
            //                                     i * Mathf.Pow(t, i - 1) * Mathf.Pow(1 - t, n - i));
            
            return n * (Polynomial(t, n - 1, i - 1) - Polynomial(t, n - 1, i));
        }

        public static float PolynomialDtSo(float x, int n, int k)
        {
            return (float)Combinations(n, k) * Mathf.Pow(x, k - 1) * Mathf.Pow(1 - x, n - k - 1) *
                   (-(n - k) * x + k * (1 - x));
        }
    }
}