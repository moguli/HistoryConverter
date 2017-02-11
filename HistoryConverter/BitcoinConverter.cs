using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryConverter
{
    public class BitcoinConverter
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        public void Run()
        {
            var fileStream = File.Open(@"E:\HistoricalData\Bitcoin\bitstampUSD.csv.gz", FileMode.Open);
            var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            var reader = new StreamReader(gzipStream);

            var resampler = new BarDataResampler(new TimeSpan(0, 1, 0));

            while (!reader.EndOfStream)
            {
                string[] data = reader.ReadLine().Split(',');

                var bar = new BarData();
                bar.Timestamp = UnixTimeStampToDateTime(double.Parse(data[0], CultureInfo.InvariantCulture));
                bar.Open = bar.High = bar.Low = bar.Close = double.Parse(data[1], CultureInfo.InvariantCulture);
                bar.Volume = double.Parse(data[2], CultureInfo.InvariantCulture);

                resampler.Add(bar);
            }

            string zorroDir = @"E:\HistoricalData\Zorro";
            Directory.CreateDirectory(zorroDir);
            Helper.SaveZorroBarData(zorroDir, "XBTUSD", resampler.Data, Zorro.DataFormat.Bar);
        }
    }
}