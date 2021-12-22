﻿#region License
// Copyright (c) 2019 Jake Fowler
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without 
// restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following 
// conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;

namespace Cmdty.Curves.Samples.Spline
{
    class Program
    {
        static void Main(string[] args)
        {
            // The following code shows how to use the spline to derive a smooth daily curve from
            // monthly and quarterly granularity input contract prices. Also demonstrated is the
            // optional seasonal adjustment factor, in this case used to apply day-of-week seasonality.

            var dayOfWeekAdjustment = new Dictionary<DayOfWeek, double>
            {
                [DayOfWeek.Monday] = 0.95,
                [DayOfWeek.Tuesday] = 0.99,
                [DayOfWeek.Wednesday] = 1.05,
                [DayOfWeek.Thursday] = 1.01,
                [DayOfWeek.Friday] = 0.98,
                [DayOfWeek.Saturday] = 0.92,
                [DayOfWeek.Sunday] = 0.91
            };
            
            DoubleCurve<Day> curve = new MaxSmoothnessSplineCurveBuilder<Day>()
                .AddContract(Month.CreateJuly(2019), 77.98)
                .AddContract(Month.CreateAugust(2019), 76.01)
                .AddContract(Month.CreateSeptember(2019), 78.74)
                .AddContract(Quarter.CreateQuarter4(2019), 85.58)
                .AddContract(Quarter.CreateQuarter1(2020), 87.01)
                .WithMultiplySeasonalAdjustment(day => dayOfWeekAdjustment[day.DayOfWeek])
                .BuildCurve();

            Console.WriteLine(curve.FormatData("F5"));

            Console.WriteLine();
            Console.WriteLine();

            //===============================================================================================
            // APPLYING AN ALTERNATIVE WEIGHTING SCHEME

            // By default, the spline calculations assume averages are weighted by the number of minutes in a contract period.
            // This assumption is fine for instruments where the commodity is delivered at a constant rate, e.g. natural gas forwards.
            // An alternative weighting scheme can be added by calling WithAverageWeighting and supplying a weighting scheme as a function.
            // The below example makes use of the Weighting helper class to provide the weighting function as the count of business days.
            // An example of when such a weighting scheme should be used is for oil swaps, based on an index which is only published on a business day
            // to create a monthly curve from quarterly granularity inputs.

            var holidays = new List<Day>() { new Day(2020, 1, 1) };
            Func<Month, double> busDayWeight = Weighting.BusinessDayCount<Month>(holidays);

            var contracts = new List<Contract<Month>>()
            {
                Contract<Month>.Create(Quarter.CreateQuarter4(2019), 76.58),
                Contract<Month>.Create(Quarter.CreateQuarter1(2020), 77.20),
                Contract<Month>.Create(Quarter.CreateQuarter2(2020), 76.01),
                Contract<Month>.Create(Quarter.CreateQuarter3(2020), 74.95),
                Contract<Month>.Create(Quarter.CreateQuarter4(2020), 74.92),
            };

            DoubleCurve<Month> curveBusDayWeight = new MaxSmoothnessSplineCurveBuilder<Month>()
                .AddContracts(contracts)
                .WithWeighting(busDayWeight)
                .BuildCurve();

            Console.WriteLine("Derived smooth curve with business day weighting:");
            Console.WriteLine(curveBusDayWeight.FormatData("F5", -1));

            Console.ReadKey();
        }
    }
}
