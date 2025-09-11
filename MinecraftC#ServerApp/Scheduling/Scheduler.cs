using DaisyCraft;
using System.Collections.Concurrent;

namespace Scheduling
{
    public class Scheduler : TickableService
    {
        ConcurrentQueue<IScheduledTask> scheduledTasks = new();
        public override void Tick(long deltaTime)
        {
            base.Tick(deltaTime);

            if(scheduledTasks.IsEmpty) 
                return;

            List<IScheduledTask> repeatTasks = new();
            while (scheduledTasks.TryDequeue(out var task))
            {
                try
                {
                    if (task.IsReady())
                        task.Execute();
                    else
                        repeatTasks.Add(task);
                }
                catch(Exception ex) { server.Logger.Exception($"Exception occurred while executing scheduled task {task.GetType().FullName}", ex); }

                if( task.ScheduleType == ScheduleType.Repeating )
                    repeatTasks.Add(task);
            }

            foreach(var task in repeatTasks)
                scheduledTasks.Enqueue(task);
        }

        public override void Start(Server server)
        {
            base.Start(server);
        }
        public void EnqueueTask(IScheduledTask task) => scheduledTasks.Enqueue(task);
        public void ScheduleTicked<T>(int ticks, Func<T> action, ScheduleType type = ScheduleType.Single) => EnqueueTask(new ScheduledTask<T>(action, ticks, WaitType.Ticks, type));
        public void ScheduleDelayed<T>(int milliseconds, Func<T> action, ScheduleType type = ScheduleType.Single) => EnqueueTask(new ScheduledTask<T>(action, milliseconds, WaitType.Milliseconds, type));
        
        public override string GetServiceName() => nameof(Scheduler);
    }
}
