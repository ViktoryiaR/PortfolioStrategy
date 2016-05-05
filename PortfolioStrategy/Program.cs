using System;

namespace PortfolioStrategy
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime[] date;

            double[] dddPrice, dddVolume;

            Import.GetDataFromFile("../../../DDD.csv", out dddPrice, out dddVolume, out date);

            var length = Import.GetFirstTimeIntervalLength(date, 2);

            Console.WriteLine(length);
        }
    }
}
