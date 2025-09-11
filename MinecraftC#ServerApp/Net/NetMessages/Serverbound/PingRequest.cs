using DaisyCraft;
using NetMessages;
using Scheduling;

namespace Net.NetMessages.Serverbound
{
    [NetMetaTag(GameState.Status, 1)]
    public class PingRequest : INetMessage
    {
        [NetVarType(NetVarTypeEnum.Long, 0)]
        long Time { get; set; }

        public override void Handle(Connection connection, Server server)
        {
            base.Handle(connection, server);
            connection.Send(new Clientbound.PingResponse { Time = Time });

            server.GetService<Scheduler>().ScheduleDelayed(3, () =>
            {
                connection.Close();
                return 0;
            });
        }
    }
}
