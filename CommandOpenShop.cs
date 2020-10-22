using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using SimpleVehicleShop.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace SimpleVehicleShop
{
    class CommandOpenS
    {
        [Command("openshop")]
        [CommandAlias("opens")]
        [CommandAlias("oshop")]
        [CommandAlias("os")]
        [CommandDescription("Open the vehicle shop.")]
        public class CommandOpenShop : Command
        {
            private readonly IVehicleShopManager m_VehicleShopManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfiguration m_Configuration;

            public CommandOpenShop(IServiceProvider serviceProvider, IVehicleShopManager vehicleShopManager, IStringLocalizer stringLocalizer, IConfiguration configuration) : base(serviceProvider)
            {
                m_VehicleShopManager = vehicleShopManager;
                m_StringLocalizer = stringLocalizer;
                m_Configuration = configuration;
            }

            protected override async Task OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;

                var vehicles = await m_VehicleShopManager.GetVehiclesAsync();
                var positions = m_VehicleShopManager.GetPositionsSync();
                if (vehicles.Count < 1)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:openshop_shopempty"]);
                }
                if (positions.Count < 3)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:openshop_positions_error"]);
                }
                if (SimpleVehicleShop.keys.Count > 1)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:openshop_shopinuse"]);
                }
                SimpleVehicleShop.keys.Add(user.Player.Player, 0);

                await UniTask.SwitchToMainThread();

                EffectManager.sendUIEffect(49000, 490, user.SteamId, true, vehicles[0].Name, vehicles[0].Fuel.ToString(), vehicles[0].Speed.ToString(), vehicles[0].Health.ToString());
                EffectManager.sendUIEffect(49001, 491, user.SteamId, true, vehicles[0].Price.ToString());

                string ss0 = positions.Where(l => l.StartsWith("0v")).First();
                string ss1 = positions.Where(l => l.StartsWith("1v")).First();
                string[] v0 = ss0.Split('v');
                string[] v1 = ss1.Split('v');

                Vector3 pos1 = new Vector3(float.Parse(v0[1]), float.Parse(v0[2]), float.Parse(v0[3]));
                Vector3 pos2 = new Vector3(float.Parse(v1[1]), float.Parse(v1[2]), float.Parse(v1[3]));

                float shopRotation = float.Parse(v0[4]);

                user.Player.Player.teleportToLocationUnsafe(pos1, shopRotation);

                ushort id = vehicles[0].Id;

                Quaternion ss = new Quaternion(0, 0, 0, 0);
                InteractableVehicle vehicle = VehicleManager.spawnVehicleV2(id, pos2, ss);
                vehicle.tellLocked(new CSteamID(0), CSteamID.Nil, true);
                SimpleVehicleShop.actualVehicle = vehicle;
                AsyncHelper.Schedule("Kick player from shop", () => SimpleVehicleShop.KickPlayerFromShop(user.Player.Player, m_Configuration.GetSection("plugin_configuration:delay_after_kick_from_shop").Get<double>()));
                user.Player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                user.Player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.NoBlur);
            }
        }

        [Command("setpos")]
        [CommandDescription("Set the spawns positions.")]
        public class CommandSetPos : Command
        {
            private readonly IVehicleShopManager m_VehicleShopManager;

            public CommandSetPos(IServiceProvider serviceProvider, IVehicleShopManager vehicleShopManager) : base(serviceProvider)
            {
                m_VehicleShopManager = vehicleShopManager;
            }

            protected override async Task OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;

                var position = user.Player.Player.transform.position;
                Vector3 pos = new Vector3(position.x, position.y + 0.5f, position.z);
                byte n = await Context.Parameters.GetAsync<byte>(0);
                var positions = await m_VehicleShopManager.GetPositionsAsync();
                if (positions.Count == 0)
                {
                    if (n == 1)
                    {
                        await m_VehicleShopManager.AddPositionAsync(pos, 0, user.Player.Player.look.yaw);
                        await user.PrintMessageAsync("Position 1 added correctly! Here the player will be teleported.", System.Drawing.Color.Aqua);
                        return;
                    }
                    if (n == 2)
                    {
                        await m_VehicleShopManager.AddPositionAsync(pos, 1, 0);
                        await user.PrintMessageAsync("Position 2 added correctly! Here the vehicles will be showed.", System.Drawing.Color.Aqua);
                        return;
                    }
                    if (n == 3)
                    {
                        await m_VehicleShopManager.AddPositionAsync(pos, 2, user.Player.Player.look.yaw);
                        await user.PrintMessageAsync("Position 3 added correctly! Here the bought vehicle will be spawned.", System.Drawing.Color.Aqua);
                        return;
                    }
                }
                var s0 = positions.Where(l => l.StartsWith("0v"));
                string ss0 = null;
                if (s0.Count() == 0)
                {
                    ss0 = null;
                }
                else if(s0.Count() != 0)
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
                    await m_VehicleShopManager.AddPositionAsync(pos, 0, user.Player.Player.look.yaw);
                    await user.PrintMessageAsync("Position 1 added correctly! Here the player will be teleported.", System.Drawing.Color.Aqua);
                }
                else if (n == 1 && ss0 != null)
                {
                    await PrintAsync("The position 1 are already defined!");
                }
                if (n == 2 && ss1 == null)
                {
                    await m_VehicleShopManager.AddPositionAsync(pos, 1, 0);
                    await user.PrintMessageAsync("Position 2 added correctly! Here the vehicles will be showed.", System.Drawing.Color.Aqua);
                }
                else if (n == 2 && ss1 != null)
                {
                    await PrintAsync("The position 2 are already defined!");
                }
                if (n == 3 && ss2 == null)
                {
                    await m_VehicleShopManager.AddPositionAsync(pos, 2, user.Player.Player.look.yaw);
                    await user.PrintMessageAsync("Position 3 added correctly! Here the bought vehicle will be spawned.", System.Drawing.Color.Aqua);
                }
                else if (n == 3 && ss2 != null)
                {
                    await PrintAsync("The position 3 are already defined!");
                }
            }
        }
    }
}
