using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "ShipWeaponUpgrade", menuName = "Upgrades/ShipWeaponUpgrade")]
    public class ShipLaserUpgrade : ShipUpgrade
    {
        public float coolRateBonus = 1f;
        public float damageBonus = 1f;
        public float heatRateBonus = 1f;
        public float maxChargeBonus = 1f;
        public float rangeBonus = 1f;
    }
}