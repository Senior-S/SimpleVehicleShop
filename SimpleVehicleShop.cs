using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using SDG.Unturned;
using System.Collections.Generic;
using Steamworks;
using SimpleVehicleShop.API;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.Core.Helpers;

[assembly: PluginMetadata("SS.SimpleVehicleShop", DisplayName = "SimpleVehicleShop")]
namespace SimpleVehicleShop
{
    public class SimpleVehicleShop : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<SimpleVehicleShop> m_Logger;

        private readonly IVehicleShopManager m_VehicleShopManager;

        public SimpleVehicleShop(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<SimpleVehicleShop> logger, 
            IVehicleShopManager vehicleShopManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_VehicleShopManager = vehicleShopManager;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToMainThread();
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            m_Logger.LogInformation("Plugin loaded correctly!");
        }

        private void OnEffectButtonClicked(Player player, string buttonName)
        {
            var vehicles = m_VehicleShopManager.GetVehiclesSync();
            var positions = m_VehicleShopManager.GetPositionsSync();
            string ss1 = positions.Where(l => l.StartsWith("1v")).First();
            string ss2 = positions.Where(l => l.StartsWith("2v")).First();
            string[] v1 = ss1.Split('v');
            string[] v2 = ss2.Split('v');
            Vector3 pos1 = new Vector3(float.Parse(v1[1]), float.Parse(v1[2]), float.Parse(v1[3]));
            Vector3 pos2 = new Vector3(float.Parse(v2[1]), float.Parse(v2[2]), float.Parse(v2[3]));
            float bvehiclerotation = float.Parse(v2[4]);
            switch (buttonName)
            {
                case "CerrarBUTTON":
                    keys.Remove(player);
                    rot = 0;
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                    EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
                    EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
                    player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                    break;
                case "NextVehicleBUTTON":
                    keys.TryGetValue(player, out int nextid);
                    VehicleInfo vehicl = null;
                    foreach (VehicleInfo vvs in vehicles)
                    {
                        var indexnew = vehicles.IndexOf(vvs);
                        VehicleInfo qw = vehicles.First(f => f.Type == vvs.Type);
                        if (vvs.Type != vehicles[nextid].Type)
                        {
                            if (vvs == qw)
                            {
                                vehicl = vvs;
                                break;
                            }
                        }
                    }
                    if (vehicl == null)
                    {
                        vehicl = vehicles[0];
                    }
                    int index = vehicles.IndexOf(vehicl);
                    keys.Remove(player);
                    keys.Add(player, index);
                    var rotation = actualVehicle.transform.rotation;
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                    CSteamID steamID = player.channel.owner.playerID.steamID;
                    EffectManager.sendUIEffect(49000, 490, steamID, true, vehicl.Name, vehicl.Fuel.ToString(), vehicl.Speed.ToString(), vehicl.Health.ToString());
                    EffectManager.sendUIEffect(49001, 491, steamID, true, vehicl.Price.ToString());
                    actualVehicle = VehicleManager.spawnLockedVehicleForPlayerV2(vehicl.Id, pos1, rotation, player);
                    break;
                case "Rotar+BUTTON":
                    keys.TryGetValue(player, out int rmasid);
                    rot += 45f;
                    Quaternion s = Quaternion.Euler(0, rot, 0);
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                    actualVehicle = VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[rmasid].Id, pos1, s, player);
                    break;
                case "Rotar-BUTTON":
                    keys.TryGetValue(player, out int rmenosid);
                    rot -= 45f;
                    Quaternion s1 = Quaternion.Euler(0, rot, 0);
                    VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                    actualVehicle = VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[rmenosid].Id, pos1, s1, player);
                    break;
                case "ChangeColorBUTTON":
                    keys.TryGetValue(player, out int changeid);
                    CSteamID steamID1 = player.channel.owner.playerID.steamID;
                    if (vehicles.Count == 1)
                    {
                        changeid = 0;
                        keys.Remove(player);
                        keys.Add(player, changeid);
                        Quaternion r1 = new Quaternion(0, rot, 0, 0);
                        VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                        EffectManager.sendUIEffect(49000, 490, steamID1, true, vehicles[0].Name, vehicles[0].Fuel.ToString(), vehicles[0].Speed.ToString(), vehicles[0].Health.ToString());
                        EffectManager.sendUIEffect(49001, 491, steamID1, true, vehicles[0].Price.ToString());
                        actualVehicle = VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[0].Id, pos1, r1, player);
                        break;
                    }
                    else
                    {
                        VehicleInfo newVehicle = null;
                        foreach (VehicleInfo vvs in vehicles)
                        {
                            var indexnew = vehicles.IndexOf(vvs);
                            if (vvs.Type == vehicles[changeid].Type && indexnew > changeid)
                            {
                                if (vvs.Id == vehicles[changeid].Id)
                                {
                                    newVehicle = vvs;
                                    int a = indexnew + 1;
                                    if (a >= vehicles.Count())
                                    {
                                        break;
                                    }
                                }
                                newVehicle = vvs;
                                break;
                            }
                        }
                        if (newVehicle == null)
                        {
                            var qw = vehicles.First(f => f.Type == vehicles[changeid].Type);
                            newVehicle = qw;
                        }
                        int l = vehicles.IndexOf(newVehicle);
                        changeid = l;
                        keys.Remove(player);
                        keys.Add(player, changeid);
                        Quaternion r = new Quaternion(0, rot, 0, 0);
                        VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                        EffectManager.sendUIEffect(49000, 490, steamID1, true, newVehicle.Name, newVehicle.Fuel.ToString(), newVehicle.Speed.ToString(), newVehicle.Health.ToString());
                        EffectManager.sendUIEffect(49001, 491, steamID1, true, newVehicle.Price.ToString());
                        actualVehicle = VehicleManager.spawnLockedVehicleForPlayerV2(newVehicle.Id, pos1, r, player);
                    }
                    break;
                case "ComprarBUTTON":
                    keys.TryGetValue(player, out int buyid);
                    CSteamID steamID2 = player.channel.owner.playerID.steamID;
                    if (!delayPlayers.Contains(player))
                    {
                        VehicleInfo acvehicle = vehicles.FirstOrDefault(k => k.Id == actualVehicle.id);
                        if (player.skills.experience < acvehicle.Price) ChatManager.say(steamID2, "You don't have money to buy this vehicle", Color.red, true);
                        else
                        {
                            VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                            EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
                            EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
                            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                            player.skills.askSpend(uint.Parse(acvehicle.Price.ToString()));
                            Quaternion ss = new Quaternion(0, bvehiclerotation, 0, 0);
                            VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[buyid].Id, pos2, ss, player);
                            ChatManager.say(steamID2, m_StringLocalizer["plugin_translations:vehicle_bought_successfully", new { vehicleName = vehicles[buyid].Name }], Color.cyan, true);
                            keys.Remove(player);
                            UniTask.SwitchToThreadPool();
                            AsyncHelper.Schedule("Delay Vehicle Buy", () => DelayVehicleBuy(player));
                            rot = 0;
                            UniTask.SwitchToMainThread();
                            break;
                        }
                    }
                    else
                    {
                        ChatManager.say(steamID2, m_StringLocalizer["plugin_translations:vehicle_bought_error"], Color.red, true);
                        break;
                    }
                    break;
            }
        }

        public async Task DelayVehicleBuy(Player player)
        {
            delayPlayers.Add(player);
            await Task.Delay(TimeSpan.FromSeconds(m_Configuration.GetSection("plugin_configuration:delay_per_buy").Get<double>()));
            delayPlayers.Remove(player);
        }

        public static async Task KickPlayerFromShop(Player player, double delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            if (!keys.TryGetValue(player, out int v))
            {
                return;
            }
            keys.Remove(player);
            VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
            EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
            EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
        }

        protected override async UniTask OnUnloadAsync()
        {
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            m_Logger.LogInformation("Plugin unloaded correctly!");
        }

        internal List<Player> delayPlayers = new List<Player>();
        internal int oldd;
        internal float rot = 0f;
        internal static InteractableVehicle actualVehicle;
        internal static Dictionary<Player, int> keys = new Dictionary<Player, int>();
        internal const string POSITIONSKEY = "spawnpositions";
    }
}
