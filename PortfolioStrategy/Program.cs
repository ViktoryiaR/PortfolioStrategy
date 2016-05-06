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

            //var startIndex = dddEstimatingPart.DayInformations.Length;
            //var dddTradingPart = ddd.GetSecondTimeInterval(startIndex);

            var kmeans = new double[numLastDays.Length][][];

            for (var j = 0; j < numLastDays.Length; j++)
            {
                kmeans[j] = KMeans.GetNormalizedMeans(dddEstimatingPart.GetPriceIntervals(numLastDays[j], numLastChanges), numClusters);
            }

            var numSkipedItems = numLastDays.Max();
            var dddRegressionPart = dddEstimatingPart.GetSecondTimeInterval(numSkipedItems);
            PlotAssetPrices(new[] { dddRegressionPart }, new[] { "DDDRegressionPart" }, new[] { OxyColors.DarkOliveGreen }, "DDDRegressionPart");

            var numRegressionItems = dddRegressionPart.DayInformations.Length;

            var averageVolume = dddRegressionPart.DayInformations.Select(_ => _.Volume).Average();

            var c = new Variable();
            var regressionX = new Function[numRegressionItems - 1][];
            //var c = -0.01;
            //var regressionX = new double[numRegressionItems - 1][];
            var regressionY = new double[numRegressionItems - 1];

            var alldp = new double[numRegressionItems - 1][];
            var allr = new double[numRegressionItems - 1];

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

                //alldp[i] = dp;
                //allr[i] = r;
            }

            var wAndC = LeastSquaresEstimate.FindWandC(regressionX, regressionY, ref c);
            //var wAndC = LeastSquaresEstimate.FindWandC(regressionX, regressionY);

            //var estPrices = new double[numRegressionItems];
            //estPrices[0] = dddRegressionPart.DayInformations[0].Price;
            //for (var i = 0; i < numRegressionItems - 1; i++)
            //{
            //    var dp = wAndC[0] + alldp[i][0]*wAndC[1] + alldp[i][1]*wAndC[2] + alldp[i][2]*wAndC[3] +
            //             allr[i]*wAndC[4];
            //    estPrices[i + 1] = estPrices[i] + dp;
            //}

            //var plotModel = new PlotModel
            //{
            //    IsLegendVisible = true,
            //    Axes = {
            //        new OxyPlot.Axes.DateTimeAxis(){
            //                Position = AxisPosition.Bottom,
            //                IntervalType = DateTimeIntervalType.Months,
            //                StringFormat = "yyyy-MM"
            //        },
            //        new OxyPlot.Axes.LinearAxis(){
            //                Position = AxisPosition.Left,
            //                Minimum = 0,
            //                Maximum = 100
            //        }
            //    }
            //};

            //var series = new OxyPlot.Series.LineSeries()
            //{
            //    Title = "estDDD",
            //    Color = OxyColors.Aqua,
            //    DataFieldX = "Date"
            //};

            //var dates = dddRegressionPart.DayInformations.Select(_ => _.Date).ToArray();
            //for(var i = 0; i < estPrices.Length; i++)
            //{
            //    series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(dates[i]), estPrices[i]));
            //}
            //plotModel.Series.Add(series);

            //var pngExporter = new PngExporter { Width = 600, Height = 400, Background = OxyColors.White };
            //pngExporter.ExportToFile(plotModel, "../../../Assets/" + "estRegrPart" + ".png");

            Console.WriteLine(wAndC[0]);
            Console.WriteLine(wAndC[1]);
            Console.WriteLine(wAndC[2]);
            Console.WriteLine(wAndC[3]);
            Console.WriteLine(wAndC[4]);


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

            var pngExporter = new PngExporter { Width = 600, Height = 400, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, "../../../Assets/" + plotTitle + ".png");
        }
    }
}
