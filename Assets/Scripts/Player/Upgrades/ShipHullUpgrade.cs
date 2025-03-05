using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "ShipHullUpgrade", menuName = "Upgrades/ShipHullUpgrade")]
    public class ShipHullUpgrade : ShipUpgrade
    {
        public float hullHealthBonus = 1f;
        public float hullRegenerationBonus = 1f;
    }
}