using System;
using FuncLib.Functions;

namespace PortfolioStrategy.Functions
{
    class BayesianRegression
    {
        static public Function Bayesian(double[] x, double[][] kmeans, Variable c)
        {
            double[][] xk = new double[kmeans.Length][];
            double[] yk = new double[kmeans.Length];

            for (int i = 0; i < kmeans.Length; i++)
            {
                xk[i] = kmeans[i].SubArray(0, x.Length - 1);
                yk[i] = kmeans[i][x.Length];
            }

            Function numerator = 0.0;
            Function denominator = 0.0;

            for (int i = 0; i < xk.Length; i++)
            {
                var exp = Function.Exp(c * Similarity.VecSim(x, xk[i]));
                numerator += yk[i] * exp;
                denominator += exp;
            }

            return numerator / denominator;
        }

        static public double Bayesian(double[] x, double[][] kmeans, double c)
        {
            double[][] xk = new double[kmeans.Length][];
            double[] yk = new double[kmeans.Length];

            for (int i = 0; i < kmeans.Length; i++)
            {
                xk[i] = kmeans[i].SubArray(0, x.Length);
                yk[i] = kmeans[i][x.Length];
            }

            double numerator = 0.0;
            double denominator = 0.0;

            for (int i = 0; i < kmeans.Length; i++)
            {
                var exp = Math.Exp(c * Similarity.VecSim(x, xk[i]));
                numerator += yk[i] * exp;
                denominator += exp;
            }

            return numerator / denominator;
        }
    }
}
