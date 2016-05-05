using System;
using System.IO;

namespace PortfolioStrategy
{
    class AssetModel
    {
        public DateTime[] Dates { get; set; }

        public double[] Prices { get; set; }

        public double[] Volume { get; set; }

        public AssetModel() {}

        public AssetModel(string sourceFilePath)
        {
            string[] lines = { };
            using (StreamReader reader = new StreamReader(sourceFilePath))
            {
                lines = reader.ReadToEnd().Split(lines, StringSplitOptions.RemoveEmptyEntries);
            }

            var m = lines.Length - 1;

            var prices = new double[m];
            var volume = new double[m];
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
                prices[i] = double.Parse(row[1].Replace('.', ','));
                volume[i] = double.Parse(row[5].Replace('.', ','));
            }

            this.Dates = dates;
            this.Prices = prices;
            this.Volume = volume;
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

            return new AssetModel()
            {
                Dates = this.Dates.SubArray(0, length),
                Prices = this.Prices.SubArray(0, length),
                Volume = this.Volume.SubArray(0, length)
            };
        }

        public AssetModel GetSecondTimeInterval(int startIndex)
        {
            var length = this.Dates.Length - startIndex;

            return new AssetModel()
            {
                Dates = this.Dates.SubArray(startIndex, length),
                Prices = this.Prices.SubArray(startIndex, length),
                Volume = this.Volume.SubArray(startIndex, length)
            };
        }
    }
}
