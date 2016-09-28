namespace MapGridCrossesGenerator.Map
{
    using Contracts;

    public class Cross : ICross
    {
        private readonly int x;
        private readonly int y;
        private static string blockName = "MAP-GRID-CROSS";

        public Cross(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static string BlockName
        {
            get
            {
                return Cross.blockName;
            }
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