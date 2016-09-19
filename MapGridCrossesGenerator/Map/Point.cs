namespace MapGridCrossesGenerator.Map
{
    using Contracts;

    public class Point : IPoint
    {
        private int x;
        private int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(double x, double y)
        {
            this.x = (int)x;
            this.y = (int)y;
        }

        public int X
        {
            get
            {
                return this.x;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
        }
    }
}