namespace MapGridCrossesGenerator.Tests
{
    using System;
    using Autodesk.AutoCAD.Geometry;
    using Helpers;
    using Contracts;
    using NUnit.Framework;
    using Map;

    public class GeometryHelperTests
    {
        [TestCase(11, 32, true)]
        [TestCase(35, 15, false)]
        public void IsLeftSide_ShouldReturnCorrectResult(int x, int y, bool result)
        {
            Point2d a = new Point2d(14.3429, 17.5163);
            Point2d b = new Point2d(29.9575, 32.0665);
            IPoint c = new Point(x, y);

            Assert.AreEqual(result, GeometryHelper.IsLeftSide(a, b, c));
        }
    }
}