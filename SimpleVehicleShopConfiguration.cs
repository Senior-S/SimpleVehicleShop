using Rocket.API;

namespace SimpleVehicleShop
{
    public class SimpleVehicleShopConfiguration : IRocketPluginConfiguration
    {
        public float delay_per_buy;
        public float delay_after_kick_from_shop;
        public bool UseUconomy;

        public void LoadDefaults()
        {
            delay_per_buy = 120;
            delay_after_kick_from_shop = 180;
            UseUconomy = false;
        }
    }
}
