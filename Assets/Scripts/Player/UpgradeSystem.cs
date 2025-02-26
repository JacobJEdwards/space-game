#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName = null!;
        public string upgradeDescription = null!;
        public Sprite upgradeIcon = null!;
        public List<ResourceCost> upgradeCosts = new();

        [Serializable]
        public class ResourceCost
        {
            public string resourceName = null!;
            public int resourceAmount = 0;
        }
    }

    [CreateAssetMenu(fileName = "ShipUpgrade", menuName = "ShipUpgrade")]
    public class ShipUpgrade : ScriptableObject
    {
        public InventoryUI.ShipInfo shipInfo = null!;
        public float speedBonus;
        public float healthBonus;
        public float shieldBonus;
        public float damageBonus;
        public float energyBonus;
        public float energyRegenBonus;
        public float boostBonus;
        public float rangeBonus;
        public UpgradeData upgradeData = null!;
        public ShipUpgrade? nextUpgrade;
    }

    [CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "WeaponUpgrade")]
    public class WeaponUpgrade : ScriptableObject
    {
        public InventoryUI.WeaponInfo weaponInfo = null!;
        public float damageBonus;
        public float fireRateBonus;
        public float energyCostBonus;
        public float rangeBonus;
        public UpgradeData upgradeData = null!;
        public WeaponUpgrade? nextUpgrade;
    }

}