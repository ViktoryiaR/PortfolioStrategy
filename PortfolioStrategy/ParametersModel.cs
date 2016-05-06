namespace PortfolioStrategy
{
    class ParametersModel
    {
        public int[] NumLastDays { get; set; }

        public int NumLastChanges { get; set; }

        public int NumClusters { get; set; }

        public double C { get; set; }

        public double Treashold { get; set; }

        public double[][][] Kmeans { get; set; }

        public double AverageVolume { get; set; }

        public double[] W { get; set; }
    }
}
