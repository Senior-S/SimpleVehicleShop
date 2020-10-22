using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SimpleVehicleShop.API;
using System;
using System.Threading.Tasks;

namespace SimpleVehicleShop
{
    class CommandRemoveVehicle
    {
        [Command("removevehicle")]
        [CommandAlias("removev")]
        [CommandSyntax("<vehicleId>")]
        [CommandDescription("Remove a vehicle of the shop.")]
        public class RemoveVehicle : Command
        {
            private readonly IVehicleShopManager m_VehicleShopManager;
            private readonly IStringLocalizer m_StringLocalizer;

            public RemoveVehicle(IServiceProvider serviceProvider, IVehicleShopManager vehicleShopManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
            {
                m_VehicleShopManager = vehicleShopManager;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async Task OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                {
                    throw new CommandWrongUsageException(Context);
                }
                ushort vehicleId = await Context.Parameters.GetAsync<ushort>(0);

                await m_VehicleShopManager.RemoveVehicleToShopAsync(vehicleId);

                await PrintAsync(m_StringLocalizer["plugin_translations:removevehicle_vehicleremoved_succesfully", new { vehicleId = vehicleId }], System.Drawing.Color.Aqua);
            }
        }
    }
}
