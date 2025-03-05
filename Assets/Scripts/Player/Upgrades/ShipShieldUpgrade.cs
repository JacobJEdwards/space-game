using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "ShipShieldUpgrade", menuName = "Upgrades/ShipShieldUpgrade")]
    public class ShipShieldUpgrade : ShipUpgrade
    {
        public float shieldHealthBonus = 1f;
        public float shieldRegenerationBonus = 1f;
    }
}