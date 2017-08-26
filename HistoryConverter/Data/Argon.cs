using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryConverter.Data
{
    public static class Argon
    {
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        private static long DateTimeToUnixTimeMilliseconds(DateTime dateTime)
        {
            var diff = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return (long)diff.TotalMilliseconds;
        }

        public static IEnumerable<BarData> Load(string path)
        {
            return Load(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), false);
        }

        public static IEnumerable<BarData> Load(Stream stream, bool leaveOpen = true)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    var bar = new BarData();
                    bar.Timestamp = UnixTimeStampToDateTime(reader.ReadInt64() / 1000.0);
                    bar.Open = reader.ReadSingle();
                    bar.High = reader.ReadSingle();
                    bar.Low = reader.ReadSingle();
                    bar.Close = reader.ReadSingle();
                    bar.Volume = reader.ReadSingle();
                    var spread = reader.ReadSingle();

                    yield return bar;
                }
            }
        }

        public static void Save(string path, IEnumerable<BarData> data)
        {
            Save(new FileStream(path, FileMode.Create), data, false);
        }

        public static void Save(Stream stream, IEnumerable<BarData> data, bool leaveOpen = true)
        {
            using (var writer = new BinaryWriter(stream, Encoding.Default, leaveOpen))
            {
                foreach (var bar in data)
                {
                    writer.Write(DateTimeToUnixTimeMilliseconds(bar.Timestamp));
                    writer.Write((float)bar.Open);
                    writer.Write((float)bar.High);
                    writer.Write((float)bar.Low);
                    writer.Write((float)bar.Close);
                    writer.Write((float)bar.Volume);
                    writer.Write((float)0f);
                }
            }
        }
    }
}
