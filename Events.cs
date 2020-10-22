using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace SimpleVehicleShop
{
    class Events
    {
        public class UserDisconnectedListener : IEventListener<IUserDisconnectedEvent>
        {
            [EventListener(Priority = EventListenerPriority.Lowest)]
            public async Task HandleEventAsync(object sender, IUserDisconnectedEvent @event)
            {
                UnturnedUser user = (UnturnedUser)@event.User;
                if (SimpleVehicleShop.keys.TryGetValue(user.Player.Player, out int v))
                {
                    SimpleVehicleShop.keys.Remove(user.Player.Player);
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, SimpleVehicleShop.actualVehicle.instanceID);
                    SimpleVehicleShop.actualVehicle = null;
                }
            }
        }

        public class UserDieListener : IEventListener<UnturnedPlayerDeathEvent>
        {
            public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
            {
                var user = @event.Player;

                if (SimpleVehicleShop.keys.TryGetValue(user.Player, out int v))
                {
                    SimpleVehicleShop.keys.Remove(user.Player);
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, SimpleVehicleShop.actualVehicle.instanceID);
                    SimpleVehicleShop.actualVehicle = null;
                }
            }
        }
    }
}
