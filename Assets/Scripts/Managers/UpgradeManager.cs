#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CollectableResources;
using Interfaces;
using Movement;
using Player;
using Player.Upgrades;
using Spaceship;
using UnityEngine;
using UnityEngine.Events;
using Weapons;

namespace Managers
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private UpgradeConfig upgradeConfig = null!;
        [SerializeField] private Inventory inventory = null!;

        [SerializeField] private GameObject playerController = null!;
        [SerializeField] private GameObject shipController = null!;

        public UnityEvent<BaseUpgrade> onUpgradeApplied = new();
        public UnityEvent<BaseRepair> onRepairApplied = new();
        private readonly Dictionary<RepairType, List<BaseRepair>> _availableRepairs = new();
        private readonly Dictionary<UpgradeType, List<BaseUpgrade>> _availableUpgrades = new();

        public readonly Dictionary<UpgradeType, List<BaseUpgrade>> AppliedUpgrades = new();
        public readonly Dictionary<RepairType, List<BaseRepair>> CompletedRepairs = new();

        public static UpgradeManager Instance { get; private set; } = null!;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);

            InitialiseUpgrades();
        }

        private GameObject GetTarget(BaseUpgrade upgrade)
        {
            return upgrade.target switch
            {
                UpgradeType.Player => playerController,
                UpgradeType.Ship => shipController,
                UpgradeType.Weapon => playerController,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private GameObject GetTarget(BaseRepair repair)
        {
            return repair.target switch
            {
                RepairType.Jetpack => playerController,
                RepairType.Thrusters => shipController,
                RepairType.Impulse => shipController,
                RepairType.Hyperdrive => shipController,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void ApplyUpgrade(BaseUpgrade upgrade)
        {
            ApplyUpgrade(upgrade, GetTarget(upgrade));
        }

        public void ApplyUpgrade(string n)
        {
            var upgrade = _availableUpgrades.Values.SelectMany(u => u).FirstOrDefault(u => u.upgradeName == n);

            print(n);
            print(upgrade);
            if (!upgrade) return;

            print("Applying upgrade: " + upgrade.upgradeName);

            ApplyUpgrade(upgrade, GetTarget(upgrade));
        }

        public void ApplyRepair(string n)
        {
            var repair = _availableRepairs.Values.SelectMany(u => u).FirstOrDefault(u => u.upgradeName == n);

            print(n);
            print(repair);
            if (!repair) return;

            print("Applying repair: " + repair.upgradeName);

            ApplyRepair(repair, GetTarget(repair));
        }


        public void ApplyRepair(BaseRepair repair)
        {
            ApplyRepair(repair, GetTarget(repair));
        }

        private void InitialiseUpgrades()
        {
            foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
            {
                _availableUpgrades[type] = new List<BaseUpgrade>();
                AppliedUpgrades[type] = new List<BaseUpgrade>();
            }

            foreach (RepairType type in Enum.GetValues(typeof(RepairType)))
            {
                _availableRepairs[type] = new List<BaseRepair>();
                CompletedRepairs[type] = new List<BaseRepair>();
            }

            LoadUpgradesFromConfig();
            UpdateAvailableUpgrades();
        }

        private void LoadUpgradesFromConfig()
        {
            if (!upgradeConfig) upgradeConfig = Resources.Load<UpgradeConfig>("Upgrades/UpgradeConfig");

            foreach (var upgrade in upgradeConfig.playerUpgrades) _availableUpgrades[UpgradeType.Player].Add(upgrade);

            foreach (var upgrade in upgradeConfig.shipUpgrades) _availableUpgrades[UpgradeType.Ship].Add(upgrade);

            foreach (var upgrade in upgradeConfig.weaponUpgrades) _availableUpgrades[UpgradeType.Weapon].Add(upgrade);

            // fix below doesnt make sense
            foreach (var repair in upgradeConfig.playerRepairs) _availableRepairs[RepairType.Jetpack].Add(repair);

            foreach (var repair in upgradeConfig.shipRepairs) _availableRepairs[RepairType.Thrusters].Add(repair);

            foreach (var repair in upgradeConfig.weaponRepairs) _availableRepairs[RepairType.Impulse].Add(repair);
        }

        private void UpdateAvailableUpgrades()
        {
            foreach (var type in from type in _availableUpgrades.Keys
                     from upgradeItem in _availableUpgrades[type]
                     where upgradeItem.CanBeApplied(this) && !AppliedUpgrades[type].Contains(upgradeItem)
                     select type)
            {
            }

            foreach (var type in from type in _availableRepairs.Keys
                     from repairItem in _availableRepairs[type]
                     where repairItem.CanBeApplied(this) && !CompletedRepairs[type].Contains(repairItem)
                     select type)
            {
            }
        }

        private void ApplyUpgrade(BaseUpgrade upgrade, GameObject target)
        {
            var upgradeable = GetUpgradeable(target, upgrade.target);

            if (!upgradeable.CanApplyUpgrade(upgrade)) return;

            if (!upgrade.CanBeApplied(this)) return;

            upgradeable.ApplyUpgrade(upgrade);
            AppliedUpgrades[upgrade.target].Add(upgrade);
            _availableUpgrades[upgrade.target].Remove(upgrade);

            if (upgrade.nextUpgrade) _availableUpgrades[upgrade.nextUpgrade.target].Add(upgrade.nextUpgrade);

            // pay cost
            var cost = upgrade.requirements.Where(req => req.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();

            foreach (var requirement in cost)
                inventory.RemoveResource(
                    (requirement.requiredObject as ResourceObject)?.resourceName ??
                    throw new InvalidOperationException(), requirement.requiredAmount);

            onUpgradeApplied.Invoke(upgrade);

            UpdateAvailableUpgrades();
        }

        public bool TryApplyUpgrade(BaseUpgrade upgrade, GameObject target)
        {
            var upgradeable = GetUpgradeable(target, upgrade.target);

            if (!upgradeable.CanApplyUpgrade(upgrade)) return false;

            if (!upgrade.CanBeApplied(this)) return false;

            ApplyUpgrade(upgrade, target);

            return true;
        }

        private void ApplyRepair(BaseRepair repair, GameObject target)
        {
            var repairable = GetRepairable(target, repair.target);

            if (!repairable.CanApplyRepair(repair)) return;

            if (!repair.CanBeApplied(this)) return;

            repairable.ApplyRepair(repair);
            CompletedRepairs[repair.target].Add(repair);
            _availableRepairs[repair.target].Remove(repair);

            if (repair.nextRepair) _availableRepairs[repair.nextRepair.target].Add(repair.nextRepair);

            if (repair.nextUpgrade) _availableUpgrades[repair.nextUpgrade.target].Add(repair.nextUpgrade);

            var cost = repair.requirements.Where(req => req.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();

            foreach (var requirement in cost)
                inventory.RemoveResource(
                    (requirement.requiredObject as ResourceObject)?.resourceName ??
                    throw new InvalidOperationException(), requirement.requiredAmount);

            onRepairApplied.Invoke(repair);

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

        private static IRepairable GetRepairable(GameObject target, RepairType upgradeType)
        {
            return upgradeType switch
            {
                RepairType.Jetpack => target.GetComponent<Jetpack>(),
                RepairType.Thrusters => target.GetComponent<Thrusters>(),
                RepairType.Hyperdrive => target.GetComponent<Hyperdrive>(),
                RepairType.Impulse => target.GetComponent<Impulse>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool IsUpgradeApplied(BaseUpgrade upgrade)
        {
            return AppliedUpgrades[upgrade.target].Contains(upgrade);
        }

        public bool IsRepairCompleted(BaseRepair repair)
        {
            return CompletedRepairs[repair.target].Contains(repair);
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

        public List<BaseRepair> GetAvailableRepairsForType(RepairType type)
        {
            return _availableRepairs[type];
        }

        public bool IsUpgradeUnlocked(BaseUpgrade? upgrade)
        {
            return upgrade && AppliedUpgrades[upgrade.target].Any(u => u == upgrade);
        }

        public bool IsRepairComplete(BaseRepair? repair)
        {
            return repair && CompletedRepairs[repair.target].Any(r => r == repair);
        }
    }
}