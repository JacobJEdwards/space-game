using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "PlayerOxygenUpgrade", menuName = "Upgrades/PlayerOxygenUpgrade")]
    public class PlayerOxygenUpgrade : PlayerUpgrade
    {
        public float oxygenBonus = 1f;
        public float oxygenConsumptionBonus = 1f;
        public float oxygenRegenerationBonus = 1f;
    }
}