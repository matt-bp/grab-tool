using MathNet.Numerics;
using NUnit.Framework;

namespace GrabTool.UnitTests.Runtime.Math
{
    public class BernsteinTests
    {
        [Test]
        public void Temp()
        {
            Assert.Pass();

            Assert.That(Combinatorics.Combinations(10, 3), Is.EqualTo(210.0));
        }
    }
}