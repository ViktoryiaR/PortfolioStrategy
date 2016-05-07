using System;
using System.Linq;
using FuncLib.Functions;
using FuncLib.Optimization;

namespace PortfolioStrategy.Functions
{
    class LeastSquaresEstimate
    {
        public static double[] FindW(double[][] x, double[] y)
        {
            var w = new Variable[x[0].Length + 1];
            for (var j = 0; j < w.Length; j++)
            {
                w[j] = new Variable();
            }

            Function f = 0;

            for (var i = 0; i < x.Length; i++)
            {
                var term = y[i] - w[0];
                for (var j = 0; j < x[i].Length; j++)
                {
                    term -= w[j + 1] * x[i][j];
                }
                f += Function.Pow(term, 2);
            }

            var variables = w;

            Optimizer o = new BfgsOptimizer();
            o.Variables.Add(variables);
            o.ObjectiveFunction = f;

            var r = new Random(1);
            var varAssignment = variables.Select(_ => new VariableAssignment(_, r.NextDouble())).ToArray();
            var or = o.Run(varAssignment);

            var wOpt = variables.Select(_ => or.OptimalPoint[_]).ToArray();

            Console.WriteLine("Minimized to: " + or.OptimalValue);

            return wOpt;
        }
    }
}
