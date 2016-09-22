namespace MapGridCrossesGenerator.Map
{
    using System.Collections.Generic;
    using Contracts;

    public static class MapGrid
    {
        public static ICollection<ICross> GenerateCrosses(IPoint lowerLeftPoint, IPoint upperRightPoint, int scale)
        {
            ICollection<ICross> crosses = new List<ICross>();

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
                    Cross cross = new Cross(x, y);

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