using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleVehicleShop.API
{
    [Service]
    public interface IVehicleShopManager
    {
        Task AddVehicleToShopAsync(VehicleInfo vehicle);

        Task RemoveVehicleToShopAsync(ushort vehicleId);

        Task<List<VehicleInfo>> GetVehiclesAsync();

        Task<List<string>> GetPositionsAsync();

        List<String> GetPositionsSync();

        Task AddPositionAsync(Vector3 position, int number, float yaw);

        List<VehicleInfo> GetVehiclesSync();
    }
}
