using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Scheduling
{
    public interface IScheduledTask
    {
        bool Completed { get; }
        WaitType WaitingType { get; }
        ScheduleType ScheduleType { get; set; }
        long WaitTime { get; set; }
        public bool IsReady();
        public void Execute();
    }
    public enum  WaitType
    {
        Ticks,
        Milliseconds,
    }

    public enum ScheduleType
    {
        Repeating,
        Single,
    }

    // This shit will probably create a lot of 2nd generation garbage though.... ( need 2 types of these classes, one for ticks and one for delta time )
    public class ScheduledTask<T> : IScheduledTask, INotifyCompletion
    {
        public bool Completed { get; private set; } = false;
        T? result = default;
        public WaitType WaitingType { get; init; }
        public ScheduleType ScheduleType { get; set; }
        public long WaitTime { get; set; }
        private long StartWaitTime { get; set; }

        public readonly Func<T> action;

        public ScheduledTask(Func<T> action, long waitTime, WaitType type, ScheduleType scheduleType = ScheduleType.Single)
        {
            ScheduleType = scheduleType;
            this.action = action;

            if (WaitType.Ticks == type)
            {
                this.WaitTime = waitTime;
                this.StartWaitTime = 0;
            }
            else if (WaitType.Milliseconds == type)
            {
                this.StartWaitTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency * 1000;
                this.WaitTime = waitTime + StartWaitTime;
            }
        }
        public bool IsReady()
        {
            if(WaitingType == WaitType.Ticks )
            {
                if (StartWaitTime >= WaitTime)
                {
                    StartWaitTime = 0;
                    return true;
                }
                StartWaitTime++;
                return false;
            }
            else if( WaitingType == WaitType.Milliseconds )
            {
                long currentTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency * 1000;
                if (currentTime - StartWaitTime >= WaitTime)
                {
                    StartWaitTime = currentTime;
                    return true;
                }
                return false;
            }


            return true;
        }
        public void Execute() => result = action.Invoke();
        public void OnCompleted(Action continuation)
        {
            Completed = true;
            continuation?.Invoke();
        }

        public T GetResult() {
            if(!Completed) 
                throw new InvalidOperationException("Task not completed yet.");

            return result!; 
        }
    }
}
