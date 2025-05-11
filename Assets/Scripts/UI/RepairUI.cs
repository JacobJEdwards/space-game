#nullable enable

using System.Linq;
using CollectableResources;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RepairUI : MonoBehaviour
    {
        [SerializeField] private Button upgradeButton = null!;
        [SerializeField] private Text upgradeName = null!;
        [SerializeField] private Text upgradeDescription = null!;
        [SerializeField] private Text upgradeCost = null!;
        public BaseRepair upgradeData = null!;

        public IRepairPanel UpgradePanel = null!;

        public void SetPanel(IRepairPanel panel)
        {
            UpgradePanel = panel;

            upgradeButton.onClick.AddListener(() =>
            {
                if (!upgradeData) return;

                UpgradePanel.TryApplyUpgrade(upgradeData, this);
            });
        }

        public void SetUpgradeData(BaseRepair data)
        {
            upgradeData = data;

            upgradeName.text = upgradeData.upgradeName;
            upgradeDescription.text = upgradeData.upgradeDescription;

            var cost = upgradeData.requirements.Where(r => r.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();
            var resources = cost.Select(r => ((ResourceObject)r.requiredObject, r.requiredAmount)).ToList();
            upgradeCost.text = string.Join("\n", resources.Select(r => $"{r.Item1.resourceName}: {r.requiredAmount}"));
        }
    }
}