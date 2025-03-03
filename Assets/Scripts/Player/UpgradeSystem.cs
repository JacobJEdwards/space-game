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
        Weapon,
        Ship,
        Player
    }

    public enum RepairType
    {
        Jetpack,
        Thrusters,
        Impulse
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

    public interface ISpaceMovementUpgrade
    {
        public float SpeedBonus { get; }
        public float HandlingBonus { get; }
        public float AccelerationBonus { get; }
        public float BoostDeprecationRateBonus { get; }
        public float BoostRechargeRateBonus { get; }
        public float MaxChargeBonus { get; }
        public float BoostBonus { get; }
    }


    [CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Upgrades/WeaponUpgrade")]
    public class WeaponUpgrade : BaseUpgrade
    {
        public float damageBonus = 1f;
        public float rangeBonus = 1f;
        public float coolRateBonus = 1f;
        public float heatRateBonus = 1f;
        public float maxChargeBonus = 1f;
    }

    [CreateAssetMenu(fileName = "ShipUpgrade", menuName = "Upgrades/ShipUpgrade")]
    public class ShipUpgrade : BaseUpgrade
    {
    }

    [CreateAssetMenu(fileName = "ShipWeaponUpgrade", menuName = "Upgrades/ShipWeaponUpgrade")]
    public class ShipLaserUpgrade : ShipUpgrade
    {
        public float damageBonus = 1f;
        public float rangeBonus = 1f;
        public float coolRateBonus = 1f;
        public float heatRateBonus = 1f;
        public float maxChargeBonus = 1f;
    }

    [CreateAssetMenu(fileName = "ShipShieldUpgrade", menuName = "Upgrades/ShipShieldUpgrade")]
    public class ShipShieldUpgrade : ShipUpgrade
    {
        public float shieldHealthBonus = 1f;
        public float shieldRegenerationBonus = 1f;
    }

    [CreateAssetMenu(fileName = "ShipEngineUpgrade", menuName = "Upgrades/ShipEngineUpgrade")]
    public class ShipEngineUpgrade : ShipUpgrade, ISpaceMovementUpgrade
    {
        public float speedBonus = 1f;
        public float accelerationBonus = 1f;
        public float handlingBonus = 1f;

        public float maxChargeBonus = 1f;
        public float boostDeprecationRateBonus = 1f;
        public float boostRechargeRateBonus = 1f;
        public float boostBonus = 1f;

        public float SpeedBonus => speedBonus;
        public float HandlingBonus => handlingBonus;
        public float AccelerationBonus => accelerationBonus;
        public float BoostDeprecationRateBonus => boostDeprecationRateBonus;
        public float BoostRechargeRateBonus => boostRechargeRateBonus;
        public float MaxChargeBonus => maxChargeBonus;
        public float BoostBonus => boostBonus;
    }

    [CreateAssetMenu(fileName = "ShipHullUpgrade", menuName = "Upgrades/ShipHullUpgrade")]
    public class ShipHullUpgrade : ShipUpgrade
    {
        public float hullHealthBonus = 1f;
        public float hullRegenerationBonus = 1f;
    }

    [CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "Upgrades/PlayerUpgrade")]
    public class PlayerUpgrade : BaseUpgrade
    {
    }

    [CreateAssetMenu(fileName = "JetpackUpgrade", menuName = "Upgrades/JetpackUpgrade")]
    public class JetpackUpgrade : PlayerUpgrade
    {
        public float jetpackFuelConsumptionBonus = 1f;
        public float jetpackFuelRegenerationBonus = 1f;
        public float jetpackFuelCapacityBonus = 1f;
        public float jetpackForceBonus = 1f;
    }

    [CreateAssetMenu(fileName = "PlayerWalkingMovementUpgrade", menuName = "Upgrades/PlayerWalkingMovementUpgrade")]
    public class PlayerWalkingMovementUpgrade : PlayerUpgrade
    {
        public float speedBonus = 1f;
        public float jumpHeightBonus = 1f;
        public float sprintSpeedBonus = 1f;
    }

    [CreateAssetMenu(fileName = "PlayerSpaceMovementUpgrade", menuName = "Upgrades/PlayerSpaceMovementUpgrade")]
    public class PlayerSpaceMovementUpgrade : PlayerUpgrade, ISpaceMovementUpgrade
    {
        public float speedBonus = 1f;
        public float accelerationBonus = 1f;
        public float handlingBonus = 1f;

        public float maxChargeBonus = 1f;
        public float boostBonus = 1f;
        public float boostDeprecationRateBonus = 1f;
        public float boostRechargeRateBonus = 1f;
        public float BoostBonus => boostBonus;

        public float SpeedBonus => speedBonus;
        public float HandlingBonus => handlingBonus;
        public float AccelerationBonus => accelerationBonus;
        public float BoostDeprecationRateBonus => boostDeprecationRateBonus;
        public float BoostRechargeRateBonus => boostRechargeRateBonus;
        public float MaxChargeBonus => maxChargeBonus;
    }

    [CreateAssetMenu(fileName = "PlayerHealthUpgrade", menuName = "Upgrades/PlayerHealthUpgrade")]
    public class PlayerHealthUpgrade : PlayerUpgrade
    {
        public float healthBonus = 1f;
        public float healthRegenerationBonus = 1f;
    }

    [CreateAssetMenu(fileName = "PlayerOxygenUpgrade", menuName = "Upgrades/PlayerOxygenUpgrade")]
    public class PlayerOxygenUpgrade : PlayerUpgrade
    {
        public float oxygenBonus = 1f;
        public float oxygenRegenerationBonus = 1f;
        public float oxygenConsumptionBonus = 1f;
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
        public RepairType target;
        public BaseRepair? nextRepair;
        public BaseUpgrade? nextUpgrade;
        public List<UpgradeRequirement> requirements = new();

        public bool CanBeApplied(UpgradeManager manager)
        {
            return requirements.All(req => req.IsMet(manager));
        }
    }


    [CreateAssetMenu(fileName = "WeaponRepair", menuName = "Repairs/WeaponRepair")]
    public class WeaponRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "ShipRepair", menuName = "Repairs/ShipRepair")]
    public class ShipRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "ThrusterRepair", menuName = "Repairs/ThrusterRepair")]
    public class ThrusterRepair : ShipRepair
    {
    }

    [CreateAssetMenu(fileName = "PlayerRepair", menuName = "Repairs/PlayerRepair")]
    public class PlayerRepair : BaseRepair
    {
    }

    [CreateAssetMenu(fileName = "JetpackRepair", menuName = "Repairs/JetpackRepair")]
    public class JetpackRepair : PlayerRepair
    {
    }

    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "UpgradeConfig")]
    public class UpgradeConfig : ScriptableObject
    {
        public List<PlayerUpgrade> playerUpgrades = new();
        public List<ShipUpgrade> shipUpgrades = new();
        public List<WeaponUpgrade> weaponUpgrades = new();

        public List<ShipRepair> shipRepairs = new();
        public List<PlayerRepair> playerRepairs = new();
        public List<WeaponRepair> weaponRepairs = new();
    }
}