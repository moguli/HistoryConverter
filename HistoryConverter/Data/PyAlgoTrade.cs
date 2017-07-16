using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HistoryConverter
{
    public class PyAlgoTrade
    {
        public static void WriteGenericBarFeed(string filename, IEnumerable<BarData> data, DateTime? fromDateTime = null, DateTime? toDateTime = null)
        {
            using (StreamWriter w = new StreamWriter(filename))
            {
                w.WriteLine("Date Time,Open,High,Low,Close,Volume,Adj Close");

                foreach (BarData bar in data)
                {
                    if (fromDateTime != null && bar.Timestamp < fromDateTime)
                        continue;

                    if (toDateTime != null && bar.Timestamp >= toDateTime)
                        break;

                    string timestamp = bar.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    string open = bar.Open.ToString(CultureInfo.InvariantCulture);
                    string high = bar.High.ToString(CultureInfo.InvariantCulture);
                    string low = bar.Low.ToString(CultureInfo.InvariantCulture);
                    string close = bar.Close.ToString(CultureInfo.InvariantCulture);
                    string volume = bar.Volume.ToString(CultureInfo.InvariantCulture);

                    w.WriteLine($"{timestamp},{open},{high},{low},{close},{volume},");
                }
            }
        }
    }
}