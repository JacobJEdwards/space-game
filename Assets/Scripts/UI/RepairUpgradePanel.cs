using System.Collections.Generic;
using Managers;
using Player;
using Player.Upgrades;
using UnityEngine;

namespace UI
{
    public interface IUpgradePanel
    {
        void TryApplyUpgrade(BaseUpgrade upgradeData, UpgradeUI upgradeUI);
    }

    public interface IRepairPanel
    {
        void TryApplyUpgrade(BaseRepair upgradeData, RepairUI upgradeUI);
    }

    public class RepairUpgradePanel : MonoBehaviour, IUpgradePanel, IRepairPanel
    {
        [SerializeField] private GameObject target = null!;

        [SerializeField] public UpgradeType upgradeType;
        [SerializeField] public RepairType repairType;

        [SerializeField] private GameObject upgradeBlock = null!;
        [SerializeField] private GameObject repairBlock = null!;

        private readonly List<RepairUI> _repairsUI = new();
        private readonly List<UpgradeUI> _upgradesUI = new();

        private UpgradeManager _upgradeManager = null!;

        private void Start()
        {
            _upgradeManager = UpgradeManager.Instance;

            _upgradeManager.onUpgradeApplied.AddListener(InitUpgrades);
            _upgradeManager.onRepairApplied.AddListener(InitRepairs);

            InitUpgrades();
            InitRepairs();
        }

        public void TryApplyUpgrade(BaseRepair upgradeData, RepairUI upgradeUI)
        {
            if (_upgradeManager.TryApplyRepair(upgradeData, target))
            {
                _repairsUI.Remove(upgradeUI);
                upgradeUI.gameObject.SetActive(false);
                Destroy(upgradeUI.gameObject);
            }

            InitRepairs();
            InitUpgrades();
        }

        public void TryApplyUpgrade(BaseUpgrade upgradeData, UpgradeUI upgradeUI)
        {
            if (_upgradeManager.TryApplyUpgrade(upgradeData, target))
            {
                _upgradesUI.Remove(upgradeUI);
                upgradeUI.gameObject.SetActive(false);
                Destroy(upgradeUI.gameObject);
            }

            InitRepairs();
            InitUpgrades();
        }

        private void InitUpgrades(BaseUpgrade upgrade)
        {
            InitUpgrades();
        }

        private void InitUpgrades()
        {
            foreach (var upgradeUI in _upgradesUI) Destroy(upgradeUI.gameObject);

            _upgradesUI.Clear();

            var upgrades = _upgradeManager.GetAvailableUpgradesForType(upgradeType);

            foreach (var upgrade in upgrades)
            {
                var upgradeUI = Instantiate(upgradeBlock, transform).GetComponent<UpgradeUI>();
                upgradeUI.SetPanel(this);
                upgradeUI.SetUpgradeData(upgrade);
                _upgradesUI.Add(upgradeUI);
            }
        }

        private void InitRepairs(BaseRepair repair)
        {
            InitRepairs();
        }

        private void InitRepairs()
        {
            foreach (var repairUI in _repairsUI) Destroy(repairUI.gameObject);

            _repairsUI.Clear();

            var repairs = _upgradeManager.GetAvailableRepairsForType(repairType);

            foreach (var repair in repairs)
            {
                var repairUI = Instantiate(repairBlock, transform).GetComponent<RepairUI>();
                repairUI.SetPanel(this);
                repairUI.SetUpgradeData(repair);
                _repairsUI.Add(repairUI);
            }
        }
    }
}