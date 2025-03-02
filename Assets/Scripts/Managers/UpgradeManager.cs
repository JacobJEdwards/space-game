#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CollectableResources;
using Interfaces;
using Movement;
using Player;
using Spaceship;
using UnityEngine;

namespace Managers
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private UpgradeConfig upgradeConfig = null!;
        [SerializeField] private Inventory inventory = null!;

        private readonly Dictionary<UpgradeType, List<BaseUpgrade>> _appliedUpgrades = new();
        private readonly Dictionary<UpgradeType, List<BaseRepair>> _availableRepairs = new();
        private readonly Dictionary<UpgradeType, List<BaseUpgrade>> _availableUpgrades = new();
        private readonly Dictionary<UpgradeType, List<BaseRepair>> _completedRepairs = new();

        public static UpgradeManager Instance { get; private set; } = null!;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            InitialiseUpgrades();
        }

        private void InitialiseUpgrades()
        {
            foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
            {
                _availableUpgrades[type] = new List<BaseUpgrade>();
                _appliedUpgrades[type] = new List<BaseUpgrade>();
                _completedRepairs[type] = new List<BaseRepair>();
                _availableRepairs[type] = new List<BaseRepair>();
            }

            LoadUpgradesFromConfig();
            UpdateAvailableUpgrades();
        }

        private void LoadUpgradesFromConfig()
        {
            foreach (var upgrade in upgradeConfig.playerUpgrades) _availableUpgrades[UpgradeType.Player].Add(upgrade);

            foreach (var upgrade in upgradeConfig.jetpackUpgrades) _availableUpgrades[UpgradeType.Player].Add(upgrade);

            foreach (var upgrade in upgradeConfig.shipUpgrades) _availableUpgrades[UpgradeType.Ship].Add(upgrade);

            foreach (var upgrade in upgradeConfig.weaponUpgrades) _availableUpgrades[UpgradeType.Weapon].Add(upgrade);

            foreach (var repair in upgradeConfig.playerRepairs) _availableRepairs[UpgradeType.Player].Add(repair);

            foreach (var repair in upgradeConfig.jetpackRepairs) _availableRepairs[UpgradeType.Player].Add(repair);

            foreach (var repair in upgradeConfig.shipRepairs) _availableRepairs[UpgradeType.Ship].Add(repair);

            foreach (var repair in upgradeConfig.weaponRepairs) _availableRepairs[UpgradeType.Weapon].Add(repair);
        }

        private void UpdateAvailableUpgrades()
        {
            foreach (var type in from type in _availableUpgrades.Keys
                     from upgradeItem in _availableUpgrades[type]
                     where upgradeItem.CanBeApplied(this) && !_appliedUpgrades[type].Contains(upgradeItem)
                     select type)
            {
            }

            foreach (var type in from type in _availableRepairs.Keys
                     from repairItem in _availableRepairs[type]
                     where repairItem.CanBeApplied(this) && !_completedRepairs[type].Contains(repairItem)
                     select type)
            {
            }
        }

        public void ApplyUpgrade(BaseUpgrade upgrade, GameObject target)
        {
            var upgradeable = GetUpgradeable(target, upgrade.target);

            if (!upgradeable.CanApplyUpgrade(upgrade)) return;

            if (!upgrade.CanBeApplied(this)) return;

            upgradeable.ApplyUpgrade(upgrade);
            _appliedUpgrades[upgrade.target].Add(upgrade);
            _availableUpgrades[upgrade.target].Remove(upgrade);

            if (upgrade.nextUpgrade) _availableUpgrades[upgrade.nextUpgrade.target].Add(upgrade.nextUpgrade);

            // pay cost
            var cost = upgrade.requirements.Where(req => req.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();

            foreach (var requirement in cost)
                inventory.RemoveResource(
                    (requirement.requiredObject as ResourceObject)?.resourceName ??
                    throw new InvalidOperationException(), requirement.requiredAmount);

            UpdateAvailableUpgrades();
        }

        public bool TryApplyUpgrade(BaseUpgrade upgrade, GameObject target)
        {
            print(target);
            var upgradeable = GetUpgradeable(target, upgrade.target);
            print(upgrade.target);

            if (!upgradeable.CanApplyUpgrade(upgrade)) return false;

            if (!upgrade.CanBeApplied(this)) return false;

            ApplyUpgrade(upgrade, target);

            return true;
        }

        public void ApplyRepair(BaseRepair repair, GameObject target)
        {
            var repairable = GetRepairable(target, repair.target);

            if (!repairable.CanApplyRepair(repair)) return;

            if (!repair.CanBeApplied(this)) return;

            repairable.ApplyRepair(repair);
            _completedRepairs[repair.target].Add(repair);
            _availableRepairs[repair.target].Remove(repair);

            if (repair.nextRepair) _availableRepairs[repair.nextRepair.target].Add(repair.nextRepair);
            if (repair.nextUpgrade) _availableUpgrades[repair.nextUpgrade.target].Add(repair.nextUpgrade);

            var cost = repair.requirements.Where(req => req.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();

            foreach (var requirement in cost)
                inventory.RemoveResource(
                    (requirement.requiredObject as ResourceObject)?.resourceName ??
                    throw new InvalidOperationException(), requirement.requiredAmount);

            UpdateAvailableUpgrades();
        }

        public bool TryApplyRepair(BaseRepair repair, GameObject target)
        {
            var repairable = GetRepairable(target, repair.target);

            if (!repairable.CanApplyRepair(repair)) return false;

            if (!repair.CanBeApplied(this)) return false;

            ApplyRepair(repair, target);

            return true;
        }

        private static IUpgradeable GetUpgradeable(GameObject target, UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.Player => target.GetComponent<PlayerController>(),
                UpgradeType.Ship => target.GetComponent<ShipController>(),
                UpgradeType.Weapon => target.GetComponent<Shooting>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IRepairable GetRepairable(GameObject target, UpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case UpgradeType.Player:
                    return target.GetComponent<Jetpack>();
                case UpgradeType.Ship:
                // return target.GetComponent<Hyperdrive>();
                case UpgradeType.Weapon:
                //return target.GetComponent<Gun>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsUpgradeApplied(BaseUpgrade upgrade)
        {
            return _appliedUpgrades[upgrade.target].Contains(upgrade);
        }

        public bool IsRepairCompleted(BaseRepair repair)
        {
            return _completedRepairs[repair.target].Contains(repair);
        }

        public bool IsUpgradeAvailable(BaseUpgrade upgrade)
        {
            return _availableUpgrades[upgrade.target].Contains(upgrade);
        }

        public bool IsRepairAvailable(BaseRepair repair)
        {
            return _availableRepairs[repair.target].Contains(repair);
        }

        public bool HasResources(ResourceObject? resource, int amount)
        {
            return resource && inventory.HasResource(resource.resourceName, amount);
        }

        public List<BaseUpgrade> GetAvailableUpgradesForType(UpgradeType type)
        {
            print(type);
            return _availableUpgrades[type];
        }

        public List<BaseRepair> GetAvailableRepairsForType(UpgradeType type)
        {
            return _availableRepairs[type];
        }

        public bool IsUpgradeUnlocked(BaseUpgrade? upgrade)
        {
            return upgrade && _appliedUpgrades[upgrade.target].Any(u => u == upgrade);
        }

        public bool IsRepairComplete(BaseRepair? repair)
        {
            return repair && _completedRepairs[repair.target].Any(r => r == repair);
        }
    }
}