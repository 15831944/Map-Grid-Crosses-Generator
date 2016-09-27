namespace MapGridCrossesGenerator.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Contracts;
    using Map;

    internal static class FileHelper
    {
        public static ICollection<IPoint> ImportCrossesFromFile(string path)
        {
            ICollection<IPoint> crosses = new List<IPoint>();

            using (StreamReader reader = new StreamReader(path, Encoding.Default))
            {
                while (reader.EndOfStream == false)
                {
                    string[] line = reader.ReadLine().Trim().Split(' ');

                    if (line.Length != 2)
                    {
                        continue;
                    }

                    double x = double.Parse(line[1]);
                    double y = double.Parse(line[0]);

                    IPoint cross = new BoundaryPoint(x, y);

                    crosses.Add(cross);
                }
            }

            return crosses;
        }
    }
}