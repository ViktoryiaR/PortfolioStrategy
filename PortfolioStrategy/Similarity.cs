using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioStrategy
{
    class Similarity
    {
        static public double VecSim(double[] a, double[] b)
        {
            if (a.Length != b.Length)
                return double.NaN;

            double numerator = 0.0;

            double meanA = Mean(a);
            double meanB = Mean(b);

            for (int i = 0; i < a.Length; i++)
            {
                numerator += (a[i] - meanA) * (b[i] - meanB);
            }

            double denominator = a.Length * Std(a) * Std(b);

            return numerator / denominator;
        }

        static public double Mean(double[] a)
        {
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i];
            }
            return sum / a.Length;
        }

        static public double Std(double[] a)
        {
            double sum = 0.0;
            double meanA = Mean(a);

            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] - meanA) * (a[i] - meanA);
            }
            return sum / a.Length;
        }
    }
}
