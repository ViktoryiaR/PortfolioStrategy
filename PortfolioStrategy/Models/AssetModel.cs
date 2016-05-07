using System;
using System.IO;
using System.Linq;

namespace PortfolioStrategy.Models
{
    class DayInformation
    {
        public DateTime Date { get; set; }

        public double Price { get; set; }

        public int Volume { get; set; }

        public double[][] LastDaysPrices { get; set; }
    }

    class AssetModel
    {
        public DayInformation[] DayInformations { get; set; }

        public AssetModel() { }

        public AssetModel(string sourceFilePath, int[] numRegressionDays)
        {
            string[] lines = { };
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                lines = reader.ReadToEnd().Split(lines, StringSplitOptions.RemoveEmptyEntries);
            }

            var m = lines.Length - 1;

            var dayInformations = new DayInformation[m];

            char[] del = { ';' };

            for (int i = 0; i < m; i++)
            {
                var row = lines[i + 1].Split(del);
                var strdate = row[0];

                dayInformations[i] = new DayInformation
                {
                    Date = new DateTime(int.Parse(strdate.Substring(0, 4)),
                                        int.Parse(strdate.Substring(4, 2)),
                                        int.Parse(strdate.Substring(6, 2))),
                    Price = double.Parse(row[1]),
                    Volume = int.Parse(row[5])
                };

                if (i < numRegressionDays.Max()) continue;

                dayInformations[i].LastDaysPrices = new double[numRegressionDays.Length][];

                for (int j = 0; j < numRegressionDays.Length; j++)
                {
                    dayInformations[i].LastDaysPrices[j] = dayInformations
                        .SubArray(i - numRegressionDays[j], numRegressionDays[j]).Select(_ => _.Price).ToArray();
                }
            }

            this.DayInformations = dayInformations;
        }

        public double[][] GetPriceIntervals(int intervalsLength, int lastChangesLength)
        {
            var intervals = new double[this.DayInformations.Length - intervalsLength][];
            for (int i = 0; i < intervals.Length; i++)
            {
                var x = this.DayInformations.Select(_ => _.Price).ToArray().SubArray(i, intervalsLength);

                var lastPriceChanges = new double[lastChangesLength];
                for (int j = 1; j <= lastChangesLength; j++)
                {
                    lastPriceChanges[j - 1] = x[intervalsLength - j] - x[intervalsLength - j - 1];
                }

                var y = lastPriceChanges.Average();

                intervals[i] = new double[intervalsLength + 1];
                x.CopyTo(intervals[i], 0);
                intervals[i][intervalsLength] = y;
            }
            return intervals;
        }

        public AssetModel GetFirstTimeInterval(int countOfYears)
        {
            var length = 0;

            for (int y = 1; y <= countOfYears; y++)
            {
                var year = this.DayInformations[length].Date.Year;
                do
                {
                    length++;
                } while (this.DayInformations[length].Date.Year == year);
            }

            return new AssetModel
            { 
                DayInformations = this.DayInformations.SubArray(0, length)
            };
        }

        public AssetModel GetSecondTimeInterval(int startIndex)
        {
            var length = this.DayInformations.Length - startIndex;

            return new AssetModel
            {
                DayInformations = this.DayInformations.SubArray(startIndex, length)
            };
        }
    }
}
