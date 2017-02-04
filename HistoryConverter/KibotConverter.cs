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

using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HistoryConverter
{
    public class KibotConverter
    {
        public void Run()
        {
            /*Console.WriteLine("Converting 1 Minute Data");
            ConvertAllSymbols(@"E:\HistoricalData\Kibot\StockTop200_1m", @"E:\Zorro\HistoryStocks1m");

            Console.WriteLine("Converting Daily Data");
            ConvertAllSymbolsDaily(@"E:\HistoricalData\Kibot\StockAll_1d", @"E:\Zorro\HistoryStocks1d");
            */
            Console.WriteLine("Converting 1 Hour Data");
            ConvertAndResampleAllSymbols(@"E:\HistoricalData\Kibot\StockTop200_1m", @"E:\Zorro\HistoryStocks1h", new TimeSpan(1, 0, 0));
        }

        private static void ConvertAllSymbolsDaily(string sourceDir, string destDir)
        {
            var files = Directory.GetFiles(sourceDir);
            for (int i = 0; i < files.Count(); ++i)
            {
                string symbol = Path.GetFileNameWithoutExtension(files[i]);
                Console.WriteLine($"Converting symbol {symbol} [{i + 1}/{files.Count()}]");
                ConvertSymbolDaily(sourceDir, destDir, symbol);
            }
        }

        private static void ConvertAllSymbols(string sourceDir, string destDir)
        {
            var files = Directory.GetFiles(sourceDir);
            for (int i = 0; i < files.Count(); ++i)
            {
                string symbol = Path.GetFileNameWithoutExtension(files[i]);
                Console.WriteLine($"Converting symbol {symbol} [{i + 1}/{files.Count()}]");
                ConvertSymbol(sourceDir, destDir, symbol);
            }
        }

        private static void ConvertAndResampleAllSymbols(string sourceDir, string destDir, TimeSpan frequency)
        {
            var files = Directory.GetFiles(sourceDir);
            for (int i = 0; i < files.Count(); ++i)
            {
                string symbol = Path.GetFileNameWithoutExtension(files[i]);
                Console.WriteLine($"Converting symbol {symbol} [{i + 1}/{files.Count()}]");
                ConvertAndResampleSymbol(sourceDir, destDir, symbol, frequency);
            }
        }

        private static void ConvertSymbolDaily(string sourceDir, string destDir, string symbol)
        {
            string sourcePath = Path.Combine(sourceDir, $"{symbol}.txt");
            string destPath = Path.Combine(destDir, $"{symbol}.t6");
            var barData = Kibot.Load(sourcePath);
            Zorro.Save(destPath, barData, Zorro.DataFormat.T6);
        }

        private static void ConvertSymbol(string sourceDir, string destDir, string symbol)
        {
            string sourcePath = Path.Combine(sourceDir, $"{symbol}.txt");

            var barData = new List<BarData>();
            foreach (var bar in Kibot.EnumerateBars(sourcePath))
            {
                if (barData.Count > 0 && bar.Timestamp.Year != barData[0].Timestamp.Year)
                {
                    string destPath = Path.Combine(destDir, $"{symbol}_{barData[0].Timestamp.Year}.bar");
                    Zorro.Save(destPath, barData, Zorro.DataFormat.Bar);
                    barData.Clear();
                }

                barData.Add(bar);
            }

            if (barData.Count > 0)
            {
                var destPath = Path.Combine(destDir, $"{symbol}_{barData[0].Timestamp.Year}.bar");
                Zorro.Save(destPath, barData, Zorro.DataFormat.Bar);
            }
        }

        private static void ConvertAndResampleSymbol(string sourceDir, string destDir, string symbol, TimeSpan frequency)
        {
            string sourcePath = Path.Combine(sourceDir, $"{symbol}.txt");

            var resampler = new BarDataResampler(frequency);
            resampler.AddRange(Kibot.EnumerateBars(sourcePath));
            resampler.Finish();

            var barData = new List<BarData>();
            foreach (var bar in resampler.Data)
            {
                if (barData.Count > 0 && bar.Timestamp.Year != barData[0].Timestamp.Year)
                {
                    string destPath = Path.Combine(destDir, $"{symbol}_{barData[0].Timestamp.Year}.bar");
                    Zorro.Save(destPath, barData, Zorro.DataFormat.Bar);
                    barData.Clear();
                }

                barData.Add(bar);
            }

            if (barData.Count > 0)
            {
                var destPath = Path.Combine(destDir, $"{symbol}_{barData[0].Timestamp.Year}.bar");
                Zorro.Save(destPath, barData, Zorro.DataFormat.Bar);
            }
        }
    }
}