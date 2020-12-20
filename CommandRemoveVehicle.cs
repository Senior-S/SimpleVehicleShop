using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleVehicleShop
{
    class CommandRemoveVehicle : IRocketCommand
    {
        private readonly VehicleShopManager m_VehicleShopManager = VehicleShopManager.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "removevehicle";

        public string Help => "Remove a vehicle from the shop.";

        public string Syntax => "<vehicleId>";

        public List<string> Aliases => new List<string> { "removev" };

        public List<string> Permissions => new List<string> { "removevehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length != 1)
            {
                ChatManager.say(user.CSteamID, "Error! Correct command usage: /removevehicle " + Syntax, Color.red, true);
                return;
            }

            var main = SimpleVehicleShop.Instance;

            ushort vehicleId = ushort.Parse(command[0]);

            m_VehicleShopManager.RemoveVehicleFromShopSync(vehicleId);

            ChatManager.say(user.CSteamID, main.Translate("removevehicle_vehicleremoved_succesfully", vehicleId), Color.cyan, true);
        }
    }
}
