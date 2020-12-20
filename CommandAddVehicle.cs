using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SimpleVehicleShop.API;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleVehicleShop
{
    class CommandAddVehicle : IRocketCommand
    {
        private readonly VehicleShopManager m_VehicleShopManager = VehicleShopManager.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "addvehicle";

        public string Help => "Add a vehicle to the shop.";

        public string Syntax => "<vehicleId> <vehicleName> <type> <price>";

        public List<string> Aliases => new List<string> { "addv" };

        public List<string> Permissions => new List<string> { "addvehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length != 4)
            {
                ChatManager.say(user.CSteamID, "Error! Correct command usage: /addvehicle "+Syntax, Color.red, true);
                return;
            }

            var main = SimpleVehicleShop.Instance;

            ushort vehicleId = ushort.Parse(command[0]);
            string vehicleName = command[1];
            string vehicleType = command[2];
            int vehiclePrice = int.Parse(command[3]);
            if (vehicleType.Length < 3)
            {
                ChatManager.say(user.CSteamID, main.Translate("addvehicle_vehicletype_error"), Color.red, true);
                return;
            }
            if (vehicleName.Length < 4)
            {
                ChatManager.say(user.CSteamID, main.Translate("addvehicle_vehiclename_error"), Color.red, true);
                return;
            }

            if (vehiclePrice < 11)
            {
                ChatManager.say(user.CSteamID, main.Translate("addvehicle_vehicleprice_error"), Color.red, true);
                return;
            }

            var v = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicleId);
            if (v == null)
            {
                ChatManager.say(user.CSteamID, main.Translate("addvehicle_vehicleid_error"), Color.red, true);
                return;
            }

            var speed = MeasurementTool.speedToKPH(v.speedMax);
            var health = v.healthMax;
            var fuel = v.fuelMax;

            VehicleInfo vehicle = new VehicleInfo
            {
                Name = vehicleName,
                Id = vehicleId,
                Type = vehicleType,
                Speed = speed,
                Health = health,
                Fuel = fuel,
                Price = vehiclePrice
            };

            m_VehicleShopManager.AddVehicleToShopSync(vehicle);
            UnturnedChat.Say(user.CSteamID, main.Translate("addvehicle_vehicleadded_successfully", vehicle.Name), Color.cyan);
        }
    }
}
