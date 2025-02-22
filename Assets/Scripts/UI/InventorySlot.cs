using CollectableResources;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class InventorySlot : VisualElement
    {
        private readonly Label _amountLabel;
        private readonly Image _icon;
        private readonly InventoryUI _inventoryUI;
        private readonly Label _nameLabel;
        private ResourceObject _resource;

        public InventorySlot(InventoryUI inventoryUI)
        {
            _inventoryUI = inventoryUI;
            _icon = new Image();
            _amountLabel = new Label();
            _nameLabel = new Label();
            Add(_icon);
            Add(_amountLabel);
            Add(_nameLabel);

            _icon.AddToClassList("slotIcon");
            _amountLabel.AddToClassList("slotAmount");
            _nameLabel.AddToClassList("slotName");

            AddToClassList("slotContainer");

            _nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _amountLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void SetResource(ResourceObject resource)
        {
            _resource = resource;
            if (!resource)
            {
                ClearSlot();
                return;
            }

            _icon.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            _amountLabel.text = resource.resourceAmount.ToString();
            _nameLabel.text = resource.resourceName;
        }

        public void ClearSlot()
        {
            _resource = null;
            _icon.style.backgroundImage = new StyleBackground();
            _amountLabel.text = string.Empty;
            _nameLabel.text = string.Empty;
        }

        public ResourceObject GetResource()
        {
            return _resource;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || !_resource) return;
            _inventoryUI.StartDrag(evt.position, this);
        }
    }
}