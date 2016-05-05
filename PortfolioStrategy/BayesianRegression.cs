using System;

using FuncLib.Functions;

namespace PortfolioStrategy
{
    class BayesianRegression
    {
        static public Function Bayesian(double[] x, double[][] kmeans, Variable c)
        {
            if (x.Length != kmeans[0].Length - 1)
                return double.NaN;

            #region xk, yk

            double[][] xk = new double[kmeans.Length][];
            double[] yk = new double[kmeans.Length];

            for (int i = 0; i < kmeans.Length; i++)
            {
                xk[i] = new double[x.Length];
                for (int j = 0; j < x.Length; j++)
                {
                    xk[i][j] = kmeans[i][j];
                }
                yk[i] = kmeans[i][x.Length];
            }

            #endregion

            Function numerator = 0.0;
            Function denominator = 0.0;

            Function ecs = 0.0;

            for (int i = 0; i < xk.Length; i++)
            {
                ecs = Function.Exp(c * Similarity.VecSim(x, xk[i]));
                numerator += yk[i] * ecs;
                denominator += ecs;
            }

            return numerator / denominator;
        }

        static public double Bayesian(double[] x, double[][] kmeans, double c)
        {
            if (x.Length != kmeans[0].Length - 1)
                return double.NaN;

            #region xk, yk

            double[][] xk = new double[kmeans.Length][];
            double[] yk = new double[kmeans.Length];

            for (int i = 0; i < kmeans.Length; i++)
            {
                xk[i] = new double[x.Length];
                for (int j = 0; j < x.Length; j++)
                {
                    xk[i][j] = kmeans[i][j];
                }
                yk[i] = kmeans[i][x.Length];
            }

            #endregion

            double numerator = 0.0;
            double denominator = 0.0;

            double ecs = 0.0;

            for (int i = 0; i < xk.Length; i++)
            {
                ecs = Math.Exp(c * Similarity.VecSim(x, xk[i]));
                numerator += yk[i] * ecs;
                denominator += ecs;
            }

            return numerator / denominator;
        }
    }
}
