#nullable enable

using System;
using CollectableResources;
using Managers;
using Player.Upgrades;
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
        Impulse,
        Hyperdrive
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
}