using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using PortfolioStrategy.Functions;
using PortfolioStrategy.Models;

namespace PortfolioStrategy
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var initialParameters = new ParametersModel()
            {
                CountOfYearsForEstimation = 2,

                NumsRegressionDays = new[] { 20, 40, 80 },
                NumLastChanges = 10,
                NumClusters = 10,

                C = -0.01
            };

            var assetNames = new []
            {
                "DDD",
                "IBM",
                "TWX",
                "CCE",
                "JNJ",
                "XOM"
            };
            var assetDirectories = new string[assetNames.Length];
            var assetModels = new AssetModel[assetNames.Length];
            var assetEstimatingParts = new AssetModel[assetNames.Length];
            var assetTradingParts = new AssetModel[assetNames.Length];
            var assetParameters = new ParametersModel[assetNames.Length];
            for (var i = 0; i < assetNames.Length; i++)
            {
                assetDirectories[i] = "../../../Assets/" + assetNames[i] + "/";

                assetModels[i] = new AssetModel(assetDirectories[i] + assetNames[i] + ".csv", initialParameters.NumsRegressionDays);
                PlotAssetPrices(
                    new[] { assetModels[i] }, 
                    new[] { assetNames[i] }, 
                    new[] { OxyColors.BlueViolet },
                    assetDirectories[i],
                    assetNames[i] + " - All Time-Series");

                assetEstimatingParts[i] = assetModels[i].GetFirstTimeInterval(initialParameters.CountOfYearsForEstimation);
                assetParameters[i] = EstimateAssetParameters(
                    assetNames[i], 
                    assetDirectories[i],
                    assetEstimatingParts[i], 
                    initialParameters);

                var startIndex = assetEstimatingParts[i].DayInformations.Length;
                assetTradingParts[i] = assetModels[i].GetSecondTimeInterval(startIndex);
            }

            var result = Trading.TradePortfolio(5000.0, 
                //new [] { 0.6713, 0.0000416844, 0.328659 },
                //new [] { 0.129775, 0.208825, 0.6614 },
                //new [] { 0.0161861, 0.177, 0.073236, 0.106351, 0.369994, 0.257233 },
                new [] {1.54406 * Math.Pow(10, -7), 1.49623 * Math.Pow(10, -7), 0.563571, 2.37744 * Math.Pow(10, -6), 0.436426, 3.27853 * Math.Pow(10, -7) }, 
                assetTradingParts, assetParameters);

            Console.WriteLine(result.Bank);
            Console.WriteLine(result.PortfolioBank);
            foreach (var w in result.Weights)
            {
                Console.WriteLine(w);
            }
            PlotData(
                new [] { result.BankDynamic, result.GMBankDynamic},
                new[] { "Bank Dynamic", "GM Bank Dynamimc (MaxReturn)" },
                new[] { OxyColors.Green , OxyColors.Red }, 
                "../../../Assets/", "Portfolio - Bank Dynamic");
            //PlotCumulativeProfits(assetResult.CumulativeProfits, "Bank", OxyColors.RosyBrown, assetDirectory, assetName + " - Bank Dynamic");
            //PlotProfits(assetResult.Profits, "Profits", OxyColors.Navy, assetDirectory, assetName + " - Profits",
            //    assetResult.CumulativeProfits[0].Date, assetResult.CumulativeProfits[assetResult.CumulativeProfits.Count - 1].Date);

            //PlotBankDynamicAndProfits(assetResult.CumulativeProfits, assetResult.Profits, 
            //    "Bank" , "Profits", OxyColors.RosyBrown, OxyColors.Navy, assetDirectory, assetName + " - Bank Dynamic and Profits");
        }

        private static void PlotData(List<ValueOnDate>[] data, string[] title, OxyColor[] color, string directory, string plotTitle)
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
                            Maximum = 1.01 * data[0].Max(_ => _.Value),
                            Minimum = -1.01 * data[0].Min(_ => _.Value)
                    }
                }
            };

            for(var i = 0; i < data.Length; i++)
            {
                var series = new OxyPlot.Series.LineSeries()
                {
                    Title = title[i],
                    Color = color[i],
                    DataFieldX = "Date"
                };
                foreach (var day in data[i])
                {
                    series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(day.Date), day.Value));
                }
                plotModel.Series.Add(series);
            }

            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, directory + plotTitle + ".png");
        }

        private static ParametersModel EstimateAssetParameters(string assetName, string assetDirectory, AssetModel estimationModel, ParametersModel initialParameters)
        {
            var parameters = new ParametersModel(initialParameters);

            parameters.Kmeans = new double[parameters.NumsRegressionDays.Length][][];
            for (var j = 0; j < parameters.NumsRegressionDays.Length; j++)
            {
                parameters.Kmeans[j] = KMeans.GetNormalizedMeans(
                    estimationModel.GetPriceIntervals(parameters.NumsRegressionDays[j], parameters.NumLastChanges),
                    parameters.NumClusters);
            }

            var dddRegressionPart = estimationModel.GetSecondTimeInterval(parameters.NumsRegressionDays.Max());
            var numRegressionItems = dddRegressionPart.DayInformations.Length;

            var regressionX = new double[numRegressionItems - 1][];
            var regressionY = new double[numRegressionItems - 1];

            var dp = new double[numRegressionItems - 1][];
            var r = new double[numRegressionItems - 1];

            parameters.AverageVolume = dddRegressionPart.DayInformations.Select(_ => _.Volume).Average();

            for (var i = 0; i < numRegressionItems - 1; i++)
            {
                dp[i] = new double[parameters.NumsRegressionDays.Length];

                for (var j = 0; j < parameters.NumsRegressionDays.Length; j++)
                {
                    dp[i][j] = BayesianRegression.Bayesian(dddRegressionPart.DayInformations[i].LastDaysPrices[j],
                        parameters.Kmeans[j], parameters.C);
                }

                r[i] = dddRegressionPart.DayInformations[i].Volume / parameters.AverageVolume;

                regressionX[i] = dp[i].AddToEnd(r[i]);

                regressionY[i] = dddRegressionPart.DayInformations[i + 1].Price -
                                 dddRegressionPart.DayInformations[i].Price;
            }

            parameters.W = LeastSquaresEstimate.FindW(regressionX, regressionY);

            var dddEstimatedRegressionPart = CreateEstimatedModel(dddRegressionPart, r, dp, parameters.W);

            PlotAssetPrices(
                new[] { dddRegressionPart, dddEstimatedRegressionPart },
                new[] { assetName, "est" + assetName },
                new[] { OxyColors.DarkOliveGreen, OxyColors.DarkMagenta },
                assetDirectory, 
                assetName + " - Regression Part Estimation");

            var meanChangeAbs = regressionY.Select(Math.Abs).Average();
            var maxBank = 0.0;
            var bestThreshold = 0.0;
            for (var t = meanChangeAbs / 100; t < 2 * meanChangeAbs; t += meanChangeAbs / 100)
            {
                parameters.Threshold = t;
                var bank = Trading.Trade(dddRegressionPart, parameters).Bank;

                if (bank <= maxBank) continue;

                maxBank = bank;
                bestThreshold = t;
            }
            parameters.Threshold = bestThreshold;

            return parameters;
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

        private static void PlotAssetPrices(AssetModel[] models, string[] titles, OxyColor[] colors, string directory, string plotTitle)
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
                            Minimum = 0
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
            pngExporter.ExportToFile(plotModel, directory + plotTitle + ".png");
        }

        private static void PlotCumulativeProfits(List<ValueOnDate> cumulativeProfits, string title, OxyColor color, string directory, string plotTitle)
        {
            var maxValue = cumulativeProfits.Select(_ => Math.Abs(_.Value)).Max();
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
                            Maximum = 1.2*maxValue,
                            Minimum = -1.2*maxValue
                    }
                }
            };

            var series = new OxyPlot.Series.LineSeries()
            {
                Title = title,
                Color = color,
                DataFieldX = "Date"
            };
            foreach (var day in cumulativeProfits)
            {
                series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(day.Date), day.Value));
            }
            plotModel.Series.Add(series);

            var series0 = new OxyPlot.Series.LineSeries()
            {
                Color = OxyColors.Black,
                StrokeThickness = 1,
                DataFieldX = "Date"
            };
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(cumulativeProfits[0].Date), 0));
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(cumulativeProfits[cumulativeProfits.Count - 1].Date), 0));
            plotModel.Series.Add(series0);

            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, directory + plotTitle + ".png");
        }

        private static void PlotBankDynamicAndProfits(List<ValueOnDate> cumulativeProfits, List<ValueOnDate> profits, 
            string titleCp, string titleP, OxyColor colorCp, OxyColor colorP, string directory, string plotTitle)
        {
            var maxValue = cumulativeProfits.Select(_ => Math.Abs(_.Value)).Max();
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
                            Maximum = 1.2*maxValue,
                            Minimum = -1.2*maxValue
                    }
                }
            };

            var series = new OxyPlot.Series.LineSeries()
            {
                Title = titleCp,
                Color = colorCp,
                DataFieldX = "Date"
            };
            foreach (var day in cumulativeProfits)
            {
                series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(day.Date), day.Value));
            }
            plotModel.Series.Add(series);

            var first = true;
            foreach (var profit in profits)
            {
                var profitSeries = new OxyPlot.Series.LineSeries()
                {
                    Color = colorP,
                    DataFieldX = "Date",
                    MarkerSize = 5,
                    MarkerFill = OxyColors.Red,
                    MarkerType = MarkerType.Circle
                };
                if (first)
                {
                    profitSeries.Title = titleP;
                    first = false;
                }
                profitSeries.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(profit.Date), profit.Value));
                profitSeries.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(profit.Date), 0));

                plotModel.Series.Add(profitSeries);
            }

            var series0 = new OxyPlot.Series.LineSeries()
            {
                Color = OxyColors.Black,
                StrokeThickness = 1,
                DataFieldX = "Date"
            };
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(cumulativeProfits[0].Date), 0));
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(cumulativeProfits[cumulativeProfits.Count - 1].Date), 0));
            plotModel.Series.Add(series0);

            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, directory + plotTitle + ".png");
        }

        private static void PlotProfits(List<ValueOnDate> profits, string title, OxyColor color, string directory, string plotTitle, DateTime start, DateTime end)
        {
            var maxValue = profits.Select(_ => Math.Abs(_.Value)).Max();
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
                            Maximum = 1.2*maxValue,
                            Minimum = -1.2*maxValue
                    }
                }
            };

            var first = true;
            foreach (var profit in profits)
            {
                var series = new OxyPlot.Series.LineSeries()
                {
                    Color = color,
                    DataFieldX = "Date",
                    MarkerSize = 5,
                    MarkerFill = OxyColors.Red,
                    MarkerType = MarkerType.Circle
                };
                if (first)
                {
                    series.Title = title;
                    first = false;
                }
                series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(profit.Date), profit.Value));
                series.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(profit.Date), 0));

                plotModel.Series.Add(series);
            }

            var series0 = new OxyPlot.Series.LineSeries()
            {
                Color = OxyColors.Black,
                DataFieldX = "Date",
                StrokeThickness = 1
            };
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(start), 0));
            series0.Points.Add(new DataPoint(
                OxyPlot.Axes.DateTimeAxis.ToDouble(end), 0));
            plotModel.Series.Add(series0);

            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(plotModel, directory + plotTitle + ".png");
        }
    }
}
