using SimpleVehicleShop.API;
using SimpleVehicleShop.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

namespace SimpleVehicleShop
{
    public class VehicleShopManager
    {
        private VehicleShopManager() {}

        private VehiclesData m_ShopCache;
        private SpawnsPositions m_Positions;

        public bool AddVehicleToShopSync(VehicleInfo vehicle)
        {
            var vehicles = GetVehiclesSync();
            if (vehicles.Any(k => k.Id.Equals(vehicle.Id)))
            {
                return false;
                //throw new UserFriendlyException("Vehicle with the same id already exists.");
            }

            m_ShopCache.Vehicles.Add(vehicle);
            var json = JsonConvert.SerializeObject(m_ShopCache, Formatting.Indented);
            var papa = path + VSKEY + ".json";
            File.WriteAllText(papa, json);
            return true;
            //await m_PluginAccesor.Instance.DataStore.SaveAsync(VSKEY, m_ShopCache);
        }

        public void InitialFiles()
        {
            var z = path + SimpleVehicleShop.POSITIONSKEY + ".json";
            var x = path + VSKEY + ".json";
            if (!File.Exists(z))
            {
                m_Positions = new SpawnsPositions();
                var json2 = JsonConvert.SerializeObject(m_Positions, Formatting.Indented);
                File.WriteAllText(z, json2);
            }
            if (!File.Exists(x))
            {
                m_ShopCache = new VehiclesData();
                var json1 = JsonConvert.SerializeObject(m_ShopCache, Formatting.Indented);
                File.WriteAllText(x, json1);
            }
        }

        public bool RemoveVehicleFromShopSync(ushort vehicleId)
        {
            var vehicles = GetVehiclesSync();
            if (vehicles.Any(k => k.Id.Equals(vehicleId)))
            {
                VehicleInfo vehi = vehicles.First(z => z.Id.Equals(vehicleId));
                m_ShopCache.Vehicles.Remove(vehi);
                var json = JsonConvert.SerializeObject(m_ShopCache, Formatting.Indented);
                var papa = path + VSKEY + ".json";
                File.WriteAllText(papa, json);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddPositionSync(Vector3 position, int number, float yaw)
        {
            ReadData();
            string nw = $"{number}v{position.x}v{position.y}v{position.z}v{yaw}";
            m_Positions.Positions.Add(nw);

            var json = JsonConvert.SerializeObject(m_Positions, Formatting.Indented);
            var papa = path + SimpleVehicleShop.POSITIONSKEY + ".json";
            File.WriteAllText(papa, json);
        }

        private void ReadData()
        {
            var z = path + SimpleVehicleShop.POSITIONSKEY + ".json";
            string json1 = File.ReadAllText(z);
            var x = path + VSKEY + ".json";
            string json2 = File.ReadAllText(x);
            var spawns = JsonConvert.DeserializeObject<SpawnsPositions>(json1);
            var vehic = JsonConvert.DeserializeObject<VehiclesData>(json2);
            if (vehic == null)
            {
                m_ShopCache = new VehiclesData();
            }
            else
            {
                m_ShopCache = vehic;
            }
            if (spawns == null)
            {
                m_Positions = new SpawnsPositions();
            }
            else
            {
                m_Positions = spawns;
            }
        }

        public List<string> GetPositionsSync()
        {
            if (m_Positions == null)
            {
                ReadData();
            }
            return m_Positions.Positions;
        }

        public List<VehicleInfo> GetVehiclesSync()
        {
            if (m_ShopCache == null)
            {
                ReadData();
            }

            return m_ShopCache.Vehicles;
        }

        // Grant the access to use this class methods in other classes.
        public static VehicleShopManager Instance
        {
            get { return instance; }
        }
        private static VehicleShopManager instance = new VehicleShopManager();


        internal const string VSKEY = "vehicles";
        public static readonly string path = $"{Environment.CurrentDirectory}/Plugins/SimpleVehicleShop/";
    }
}
