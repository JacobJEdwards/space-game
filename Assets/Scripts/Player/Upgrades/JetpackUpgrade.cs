using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "JetpackUpgrade", menuName = "Upgrades/JetpackUpgrade")]
    public class JetpackUpgrade : PlayerUpgrade
    {
        public float jetpackForceBonus = 1f;
        public float jetpackFuelCapacityBonus = 1f;
        public float jetpackFuelConsumptionBonus = 1f;
        public float jetpackFuelRegenerationBonus = 1f;
    }
}