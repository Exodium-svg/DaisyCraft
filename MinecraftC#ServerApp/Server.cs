using DaisyCraft.Utils;
using TickableServices;
using Net;
using System.Collections.Concurrent;
using Utils;
using Services;

namespace DaisyCraft
{
    public class Server
    {
        public bool IsRunning { get; private set; } = true;
        public int TPS { get; private set; } = 20; // ticks per second

        ConcurrentDictionary<string, TickableService> tickableServices = new();

        public ServerStatus Status { get; set; } = new(772, true, "DaisyCraft server", 20, 0);
        public Logger Logger { get; init; }
        public Settings Options { get; init; }
        public SessionService SessionService { get; init; }
        public Server(Logger logger, Settings options)
        {
            Options = options;
            Logger = logger;
            SessionService = new("user-cache");
        }
        
        public void RegisterService(TickableService service) {
            string serviceName = service.GetServiceName();
            tickableServices[serviceName] = service;

            Logger.Info($"Registered service '{serviceName}'");
            service.Start(this);
        }

        public T GetService<T>() where T : TickableService
        {
            string serviceName = typeof(T).Name;
            return tickableServices.TryGetValue(serviceName, out var service) ? (T)service : throw new Exception($"Missing service {serviceName}");
        }

        public void DoTick(long deltaTime)
        {
            foreach (var service in tickableServices.Values)
            {
                using (Profiler.Measure($"TICK {service.GetServiceName()}"))
                    service.Tick(deltaTime);
            }
        }
    }
}
