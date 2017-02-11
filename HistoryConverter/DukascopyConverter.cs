/*
Copyright(c) 2016 Markus Trenkwalder

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HistoryConverter
{
    public class DukascopyConverter
    {
        public void Run()
        {
            var pointValues = new Dictionary<string, double>();
            pointValues.Add("EURUSD", 0.00001);
            pointValues.Add("AUDUSD", 0.00001);
            pointValues.Add("GBPUSD", 0.00001);
            pointValues.Add("NZDUSD", 0.00001);
            pointValues.Add("USDCAD", 0.00001);
            pointValues.Add("USDCHF", 0.00001);
            pointValues.Add("USDJPY", 0.001);

            pointValues.Add("AUDJPY", 0.001);
            pointValues.Add("CHFJPY", 0.001);
            pointValues.Add("EURCAD", 0.00001);
            pointValues.Add("EURCHF", 0.00001);
            pointValues.Add("EURGBP", 0.00001);
            pointValues.Add("EURJPY", 0.001);
            pointValues.Add("GBPCHF", 0.00001);
            pointValues.Add("GBPJPY", 0.001);
            pointValues.Add("NZDJPY", 0.001);

            var symbols = new string[]
            {
                "EURUSD"
            };

            var dukascopy = new Dukascopy(@"E:\HistoricalData\Dukascopy");

            foreach (var symbol in symbols)
            {
                DateTime fromDateTime = new DateTime(2001, 1, 1);
                DateTime toDateTime = DateTime.UtcNow;

                //////////////////////////////////////////////////
                // Resample data

                // Load bid prices and resample to M1 bars
                Console.WriteLine($"Loading and resampling bid bars for {symbol}");
                var resampler1 = new BarDataResampler(new TimeSpan(0, 1, 0));
                resampler1.AddRange(dukascopy.LoadBars(symbol, pointValues[symbol], fromDateTime, toDateTime, bidPrices: true));
                resampler1.Finish();
                var bidBars = resampler1.Data;

                // Load ask prices and resample to M1 bars
                Console.WriteLine($"Loading and resampling ask bars for {symbol}");
                var resampler2 = new BarDataResampler(new TimeSpan(0, 1, 0));
                resampler2.AddRange(dukascopy.LoadBars(symbol, pointValues[symbol], fromDateTime, toDateTime, bidPrices: false));
                resampler2.Finish();
                var askBars = resampler2.Data;

                // Compute spread
                var spread = new List<BarData>();
                for (int i = 0; i < bidBars.Count; i++)
                {
                    double value = askBars[i].Close - bidBars[i].Close;
                    spread.Add(new BarData() { Timestamp = askBars[i].Timestamp, Open = value, High = value, Low = value, Close = value });
                }

                //////////////////////////////////////////////////
                // Save data to Zorro format

                string zorroDir = @"E:\HistoricalData\Zorro";
                Directory.CreateDirectory(zorroDir);

                // Save to Zorro .bar format.
                Console.WriteLine($"Saving Zorro .bar data for {symbol}");
                SaveZorroBarData(zorroDir, symbol, askBars, Zorro.DataFormat.Bar);

                // Save spread data to Zorro .bar format for special symbol.
                Console.WriteLine($"Saving Zorro .bar spread data for {symbol}s");
                SaveZorroBarData(zorroDir, symbol + "s", spread, Zorro.DataFormat.Bar);

                //// Save to Zorro T1 format (price + spread)
                //Console.WriteLine($"Saving Zorro T1 data for {symbol}");
                //SaveZorroT1Data(dukascopy, symbol, pointValues[symbol]);

                //////////////////////////////////////////////////
                // Save data to ForexTester format

                // Save to ForexTester format (can be easily loaded in MT4)
                //Console.WriteLine($"Saving ForexTester M1 data for {symbol}");
                //Directory.CreateDirectory("ForexTester");
                //ForexTester.Save(Path.Combine("ForexTester", symbol + ".csv"), symbol, bidBars);
            }
        }

        private static void SaveZorroBarData(string dirPath, string symbol, List<BarData> bars, Zorro.DataFormat format)
        {
            DateTime minDate = bars.First().Timestamp;
            DateTime maxDate = bars.Last().Timestamp;

            Console.WriteLine($"Saving Zorro {symbol} data ({minDate} - {maxDate})");
            for (int year = minDate.Year; year <= maxDate.Year; year++)
            {
                var path = Path.Combine(dirPath, $"Zorro/{symbol}_{year}.bar");
                Zorro.Save(path, bars.Where(x => x.Timestamp.Year == year), format);
            }
        }

        private static void SaveZorroT1Data(string dirPath, Dukascopy dukascopy, string symbol, double pointValue)
        {
            for (int year = 2007; year <= DateTime.UtcNow.Year; ++year)
            {
                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = startDate.AddYears(1);

                var ticks = dukascopy.LoadTickFeed(symbol, pointValue, startDate, endDate, false).ToList();
                if (ticks.Count != 0)
                {
                    Zorro.SaveTicks(Path.Combine("Zorro", $"{symbol}_{year}.t1"), ticks);
                    var spread = new List<TickFeed.Tick>();
                    foreach (var t in ticks)
                        spread.Add(new TickFeed.Tick() { Timestamp = t.Timestamp, Bid = 0, Ask = t.Ask - t.Bid });
                    Zorro.SaveTicks(Path.Combine(dirPath, "Zorro", $"{symbol}s_{year}.t1"), ticks);
                }
            }
        }

        private static void ConvertForexTesterToZorro()
        {
            var symbols = new string[]
            {
                "AUDJPY",
                "AUDUSD",
                "CHFJPY",
                "EURCAD",
                "EURCHF",
                "EURGBP",
                "EURJPY",
                "EURUSD",
                "GBPCHF",
                "GBPJPY",
                "GBPUSD",
                "NZDJPY",
                "NZDUSD",
                "USDCAD",
                "USDCHF",
                "USDJPY"
            };

            var majors = new string[] { "EURUSD", "AUDUSD", "GBPUSD", "NZDUSD", "USDCAD", "USDCHF", "USDJPY" };

            Directory.CreateDirectory("Zorro");

            foreach (var symbol in symbols)
            {
                Console.WriteLine($"Loading {symbol} data");

                var bars = ForexTester.Load($"ForexTester/{symbol}.txt");

                DateTime minDate = bars[0].Timestamp;
                DateTime maxDate = bars[0].Timestamp;

                foreach (var bar in bars)
                {
                    if (bar.Timestamp < minDate)
                        minDate = bar.Timestamp;
                    if (bar.Timestamp > maxDate)
                        maxDate = bar.Timestamp;
                }

                Console.WriteLine($"Saving Zorro {symbol} data ({minDate} - {maxDate})");
                for (int year = minDate.Year; year <= maxDate.Year; year++)
                    Zorro.Save($"Zorro/{symbol}_{year}.bar", bars.Where(x => x.Timestamp.Year == year), Zorro.DataFormat.Bar);
            }
        }
    }
}