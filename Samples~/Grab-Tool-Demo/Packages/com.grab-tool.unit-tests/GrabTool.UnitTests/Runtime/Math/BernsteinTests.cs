using GrabTool.Math;
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

        [Test]
        public void PolynomialDt_AgainstSO_IsTheSame()
        {
            var r = 1 / 3.0f;
            var n = 4;
            var i = 3;
            var mine = Bernstein.PolynomialDt(r, n, i);
            var theirs = Bernstein.PolynomialDtSo(r, n, i);
            
            Assert.That(mine, Is.EqualTo(theirs).Within(0.0000001));
        }
    }
}