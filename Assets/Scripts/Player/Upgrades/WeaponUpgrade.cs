using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Upgrades/WeaponUpgrade")]
    public class WeaponUpgrade : BaseUpgrade
    {
        public float coolRateBonus = 1f;
        public float damageBonus = 1f;
        public float heatRateBonus = 1f;
        public float maxChargeBonus = 1f;
        public float rangeBonus = 1f;
    }
}