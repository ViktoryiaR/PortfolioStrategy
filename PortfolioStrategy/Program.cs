using System;
using System.Linq;
using FuncLib.Functions;

namespace PortfolioStrategy
{
    class Program
    {
        static void Main(string[] args)
        {
            var numLastDays = new[] {30, 60, 120};
            var numLastChanges = 10;
            var numClusters = 20;

            var ddd = new AssetModel("../../../DDD.csv", numLastDays);

            var dddEstimatingPart = ddd.GetFirstTimeInterval(countOfYears: 2);

            //var startIndex = dddEstimatingPart.DayInformations.Length;
            //var dddTradingPart = ddd.GetSecondTimeInterval(startIndex);

            var kmeans = new double[numLastDays.Length][][];

            for (var j = 0; j < numLastDays.Length; j++)
            {
                kmeans[j] = KMeans.GetNormalizedMeans(dddEstimatingPart.GetPriceIntervals(numLastDays[j], numLastChanges), numClusters);
            }

            var numSkipedItems = numLastDays.Max();
            var dddRegressionPart = dddEstimatingPart.GetSecondTimeInterval(numSkipedItems);

            var numRegressionItems = dddRegressionPart.DayInformations.Length;

            var averageVolume = dddRegressionPart.DayInformations.Select(_ => _.Volume).Average();

            var c = new Variable();
            var regressionX = new Function[numRegressionItems - 1][];
            //var c = -1;
            //var regressionX = new double[numRegressionItems - 1][];
            var regressionY = new double[numRegressionItems - 1];

            for (var i = 0; i < numRegressionItems - 1; i++)
            {
                var dp = new Function[numLastDays.Length];
                //var dp = new double[numLastDays.Length];

                for (var j = 0; j < numLastDays.Length; j++)
                {
                    dp[j] = BayesianRegression.Bayesian(dddRegressionPart.DayInformations[i].LastDaysPrices[j],
                        kmeans[j], c);
                }

                var r = dddRegressionPart.DayInformations[i].Volume / averageVolume;

                regressionX[i] = dp.AddToEnd(r);

                regressionY[i] = dddRegressionPart.DayInformations[i + 1].Price -
                                 dddRegressionPart.DayInformations[i].Price;
            }

            var wAndC = LeastSquaresEstimate.FindWandC(regressionX, regressionY, ref c);
            //var wAndC = LeastSquaresEstimate.FindWandC(regressionX, regressionY);

            Console.WriteLine(wAndC[0]);
            Console.WriteLine(wAndC[1]);
            Console.WriteLine(wAndC[2]);
            Console.WriteLine(wAndC[3]);
            Console.WriteLine(wAndC[4]);
        }
    }
}
