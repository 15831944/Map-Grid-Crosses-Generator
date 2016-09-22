namespace MapGridCrossesGenerator.Helpers
{
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Contracts;
    using Map;

    internal static class GeometryHelper
    {
        public static bool IsLeftSide(IPoint a, IPoint b, ICross c)
        {
            return (((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X))) > 0;
        }

        public static bool InsidePolygon(Polyline polygon, ICross point)
        {
            int numberOfVertices = polygon.GetPoint2dAt(0) == polygon.GetPoint2dAt(polygon.NumberOfVertices - 1) ? polygon.NumberOfVertices - 1 : polygon.NumberOfVertices;

            for (int vertexID = 0; vertexID < numberOfVertices; vertexID++)
            {
                int nextVertexID = vertexID + 1 == numberOfVertices ? 0 : vertexID + 1;

                Point2d currentVertex = polygon.GetPoint2dAt(vertexID);
                Point2d nextVertex = polygon.GetPoint2dAt(nextVertexID);

                BoundaryPoint a = new BoundaryPoint(currentVertex.X, currentVertex.Y);
                BoundaryPoint b = new BoundaryPoint(nextVertex.X, nextVertex.Y);

                if (GeometryHelper.IsLeftSide(a, b, point))
                {
                    return false;
                }
            }

            return true;
        }
    }
}