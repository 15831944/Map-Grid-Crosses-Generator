namespace MapGridCrossesGenerator.Tests
{
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
            IPoint lineStartPoint = new BoundaryPoint(14.3429, 17.5163);
            IPoint lineEndPoint = new BoundaryPoint(29.9575, 32.0665);
            ICross point = new Cross(x, y);

            Assert.AreEqual(result, GeometryHelper.IsLeftSide(lineStartPoint, lineEndPoint, point));
        }
    }
}