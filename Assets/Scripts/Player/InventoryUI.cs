using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Player
{
    public class InventoryUI : MonoBehaviour, IUIPanel
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private UiManager uiManager;

        [SerializeField] public ShipInfo currentShip;

        [SerializeField] public WeaponInfo currentWeapon;

        [SerializeField] private int slotsCount = 6;
        private readonly List<InventorySlot> _inventorySlots = new();
        private InventorySlot _draggedSlot;
        private VisualElement _ghostIcon;

        private bool _isDragging;

        private VisualElement _root;

        private VisualElement _shipIcon;
        private Label _shipName;
        private VisualElement _slotsContainer;
        private VisualElement _weaponIcon;
        private Label _weaponName;

        private void Awake()
        {
            _root = GetComponentInChildren<UIDocument>().rootVisualElement;
            Assert.IsNotNull(_root, "Root is not found");

            _slotsContainer = _root.Q<VisualElement>("SlotContainer");
            Assert.IsNotNull(_slotsContainer, "Slots container is not found");

            _ghostIcon = _root.Q<VisualElement>("GhostIcon");
            Assert.IsNotNull(_ghostIcon, "Ghost icon is not found");

            _shipIcon = _root.Q<VisualElement>("ShipImage");
            Assert.IsNotNull(_shipIcon, "Ship icon is not found");

            _shipName = _root.Q<Label>("ShipName");
            Assert.IsNotNull(_shipName, "Ship name is not found");

            _weaponIcon = _root.Q<VisualElement>("WeaponImage");
            Assert.IsNotNull(_weaponIcon, "Weapon icon is not found");

            _weaponName = _root.Q<Label>("WeaponName");
            Assert.IsNotNull(_weaponName, "Weapon name is not found");

            InitSlots();
            UpdateEquipmentDisplay();
        }

        private void Start()
        {
            uiManager.RegisterPanel(this);
            inventory.OnInventoryChanged += _ => UpdateInventory();
        }

        private void OnEnable()
        {
            _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _root.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public UIState AssociatedState => UIState.Inventory;

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
        }

        public void Show()
        {
            _root.style.display = DisplayStyle.Flex;
        }

        private void UpdateEquipmentDisplay()
        {
            if (currentShip != null)
            {
                _shipIcon.style.backgroundImage = new StyleBackground(currentShip.shipSprite);
                _shipName.text = currentShip.shipName;
            }

            if (currentWeapon != null)
            {
                _weaponIcon.style.backgroundImage = new StyleBackground(currentWeapon.weaponSprite);
                _weaponName.text = currentWeapon.weaponName;
            }
        }

        public void SetCurrentShip(ShipInfo ship)
        {
            currentShip = ship;
            UpdateEquipmentDisplay();
        }

        public void SetCurrentWeapon(WeaponInfo weapon)
        {
            currentWeapon = weapon;
            UpdateEquipmentDisplay();
        }

        private void InitSlots()
        {
            for (var i = 0; i < slotsCount; i++)
            {
                var slot = new InventorySlot(this);
                _inventorySlots.Add(slot);
                _slotsContainer.Add(slot);
            }

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

        public void StartDrag(Vector2 position, InventorySlot slot)
        {
            _isDragging = true;
            _draggedSlot = slot;

            _ghostIcon.style.backgroundImage = new StyleBackground(slot.GetResource().resourceSprite);
            _ghostIcon.style.display = DisplayStyle.Flex;
            _ghostIcon.style.left = position.x - _ghostIcon.layout.width / 2;
            _ghostIcon.style.top = position.y - _ghostIcon.layout.height / 2;
            _ghostIcon.style.visibility = Visibility.Visible;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging) return;
            _ghostIcon.style.left = evt.localPosition.x - _ghostIcon.layout.width / 2;
            _ghostIcon.style.top = evt.localPosition.y - _ghostIcon.layout.height / 2;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging || _draggedSlot == null) return;

            var slots = _inventorySlots.Where(x => x.worldBound.Overlaps(_ghostIcon.worldBound));
            var targetSlot = slots.OrderBy(x => Vector2.Distance(x.worldBound.center, _ghostIcon.worldBound.center))
                .FirstOrDefault();

            if (targetSlot != null && targetSlot != _draggedSlot)
            {
                var draggedResource = _draggedSlot.GetResource();
                var targetResource = targetSlot.GetResource();

                if (targetResource)
                {
                    if (targetResource.resourceName == draggedResource.resourceName)
                    {
                        targetResource.resourceAmount += draggedResource.resourceAmount;
                        _draggedSlot.ClearSlot();
                    }
                    else
                    {
                        _draggedSlot.SetResource(targetResource);
                        targetSlot.SetResource(draggedResource);
                    }
                }
                else
                {
                    targetSlot.SetResource(draggedResource);
                    _draggedSlot.ClearSlot();
                }
            }


            _isDragging = false;
            _ghostIcon.style.visibility = Visibility.Hidden;
            _draggedSlot = null;
            UpdateInventory();
        }

        [Serializable]
        public class ShipInfo
        {
            public Sprite shipSprite;
            public string shipName;
        }

        [Serializable]
        public class WeaponInfo
        {
            public Sprite weaponSprite;
            public string weaponName;
        }
    }
}