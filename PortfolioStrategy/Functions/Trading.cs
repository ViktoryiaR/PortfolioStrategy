using System;
using System.Collections.Generic;
using PortfolioStrategy.Models;

namespace PortfolioStrategy.Functions
{
    static class Trading
    {
        static public void Trade(AssetModel model, ParametersModel parameters)
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
                dp[i] = new double[parameters.Kmeans.Length]; //number of regression intervals

                for (var j = 0; j < parameters.Kmeans.Length; j++)
                {
                    dp[i][j] = BayesianRegression.Bayesian(model.DayInformations[i].LastDaysPrices[j],
                        parameters.Kmeans[j], parameters.C);
                }

                r[i] = model.DayInformations[i].Volume / parameters.AverageVolume;

                var dP = parameters.W[0];
                for (var j = 0; j < dp[i].Length; j++)
                {
                    dP += dp[i][j] * parameters.W[j + 1];
                }
                dP += r[i] * parameters.W[dp[i].Length + 1];

                error += Math.Pow(model.DayInformations[i + 1].Price - (model.DayInformations[i].Price + dP), 2);

                //BUY
                if (dP > parameters.Threshold && position <= 0)
                {
                    position++;
                    bank -= model.DayInformations[i].Price;
                    Console.WriteLine("BUY - Bank: " + bank + " - Position: " + position);
                    isTrade = true;
                }
                //SELL
                if (dP < -parameters.Threshold && position >= 0)
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
