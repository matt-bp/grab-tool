using System;
using UnityEngine;

namespace GrabTool.Math
{
    public static class ClosestPoint
    {
        /// <summary>
        /// <para>This takes in two rays, and computes the closest point, on each of the rays (s for ray1, t for ray2) to each other.</para>
        /// <para>See Real-Time Collision Detection, Section 5.1.8 for explanation.</para>
        /// </summary>
        /// <param name="ray1">First ray</param>
        /// <param name="ray2">Second ray</param>
        /// <returns>Numbers to pass into Ray.GetPoint(...) to get closest point between these rays.</returns>
        public static (float s, float t) RayRay(Ray ray1, Ray ray2)
        {
            var a = Vector3.Dot(ray1.direction, ray1.direction);
            var b = Vector3.Dot(ray1.direction, ray2.direction);
            var r = ray1.origin - ray2.origin;
            var c = Vector3.Dot(ray1.direction, r);
            var e = Vector3.Dot(ray2.direction, ray2.direction);
            var f = Vector3.Dot(ray2.direction, r);
            var d = a * e - Mathf.Pow(b, 2);

            if (d == 0)
            {
                throw new ArgumentException("These lines are parallel");
            }

            var s = (b * f - c * e) / d;
            var t = (a * f - b * c) / d;
            
            return (s, t);
        }
    }
}