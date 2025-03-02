using System;
using Interfaces;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class MenuUI : MonoBehaviour, IUIPanel
    {
        public UIState AssociatedState => UIState.Inventory;

        private UiManager _uiManager;

        private enum View
        {
            Inventory,
            Gear,
            Starship
        }

        private InventoryUI _inventoryUI;
        private GearUI _gearUI;
        private StarshipUI _starshipUI;

        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button gearButton;
        [SerializeField] private Button starshipButton;

        private View _currentView = View.Inventory;

        private void Start()
        {
            _inventoryUI = GetComponentInChildren<InventoryUI>(true);
            _gearUI = GetComponentInChildren<GearUI>(true);
            _starshipUI = GetComponentInChildren<StarshipUI>(true);

            _uiManager = UiManager.Instance;
            _uiManager.RegisterPanel(this);

            inventoryButton.onClick.AddListener(() => MoveToView(View.Inventory));
            gearButton.onClick.AddListener(() => MoveToView(View.Gear));
            starshipButton.onClick.AddListener(() => MoveToView(View.Starship));

            MoveToView(View.Inventory);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void MoveToView(View view)
        {
            _inventoryUI.gameObject.SetActive(false);
            _gearUI.gameObject.SetActive(false);
            _starshipUI.gameObject.SetActive(false);

            switch (view)
            {
                case View.Inventory:
                    _inventoryUI.gameObject.SetActive(true);
                    break;
                case View.Gear:
                    _gearUI.gameObject.SetActive(true);
                    break;
                case View.Starship:
                    _starshipUI.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }
    }
}