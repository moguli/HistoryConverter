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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryConverter.Data
{
    public class Kibot
    {
        /// <summary>
        /// Load Kibot historical data from a file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static List<BarData> Load(string path, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            using (var stream = File.Open(path, FileMode.Open))
                return Load(stream, fromDateTime, toDateTime);
        }

        /// <summary>
        /// Loads Kibot data from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static List<BarData> Load(Stream stream, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            return EnumerateBars(stream).ToList();
        }

        /// <summary>
        /// Enumerates Kibot historical data from a file between the specified times.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static IEnumerable<BarData> EnumerateBars(string path, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            return EnumerateBars(File.Open(path, FileMode.Open), fromDateTime, toDateTime);
        }

        /// <summary>
        /// Enumerates Kibot historical data from a stream between the specified times.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <returns></returns>
        public static IEnumerable<BarData> EnumerateBars(Stream stream, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            using (var reader = new StreamReader(stream))
            {
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    string[] items = line.Split(',');

                    if (items.Length != 6 && items.Length != 7)
                        throw new Exception("Invalid input format in history file.");

                    int index = 0;
                    BarData bar = new BarData();

                    var date = DateTime.Parse(items[index++], CultureInfo.InvariantCulture);

                    if (items.Length == 7)
                    {
                        var time = DateTime.ParseExact(items[index++], "HH:mm", CultureInfo.InvariantCulture);
                        date = date.AddHours(time.Hour);
                        date = date.AddMinutes(time.Minute);
                    }

                    bar.Timestamp = TimeZoneInfo.ConvertTimeToUtc(date, easternZone);
                    bar.Open = double.Parse(items[index++], CultureInfo.InvariantCulture);
                    bar.High = double.Parse(items[index++], CultureInfo.InvariantCulture);
                    bar.Low = double.Parse(items[index++], CultureInfo.InvariantCulture);
                    bar.Close = double.Parse(items[index++], CultureInfo.InvariantCulture);
                    bar.Volume = double.Parse(items[index++], CultureInfo.InvariantCulture);

                    if (fromDateTime != null && bar.Timestamp < fromDateTime)
                        continue;

                    if (toDateTime != null && bar.Timestamp > toDateTime)
                        break;

                    yield return bar;
                }
            }
        }
    }
}