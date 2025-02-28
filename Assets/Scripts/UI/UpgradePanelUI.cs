#nullable enable

using Managers;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UpgradePanelUI : MonoBehaviour
    {
        [SerializeField] private UpgradeManager upgradeManager = null!;
        [SerializeField] private Inventory inventory = null!;

        private VisualElement _root = null!;
        private VisualElement _shipUpgradePanel = null!;
        private VisualElement _weaponUpgradePanel = null!;
        private ScrollView _shipUpgradeOptions = null!;
        private ScrollView _weaponUpgradeOptions = null!;
        private Button _shipUpgradeButton = null!;
        private Button _weaponUpgradeButton = null!;
        private Button _closeShipUpgradeButton = null!;
        private Button _closeWeaponUpgradeButton = null!;

        private void Awake()
        {
            _root = GetComponentInChildren<UIDocument>().rootVisualElement;

            _shipUpgradePanel = _root.Q<VisualElement>("ShipUpgradePanel");
            _weaponUpgradePanel = _root.Q<VisualElement>("WeaponUpgradePanel");
            _shipUpgradeOptions = _root.Q<ScrollView>("ShipUpgradeOptions");
            _weaponUpgradeOptions = _root.Q<ScrollView>("WeaponUpgradeOptions");
            _shipUpgradeButton = _root.Q<Button>("ShipUpgradeButton");
            _weaponUpgradeButton = _root.Q<Button>("WeaponUpgradeButton");
            _closeShipUpgradeButton = _root.Q<Button>("CloseShipUpgradeButton");
            _closeWeaponUpgradeButton = _root.Q<Button>("CloseWeaponUpgradeButton");

            _shipUpgradeButton.RegisterCallback<ClickEvent>(_ => ShowShipUpgradePanel());
            _weaponUpgradeButton.RegisterCallback<ClickEvent>(_ => ShowWeaponUpgradePanel());
            _closeShipUpgradeButton.RegisterCallback<ClickEvent>(_ => HideShipUpgradePanel());
            _closeWeaponUpgradeButton.RegisterCallback<ClickEvent>(_ => HideWeaponUpgradePanel());

            _shipUpgradePanel.style.display = DisplayStyle.None;
            _weaponUpgradePanel.style.display = DisplayStyle.None;

            inventory.OnInventoryChanged += _ => RefreshUpgradeOptions();
        }

        private void Start()
        {
            upgradeManager = UpgradeManager.Instance;
            RefreshUpgradeOptions();
        }

        public void ShowShipUpgradePanel()
        {
            // Center the panel
            _shipUpgradePanel.style.left = _root.panel.visualTree.worldBound.width / 2 - 200;
            _shipUpgradePanel.style.top = _root.panel.visualTree.worldBound.height / 2 - 200;

            // Show the panel
            _shipUpgradePanel.style.display = DisplayStyle.Flex;

            // Ensure weapon panel is hidden
            _weaponUpgradePanel.style.display = DisplayStyle.None;

            // Populate with current upgrade options
            PopulateShipUpgrades();
        }

        public void ShowWeaponUpgradePanel()
        {
            // Center the panel
            _weaponUpgradePanel.style.left = _root.panel.visualTree.worldBound.width / 2 - 200;
            _weaponUpgradePanel.style.top = _root.panel.visualTree.worldBound.height / 2 - 200;

            // Show the panel
            _weaponUpgradePanel.style.display = DisplayStyle.Flex;

            // Ensure ship panel is hidden
            _shipUpgradePanel.style.display = DisplayStyle.None;

            // Populate with current upgrade options
            PopulateWeaponUpgrades();
        }

        public void HideShipUpgradePanel()
        {
            _shipUpgradePanel.style.display = DisplayStyle.None;
        }

        public void HideWeaponUpgradePanel()
        {
            _weaponUpgradePanel.style.display = DisplayStyle.None;
        }

        private void RefreshUpgradeOptions()
        {
            // Update button states
            _shipUpgradeButton.SetEnabled(HasAvailableShipUpgrades());
            _weaponUpgradeButton.SetEnabled(HasAvailableWeaponUpgrades());

            // If panels are open, refresh their content
            if (_shipUpgradePanel.style.display == DisplayStyle.Flex)
            {
                PopulateShipUpgrades();
            }

            if (_weaponUpgradePanel.style.display == DisplayStyle.Flex)
            {
                PopulateWeaponUpgrades();
            }
        }

        private bool HasAvailableShipUpgrades()
        {
            var availableUpgrades = upgradeManager.GetShipUpgrades();
            return availableUpgrades.Count > 0;
        }

        private bool HasAvailableWeaponUpgrades()
        {
            var availableUpgrades = upgradeManager.GetWeaponUpgrades();
            return availableUpgrades.Count > 0;
        }

        private void PopulateShipUpgrades()
        {
            _shipUpgradeOptions.Clear();

            var availableUpgrades = upgradeManager.GetShipUpgrades();
            foreach (var upgrade in availableUpgrades)
            {
                _shipUpgradeOptions.Add(CreateUpgradeOption(upgrade, true));
            }
        }

        private void PopulateWeaponUpgrades()
        {
            _weaponUpgradeOptions.Clear();

            var availableUpgrades = upgradeManager.GetWeaponUpgrades();
            foreach (var upgrade in availableUpgrades)
            {
                _weaponUpgradeOptions.Add(CreateUpgradeOption(upgrade, false));
            }
        }

        private VisualElement CreateUpgradeOption(object upgrade, bool isShip)
        {
            var option = new VisualElement();
            option.AddToClassList("upgradeOption");

            var icon = new VisualElement();
            icon.AddToClassList("upgradeOptionIcon");
            option.Add(icon);

            var info = new VisualElement();
            info.AddToClassList("upgradeInfo");
            option.Add(info);

            var title = new Label();
            title.AddToClassList("upgradeTitle");
            info.Add(title);

            var description = new Label();
            description.AddToClassList("upgradeDescription");
            info.Add(description);

            var requirementsContainer = new VisualElement();
            info.Add(requirementsContainer);

            var button = new Button();
            button.AddToClassList("upgradeOptionButton");
            option.Add(button);

            // Set data based on upgrade type
            UpgradeData? upgradeData = null;
            var canUpgrade = false;

            switch (isShip)
            {
                case true when upgrade is ShipUpgrade shipUpgrade:
                    upgradeData = shipUpgrade.upgradeData;
                    title.text = shipUpgrade.shipInfo.shipName ?? "Unknown Ship";
                    description.text = $"Speed: +{shipUpgrade.speedBonus}, Health: +{shipUpgrade.healthBonus}, Shield: +{shipUpgrade.shieldBonus}";
                    icon.style.backgroundImage = new StyleBackground(ModelPreviewManager.Instance.ToTexture2D(shipUpgrade.shipInfo.shipObject));
                    canUpgrade = upgradeManager.CanUpgradeShip(shipUpgrade);
                    button.clicked += () => { upgradeManager.UpgradeShip(shipUpgrade); HideShipUpgradePanel(); };
                    break;
                case false when upgrade is WeaponUpgrade weaponUpgrade:
                    upgradeData = weaponUpgrade.upgradeData;
                    title.text = weaponUpgrade.weaponInfo.weaponName ?? "Unknown Weapon";
                    var text = GetWeaponText(weaponUpgrade);
                    description.text = text;
                    icon.style.backgroundImage = new StyleBackground(ModelPreviewManager.Instance.ToTexture2D(weaponUpgrade.weaponInfo.weaponObject));
                    canUpgrade = upgradeManager.CanUpgradeWeapon(weaponUpgrade);
                    button.clicked += () => { upgradeManager.UpgradeWeapon(weaponUpgrade); HideWeaponUpgradePanel(); };
                    break;
            }

            // Add resource requirements
            if (upgradeData)
            {
                foreach (var cost in upgradeData.upgradeCosts)
                {
                    var resourceReq = new VisualElement();
                    resourceReq.AddToClassList("resourceRequirement");

                    var resourceIcon = new VisualElement();
                    resourceIcon.AddToClassList("resourceIcon");

                    var resourceText = new Label
                    {
                        text = $"{cost.resourceName}: {cost.resourceAmount}"
                    };

                    // Check if player has enough resources
                    var resource = inventory.GetResource(cost.resourceName);
                    var hasEnough = resource && resource.resourceAmount >= cost.resourceAmount;

                    resourceText.AddToClassList(hasEnough ? "resourceAvailable" : "resourceMissing");

                    resourceReq.Add(resourceIcon);
                    resourceReq.Add(resourceText);
                    requirementsContainer.Add(resourceReq);
                }
            }

            // Set button state and text
            button.text = canUpgrade ? "Upgrade" : "Not Enough";
            button.SetEnabled(canUpgrade);

            return option;
        }

        private static string GetWeaponText(WeaponUpgrade upgrade)
        {
            var fireRate = upgrade.fireRateBonus > 0 ? $", Fire Rate: +{upgrade.fireRateBonus}" : "";
            var energyCost = upgrade.energyCostBonus > 0 ? $", Energy Cost: +{upgrade.energyCostBonus}" : "";
            var range = upgrade.rangeBonus > 0 ? $", Range: +{upgrade.rangeBonus}" : "";

            return $"Damage: +{upgrade.damageBonus}{fireRate}{energyCost}{range}";
        }


    }
}