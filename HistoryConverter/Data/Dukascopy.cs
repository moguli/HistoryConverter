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

using Be.IO;
using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HistoryConverter
{
    public class Dukascopy : IDisposable
    {
        public class Tick
        {
            public int Milliseconds { get; set; }
            public int Ask { get; set; }
            public int Bid { get; set; }
            public float AskVolume { get; set; }
            public float BidVolume { get; set; }
        }

        private string basePath;
        private WebClient webClient;

        public Dukascopy(string basePath)
        {
            this.basePath = basePath;
            Directory.CreateDirectory(basePath);
        }

        /// <summary>
        /// Downloads the tick data.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <returns>The binary data from the h_ticks.bi5 ticks file.</returns>
        public byte[] DownloadData(string symbol, int year, int month, int day, int hour)
        {
            if (webClient == null)
                webClient = new WebClient();

            return webClient.DownloadData($"http://www.dukascopy.com/datafeed/{symbol}/{year}/{month - 1:D2}/{day:D2}/{hour:D2}h_ticks.bi5");
        }

        /// <summary>
        /// Downloads and saves the tick data file. Returns chached data if available.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <returns></returns>
        public void DownloadFile(string symbol, int year, int month, int day, int hour)
        {
            string path = Path.Combine(basePath, symbol, year.ToString("D2"), (month - 1).ToString("D2"), day.ToString("D2"));
            string file = Path.Combine(path, $"{hour:D2}h_ticks.bi5");

            if (File.Exists(file))
                return;

            byte[] data = DownloadData(symbol, year, month, day, hour);

            //if (data.Length != 0)
            //{
            Directory.CreateDirectory(path);
            File.WriteAllBytes(file, data);
            //}
        }

        /// <summary>
        /// Loads ticks from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public List<Tick> LoadTicks(string filename)
        {
            var data = File.ReadAllBytes(filename);
            if (data.Length == 0)
                return null;
            return LoadTicks(data);
        }

        /// <summary>
        /// Gets the ticks from the binary data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public List<Tick> LoadTicks(byte[] data)
        {
            var result = new List<Tick>();

            byte[] decompressed = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(data);

            using (var reader = new BeBinaryReader(new MemoryStream(decompressed)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var tick = new Tick();

                    tick.Milliseconds = reader.ReadInt32();
                    tick.Ask = reader.ReadInt32();
                    tick.Bid = reader.ReadInt32();
                    tick.AskVolume = reader.ReadSingle();
                    tick.BidVolume = reader.ReadSingle();

                    result.Add(tick);
                }
            }

            return result;
        }

        /// <summary>
        /// Loads the ticks from the tick file for the given date.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <returns></returns>
        public List<Tick> LoadTicks(string symbol, int year, int month, int day, int hour)
        {
            string path = Path.Combine(basePath, symbol, year.ToString("D2"), (month - 1).ToString("D2"), day.ToString("D2"));
            string file = Path.Combine(path, $"{hour:D2}h_ticks.bi5");
            return LoadTicks(file);
        }

        /// <summary>
        /// Loads the ticks from the tick file for the given date.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<Tick> LoadTicks(string symbol, DateTime dateTime)
        {
            return LoadTicks(symbol, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour);
        }

        /// <summary>
        /// Downloads the tick data for specified symbol and timeframe.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="progress">Callback to report the current download progress.
        /// Called with the symbol and current time.
        /// When it returns false downloading will stop.</param>
        public void Download(string symbol, DateTime fromDateTime, DateTime toDateTime, Func<string, DateTime, bool> progress = null)
        {
            var baseDateTime = new DateTime(fromDateTime.Year, fromDateTime.Month, fromDateTime.Day, fromDateTime.Hour, 0, 0);

            while (baseDateTime < toDateTime)
            {
                try
                {
                    DownloadFile(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);

                    if (progress != null)
                    {
                        if (!progress(symbol, baseDateTime))
                            return;
                    }
                }
                catch (WebException ex)
                {
                    // ignore HTTP 404 Not Found errors for weekend files
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if (response.StatusCode != HttpStatusCode.NotFound)
                                throw;

                            if (response.StatusCode == HttpStatusCode.NotFound &&
                                baseDateTime.DayOfWeek != DayOfWeek.Saturday &&
                                baseDateTime.DayOfWeek != DayOfWeek.Sunday)
                            {
                                throw;
                            }
                        }
                    }
                }

                baseDateTime = baseDateTime.AddHours(1);
            }
        }

        /// <summary>
        /// Enumerates the bar data from the stored files.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="pointValue">The point value.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="downloadIfMissing">If set to <c>true</c>  download missing files.</param>
        /// <param name="bidPrices">if set to <c>true</c> use bid prices otherwise ask prices.</param>
        /// <returns></returns>
        public IEnumerable<BarData> LoadBars(
            string symbol,
            double pointValue,
            DateTime fromDateTime,
            DateTime toDateTime,
            bool downloadIfMissing = false,
            bool bidPrices = true)
        {
            var baseDateTime = new DateTime(fromDateTime.Year, fromDateTime.Month, fromDateTime.Day, fromDateTime.Hour, 0, 0);

            while (baseDateTime < toDateTime)
            {
                IEnumerable<Tick> ticks = null;

                try
                {
                    if (downloadIfMissing)
                        DownloadFile(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);
                    ticks = LoadTicks(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);
                }
                catch (Exception)
                {
                }

                if (ticks != null)
                {
                    foreach (var t in ticks)
                    {
                        var timestamp = baseDateTime.AddMilliseconds(t.Milliseconds);
                        var price = (bidPrices ? t.Bid : t.Ask) * pointValue;
                        var volume = bidPrices ? t.BidVolume : t.AskVolume;

                        if (timestamp >= fromDateTime && timestamp < toDateTime)
                            yield return new BarData() { Timestamp = timestamp, Open = price, High = price, Low = price, Close = price, Volume = volume };
                    }
                }

                baseDateTime = baseDateTime.AddHours(1);
            }
        }

        /// <summary>
        /// Loads the tick feed.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="pointValue">The point value.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        /// <param name="downloadIfMissing">if set to <c>true</c> [download if missing].</param>
        /// <returns></returns>
        public IEnumerable<TickFeed.Tick> LoadTickFeed(string symbol, double pointValue, DateTime fromDateTime, DateTime toDateTime, bool downloadIfMissing = false)
        {
            var baseDateTime = new DateTime(fromDateTime.Year, fromDateTime.Month, fromDateTime.Day, fromDateTime.Hour, 0, 0);

            while (baseDateTime < toDateTime)
            {
                IEnumerable<Tick> ticks = null;

                try
                {
                    if (downloadIfMissing)
                    {
                        DownloadFile(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);
                        ticks = LoadTicks(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);
                    }
                    else
                    {
                        ticks = LoadTicks(symbol, baseDateTime.Year, baseDateTime.Month, baseDateTime.Day, baseDateTime.Hour);
                    }
                }
                catch (Exception)
                {
                }

                if (ticks != null)
                {
                    foreach (var t in ticks)
                    {
                        var timestamp = baseDateTime.AddMilliseconds(t.Milliseconds);
                        if (timestamp >= fromDateTime && timestamp < toDateTime)
                            yield return new TickFeed.Tick() { Timestamp = timestamp, Bid = t.Bid * pointValue, Ask = t.Ask * pointValue };
                    }
                }

                baseDateTime = baseDateTime.AddHours(1);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    webClient.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}