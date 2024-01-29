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
    }
}