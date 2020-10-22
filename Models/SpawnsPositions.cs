using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleVehicleShop.Models
{
    [Serializable]
    public class SpawnsPositions
    {
        public SpawnsPositions()
        {
            Positions = new List<String>();
        }

        public List<string> Positions { get; set; }
    }
}
