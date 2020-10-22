using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using SimpleVehicleShop.API;
using SimpleVehicleShop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleVehicleShop.Providers
{
    [ServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, Priority = OpenMod.API.Prioritization.Priority.Low)]
    public class VehicleShopManager : IVehicleShopManager
    {
        private VehiclesData m_ShopCache;
        private SpawnsPositions m_Positions;

        private readonly IPluginAccessor<SimpleVehicleShop> m_PluginAccesor;

        public VehicleShopManager(IPluginAccessor<SimpleVehicleShop> pluginAccessor)
        {
            m_PluginAccesor = pluginAccessor;
        }

        public async Task AddVehicleToShopAsync(VehicleInfo vehicle)
        {
            var vehicles = await GetVehiclesAsync();
            if (vehicles.Any(k => k.Id.Equals(vehicle.Id)))
            {
                throw new UserFriendlyException("Vehicle with the same id already exists.");
            }

            m_ShopCache.Vehicles.Add(vehicle);
            await m_PluginAccesor.Instance.DataStore.SaveAsync(VSKEY, m_ShopCache);
        }

        public async Task RemoveVehicleToShopAsync(ushort vehicleId)
        {
            var vehicles = await GetVehiclesAsync();
            if (vehicles.Any(k => k.Id.Equals(vehicleId)))
            {
                VehicleInfo vehi = vehicles.OrderBy(v => v.Id == vehicleId).First();
                m_ShopCache.Vehicles.Remove(vehi);
                await m_PluginAccesor.Instance.DataStore.SaveAsync(VSKEY, m_ShopCache);
            }
            else
            {
                throw new UserFriendlyException("An vehicle with the specific ID don't exists.");
            }
        }

        public async Task AddPositionAsync(Vector3 position, int number, float yaw)
        {
            await ReadData();
            string nw = $"{number}v{position.x}v{position.y}v{position.z}v{yaw}";
            m_Positions.Positions.Add(nw);

            await m_PluginAccesor.Instance.DataStore.SaveAsync(SimpleVehicleShop.POSITIONSKEY, m_Positions);
        }

        public async Task<List<VehicleInfo>> GetVehiclesAsync()
        {
            if (m_ShopCache == null)
            {
                await ReadData();
            }
            return m_ShopCache.Vehicles;
        }

        public async Task<List<string>> GetPositionsAsync()
        {
            if (m_Positions == null)
            {
                await ReadData();
            }
            return m_Positions.Positions;
        }

        private async Task ReadData()
        {
            m_ShopCache = await m_PluginAccesor.Instance.DataStore.LoadAsync<VehiclesData>(VSKEY)
                            ?? new VehiclesData();
            m_Positions = await m_PluginAccesor.Instance.DataStore.LoadAsync<SpawnsPositions>(SimpleVehicleShop.POSITIONSKEY)
                            ?? new SpawnsPositions();
        }

        public List<string> GetPositionsSync()
        {
            return m_Positions.Positions;
        }

        public List<VehicleInfo> GetVehiclesSync()
        {
            return m_ShopCache.Vehicles;
        }

        internal const string VSKEY = "vehicles";
    }
}
