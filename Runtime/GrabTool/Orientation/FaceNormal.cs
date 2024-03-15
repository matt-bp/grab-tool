using System;
using GrabTool.Mesh;
using UnityEngine;

namespace GrabTool.Orientation
{
    public class FaceNormal : MonoBehaviour
    {
        public Vector3 surfaceNormalToMatch;

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(surfaceNormalToMatch);
        }
    }
}