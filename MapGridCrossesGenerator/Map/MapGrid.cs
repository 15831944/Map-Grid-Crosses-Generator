namespace MapGridCrossesGenerator.Map
{
    using System.Collections.Generic;
    using Contracts;

    public static class MapGrid
    {
        public static ICollection<IPoint> GenerateCrosses(IPoint lowerLeftPoint, IPoint upperRightPoint, int scale)
        {
            ICollection<IPoint> crosses = new List<IPoint>();

            int gridSize = MapGrid.GetGridSizeByMapScale(scale);

            int lowerLeftPointReductionX = (int)lowerLeftPoint.X % gridSize;
            int lowerLeftPointReductionY = (int)lowerLeftPoint.Y % gridSize;
            int upperRigthPointReductionX = (int)upperRightPoint.X % gridSize;
            int upperRigthPointReductionY = (int)upperRightPoint.Y % gridSize;

            int originX = (int)lowerLeftPoint.X - lowerLeftPointReductionX;
            int originY = (int)lowerLeftPoint.Y - lowerLeftPointReductionY;

            int limitX = (int)upperRightPoint.X - upperRigthPointReductionX + gridSize;
            int limitY = (int)upperRightPoint.Y - upperRigthPointReductionY + gridSize;

            for (int x = originX; x <= limitX; x += gridSize)
            {
                for (int y = originY; y <= limitY; y += gridSize)
                {
                    Point cross = new Point(x, y);

                    crosses.Add(cross);
                }
            }

            return crosses;
        }

        private static int GetGridSizeByMapScale(int scale)
        {
            return scale / 10;
        }
    }
}