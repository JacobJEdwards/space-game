#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "UpgradeConfig")]
    public class UpgradeConfig : ScriptableObject
    {
        public List<PlayerRepair> playerRepairs = new();
        public List<PlayerUpgrade> playerUpgrades = new();
        public List<ShipRepair> shipRepairs = new();
        public List<ShipUpgrade> shipUpgrades = new();
        public List<WeaponRepair> weaponRepairs = new();
        public List<WeaponUpgrade> weaponUpgrades = new();
    }
}