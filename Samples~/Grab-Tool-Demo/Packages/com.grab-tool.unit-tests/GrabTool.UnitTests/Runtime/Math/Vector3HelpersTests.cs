using GrabTool.Math;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using UnityEngine;

namespace GrabTool.UnitTests.Runtime.Math
{
    public class Vector3HelpersTests
    {
        [Test]
        public void OrthogonalVector_WithXRight_ReturnsNegativeZ()
        {
            var v = Vector3.right;

            var result = Vector3Helpers.OrthogonalVector(v);
            
            Assert.That(result, Is.EqualTo(Vector3.back));
            Assert.That(Vector3.Dot(v, result), Is.EqualTo(0));
        }
        
        [Test]
        public void OrthogonalVector_InXZPlane_ReturnsNegativeY()
        {
            var v = new Vector3(1, 0, 1);

            var result = Vector3Helpers.OrthogonalVector(v);
            
            Assert.That(result, Is.EqualTo(Vector3.down));
            Assert.That(Vector3.Dot(v, result), Is.EqualTo(0));
        }
    }
}