using System;

using FuncLib.Functions;
using FuncLib.Optimization;

namespace PortfolioStrategy
{
    class LeastSquaresEstimate
    {
        public static double[] FindW(Function[][] x, ref Variable c, double[] y)
        {
            Variable w0 = new Variable();
            Variable w1 = new Variable();
            Variable w2 = new Variable();
            Variable w3 = new Variable();
            Variable w4 = new Variable();

            Function f = 0;

            for (int i = 0; i < x.Length; i++)
            {
                f += Function.Pow(y[i] - w0 - w1 * x[i][0] - w2 * x[i][1] - w3 * x[i][2] - w4 * x[i][3], 2);
            }

            Optimizer o = new BfgsOptimizer();
            o.Variables.Add(w0, w1, w2, w3, w4, c);
            o.ObjectiveFunction = f;

            Random r = new Random(1);
            IOptimizerResult or = o.Run(w0 | r.NextDouble(), w1 | r.NextDouble(), w2 | r.NextDouble(), w3 | r.NextDouble(), w4 | r.NextDouble(), c | r.NextDouble());

            double[] w_c = new double[] { or.OptimalPoint[w0], or.OptimalPoint[w1], or.OptimalPoint[w2], or.OptimalPoint[w3], or.OptimalPoint[w4], or.OptimalPoint[c]};

            Console.WriteLine("Minimized to: " + or.OptimalValue);

            return w_c;
        }
    }
}
