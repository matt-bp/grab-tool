using UnityEngine;

namespace GrabTool.Math
{
    public static class Collisions
    {
        /// <summary>
        /// Check out Real-Time Collision Detection, 5.1.5 Closest Point on Triangle to Point for code source.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector3 ClosestPointInTriangleToPoint(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var bc = c - b;
 
            // Compute parametric position s for projection P’ of P on AB,
            // P’=A+ s*AB, s=snom/(snom+sdenom)
            float snom = Vector3.Dot(p - a, ab), sdenom = Vector3.Dot(p- b, a - b);
 
            // Compute parametric position t for projection P’ of P on AC,
            // P’=A + t*AC, s = tnom/(tnom+tdenom)
            float tnom = Vector3.Dot(p - a, ac), tdenom = Vector3.Dot(p - c, a - c);
 
            if (snom <= 0.0f && tnom <= 0.0f) return a; // Vertex region early out
 
            // Compute parametric position u for projection P’ of P on BC,
            // P’=B + u*BC, u = unom/(unom+udenom)
            float unom = Vector3.Dot(p - b, bc), udenom = Vector3.Dot(p - c, b - c);
 
            if (sdenom <= 0.0f && unom <= 0.0f) return b; // Vertex region early out
            if (tdenom <= 0.0f && udenom <= 0.0f) return c; // Vertex region early out
 
            // P is outside (or on) AB if the triple scalar product [N PA PB] <= 0
            var n = Vector3.Cross(b - a, c - a);
            float vc = Vector3.Dot(n, Vector3.Cross(a - p, b - p));
            // If P outside AB and within feature region of AB,
            // return projection of P onto AB
            if (vc <= 0.0f && snom >= 0.0f && sdenom >= 0.0f)
                return a + snom / (snom + sdenom) * ab;
 
            // P is outside (or on) BC if the triple scalar product [N PB PC] <= 0
            float va = Vector3.Dot(n, Vector3.Cross(b - p, c - p));
            // If P outside BC and within feature region of BC,
            // return projection of P onto BC
            if (va <= 0.0f && unom >= 0.0f && udenom >= 0.0f)
                return b + unom / (unom + udenom) * bc;
 
            // P is outside (or on) CA if the triple scalar product [N PC PA] <= 0
            float vb = Vector3.Dot(n, Vector3.Cross(c - p, a - p));
            
            // If P outside CA and within feature region of CA,
            // return projection of P onto CA
            if (vb <= 0.0f && tnom >= 0.0f && tdenom >= 0.0f)
                return a + tnom / (tnom + tdenom) * ac;
 
            // P must project inside face region. Compute Q using barycentric coordinates
            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc/(va + vb + vc)
            return u * a + v * b + w * c;
        }
    }
}