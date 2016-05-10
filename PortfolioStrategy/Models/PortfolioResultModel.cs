using System.Collections.Generic;
using System.Windows.Documents;

namespace PortfolioStrategy.Models
{
    class PortfolioResultModel
    {
        public double Bank { get; set; }

        public List<ValueOnDate> BankDynamic { get; set; }

        public List<ValueOnDate> GMBankDynamic { get; set; }

        public double[] Weights { get; set; }

        public double PortfolioBank { get; set; }
    }
}
