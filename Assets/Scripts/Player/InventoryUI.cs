using System;
using System.Collections.Generic;
using CollectableResources;
using UnityEngine.Assertions;
using Interfaces;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Player
{
    public class InventoryUI : MonoBehaviour, IUIPanel
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private UiManager uiManager;

        public UIState AssociatedState => UIState.Inventory;
        private readonly List<InventorySlot> _inventorySlots = new();

        private VisualElement _root;
        private VisualElement _slotsContainer;

        [SerializeField] private int slotsCount = 6;

        private void Awake()
        {
            _root = GetComponentInChildren<UIDocument>().rootVisualElement;
            Assert.IsNotNull(_root, "Root is not found");

            _slotsContainer = _root.Q<VisualElement>("SlotContainer");
            Assert.IsNotNull(_slotsContainer, "Slots container is not found");

            InitSlots();
        }

        private void InitSlots()
        {
            for (var i = 0; i < slotsCount; i++)
            {
                var slot = new InventorySlot();
                _inventorySlots.Add(slot);
                _slotsContainer.Add(slot);
            }

            UpdateInventory(inventory);
        }

        private void Start()
        {
            uiManager.RegisterPanel(this);
            inventory.OnInventoryChanged += UpdateInventory;
        }

        private void UpdateInventory(Inventory _)
        {
            foreach (var resource in inventory.resources)
            {
                foreach (var slot in _inventorySlots)
                {
                    if (!slot.GetResource())
                    {
                        slot.SetResource(resource);
                        break;
                    }

                    if (slot.GetResource().resourceName != resource.resourceName) continue;

                    slot.SetResource(resource);
                    break;
                }
            }
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
        }

        public void Show()
        {
            _root.style.display = DisplayStyle.Flex;
        }

    }
}