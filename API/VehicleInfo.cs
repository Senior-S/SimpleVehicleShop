using System;

namespace SimpleVehicleShop.API
{
    [Serializable]
    public class VehicleInfo
    {
        public string Name { get; set; }
        public ushort Id { get; set; }
        public string Type { get; set; }
        public float Speed { get; set; }
        public ushort Health { get; set; }
        public ushort Fuel { get; set; }
        public int Price { get; set; }
    }
}