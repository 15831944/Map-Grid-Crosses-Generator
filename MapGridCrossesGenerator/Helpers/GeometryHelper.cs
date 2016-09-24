namespace MapGridCrossesGenerator.Helpers
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Contracts;
    using Map;

    internal static class GeometryHelper
    {
        private static double[] constant;
        private static double[] multiple;

        public static bool IsLeftSide(IPoint lineStartPoint, IPoint lineEndPoint, ICross point)
        {
            return (((lineEndPoint.X - lineStartPoint.X) * (point.Y - lineStartPoint.Y)) - ((lineEndPoint.Y - lineStartPoint.Y) * (point.X - lineStartPoint.X))) > 0;
        }

        public static bool InsidePolygon(Polyline polygon, ICross point)
        {
            int numberOfVertices = polygon.GetPoint2dAt(0) == polygon.GetPoint2dAt(polygon.NumberOfVertices - 1) ? polygon.NumberOfVertices - 1 : polygon.NumberOfVertices;

            for (int vertexID = 0; vertexID < numberOfVertices; vertexID++)
            {
                int nextVertexID = vertexID + 1 == numberOfVertices ? 0 : vertexID + 1;

                Point2d currentVertex = polygon.GetPoint2dAt(vertexID);
                Point2d nextVertex = polygon.GetPoint2dAt(nextVertexID);

                BoundaryPoint lineStartPoint = new BoundaryPoint(currentVertex.X, currentVertex.Y);
                BoundaryPoint lineEndPoint = new BoundaryPoint(nextVertex.X, nextVertex.Y);

                if (GeometryHelper.IsLeftSide(lineStartPoint, lineEndPoint, point))
                {
                    return false;
                }
            }

            return true;
        }

        public static IPoint[] CreatePolygonFromPolyline(Polyline polyline)
        {
            IPoint[] polygon = new IPoint[polyline.NumberOfVertices];

            for (int vertexID = 0; vertexID < polyline.NumberOfVertices; vertexID++)
            {
                Point2d vertex = polyline.GetPoint2dAt(vertexID);

                polygon[vertexID] = new BoundaryPoint(vertex.X, vertex.Y);
            }
            
            return polygon;
        }

        public static void InitializePolygon(IPoint[] polygon)
        {
            GeometryHelper.constant = new double[polygon.Length];
            GeometryHelper.multiple = new double[polygon.Length];

            int i, j = polygon.Length - 1;

            for (i = 0; i < polygon.Length; i++)
            {
                if (polygon[j].Y == polygon[i].Y)
                {
                    constant[i] = polygon[i].X;
                    multiple[i] = 0;
                }
                else
                {
                    constant[i] = polygon[i].X - (polygon[i].Y * polygon[j].X) / (polygon[j].Y - polygon[i].Y) + (polygon[i].Y * polygon[i].X) / (polygon[j].Y - polygon[i].Y);
                    multiple[i] = (polygon[j].X - polygon[i].X) / (polygon[j].Y - polygon[i].Y);
                }

                j = i;
            }
        }

        public static bool InsideComplexPolygon(IPoint[] polygon, ICross point)
        {
            int i, j = polygon.Length - 1;
            bool insidePolygon = false;

            for (i = 0; i < polygon.Length; i++)
            {
                if ((polygon[i].Y < point.Y && polygon[j].Y >= point.Y) || (polygon[j].Y < point.Y && polygon[i].Y >= point.Y))
                {
                    insidePolygon ^= (point.Y * multiple[i] + constant[i] < point.X);
                }

                j = i;
            }

            return insidePolygon;
        }
    }
}