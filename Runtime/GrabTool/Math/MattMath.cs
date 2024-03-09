using System.Linq;
using UnityEngine;

namespace GrabTool.Math
{
    public class MattMath
    {
        public static bool Raycast(Ray ray, MeshFilter meshFilter, out Intersections.CustomRaycastHit hit)
        {
            var gameObjectTransform = meshFilter.gameObject.transform;
            var sourceTriangles = meshFilter.sharedMesh.triangles;

            Vector3 GetVertexInWorldSpace(int index) =>
                gameObjectTransform.TransformPoint(meshFilter.sharedMesh.vertices[index]);

            var triangles = sourceTriangles
                .Select((v, i) => new { v, i })
                .GroupBy(x => x.i / 3)
                .Select(g => g.Select(x => GetVertexInWorldSpace(x.v)).ToList())
                .ToList();

            var distanceToOrigin = float.MaxValue;
            Intersections.CustomRaycastHit currentHit = null;

            foreach (var triangle in triangles)
            {
                Debug.Assert(triangle.Count == 3);

                // Intersect ray with triangle
                if (!Intersections.RayTriangle(ray, triangle[0], triangle[1], triangle[2], out var triangleHit))
                    continue;

                var newDistance = Vector3.Distance(ray.origin, triangleHit.Point);

                if (newDistance >= distanceToOrigin)
                    continue;
                
                currentHit = new Intersections.CustomRaycastHit()
                {
                    Point = triangleHit.Point,
                    Normal = triangleHit.Normal,
                    Transform = gameObjectTransform
                };

                distanceToOrigin = newDistance;
            }
            
            hit = currentHit ?? new Intersections.CustomRaycastHit();
            return currentHit != null;
        }
    }
}