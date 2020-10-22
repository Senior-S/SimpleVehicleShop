using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using SimpleVehicleShop.API;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace SimpleVehicleShop
{
    class CommandAV
    {
        [Command("addvehicle")]
        [CommandAlias("addv")]
        [CommandSyntax("<vehicleId> <vehicleName> <type> <price>")]
        [CommandDescription("Add a vehicle to the shop.")]
        public class CommandAddVehicle : Command
        {
            private readonly IVehicleShopManager m_VehicleShopManager;
            private readonly IStringLocalizer m_StringLocalizer;

            public CommandAddVehicle(IServiceProvider serviceProvider, IVehicleShopManager vehicleShopManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
            {
                m_VehicleShopManager = vehicleShopManager;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async Task OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (Context.Parameters.Count != 4)
                {
                    throw new CommandWrongUsageException(Context);
                }

                ushort vehicleId = await Context.Parameters.GetAsync<ushort>(0);
                string vehicleName = await Context.Parameters.GetAsync<string>(1);
                string vehicleType = await Context.Parameters.GetAsync<string>(2);
                int vehiclePrice = await Context.Parameters.GetAsync<int>(3);
                if (vehicleType.Length < 3)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:addvehicle_vehicletype_error"]);
                }
                if (vehicleName.Length < 5)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:addvehicle_vehiclename_error"]);
                }

                if (vehiclePrice < 11)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:addvehicle_vehicleprice_error"]);
                }

                var v = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicleId);
                if (v == null)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:addvehicle_vehicleid_error"]);
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

                await m_VehicleShopManager.AddVehicleToShopAsync(vehicle);
                await PrintAsync(m_StringLocalizer["plugin_translations:addvehicle_vehicleadded_successfully", new { vehicleName = vehicle.Name }], System.Drawing.Color.Aqua);
            }
        }
    }
}
