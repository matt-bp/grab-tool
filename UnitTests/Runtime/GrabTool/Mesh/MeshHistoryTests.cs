using GrabTool.Mesh;
using NUnit.Framework;
using UnityEngine;


namespace UnitTests.Runtime.GrabTool.Mesh
{
    public class MeshHistoryTests
    {
        [Test]
        public void Undo_UndoWithOne_EnsuresOneIsAlwaysPresent()
        {
            var mesh = new UnityEngine.Mesh
            {
                vertices = new[] { Vector3.forward }
            };
            var mh = new MeshHistory(mesh);
            
            mh.Undo();
            
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.forward));
        }

        // [Test]
        // public void Undo_WithMoreThanOne_MovesIndexBack()
        // {
        //     var mesh = new UnityEngine.Mesh();
        //     var mh = new MeshHistory(mesh);
        //     mh.AddMesh(mesh);
        //     
        //     mh.Undo();
        //     
        //     Assert.That(mh.CurrentMeshIndex == 0);
        // }
        
    }
}