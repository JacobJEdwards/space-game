using System.Collections.Generic;
using Managers;
using Player;
using Player.Upgrades;
using UnityEngine;

namespace UI
{
    public class UpgradePanel : MonoBehaviour, IUpgradePanel
    {
        [SerializeField] private GameObject target = null!;
        [SerializeField] public UpgradeType upgradeType;

        [SerializeField] private GameObject upgradeBlock = null!;
        private readonly List<UpgradeUI> _upgradesUI = new();
        private UpgradeManager _upgradeManager = null!;

        private void Start()
        {
            _upgradeManager = UpgradeManager.Instance;

            _upgradeManager.onUpgradeApplied.AddListener(InitUpgrades);

            InitUpgrades();
        }

        public void TryApplyUpgrade(BaseUpgrade upgradeData, UpgradeUI upgradeUI)
        {
            _upgradeManager.TryApplyUpgrade(upgradeData, target);

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
    }
}