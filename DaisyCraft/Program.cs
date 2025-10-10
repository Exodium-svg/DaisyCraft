using DaisyCraft;
using DaisyCraft.Utils;
using Game.Registry;
using Net;
using Scheduling;
using System.Buffers.Text;
using System.Diagnostics;
using System.Reflection;
using TickableServices;
using Utils;

internal class Program
{
    static void Main(string[] args)
    {
        Logger logger = new Logger(new Stream[] { Console.OpenStandardOutput() });
        RegistryCodec registry;

        {
            CodecLoader loader = new("Registry"); // Make sure it exists within it's own scope so it can be collected when done.
            logger.Info("Loading registries...");
            loader.Load(logger).Wait();
            registry = loader.CreateCodec();
        }

        Settings settings = new Settings();
        settings.StartAsync("Resource/settings.txt", logger).Wait();

        Server server = new Server(logger, settings, registry);

        const string ICON_PATH = "Resource/server-icon.png";
        if ( File.Exists(ICON_PATH) )
            server.Status.Icon += Convert.ToBase64String(File.ReadAllBytes(ICON_PATH));


        IEnumerable<string>? bannedIps = null;
        const string BANNED_IPS_PATH = "Resource/banned-ips.txt";

        if (File.Exists(BANNED_IPS_PATH))
        {
            bannedIps = File.ReadAllLines(BANNED_IPS_PATH).Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")).Select(line => line.Trim());
            logger.Info($"Loaded {bannedIps.Count()} banned addresses");
        }

        server.RegisterService(new Scheduler());
        server.RegisterService(
            new Network(
                settings.GetVar<string>("address", "0.0.0.0"), 
                settings.GetVar<int>("port", 25565), logger, 
                Assembly.GetExecutingAssembly(), bannedIps)
            );


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