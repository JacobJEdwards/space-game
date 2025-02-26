using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

namespace Managers
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; } = null!;

        [SerializeField] private Inventory inventory = null!;
        [SerializeField] private InventoryUI inventoryUI = null!;

        [SerializeField] private ShipUpgrade? currentShipUpgrade;
        [SerializeField] private WeaponUpgrade? currentWeaponUpgrade;

        [SerializeField] private List<ShipUpgrade> shipUpgrades = new();
        [SerializeField] private List<WeaponUpgrade> weaponUpgrades = new();

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
        }

        private void Start()
        {
            if (currentShipUpgrade)
            {
                inventoryUI.SetCurrentShip(currentShipUpgrade.shipInfo);
            }

            if (currentWeaponUpgrade)
            {
                inventoryUI.SetCurrentWeapon(currentWeaponUpgrade.weaponInfo);
            }
        }

        public bool CanUpgradeShip(ShipUpgrade upgrade)
        {
            if (upgrade.upgradeData.upgradeCosts.Count == 0) return false;

            return !(from cost in upgrade.upgradeData.upgradeCosts
                let resource = inventory.GetResource(cost.resourceName)
                where !resource || resource.resourceAmount < cost.resourceAmount
                select cost).Any();
        }

        public bool CanUpgradeWeapon(WeaponUpgrade upgrade)
        {
            if (upgrade.upgradeData.upgradeCosts.Count == 0) return false;

            return !(from cost in upgrade.upgradeData.upgradeCosts
                let resource = inventory.GetResource(cost.resourceName)
                where !resource || resource.resourceAmount < cost.resourceAmount
                select cost).Any();
        }

        public void UpgradeShip(ShipUpgrade upgrade)
        {
            if (!CanUpgradeShip(upgrade)) return;

            foreach (var cost in upgrade.upgradeData.upgradeCosts)
            {
                inventory.RemoveResource(cost.resourceName, cost.resourceAmount);
            }

            currentShipUpgrade = upgrade;
            inventoryUI.SetCurrentShip(upgrade.shipInfo);
        }

        public void UpgradeWeapon(WeaponUpgrade upgrade)
        {
            if (!CanUpgradeWeapon(upgrade)) return;

            foreach (var cost in upgrade.upgradeData.upgradeCosts)
            {
                inventory.RemoveResource(cost.resourceName, cost.resourceAmount);
            }

            currentWeaponUpgrade = upgrade;
            inventoryUI.SetCurrentWeapon(upgrade.weaponInfo);
        }

        public ShipUpgrade? GetCurrentShipUpgrade()
        {
            return currentShipUpgrade;
        }

        public WeaponUpgrade? GetCurrentWeaponUpgrade()
        {
            return currentWeaponUpgrade;
        }

        public List<ShipUpgrade> GetShipUpgrades()
        {
            return shipUpgrades;
        }

        public List<WeaponUpgrade> GetWeaponUpgrades()
        {
            return weaponUpgrades;
        }


    }
}