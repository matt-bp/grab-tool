using System.Collections.Generic;

namespace GrabTool.Mesh
{
    public class MeshHistory
    {
        private List<UnityEngine.Mesh> _meshes = new();
        private int _currentMesh;
        public UnityEngine.Mesh CurrentMesh => _meshes[_currentMesh];

        public MeshHistory(List<UnityEngine.Mesh> startingMeshes)
        {
            _meshes = startingMeshes;
        }
        
        public MeshHistory(UnityEngine.Mesh startingMesh)
        {
            _meshes.Add(startingMesh);
        }

        public void Undo()
        {
            if (_currentMesh > 0)
            {
                _currentMesh--;
            }
        }

        public void AddMesh(UnityEngine.Mesh mesh)
        {
            if (_currentMesh < _meshes.Count - 1)
            {
                // Remove range
                _meshes.RemoveRange(_currentMesh + 1, _meshes.Count - _currentMesh);
            }
                
            _meshes.Add(mesh);
        }
    }
}