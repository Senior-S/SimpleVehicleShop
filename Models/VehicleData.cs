using SimpleVehicleShop.API;
using System;
using System.Collections.Generic;

namespace SimpleVehicleShop.Models
{
    [Serializable]
    public class VehiclesData
    {
        public VehiclesData()
        {
            Vehicles = new List<VehicleInfo>();
        }

        public VehiclesData(List<VehicleInfo> vehic)
        {
            Vehicles = vehic;
        }

        public List<VehicleInfo> Vehicles { get; set; }
    }
}