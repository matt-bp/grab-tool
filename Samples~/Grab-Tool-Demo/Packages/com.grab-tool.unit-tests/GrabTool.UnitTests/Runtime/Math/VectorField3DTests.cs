using NUnit.Framework;
using UnityEngine;

namespace GrabTool.UnitTests.Runtime.Math
{
    public class VectorField3DTests
    {
        [Test]
        public void GetVelocity_WithNewPoint_ShouldReturnNan()
        {
            var vf = new GrabTool.Math.VectorField3D
            {
                C = new Vector3(2.38658714f, -12.8863211f, 0),
                DesiredTranslation = new Vector3(-2.28419328f, 11.9880772f, 0),
                Ri = 1,
                Ro = 4
            };
            var point = new Vector3(2.38658714f, -11.8863211f, 0);

            var result = vf.GetVelocity(point);

            Assert.That(result.x, Is.NaN);
            Assert.That(result.y, Is.NaN);
            Assert.That(result.z, Is.NaN);
        }
    }
}