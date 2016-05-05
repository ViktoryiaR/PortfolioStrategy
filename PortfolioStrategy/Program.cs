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

            Console.WriteLine(dddEstimatingPart.Prices.Length);
            Console.WriteLine(dddEstimatingPart.PriceIntervals30Days.Length);
            Console.WriteLine(dddEstimatingPart.PriceIntervals30Days[0].X);
            Console.WriteLine(dddEstimatingPart.PriceIntervals30Days[0].Y);
        }
    }
}
