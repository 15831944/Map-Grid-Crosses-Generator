namespace MapGridCrossesGenerator.Helpers
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Contracts;

    internal static class GeometryHelper
    {
        public static bool IsLeftSide(Point2d a, Point2d b, IPoint c)
        {
            return (((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X))) > 0;
        }

        public static bool InsidePolygon(Polyline polygon, IPoint point)
        {
            for (int vertexID = 0; vertexID < polygon.NumberOfVertices - 1; vertexID++)
            {
                int nextVertexID = vertexID + 1;

                if (GeometryHelper.IsLeftSide(polygon.GetPoint2dAt(vertexID), polygon.GetPoint2dAt(nextVertexID), point))
                {
                    return false;
                }
            }

            return true;
        }
    }
}