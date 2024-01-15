using System.Collections.Generic;

namespace GrabTool.Mesh
{
    public class MeshHistory
    {
        private readonly Stack<UnityEngine.Mesh> _currentHistory = new();
        private int _currentMesh;
        public UnityEngine.Mesh CurrentMesh => MeshCopier.MakeCopy(_currentHistory.Peek());

        public MeshHistory(UnityEngine.Mesh startingMesh)
        {
            var copy = MeshCopier.MakeCopy(startingMesh);

            _currentHistory.Push(copy);
        }

        public void Undo()
        {
            if (_currentHistory.Count == 1) return;
 
            _currentHistory.Pop();
        }

        public void AddMesh(UnityEngine.Mesh mesh)
        {
            _currentHistory.Push(MeshCopier.MakeCopy(mesh));
        }
    }
}