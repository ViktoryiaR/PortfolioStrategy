using System;

namespace PortfolioStrategy
{
    class Program
    {
        static void Main(string[] args)
        {
            var ddd = new AssetModel("../../../DDD.csv");

            var dddEstimatingPart = ddd.GetFirstTimeInterval(countOfYears: 2);

            var startIndex = dddEstimatingPart.Dates.Length;
            var dddTradingPart = ddd.GetSecondTimeInterval(startIndex);

            Console.WriteLine(ddd.Dates.Length);
            Console.WriteLine(dddEstimatingPart.Dates.Length);
            Console.WriteLine(dddTradingPart.Dates.Length);
        }
    }
}
