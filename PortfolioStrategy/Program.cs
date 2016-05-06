using System;
using System.Collections.Generic;
using System.Linq;
using FuncLib.Functions;
using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using DateTimeAxis = OxyPlot.Wpf.DateTimeAxis;

namespace PortfolioStrategy
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var numLastDays = new[] {30, 60, 120};
            var numLastChanges = 10;
            var numClusters = 20;

            var ddd = new AssetModel("../../../Assets/DDD.csv", numLastDays);
            PlotAssetPrices(new[] { ddd }, new[] { "DDD" }, new[] { OxyColors.BlueViolet }, "DDD");

            var dddEstimatingPart = ddd.GetFirstTimeInterval(countOfYears: 2);

            var kmeans = new double[numLastDays.Length][][];

            for (var j = 0; j < numLastDays.Length; j++)
            {
                kmeans[j] = KMeans.GetNormalizedMeans(dddEstimatingPart.GetPriceIntervals(numLastDays[j], numLastChanges), numClusters);
            }

            var numSkipedItems = numLastDays.Max();
            var dddRegressionPart = dddEstimatingPart.GetSecondTimeInterval(numSkipedItems);

            var numRegressionItems = dddRegressionPart.DayInformations.Length;

            var averageVolume = dddRegressionPart.DayInformations.Select(_ => _.Volume).Average();

            var c = -0.04;
            var regressionX = new double[numRegressionItems - 1][];
            var regressionY = new double[numRegressionItems - 1];

            var dp = new double[numRegressionItems - 1][];
            var r = new double[numRegressionItems - 1];

            for (var i = 0; i < numRegressionItems - 1; i++)
            {
                dp[i] = new double[numLastDays.Length];

                for (var j = 0; j < numLastDays.Length; j++)
                {
                    dp[i][j] = BayesianRegression.Bayesian(dddRegressionPart.DayInformations[i].LastDaysPrices[j],
                        kmeans[j], c);
                }

                r[i] = dddRegressionPart.DayInformations[i].Volume / averageVolume;

                regressionX[i] = dp[i].AddToEnd(r[i]);

                regressionY[i] = dddRegressionPart.DayInformations[i + 1].Price -
                                 dddRegressionPart.DayInformations[i].Price;
            }

            var w = LeastSquaresEstimate.FindW(regressionX, regressionY);

            var dddEstimatedRegressionPart = CreateEstimatedModel(dddRegressionPart, r, dp, w);

            PlotAssetPrices(
                new[] { dddRegressionPart, dddEstimatedRegressionPart }, 
                new[] { "DDD", "estDDD" }, 
                new[] { OxyColors.DarkOliveGreen, OxyColors.DarkMagenta }, 
                "DDDRegression");

            Console.WriteLine(w[0]);
            Console.WriteLine(w[1]);
            Console.WriteLine(w[2]);
            Console.WriteLine(w[3]);
            Console.WriteLine(w[4]);

            var startIndex = dddEstimatingPart.DayInformations.Length;
            var dddTradingPart = ddd.GetSecondTimeInterval(startIndex);

            var treashold = 0.05;
            Trading.Trade(dddTradingPart, treashold, w, c, averageVolume, kmeans);
        }

        private static AssetModel CreateEstimatedModel(AssetModel model, double[] r, double[][] dp, double[] w)
        {
            var estInformations = new DayInformation[model.DayInformations.Length];
            estInformations[0] = model.DayInformations[0];
            for (var i = 0; i < model.DayInformations.Length - 1; i++)
            {
                var dX = w[0];
                for (var j = 0; j < dp[i].Length; j++)
                {
                    dX += dp[i][j]*w[j + 1];
                }
                dX += r[i]*w[dp[i].Length + 1];

                estInformations[i + 1] = new DayInformation
                {
                    Date = model.DayInformations[i + 1].Date,
                    Price = model.DayInformations[i].Price + dX
                };
            }
            return new AssetModel
            {
                DayInformations = estInformations
            };
        }

        private static void PlotAssetPrices(AssetModel[] models, string[] titles, OxyColor[] colors, string plotTitle)
        {
            var plotModel = new PlotModel
            {
                IsLegendVisible = true,
                Axes = {
                    new OxyPlot.Axes.DateTimeAxis(){
                            Position = AxisPosition.Bottom,
                            IntervalType = DateTimeIntervalType.Months,
                            StringFormat = "yyyy-MM"
                    },
                    new OxyPlot.Axes.LinearAxis(){
                            Position = AxisPosition.Left,
                            Minimum = 0,
                            Maximum = 100
                    }
                }
            };

            for (var i = 0; i < models.Length; i++)
            {
                var series = new OxyPlot.Series.LineSeries()
                {
                    Title = titles[i],
                    Color = colors[i],
                    DataFieldX = "Date"
                };
                foreach (var day in models[i].DayInformations)
                {
                    series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(day.Date), day.Price));
                }
                plotModel.Series.Add(series);
            }

            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, "../../../Assets/" + plotTitle + ".png");
        }
    }
}
