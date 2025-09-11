using DaisyCraft;
using DaisyCraft.Utils;
using Scheduling;
using System.Buffers.Text;
using System.Diagnostics;
using System.Reflection;

internal class Program
{
    static void Main(string[] args)
    {
        Logger logger = new Logger(new Stream[] { Console.OpenStandardOutput() });
        Server server = new Server(logger);

        if( File.Exists("server-icon.png"))
        {
            byte[] pngData = File.ReadAllBytes("server-icon.png");

            server.Status.Icon += Convert.ToBase64String(pngData);

            logger.Info("Favicon loaded.");
        }

        IEnumerable<string>? bannedIps = null;

        if (File.Exists("banned-ips.txt"))
        {
            bannedIps = File.ReadAllLines("banned-ips.txt").Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")).Select(line => line.Trim());
            logger.Info($"Loaded {bannedIps.Count()} banned IPs from banned-ips.txt");
        }

        server.RegisterService(new Scheduler());
        server.RegisterService(new Net.Network("0.0.0.0", 25565, logger, Assembly.GetExecutingAssembly(), bannedIps));


        const int MILLISECOND = 1000;

        long lastTimeStamp = Stopwatch.GetTimestamp();
        long lastTickTime = lastTimeStamp;
        double tickInterval = (double)Stopwatch.Frequency / server.TPS;

        while (server.IsRunning)
        {
            long currentTimeStamp = Stopwatch.GetTimestamp();

            lastTimeStamp = currentTimeStamp;

            if ((currentTimeStamp - lastTickTime) >= tickInterval)
            {
                Console.Title = $"TPS: TODO | Uptime: TODO minutes | Players: TODO";
                //logger.Info("we ticked once");

                long deltaTime = ((currentTimeStamp - lastTimeStamp) * MILLISECOND) / Stopwatch.Frequency;
                server.DoTick(deltaTime);

                lastTickTime += (long)tickInterval; // move to next tick
            }
            else
            {
                // Sleep until next tick
                long remainingTicks = (long)(tickInterval - (currentTimeStamp - lastTickTime));
                int sleepMs = (int)((remainingTicks * MILLISECOND) / Stopwatch.Frequency);

                if (sleepMs > 0)
                    Thread.Sleep(sleepMs);
            }
        }
    }
}