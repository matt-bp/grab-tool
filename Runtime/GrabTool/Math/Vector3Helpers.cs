using System;
using UnityEngine;

namespace GrabTool.Math
{
    public static class Vector3Helpers
    {
        /// <summary>
        /// <para>Returns a normalized vector orthogonal to the one passed in.</para>
        /// <para>Based on similarly named function in https://imagecomputing.net/cgp/content/01_general/index.html.</para>
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Vector3 OrthogonalVector(Vector3 vec)
        {
            if (vec.magnitude < float.Epsilon)
            {
                return Vector3.right;
            }

            bool TryGetOrthogonal(Vector3 axis, out Vector3 orthogonalVec)
            {
                var test = Vector3.Cross(axis, vec);
                orthogonalVec = test.normalized;
                // If axis and vector are in line, the magnitude will be close to zero.
                return test.magnitude > Vector3.kEpsilon;
            }

            if (TryGetOrthogonal(Vector3.right, out var inYOrZ))
            {
                return inYOrZ;
            } 
            
            if (TryGetOrthogonal(Vector3.up, out var inXOrZ))
            {
                return inXOrZ;
            }
            
            if (TryGetOrthogonal(Vector3.forward, out var inXOrY))
            {
                return inXOrY;
            }

            throw new ArgumentException($"Cant find an orthogonal vector to {vec}");
        }
    }
}