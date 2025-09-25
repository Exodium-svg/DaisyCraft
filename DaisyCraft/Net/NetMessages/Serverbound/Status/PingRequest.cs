using DaisyCraft;
using Net.NetMessages.Clientbound.Status;
using NetMessages.Serverbound;
using Scheduling;

namespace Net.NetMessages.Serverbound.Status
{
    [PacketMetaData(GameState.Status, 0x01)]
    public class PingRequest : ServerBoundPacket
    {
        [NetVarType(NetVarTypeEnum.Long, 0)]
        long Time { get; set; }

        public override async Task Handle(Player player, Server server)
        {
            await player.SendAsync(new PingResponse { Time = Time });

            server.GetService<Scheduler>().ScheduleDelayed(3, () =>
            {
                player.Connection.Close();
                return 0;
            });
        }
    }
}
