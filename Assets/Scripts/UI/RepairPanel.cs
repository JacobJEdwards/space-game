using System.Collections.Generic;
using Managers;
using Player;
using UnityEngine;

namespace UI
{
    public class RepairPanel : MonoBehaviour, IRepairPanel
    {
        [SerializeField] private GameObject target = null!;
        [SerializeField] public RepairType repairType;

        [SerializeField] private GameObject upgradeBlock = null!;
        private readonly List<RepairUI> _upgradesUI = new();
        private UpgradeManager _upgradeManager = null!;

        private void Start()
        {
            _upgradeManager = UpgradeManager.Instance;

            InitUpgrades();
        }

        public void TryApplyUpgrade(BaseRepair upgradeData, RepairUI upgradeUI)
        {
            if (_upgradeManager.TryApplyRepair(upgradeData, target))
            {
                _upgradesUI.Remove(upgradeUI);
                upgradeUI.gameObject.SetActive(false);
                Destroy(upgradeUI.gameObject);
            }

            InitUpgrades();
        }

        private void InitUpgrades()
        {
            foreach (var upgradeUI in _upgradesUI) Destroy(upgradeUI.gameObject);

            var upgrades = _upgradeManager.GetAvailableRepairsForType(repairType);

            foreach (var upgrade in upgrades)
            {
                var upgradeUI = Instantiate(upgradeBlock, transform).GetComponent<RepairUI>();
                upgradeUI.SetPanel(this);
                upgradeUI.SetUpgradeData(upgrade);
                _upgradesUI.Add(upgradeUI);
            }
        }
    }
}