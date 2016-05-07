using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace PortfolioStrategy.Models
{
    class ValueOnDate
    {
        public DateTime Date { get; set; }

        public double Value { get; set; }
    }
    class ResultModel
    {
        public double Bank { get; set; }

        public double AveragePredictionError { get; set; }

        public List<ValueOnDate> Profits { get; set; }

        public List<ValueOnDate> CumulativeProfits { get; set; }

        public double SharpeRatio { get; set; }
    }
}
