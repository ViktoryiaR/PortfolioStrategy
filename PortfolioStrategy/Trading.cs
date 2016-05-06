using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioStrategy
{
    static class Trading
    {
        static public void Trade(AssetModel model, double threshold, double[] w, double c, double estAverageVolume ,double[][][] kmeans)
        {
            var position = 0;
            var bank = 0.0;

            var error = 0.0;

            bool isTrade = false;
            var profits = new List<double>();
            var cumProfit = 0.0;

            var n = model.DayInformations.Length - 1;
            var dp = new double[n][];
            var r = new double[n];

            for (var i = 0; i < n; i++)
            {
                dp[i] = new double[kmeans.Length]; //number of regression intervals

                for (var j = 0; j < kmeans.Length; j++)
                {
                    dp[i][j] = BayesianRegression.Bayesian(model.DayInformations[i].LastDaysPrices[j],
                        kmeans[j], c);
                }

                r[i] = model.DayInformations[i].Volume / estAverageVolume;

                var dP = w[0];
                for (var j = 0; j < dp[i].Length; j++)
                {
                    dP += dp[i][j] * w[j + 1];
                }
                dP += r[i] * w[dp[i].Length + 1];

                error += Math.Pow(model.DayInformations[i + 1].Price - (model.DayInformations[i].Price + dP), 2);

                //BUY
                if (dP > threshold && position <= 0)
                {
                    position++;
                    bank -= model.DayInformations[i].Price;
                    Console.WriteLine("BUY - Bank: " + bank + " - Position: " + position);
                    isTrade = true;
                }
                //SELL
                if (dP < -threshold && position >= 0)
                {
                    position--;
                    bank += model.DayInformations[i].Price;
                    Console.WriteLine("SELL - Bank: " + bank + " - Position: " + position);
                    isTrade = true;
                }

                if (isTrade && position == 0)
                {
                    profits.Add(bank - cumProfit);
                    cumProfit = bank;
                }

                isTrade = false;
            }

            Console.WriteLine("Error = " + error);
            Console.WriteLine("Bank = " + bank);
        }
    }
}
