using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioStrategy
{
    class Trading
    {
        static public void Trade(double t, ref StreamWriter swTreshold,double[] prices, double[] askV, double[] bidV, double[] w_c,
            double[][] kmeans30s, double[][] kmeans60s, double[][] kmeans120s, int d30, int d60, int d120)
        {
            double c = w_c[5];

            double threshold = t;
            double position = 0;
            double bank = 0;

            double error = 0;

            int L = 0;
            bool isTrade = false;
            List<double> profits = new List<double>();
            double cumProfit = 0.0;
            List<double> holdTime = new List<double>();
            double curHoldTime = 0;

            //StreamWriter writerProfit = new StreamWriter("PROFIT.csv");
            StreamWriter writer = new StreamWriter("PricesPredictProfits.csv");
            for (int i = 0; i <= d120 + 30; i++)
            {
                writer.WriteLine(prices[i]);
            }

            for (int i = d120 + 30; i < prices.Length - 1; i++)
            {
                #region ith price30, price60, price120

                double[] price30 = new double[d30];
                for (int j = i - d30 + 1; j <= i; j++)
                {
                    price30[j - i + d30 - 1] = prices[j];
                }

                double[] price60 = new double[d60];
                for (int j = i - d60 + 1; j <= i; j++)
                {
                    price60[j - i + d60 - 1] = prices[j];
                }

                double[] price120 = new double[d120];
                for (int j = i - d120 + 1; j <= i; j++)
                {
                    price120[j - i + d120 - 1] = prices[j];
                }

                #endregion

                double dp1 = BayesianRegression.Bayesian(price30, kmeans30s, c);
                double dp2 = BayesianRegression.Bayesian(price60, kmeans60s, c);
                double dp3 = BayesianRegression.Bayesian(price120, kmeans120s, c);

                double r = (bidV[i] - askV[i]) / (bidV[i] + askV[i]);

                double dp = w_c[0] + w_c[1] * dp1 + w_c[2] * dp2 + w_c[3] * dp3 + w_c[4] * r;

                error += Math.Pow(prices[i + 1] - (prices[i] + dp), 2);
                writer.Write(prices[i + 1] + ";" + (prices[i] + dp) + ";");

                //BUY
                if (dp > threshold && position <= 0)
                {
                    position++;
                    bank -= prices[i];
                    Console.WriteLine("BUY - Bank: " + bank + " - Position: " + position);
                    isTrade = true;
                }
                //SELL
                if (dp < - threshold && position >= 0)
                {
                    position--;
                    bank += prices[i];
                    Console.WriteLine("SELL - Bank: " + bank + " - Position: " + position);
                    isTrade = true;
                }

                if (position == 0 && isTrade)
                {
                    //writer.Write(bank + ";");
                    profits.Add(bank - cumProfit);
                    cumProfit = bank;
                    L++;
                }

                if (isTrade == true)
                {
                    holdTime.Add(curHoldTime);
                    curHoldTime = 0;
                }
                else
                {
                    curHoldTime++;
                }

                writer.WriteLine(cumProfit + ";");
                isTrade = false;
            }

            if(position == 1){
                position--;
                bank += prices[prices.Length - 1];
                Console.WriteLine("SELL - Bank: " + bank + " - Position: " + position);
                writer.Write(bank + ";");

                profits.Add(bank - cumProfit);
                cumProfit = bank;
                L++;

                holdTime.Add(curHoldTime);
                curHoldTime = 0;
            }
            if (position == -1)
            {
                position++;
                bank -= prices[prices.Length - 1];
                Console.WriteLine("BUY - Bank: " + bank + " - Position: " + position);
                writer.Write(bank + ";");

                profits.Add(bank - cumProfit);
                cumProfit = bank;
                L++;

                holdTime.Add(curHoldTime);
                curHoldTime = 0;
            }

            //writer.Write(bank + ";");
            writer.Close();

            error /= (prices.Length - 1);

            Console.WriteLine("Bank: " + bank);
            Console.WriteLine("Error: " + error);

            Console.WriteLine();

            double meanP = Similarity.Mean(profits.ToArray());
            double stdP = Similarity.Std(profits.ToArray());

            double C = Math.Abs(prices[0] - prices[prices.Length - 1]);
            double SharpeRatio = (meanP * L - C) / L / stdP;

            Console.WriteLine("L: " + L);
            Console.WriteLine("SharpeRatio: " + SharpeRatio);

            Console.WriteLine();

            double meanHT = Similarity.Mean(holdTime.ToArray());

            Console.WriteLine("Average profit: " + meanP);
            Console.WriteLine("Average holding time: " + meanHT);

            swTreshold.WriteLine(t + ";" + L + ";" + meanP + ";" + bank + ";" + meanHT + ";" + error);
        }
    }
}
