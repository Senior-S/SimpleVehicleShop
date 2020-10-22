using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using SimpleVehicleShop.API;
using SimpleVehicleShop.Providers;

namespace SimpleVehicleShop
{
    class ServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IVehicleShopManager, VehicleShopManager>();
        }
    }
}
