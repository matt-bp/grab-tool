using JetBrains.Annotations;
using UnityEngine;

namespace GrabTool.Models
{
    public class MeshHistory : MonoBehaviour
    {
        public bool NeedsCreated => _meshHistory == null;
        [CanBeNull] private Mesh.MeshHistory _meshHistory;
        [CanBeNull] public UnityEngine.Mesh CurrentMesh => _meshHistory?.CurrentMesh;
        
        public void SetInitialMesh(UnityEngine.Mesh startingMesh)
        {
            _meshHistory = new Mesh.MeshHistory(startingMesh);
        }

        public void AddMesh(UnityEngine.Mesh mesh)
        {
            _meshHistory?.AddMesh(mesh);
        }

        public void Undo()
        {
            _meshHistory?.Undo();
        }
        
    }
}