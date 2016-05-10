using PortfolioStrategy.Models;

namespace PortfolioStrategy.Functions
{
    static class MarkowitzPortfolio
    {
        public static double[] CreateOptimalPortfolio(AssetModel[] models)
        {
            var nassets = models.Length;
            var weights = new double[nassets];

            return weights;
        }
    }
}
