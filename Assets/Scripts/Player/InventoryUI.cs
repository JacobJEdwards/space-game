#nullable enable

using System;
using System.Linq;
using CollectableResources;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Player
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Inventory inventory = null!;
        [SerializeField] private Text selectedResourceLabel = null!;
        [SerializeField] private Image selectedResourceIcon = null!;
        [SerializeField] private Text selectedResourceInfo = null!;

        private InventorySlot? _draggedSlot;

        private InventorySlot[] _inventorySlots = null!;
        private ResourceObject? _selectedResource;

        private void Start()
        {
            Assert.IsNotNull(inventory, "Inventory is not assigned");
            inventory.onInventoryChanged.AddListener(UpdateInventory);
            InitSlots();
        }

        private void InitSlots()
        {
            _inventorySlots = GetComponentsInChildren<InventorySlot>(true);
            UpdateInventory();
        }

        private void UpdateInventory()
        {
            foreach (var slot in _inventorySlots) slot.ClearSlot();

            foreach (var resource in inventory.resources)
            {
                var emptySlot = _inventorySlots.FirstOrDefault(x => !x.GetResource());
                emptySlot?.SetResource(resource);
            }
        }

        public void SetSelectedResource(ResourceObject resource)
        {
            _selectedResource = resource;

            selectedResourceIcon.enabled = true;
            selectedResourceIcon.sprite = resource.resourceSprite;
            selectedResourceLabel.text = resource.resourceName;
            selectedResourceInfo.text = resource.resourceDescription;
            selectedResourceIcon.gameObject.SetActive(true);
        }

        public void ClearSelectedResource()
        {
            _selectedResource = null;
            selectedResourceIcon.enabled = false;
            selectedResourceIcon.sprite = null;
            selectedResourceLabel.text = string.Empty;
            selectedResourceInfo.text = string.Empty;
            selectedResourceIcon.gameObject.SetActive(false);
        }

        public void SetDraggedSlot(InventorySlot slot)
        {
            _draggedSlot = slot;
        }

        public void ClearDraggedSlot()
        {
            _draggedSlot = null;
        }

        public InventorySlot? GetDraggedSlot()
        {
            return _draggedSlot;
        }

        public static void SwapItems(InventorySlot fromSlot, InventorySlot toSlot)
        {
            var fromResource = fromSlot.GetResource();
            var toResource = toSlot.GetResource();

            fromSlot.ClearSlot();
            toSlot.ClearSlot();

            if (fromResource)
                toSlot.SetResource(fromResource);
            else
                toSlot.ClearSlot();

            if (toResource)
                fromSlot.SetResource(toResource);
            else
                fromSlot.ClearSlot();
        }


        [Serializable]
        public class ShipInfo
        {
            public GameObject? shipObject;
            public string? shipName;
        }

        [Serializable]
        public class WeaponInfo
        {
            public GameObject? weaponObject;
            public string? weaponName;
        }
    }
}