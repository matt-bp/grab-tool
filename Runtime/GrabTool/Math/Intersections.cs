using System;
using UnityEngine;

namespace GrabTool.Math
{
    public static class Intersections
    {
        public class CustomRaycastHit
        {
            public Vector3 Point { get; set; }
            public Transform Transform { get; set; }
            public Vector3 Normal { get; set; }
        }

        public static bool RayPlane(Ray ray, Vector3 point, Vector3 planeNormal, out CustomRaycastHit hit)
        {
            var denom = Vector3.Dot(planeNormal, ray.direction);
            if (Mathf.Abs(denom) > Mathf.Epsilon)
            {
                var t = Vector3.Dot(point - ray.origin, planeNormal) / denom;
                if (t >= 0)
                {
                    hit = new CustomRaycastHit
                    {
                        Point = ray.GetPoint(t),
                        Normal = planeNormal
                    };
                    return true;
                }
            }

            hit = new CustomRaycastHit();
            return false;
        }

        /// <summary>
        /// https://iquilezles.org/articles/intersectors/
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool RayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out CustomRaycastHit hit)
        {
            var v1v0 = v1 - v0;
            var v2v0 = v2 - v0;
            var rov0 = ray.origin - v0;
            var n = Vector3.Cross(v1v0, v2v0);
            var q = Vector3.Cross(rov0, ray.direction);
            var d = 1.0f / Vector3.Dot(ray.direction, n);
            var u = d * Vector3.Dot(-q, v2v0);
            var v = d * Vector3.Dot(q, v1v0);
            var t = d * Vector3.Dot(-n, rov0);
            if (u < 0.0 || v < 0.0 || (u + v) > 1.0)
            {
                hit = new CustomRaycastHit();
                return false;
            }

            hit = new CustomRaycastHit
            {
                Point = ray.GetPoint(t),
                Normal = n,
            };
            return true;
        }

        /// <summary>
        /// Mostly taken from https://planetcalc.com/8108/, simplified for my use case, and extended to 3D.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool PointInTriangle(Vector3 point, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var pv0 = point - v0;
            var v1v0 = v1 - v0;
            var pv0xv1v0 = Vector3.Cross(pv0, v1v0);
            
            var pv1 = point - v1;
            var v2v1 = v2 - v1;
            var pv1xv2v1 = Vector3.Cross(pv1, v2v1);
            
            var pv2 = point - v2;
            var v0v2 = v0 - v2;
            var pv2xv0v2 = Vector3.Cross(pv2, v0v2);

            // Last, we find if all the cross products are parallel (or zero), if they are, the point is in the triangle!
            var firstTwoParallel = Vector3.Dot(pv0xv1v0, pv1xv2v1) >= 0;
            var secondAndThirdParallel = Vector3.Dot(pv1xv2v1, pv2xv0v2) >= 0;

            return firstTwoParallel && secondAndThirdParallel;
        }
    }
}