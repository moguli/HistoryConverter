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
using System.Linq;

namespace HistoryConverter.Data
{
    public class Zorro
    {
        public enum DataFormat
        {
            T1,
            T6,
            Bar
        }

        /// <summary>
        /// Saves the bar data to the specified file in .bar format.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="data">The data.</param>
        public static void Save(string path, IEnumerable<BarData> data, DataFormat format)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
                Save(stream, data, format);
        }

        /// <summary>
        /// Saves the bar data to the specified stream in .bar format.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        public static void Save(Stream stream, IEnumerable<BarData> data, DataFormat format)
        {
            var writer = new BinaryWriter(stream);

            switch (format)
            {
                case DataFormat.T1:
                    SaveAsT1(data, writer);
                    break;

                case DataFormat.T6:
                    SaveAsT6(data, writer);
                    break;

                case DataFormat.Bar:
                    SaveAsBar(data, writer);
                    break;
            }
        }

        // http://www.zorro-trader.com/manual/en/export.htm

        private static void SaveAsT1(IEnumerable<BarData> data, BinaryWriter writer)
        {
            foreach (var bar in data.Reverse())
            {
                writer.Write((double)bar.Timestamp.ToOADate());
                writer.Write((float)bar.Close);
            }
        }

        private static void SaveAsT6(IEnumerable<BarData> data, BinaryWriter writer)
        {
            foreach (var bar in data.Reverse())
            {
                writer.Write((double)bar.Timestamp.ToOADate());
                writer.Write((float)bar.High);
                writer.Write((float)bar.Low);
                writer.Write((float)bar.Open);
                writer.Write((float)bar.Close);
                writer.Write((float)0.0);
                writer.Write((float)bar.Volume);
            }
        }

        private static void SaveAsBar(IEnumerable<BarData> data, BinaryWriter writer)
        {
            foreach (var bar in data.Reverse())
            {
                writer.Write((float)bar.Open);
                writer.Write((float)bar.Close);
                writer.Write((float)bar.High);
                writer.Write((float)bar.Low);
                writer.Write((double)bar.Timestamp.ToOADate());
            }
        }

        /// <summary>
        /// Saves the ask tick data to a T1 format file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="ticks">The ticks.</param>
        public static void Save(string path, IEnumerable<TickFeed.Tick> ticks)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
                Save(stream, ticks);
        }

        /// <summary>
        /// Saves the ask tick data to the specified stream in T1 format.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="ticks">The ticks.</param>
        public static void Save(Stream stream, IEnumerable<TickFeed.Tick> ticks)
        {
            var writer = new BinaryWriter(stream);

            foreach (var tick in ticks.Reverse())
            {
                writer.Write((float)tick.Ask);
                writer.Write((double)tick.Timestamp.ToOADate());
            }
        }
    }
}