using System;
using System.IO;

namespace PortfolioStrategy
{
    class AssetModel
    {
        public DateTime[] Dates { get; set; }
        public double[] Prices { get; set; }
        public int[] Volume { get; set; }

        public IntervalModel[] PriceIntervals30Days { get; set; }
        public IntervalModel[] PriceIntervals60Days { get; set; }
        public IntervalModel[] PriceIntervals120Days { get; set; }

        public AssetModel(DateTime[] dates, double[] prices, int[] volume)
        {
            this.Dates = dates;
            this.Prices = prices;
            this.Volume = volume;

            this.PriceIntervals30Days = this.GetPriceIntervals(30);
            this.PriceIntervals60Days = this.GetPriceIntervals(60);
            this.PriceIntervals120Days = this.GetPriceIntervals(120);
        }

        public AssetModel(string sourceFilePath)
        {
            string[] lines = { };
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                lines = reader.ReadToEnd().Split(lines, StringSplitOptions.RemoveEmptyEntries);
            }

            var m = lines.Length - 1;

            var prices = new double[m];
            var volume = new int[m];
            var dates = new DateTime[m];

            char[] del = { ';' };

            for (int i = 0; i < m; i++)
            {
                var row = lines[i + 1].Split(del);
                var strdate = row[0];
                dates[i] = new DateTime(
                    int.Parse(strdate.Substring(0, 4)),
                    int.Parse(strdate.Substring(4, 2)),
                    int.Parse(strdate.Substring(6, 2)));
                prices[i] = double.Parse(row[1]);
                volume[i] = int.Parse(row[5]);
            }

            this.Dates = dates;
            this.Prices = prices;
            this.Volume = volume;

            this.PriceIntervals30Days = GetPriceIntervals(30);
            this.PriceIntervals60Days = GetPriceIntervals(60);
            this.PriceIntervals120Days = GetPriceIntervals(120);
        }

        private IntervalModel[] GetPriceIntervals(int intervalsLength)
        {
            var intervals = new IntervalModel[this.Prices.Length - intervalsLength];
            for (int i = 0; i < intervals.Length; i++)
            {
                intervals[i] = new IntervalModel(this.Prices.SubArray(i, intervalsLength));
            }
            return intervals;
        }

        public AssetModel GetFirstTimeInterval(int countOfYears)
        {
            var length = 0;

            for (int y = 1; y <= countOfYears; y++)
            {
                var year = this.Dates[length].Year;
                do
                {
                    length++;
                } while (this.Dates[length].Year == year);
            }

            return new AssetModel
            (
                this.Dates.SubArray(0, length),
                this.Prices.SubArray(0, length),
                this.Volume.SubArray(0, length)
            );
        }

        public AssetModel GetSecondTimeInterval(int startIndex)
        {
            var length = this.Dates.Length - startIndex;

            return new AssetModel
            (
                this.Dates.SubArray(startIndex, length),
                this.Prices.SubArray(startIndex, length),
                this.Volume.SubArray(startIndex, length)
            );
        }
    }
}
