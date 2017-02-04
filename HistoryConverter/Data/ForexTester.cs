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
using System.Globalization;
using System.IO;

namespace HistoryConverter.Data
{
    public class ForexTester
    {
        /// <summary>
        /// Loads the bar data from the specified file.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static List<BarData> Load(string path, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            return Load(File.OpenRead(path), fromDateTime, toDateTime);
        }

        /// <summary>
        /// Loads bar data from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static List<BarData> Load(Stream stream, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            List<BarData> result = new List<BarData>();
            StreamReader r = new StreamReader(stream);

            // Skip header
            r.ReadLine();

            while (!r.EndOfStream)
            {
                string[] col = r.ReadLine().Split(',');

                BarData bar = new BarData();

                string date = col[1];
                string time = col[2];
                bar.Open = double.Parse(col[3], CultureInfo.InvariantCulture);
                bar.High = double.Parse(col[4], CultureInfo.InvariantCulture);
                bar.Low = double.Parse(col[5], CultureInfo.InvariantCulture);
                bar.Close = double.Parse(col[6], CultureInfo.InvariantCulture);
                bar.Volume = double.Parse(col[7], CultureInfo.InvariantCulture);
                bar.Timestamp = DateTime.ParseExact(date + time, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                bar.Timestamp = DateTime.SpecifyKind(bar.Timestamp, DateTimeKind.Utc);

                if (fromDateTime != null && bar.Timestamp < fromDateTime)
                    continue;

                if (toDateTime != null && bar.Timestamp >= toDateTime)
                    break;

                result.Add(bar);
            }

            return result;
        }

        /// <summary>
        /// Saves the bar data to the specified file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="symbol">The symbol e.g. EURUSD.</param>
        /// <param name="data">The data.</param>
        public static void Save(string path, string symbol, IEnumerable<BarData> data)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
                Save(stream, symbol, data);
        }

        /// <summary>
        /// Saves the bar data to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="symbol">The symbol e.g. EURUSD.</param>
        /// <param name="data">The data.</param>
        public static void Save(Stream stream, string symbol, IEnumerable<BarData> data)
        {
            StreamWriter w = new StreamWriter(stream);

            w.WriteLine("<TICKER>,<DTYYYYMMDD>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>");

            foreach (BarData bar in data)
            {
                string date = bar.Timestamp.ToString("yyyyMMdd");
                string time = bar.Timestamp.ToString("HHmmss");
                string open = bar.Open.ToString(CultureInfo.InvariantCulture);
                string high = bar.High.ToString(CultureInfo.InvariantCulture);
                string low = bar.Low.ToString(CultureInfo.InvariantCulture);
                string close = bar.Close.ToString(CultureInfo.InvariantCulture);
                string volume = bar.Volume.ToString(CultureInfo.InvariantCulture);

                w.WriteLine($"{symbol},{date},{time},{open},{high},{low},{close},{volume}");
            }

            w.Flush();
        }
    }
}