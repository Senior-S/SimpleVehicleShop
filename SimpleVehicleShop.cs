using Rocket.Core.Plugins;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using SimpleVehicleShop.API;
using Steamworks;
using fr34kyn01535.Uconomy;

namespace SimpleVehicleShop
{
    public class SimpleVehicleShop : RocketPlugin<SimpleVehicleShopConfiguration>
    {
        public static SimpleVehicleShop Instance;
        private readonly VehicleShopManager m_VehicleShopManager = VehicleShopManager.Instance;

        protected override void Load()
        {
            Instance = this;
            m_VehicleShopManager.InitialFiles();
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            Provider.onEnemyDisconnected += OnEnemyDisconnect;
            PlayerLife.onPlayerDied += OnPlayerDied;
            Logger.Log("[SimpleVehicleShop] Plugin loaded correctly!");
        }

        private void OnPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            var user = sender.player;

            if (SimpleVehicleShop.keys.TryGetValue(user, out int v))
            {
                EffectManager.askEffectClearByID(49000, user.channel.owner.playerID.steamID);
                EffectManager.askEffectClearByID(49001, user.channel.owner.playerID.steamID);
                user.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                SimpleVehicleShop.keys.Remove(user);
                VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, SimpleVehicleShop.actualVehicle.instanceID);
                SimpleVehicleShop.actualVehicle = null;
            }
        }

        private void OnEnemyDisconnect(SteamPlayer player)
        {
            Player user = player.player;
            if (SimpleVehicleShop.keys.TryGetValue(user, out int v))
            {
                EffectManager.askEffectClearByID(49000, user.channel.owner.playerID.steamID);
                EffectManager.askEffectClearByID(49001, user.channel.owner.playerID.steamID);
                user.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                SimpleVehicleShop.keys.Remove(user);
                VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, SimpleVehicleShop.actualVehicle.instanceID);
                SimpleVehicleShop.actualVehicle = null;
            }
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
                        if (vvs.Type != vehicles[nextid].Type && indexnew > nextid)
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
                        if (Configuration.Instance.UseUconomy)
                        {
                            var balance = Uconomy.Instance.Database.GetBalance(player.channel.owner.playerID.steamID.ToString());
                            if (balance < acvehicle.Price)
                            {
                                ChatManager.say(steamID2, Translate("vehicle_error_nomoney"), Color.red, true);
                                break;
                            }
                        }
                        if (!Configuration.Instance.UseUconomy && player.skills.experience < acvehicle.Price)
                        {
                            ChatManager.say(steamID2, Translate("vehicle_error_nomoney"), Color.red, true);
                            break;
                        }
                        if (Configuration.Instance.UseUconomy)
                        {
                            var balance = Uconomy.Instance.Database.GetBalance(player.channel.owner.playerID.steamID.ToString());
                            if (balance >= acvehicle.Price)
                            {
                                VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                                EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
                                EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
                                player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                                decimal uprice = decimal.Parse(acvehicle.Price.ToString());
                                Uconomy.Instance.Database.IncreaseBalance(player.channel.owner.playerID.steamID.ToString(), -uprice);
                                Quaternion qua = new Quaternion(0, bvehiclerotation, 0, 0);
                                VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[buyid].Id, pos2, qua, player);
                                ChatManager.say(steamID2, Translate("vehicle_bought_successfully", vehicles[buyid].Name), Color.cyan, true);
                                keys.Remove(player);
                                StartCoroutine(DelayVehicleBuy(player));
                                rot = 0;
                                break;
                            }
                        }
                        if (!Configuration.Instance.UseUconomy && player.skills.experience >= acvehicle.Price)
                        {
                            VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
                            EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
                            EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
                            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                            player.skills.askSpend(uint.Parse(acvehicle.Price.ToString()));
                            Quaternion ss = new Quaternion(0, bvehiclerotation, 0, 0);
                            VehicleManager.spawnLockedVehicleForPlayerV2(vehicles[buyid].Id, pos2, ss, player);
                            ChatManager.say(steamID2, Translate("vehicle_bought_successfully", vehicles[buyid].Name), Color.cyan, true);
                            keys.Remove(player);
                            StartCoroutine(DelayVehicleBuy(player));
                            rot = 0;
                        }
                        break;
                    }
                    else
                    {
                        ChatManager.say(steamID2, Translate("vehicle_bought_error"), Color.red, true);
                        break;
                    }
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                    { "addvehicle_vehicletype_error", "The type of the vehicle need to have more than 2 letters."},
                    { "addvehicle_vehiclename_error", "The name of the vehicle need to have more than 3 letters." },
                    { "addvehicle_vehicleprice_error", "The price of the vehicle need to be upper than $10." },
                    { "addvehicle_vehicleid_error", "Vehicle not found! Please specify a valid ID" },
                    { "addvehicle_vehicleadded_successfully", "Vehicle added successfully! Vehicle name: {0}" },
                    { "removevehicle_vehicleremoved_succesfully", "Vehicle removed successfully! Vehicle ID: {0}" },
                    { "openshop_shopempty", "The shop is empty!" },
                    { "openshop_positions_error", "The positions of the shop are not defined, contact a administrator to fix this!" },
                    { "openshop_shopinuse", "The shop is in use by other person!" },
                    { "vehicle_bought_successfully", "You bought the vehicle {0} successfully" },
                    { "vehicle_error_nomoney", "You don't have money to buy this vehicle." },
                    { "vehicle_bought_error", "You need to wait to buy other vehicle!" }
                };
            }
        }

        public IEnumerator AvoidUIOverlapping(Player player)
        {
            while (keys.TryGetValue(player, out int val))
            {
                var vehicles = m_VehicleShopManager.GetVehiclesSync();
                EffectManager.sendUIEffect(49000, 490, player.channel.owner.playerID.steamID, true, vehicles[val].Name, vehicles[val].Fuel.ToString(), vehicles[val].Speed.ToString(), vehicles[val].Health.ToString());
                EffectManager.sendUIEffect(49001, 491, player.channel.owner.playerID.steamID, true, vehicles[val].Price.ToString());
                yield return new WaitForSeconds(5f);
            }
            EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
            EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
            yield return null;
        }

        public IEnumerator DelayVehicleBuy(Player player)
        {
            delayPlayers.Add(player);
            yield return new WaitForSeconds(Configuration.Instance.delay_per_buy);
            delayPlayers.Remove(player);
        }

        public IEnumerator KickPlayerFromShop(Player player)
        {
            yield return new WaitForSeconds(Configuration.Instance.delay_after_kick_from_shop);
            if (!keys.TryGetValue(player, out int v))
            {
                StopCoroutine(KickPlayerFromShop(player));
            }
            keys.Remove(player);
            VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, actualVehicle.instanceID);
            EffectManager.askEffectClearByID(49000, player.channel.owner.playerID.steamID);
            EffectManager.askEffectClearByID(49001, player.channel.owner.playerID.steamID);
            player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
        }

        protected override void Unload()
        {
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            Logger.Log("[SimpleVehicleShop] Plugin unloaded correctly!");
        }

        internal List<Player> delayPlayers = new List<Player>();
        internal int oldd;
        internal float rot = 0f;
        internal static InteractableVehicle actualVehicle;
        internal static Dictionary<Player, int> keys = new Dictionary<Player, int>();
        internal const string POSITIONSKEY = "spawnpositions";
    }
}