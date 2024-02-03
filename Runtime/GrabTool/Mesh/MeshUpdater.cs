using UnityEngine;

namespace GrabTool.Mesh
{
    public static class MeshUpdater
    {
        public static void UpdateMeshes(UnityEngine.Mesh meshToUpdate, MeshCollider meshCollider, Vector3[] newPositions)
        {
            meshToUpdate.vertices = newPositions;
            meshToUpdate.RecalculateBounds();
            meshToUpdate.RecalculateNormals();

            // Need to assign the mesh every frame to get intersections happening correctly.
            // See: https://forum.unity.com/threads/how-to-update-a-mesh-collider.32467/
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = meshToUpdate;
            }
        }
    }
}