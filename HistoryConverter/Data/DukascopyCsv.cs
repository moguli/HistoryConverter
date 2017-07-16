using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryConverter
{
    public class DukascopyCsv
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

                string dateAndTime = col[0];
                bar.Open = double.Parse(col[1], CultureInfo.InvariantCulture);
                bar.High = double.Parse(col[2], CultureInfo.InvariantCulture);
                bar.Low = double.Parse(col[3], CultureInfo.InvariantCulture);
                bar.Close = double.Parse(col[4], CultureInfo.InvariantCulture);
                bar.Volume = double.Parse(col[5], CultureInfo.InvariantCulture);
                bar.Timestamp = DateTime.ParseExact(dateAndTime, "dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
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
        /// <param name="data">The data.</param>
        public static void Save(string path, IEnumerable<BarData> data)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
                Save(stream, data);
        }

        /// <summary>
        /// Saves the bar data to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        public static void Save(Stream stream, IEnumerable<BarData> data)
        {
            StreamWriter w = new StreamWriter(stream);

            w.WriteLine("GMT time, Open, High, Low, Close, Volume");

            foreach (BarData bar in data)
            {
                string date = bar.Timestamp.ToString("dd.MM.yyyy HH:mm:ss.fff");
                string open = bar.Open.ToString(CultureInfo.InvariantCulture);
                string high = bar.High.ToString(CultureInfo.InvariantCulture);
                string low = bar.Low.ToString(CultureInfo.InvariantCulture);
                string close = bar.Close.ToString(CultureInfo.InvariantCulture);
                string volume = bar.Volume.ToString(CultureInfo.InvariantCulture);

                w.WriteLine($"{date},{open},{high},{low},{close},{volume}");
            }

            w.Flush();
        }
    }
}