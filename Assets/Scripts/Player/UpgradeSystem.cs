#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CollectableResources;
using Managers;
using UnityEngine;

namespace Player
{
    public enum UpgradeType
    {
        Jetpack,
        Weapon,
        Ship,
        Player
    }

    public abstract class BaseUpgrade : ScriptableObject
    {
        public string upgradeName = null!;
        public string upgradeDescription = null!;
        public Sprite upgradeIcon = null!;
        public UpgradeType target;
        public BaseUpgrade? nextUpgrade;
        public List<UpgradeRequirement> requirements = new();

        public bool CanBeApplied(UpgradeManager manager)
        {
            return requirements.All(req => req.IsMet(manager));
        }
    }

    [CreateAssetMenu(fileName = "JetpackUpgrade", menuName = "Upgrades/JetpackUpgrade")]
    public class JetpackUpgrade : BaseUpgrade
    {
        public float jetpackFuelConsumptionBonus;
        public float jetpackFuelRegenerationBonus;
        public float jetpackFuelCapacityBonus;
        public float jetpackForceBonus;
    }

    [CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Upgrades/WeaponUpgrade")]
    public class WeaponUpgrade : BaseUpgrade
    {
        public float damageBonus;
        public float fireRateBonus;
        public float rangeBonus;
        public float accuracyBonus;
    }

    [CreateAssetMenu(fileName = "ShipUpgrade", menuName = "Upgrades/ShipUpgrade")]
    public class ShipUpgrade : BaseUpgrade
    {
        public float speedBonus;
        public float healthBonus;
        public float shieldBonus;
        public float cargoBonus;
    }

    [CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "Upgrades/PlayerUpgrade")]
    public class PlayerUpgrade : BaseUpgrade
    {
        public float healthBonus;
        public float shieldBonus;
        public float speedBonus;
        public float oxygenBonus;
    }

    [Serializable]
    public class UpgradeRequirement
    {
        public enum RequirementType
        {
            PreviousUpgrade,
            Resource,
            RepairComplete
        }

        public RequirementType type;
        public ScriptableObject requiredObject = null!;
        public int requiredAmount;

        public bool IsMet(UpgradeManager manager)
        {
            return type switch
            {
                RequirementType.PreviousUpgrade => manager.IsUpgradeUnlocked(requiredObject as BaseUpgrade),
                RequirementType.Resource => manager.HasResources(requiredObject as ResourceObject, requiredAmount),
                RequirementType.RepairComplete => manager.IsRepairComplete(requiredObject as BaseRepair),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    [Serializable]
    public class BaseRepair : ScriptableObject
    {
        public string upgradeName = null!;
        public string upgradeDescription = null!;
        public Sprite upgradeIcon = null!;
        public UpgradeType target;
        public BaseRepair? nextRepair;
        public BaseUpgrade? nextUpgrade;
        public List<UpgradeRequirement> requirements = new();

        public bool CanBeApplied(UpgradeManager manager)
        {
            return requirements.All(req => req.IsMet(manager));
        }
    }

    [CreateAssetMenu(fileName = "JetpackRepair", menuName = "Repairs/JetpackRepair")]
    public class JetpackRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "WeaponRepair", menuName = "Repairs/WeaponRepair")]
    public class WeaponRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "ShipRepair", menuName = "Repairs/ShipRepair")]
    public class ShipRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "PlayerRepair", menuName = "Repairs/PlayerRepair")]
    public class PlayerRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "UpgradeConfig")]
    public class UpgradeConfig : ScriptableObject
    {
        public List<PlayerUpgrade> playerUpgrades = new();
        public List<ShipUpgrade> shipUpgrades = new();
        public List<WeaponUpgrade> weaponUpgrades = new();
        public List<JetpackUpgrade> jetpackUpgrades = new();

        public List<ShipRepair> shipRepairs = new();
        public List<PlayerRepair> playerRepairs = new();
        public List<WeaponRepair> weaponRepairs = new();
        public List<JetpackRepair> jetpackRepairs = new();
    }
}