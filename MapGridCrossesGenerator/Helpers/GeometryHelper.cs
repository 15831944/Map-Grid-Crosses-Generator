namespace MapGridCrossesGenerator.Helpers
{
    using System.Linq;
    using Contracts;
    using Autodesk.AutoCAD.DatabaseServices;

    internal static class GeometryHelper
    {
        public static bool IsPointInPolygon(IPoint[] polygon, IPoint testPoint)
        {
            bool result = false;
            int j = polygon.Count() - 1;

            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }

        public static bool IsPointInsideSheet(Polyline polygon, IPoint point)
        {
            for (int i = 0; i < polygon.EndParam - 1; i++)
            {
                int j = i + 1;

                if (((polygon.GetPoint2dAt(j).X - polygon.GetPoint2dAt(i).X) * (point.Y - polygon.GetPoint2dAt(i).Y))
                      - ((point.X - polygon.GetPoint2dAt(i).X) * (polygon.GetPoint2dAt(j).Y - polygon.GetPoint2dAt(i).Y)) < 0.000)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsPointInsideSheet(IPoint[] points, IPoint point)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                int j = i + 1;

                if (((points[j].X - points[i].X) * (point.Y - points[i].Y))
                      - ((point.X - points[i].X) * (points[j].Y - points[i].Y)) < 0.000)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
