﻿using System;
using System.Linq;

namespace PortfolioStrategy
{
    class IntervalModel
    {
        public double[] X { get; set; }

        public double Y { get; set; }

        public double[] XandY { get; set; }

        public IntervalModel(double[] x)
        {
            this.X = x;

            var length = x.Length;
            var lastChangesLength = 10;

            var lastPriceChanges = new double[lastChangesLength];
            for (int i = 1; i <= lastChangesLength; i++)
            {
                lastPriceChanges[i - 1] = x[length - i] - x[length - i - 1];
            }

            this.Y = lastPriceChanges.Average(); 

            this.XandY = new double[length + 1];
            this.X.CopyTo(this.XandY, 0);
            this.XandY[length] = this.Y;
        }
    }
}