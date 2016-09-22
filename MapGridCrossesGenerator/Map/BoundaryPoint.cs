namespace MapGridCrossesGenerator.Map
{
    using Contracts;

    public class BoundaryPoint : IPoint
    {
        private readonly double x;
        private readonly double y;

        public BoundaryPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X
        {
            get
            {
                return this.x;
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
        }
    }
}