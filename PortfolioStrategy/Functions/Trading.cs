﻿using System;
using System.Collections.Generic;
using System.Linq;
using PortfolioStrategy.Models;

namespace PortfolioStrategy.Functions
{
    class Trading
    {
        public static PortfolioResultModel TradePortfolio(double additionalBank, double[] weights, AssetModel[] models, ParametersModel[] parameters)
        {
            var bank = additionalBank;

            var ndays = models[0].DayInformations.Length - 1;
            var nassets = models.Length;

            for (var i = 0; i < ndays; i++)
            {
                for (var j = 0; j < nassets; j++)
                {
                    var dp = new double[parameters[j].Kmeans.Length];
                    for (var k = 0; k < dp.Length; k++)
                    {
                        dp[k] = BayesianRegression.Bayesian(models[j].DayInformations[i].LastDaysPrices[k],
                            parameters[j].Kmeans[k], parameters[j].C);
                    }
                    var r = models[j].DayInformations[i].Volume / parameters[j].AverageVolume;

                    var dP = parameters[j].W[0];
                    for (var k = 0; k < dp.Length; k++)
                    {
                        dP += dp[k] * parameters[j].W[k + 1];
                    }
                    dP += r * parameters[j].W[dp.Length + 1];

                    //error += Math.Pow(model.DayInformations[i + 1].Price - (model.DayInformations[i].Price + dP), 2);

                    //BUY
                    if (dP > parameters[j].Threshold && bank >= models[j].DayInformations[i].Price)
                    {
                        //position++;
                        bank -= models[j].DayInformations[i].Price;
                        weights[j]++;
                        //isTrade = true;
                    }
                    //SELL
                    if (dP < -parameters[j].Threshold && weights[j] >= 1)
                    {
                        //position--;
                        bank += models[j].DayInformations[i].Price;
                        weights[j]--;
                        //isTrade = true;
                    }
                }
            }
            return new PortfolioResultModel
            {
                Bank = bank,
                Weights = weights
            };
        }

        public static ResultModel Trade(AssetModel model, ParametersModel parameters)
        {
            var position = 0;
            var bank = 0.0;

            var error = 0.0;

            int L = 0;

            bool isTrade = false;
            var profits = new List<ValueOnDate>();
            var cumulativeProfits = new List<ValueOnDate>();
            var lastBank = 0.0;

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
                    isTrade = true;
                }
                //SELL
                if (dP < -parameters.Threshold && position >= 0)
                {
                    position--;
                    bank += model.DayInformations[i].Price;
                    isTrade = true;
                }

                if (isTrade && position == 0)
                {
                    L++;
                    profits.Add(new ValueOnDate {
                        Date = model.DayInformations[i].Date,
                        Value = bank - lastBank
                    });
                    lastBank = bank;
                }

                isTrade = false;
                cumulativeProfits.Add(new ValueOnDate
                {
                    Date = model.DayInformations[i].Date,
                    Value = bank
                });
            }

            if (position == 1)
            {
                position--;

                bank += model.DayInformations[n].Price;

                L++;
                profits.Add(new ValueOnDate
                {
                    Date = model.DayInformations[n].Date,
                    Value = bank - lastBank
                });
                cumulativeProfits.Add(new ValueOnDate
                {
                    Date = model.DayInformations[n].Date,
                    Value = bank
                });
                lastBank = bank;
            }
            if (position == -1)
            {
                position++;
                bank -= model.DayInformations[n].Price;

                L++;
                profits.Add(new ValueOnDate
                {
                    Date = model.DayInformations[n].Date,
                    Value = bank - lastBank
                });
                cumulativeProfits.Add(new ValueOnDate
                {
                    Date = model.DayInformations[n].Date,
                    Value = bank
                });
                lastBank = bank;
            }

            error /= n;

            double meanP = Similarity.Mean(profits.Select(_ => _.Value).ToArray());
            double stdP = Similarity.Std(profits.Select(_ => _.Value).ToArray());

            double C = Math.Abs(model.DayInformations[0].Price - model.DayInformations[n].Price);
            double sharpeRatio = (meanP * L - C) / L / stdP;

            return new ResultModel()
            {
                Bank = bank,
                AveragePredictionError = error,
                Profits = profits,
                CumulativeProfits = cumulativeProfits,
                SharpeRatio = sharpeRatio
            };
        }
    }
}
