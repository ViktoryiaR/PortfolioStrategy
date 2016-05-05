using System;
using System.IO;

namespace PortfolioStrategy
{
    class Import
    {
        static public void GetDataFromFile(string fileName,out double[] price, out double[] volume, out DateTime[] date)
        {
            string[] lines = { };
            using (StreamReader reader = new StreamReader(fileName))
            {
                lines = reader.ReadToEnd().Split(lines, StringSplitOptions.RemoveEmptyEntries);
            }

            var m = lines.Length - 1;

            price = new double[m];
            volume = new double[m];
            date = new DateTime[m];
            
            char[] del = { ';' };
            string[] row;

            for (int i = 0; i < m; i++)
            {
                row = lines[i + 1].Split(del);
                var strdate = row[0];
                date[i] = new DateTime(
                    int.Parse(strdate.Substring(0, 4)), 
                    int.Parse(strdate.Substring(4, 2)),
                    int.Parse(strdate.Substring(6, 2)));
                price[i] = double.Parse(row[1].Replace('.', ','));
                volume[i] = double.Parse(row[5].Replace('.', ','));
            }
        }

        static public int GetFirstTimeIntervalLength(DateTime[] date, int countOfYears)
        {
            var length = 0;

            for (int y = 1; y <= countOfYears; y++)
            {
                var year = date[length].Year;
                while(date[length + 1].Year == year)
                {
                    length++;
                }
            }

            return length;
        }
    }
}
