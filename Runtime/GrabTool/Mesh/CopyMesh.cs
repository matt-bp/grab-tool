using System.Collections.Generic;
using UnityEngine;

namespace GrabTool.Mesh
{
    [RequireComponent(typeof(MeshFilter))]
    public class CopyMesh : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var meshFilter = GetComponent<MeshFilter>();
            
            var newMesh = MeshCopier.MakeCopy(meshFilter.sharedMesh);

            meshFilter.sharedMesh = newMesh;
        }
    }
}
