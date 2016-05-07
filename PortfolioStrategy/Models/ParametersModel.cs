namespace PortfolioStrategy.Models
{
    class ParametersModel
    {
        public int[] NumsRegressionDays { get; set; }

        public int NumLastChanges { get; set; }

        public int NumClusters { get; set; }

        public double C { get; set; }

        public double Threshold { get; set; }

        public double[][][] Kmeans { get; set; }

        public double AverageVolume { get; set; }

        public double[] W { get; set; }

        public ParametersModel() { }

        public ParametersModel(ParametersModel initialParameters)
        {
            this.NumsRegressionDays = initialParameters.NumsRegressionDays;
            this.NumLastChanges = initialParameters.NumLastChanges;
            this.NumClusters = initialParameters.NumClusters;

            this.C = initialParameters.C;
        }
    }
}
