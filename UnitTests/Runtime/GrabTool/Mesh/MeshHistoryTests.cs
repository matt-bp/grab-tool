using System.Collections.Generic;
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
            var mh = new MeshHistory(MakeMesh(Vector3.forward));
            
            mh.Undo();
            
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void Undo_WithMoreThanOne_MovesIndexBack()
        {
            var mh = new MeshHistory(MakeMeshes());
            
            mh.Undo();
            
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.back));
        }

        [Test]
        public void AddMesh_AddingOne_MakesThatTheCurrentMesh()
        {
            var mh = new MeshHistory(MakeMesh(Vector3.forward));
            
            mh.AddMesh(MakeMesh(Vector3.right));
            
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.right));
        }

        [Test]
        public void UndoAddMesh_ChainedTogether_KeepsCurrentMeshValid()
        {
            var mh = new MeshHistory(MakeMesh(Vector3.forward));
            
            mh.AddMesh(MakeMesh(Vector3.right));
            mh.AddMesh(MakeMesh(Vector3.left));
            mh.AddMesh(MakeMesh(Vector3.up));
            mh.AddMesh(MakeMesh(Vector3.down));

            mh.Undo(); // to up
            mh.Undo(); // to left
            
            mh.AddMesh(MakeMesh(Vector3.back));
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.back));
            
            mh.Undo();
            mh.Undo();
            
            Assert.That(mh.CurrentMesh.vertices[0], Is.EqualTo(Vector3.right));
        }
        
        
        #region Helpers

        private static UnityEngine.Mesh MakeMesh(Vector3 vector)
        {
            var mesh = new UnityEngine.Mesh
            {
                vertices = new[] { vector }
            };
            return mesh;
        }

        private static List<UnityEngine.Mesh> MakeMeshes()
        {
            return new List<UnityEngine.Mesh>
            {
                MakeMesh(Vector3.forward),
                MakeMesh(Vector3.back),
                MakeMesh(Vector3.left),
            };
        }
        
        
        #endregion
    }
}