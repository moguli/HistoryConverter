/*
MIT License

Copyright(c) 2017 trenki2

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;

namespace HistoryConverter.Data
{
    public class BarDataResampler
    {
        private BarData currentData = new BarData();
        private DateTime lastTime = new DateTime(0);
        private bool firstIteration = true;
        private long frequency;

        public List<BarData> Data { get; } = new List<BarData>();
        public BarData Current { get { return currentData; } }

        public BarDataResampler(TimeSpan frequency)
        {
            this.frequency = frequency.Ticks;
        }

        public void Add(BarData bar)
        {
            DateTime currentTime = new DateTime((bar.Timestamp.Ticks / frequency) * frequency);

            if (currentTime == lastTime)
            {
                currentData.Low = Math.Min(currentData.Low, bar.Low);
                currentData.High = Math.Max(currentData.High, bar.High);
                currentData.Close = bar.Close;
                currentData.Volume += bar.Volume;
            }
            else
            {
                if (!firstIteration)
                    Data.Add(currentData);

                currentData = new BarData();
                currentData.Timestamp = currentTime;
                currentData.Open = bar.Open;
                currentData.Low = bar.Low;
                currentData.High = bar.High;
                currentData.Close = bar.Close;
                currentData.Volume = bar.Volume;

                firstIteration = false;
            }

            lastTime = currentTime;
        }

        public void AddRange(IEnumerable<BarData> data)
        {
            foreach (BarData bar in data)
                Add(bar);
        }

        public void Finish()
        {
            Data.Add(currentData);
        }
    }
}