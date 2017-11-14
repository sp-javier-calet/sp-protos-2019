namespace Photon.Stardust.S2S.Server.Diagnostics
{
    using System.Configuration;
    using System.IO;
    using System.Text;

    public class StatsLogger
    {
        static string csvPath = ConfigurationManager.AppSettings["CSVPath"];
        
        static System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();

        public static void Initialize()
        {
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";

            File.Delete(csvPath);

            StringBuilder csv = new StringBuilder();
            var newLine = string.Format("clients,cpu,cputotal,memory,bytessent,bytesreceived,bytestotal");
            csv.AppendLine(newLine);
            File.AppendAllText(csvPath, csv.ToString());
        }

        public static bool IsValid()
        {
            return csvPath != null;
        }

        public static void Print()
        {
            StringBuilder csv = new StringBuilder();

            float clients = Counters.ConnectedClients.GetNextValue();
            float cpu = Photon.CounterPublisher.SystemCounter.Cpu.GetNextValue();
            float cpuTotal = Photon.CounterPublisher.SystemCounter.CpuTotal.GetNextValue();
            float memory = Photon.CounterPublisher.SystemCounter.Memory.GetNextValue();
            float bytesSent = Photon.CounterPublisher.SystemCounter.BytesSentPerSecond.GetNextValue();
            float bytesReceived = Photon.CounterPublisher.SystemCounter.BytesReceivedPerSecond.GetNextValue();
            float bytesTotal = Photon.CounterPublisher.SystemCounter.BytesTotalPerSecond.GetNextValue();

            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}", clients.ToString(nfi), cpu.ToString(nfi), cpuTotal.ToString(nfi), memory.ToString(nfi), bytesSent.ToString(nfi), bytesReceived.ToString(nfi), bytesTotal.ToString(nfi));
            csv.AppendLine(newLine);

            File.AppendAllText(csvPath, csv.ToString());
        }
    }
}
