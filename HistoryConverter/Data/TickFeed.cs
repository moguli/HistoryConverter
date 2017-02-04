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
using System.IO;

namespace HistoryConverter.Data
{
    public class TickFeed
    {
        public class Tick
        {
            public DateTime Timestamp { get; set; }
            public double Bid { get; set; }
            public double Ask { get; set; }
        }

        /// <summary>
        /// Loads tick data from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static IEnumerable<Tick> Load(string path, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            return Load(File.OpenRead(path), fromDateTime, toDateTime);
        }

        /// <summary>
        /// Loads ticks from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static IEnumerable<Tick> Load(Stream stream, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            var r = new BinaryReader(stream);

            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                var timestamp = new DateTime(r.ReadInt64(), DateTimeKind.Utc);
                var bid = r.ReadDouble();
                var ask = r.ReadDouble();
                if ((fromDateTime == null || timestamp >= fromDateTime) && (toDateTime == null || timestamp < toDateTime))
                    yield return new Tick() { Timestamp = timestamp, Bid = bid, Ask = ask };
            }
        }

        /// <summary>
        /// Saves the ticks to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        public static void Save(string path, IEnumerable<Tick> data)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
                Save(stream, data);
        }

        /// <summary>
        /// Saves the ticks to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        public static void Save(Stream stream, IEnumerable<Tick> data)
        {
            var w = new BinaryWriter(stream);

            foreach (var tick in data)
            {
                w.Write(tick.Timestamp.Ticks);
                w.Write(tick.Bid);
                w.Write(tick.Ask);
            }
        }

        /// <summary>
        /// Converts tick data to bar data & spread.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <param name="bars">The bars.</param>
        /// <param name="spread">The spread.</param>
        public static void ConvertToBars(IEnumerable<Tick> ticks, out List<BarData> bars, out List<double> spread)
        {
            bars = new List<BarData>();
            spread = new List<double>();

            foreach (var t in ticks)
            {
                bars.Add(new BarData() { Timestamp = t.Timestamp, Open = t.Bid, High = t.Bid, Low = t.Bid, Close = t.Bid, Volume = 0 });
                spread.Add(t.Ask - t.Bid);
            }
        }
    }
}