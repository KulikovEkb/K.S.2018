using System;
using Serilog;

namespace Kontur.ImageTransformer
{
    public class EntryPoint
    {
        internal static Serilog.Core.Logger log = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "log.txt")
            .CreateLogger();

        public static void Main(string[] args)
        {
            using (var server = new AsyncHttpServer())
            {
                server.Start("http://+:8080/");

                Console.ReadKey(true);
            }
        }
    }
}
