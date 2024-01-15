using System.Collections.Generic;
using System.Linq;

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
            _currentMesh = startingMeshes.Count - 1;
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
                var count = _meshes.Count - (_currentMesh + 1);
                _meshes.RemoveRange(_currentMesh + 1, count);
            }

            _meshes.Add(mesh);
            _currentMesh++;
        }
    }
}