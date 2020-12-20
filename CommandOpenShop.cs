using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleVehicleShop
{
    class CommandOpenShop : IRocketCommand
    {
        private readonly VehicleShopManager m_VehicleShopManager = VehicleShopManager.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "openshop";

        public string Help => "Open the vehicle shop.";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string> { "opens", "oshop", "os" };

        public List<string> Permissions => new List<string> { "openshop" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;

            var main = SimpleVehicleShop.Instance;
            var vehicles = m_VehicleShopManager.GetVehiclesSync();
            var positions = m_VehicleShopManager.GetPositionsSync();
            if (vehicles.Count < 1)
            {
                UnturnedChat.Say(user.CSteamID, main.Translate("openshop_shopempty"), Color.red, true);
                return;
            }
            if (positions.Count < 3)
            {
                UnturnedChat.Say(user.CSteamID, main.Translate("openshop_positions_error"), Color.red, true);
                return;
            }
            if (SimpleVehicleShop.keys.Count >= 1)
            {
                UnturnedChat.Say(user.CSteamID, main.Translate("openshop_shopinuse"), Color.red, true);
                return;
            }
            SimpleVehicleShop.keys.Add(user.Player, 0);

            EffectManager.sendUIEffect(49000, 490, user.CSteamID, true, vehicles[0].Name, vehicles[0].Fuel.ToString(), vehicles[0].Speed.ToString(), vehicles[0].Health.ToString());
            EffectManager.sendUIEffect(49001, 491, user.CSteamID, true, vehicles[0].Price.ToString());

            string ss0 = positions.Where(l => l.StartsWith("0v")).First();
            string ss1 = positions.Where(l => l.StartsWith("1v")).First();
            string[] v0 = ss0.Split('v');
            string[] v1 = ss1.Split('v');

            Vector3 pos1 = new Vector3(float.Parse(v0[1]), float.Parse(v0[2]), float.Parse(v0[3]));
            Vector3 pos2 = new Vector3(float.Parse(v1[1]), float.Parse(v1[2]), float.Parse(v1[3]));

            float shopRotation = float.Parse(v0[4]);

            user.Player.teleportToLocationUnsafe(pos1, shopRotation);

            ushort id = vehicles[0].Id;
            Quaternion ss = new Quaternion(0, 0, 0, 0);
            InteractableVehicle vehicle = VehicleManager.spawnVehicleV2(id, pos2, ss);
            vehicle.tellLocked(user.CSteamID, CSteamID.Nil, true);
            SimpleVehicleShop.actualVehicle = vehicle;
            main.StartCoroutine(main.KickPlayerFromShop(user.Player));
            main.StartCoroutine(main.AvoidUIOverlapping(user.Player));
            user.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
            user.Player.enablePluginWidgetFlag(EPluginWidgetFlags.NoBlur);
        }
    }

    class CommandSetPos : IRocketCommand
    {
        private readonly VehicleShopManager m_VehicleShopManager = VehicleShopManager.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setposition";

        public string Help => "Set the spawns positons";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string> { "setpos" };

        public List<string> Permissions => new List<string> { "setposition" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;

            if (command.Length != 1)
            {
                UnturnedChat.Say(user.CSteamID, "Error! Specificy a number 1-3", Color.red, true);
                return;
            }

            var position = user.Player.transform.position;
            Vector3 pos = new Vector3(position.x, position.y + 0.5f, position.z);
            byte n = byte.Parse(command[0]);
            var positions = m_VehicleShopManager.GetPositionsSync();
            if (positions.Count == 0)
            {
                if (n == 1)
                {
                    m_VehicleShopManager.AddPositionSync(pos, 0, user.Player.look.yaw);
                    UnturnedChat.Say(user.CSteamID, "Position 1 added correctly! Here the player will be teleported.", Color.cyan, true);
                    return;
                }
                if (n == 2)
                {
                    m_VehicleShopManager.AddPositionSync(pos, 1, 0);
                    UnturnedChat.Say(user.CSteamID, "Position 2 added correctly! Here the vehicles will be showed.", Color.cyan, true);
                    return;
                }
                if (n == 3)
                {
                    m_VehicleShopManager.AddPositionSync(pos, 2, user.Player.look.yaw);
                    UnturnedChat.Say(user.CSteamID, "Position 3 added correctly! Here the bought vehicle will be spawned.", Color.cyan, true);
                    return;
                }
            }
            var s0 = positions.Where(l => l.StartsWith("0v"));
            string ss0 = null;
            if (s0.Count() == 0)
            {
                ss0 = null;
            }
            else if (s0.Count() != 0)
            {
                ss0 = s0.First();
            }
            var s1 = positions.Where(l => l.StartsWith("1v"));
            string ss1 = null;
            if (s1.Count() == 0)
            {
                ss1 = null;
            }
            else if (s1.Count() != 0)
            {
                ss1 = s1.First();
            }
            var s2 = positions.Where(l => l.StartsWith("2v"));
            string ss2 = null;
            if (s2.Count() == 0)
            {
                ss2 = null;
            }
            else if (s2.Count() != 0)
            {
                ss2 = s2.First();
            }

            if (n == 1 && ss0 == null)
            {
                m_VehicleShopManager.AddPositionSync(pos, 0, user.Player.look.yaw);
                UnturnedChat.Say(user.CSteamID, "Position 1 added correctly! Here the player will be teleported.", Color.cyan, true);
            }
            else if (n == 1 && ss0 != null)
            {
                UnturnedChat.Say(user.CSteamID, "The position 1 are already defined!", Color.red, true);
            }
            if (n == 2 && ss1 == null)
            {
                m_VehicleShopManager.AddPositionSync(pos, 1, 0);
                UnturnedChat.Say(user.CSteamID, "Position 2 added correctly! Here the vehicles will be showed.", Color.cyan, true);
            }
            else if (n == 2 && ss1 != null)
            {
                UnturnedChat.Say(user.CSteamID, "The position 2 are already defined!", Color.red, true);
            }
            if (n == 3 && ss2 == null)
            {
                m_VehicleShopManager.AddPositionSync(pos, 2, user.Player.look.yaw);
                UnturnedChat.Say(user.CSteamID, "Position 3 added correctly! Here the bought vehicle will be spawned.", Color.cyan, true);
            }
            else if (n == 3 && ss2 != null)
            {
                UnturnedChat.Say(user.CSteamID, "The position 3 are already defined!", Color.red, true);
            }
        }
    }
}
