using HistoryConverter.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryConverter
{
    public class Helper
    {
        public static void SaveZorroBarData(string dirPath, string symbol, List<BarData> bars, Zorro.DataFormat format)
        {
            DateTime minDate = bars.First().Timestamp;
            DateTime maxDate = bars.Last().Timestamp;

            Console.WriteLine($"Saving Zorro {symbol} data ({minDate} - {maxDate})");
            for (int year = minDate.Year; year <= maxDate.Year; year++)
            {
                var path = Path.Combine(dirPath, $"{symbol}_{year}.bar");
                Zorro.Save(path, bars.Where(x => x.Timestamp.Year == year), format);
            }
        }
    }
}