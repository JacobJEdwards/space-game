#nullable enable

using System.Linq;
using CollectableResources;
using Player;
using Player.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeUI : MonoBehaviour
    {
        [SerializeField] private Button upgradeButton = null!;
        [SerializeField] private Text upgradeName = null!;
        [SerializeField] private Text upgradeDescription = null!;
        [SerializeField] private Text upgradeCost = null!;
        public BaseUpgrade upgradeData = null!;

        public IUpgradePanel UpgradePanel = null!;

        public void SetPanel(IUpgradePanel panel)
        {
            UpgradePanel = panel;

            upgradeButton.onClick.AddListener(() =>
            {
                if (!upgradeData) return;

                UpgradePanel.TryApplyUpgrade(upgradeData, this);
            });
        }


        public void SetUpgradeData(BaseUpgrade data)
        {
            upgradeData = data;

            upgradeName.text = upgradeData.upgradeName;
            upgradeDescription.text = upgradeData.upgradeDescription;

            // cast to resource
            var cost = upgradeData.requirements.Where(r => r.type == UpgradeRequirement.RequirementType.Resource)
                .ToList();
            var resources = cost.Select(r => ((ResourceObject)r.requiredObject, r.requiredAmount)).ToList();
            upgradeCost.text = string.Join("\n", resources.Select(r => $"{r.Item1.resourceName}: {r.Item2}"));
        }
    }
}