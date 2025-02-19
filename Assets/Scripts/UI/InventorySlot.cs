using System;
using CollectableResources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UI
{
    public class InventorySlot : VisualElement
    {
        private ResourceObject _resource;
        private readonly Image _icon;
        private readonly Label _amountLabel;
        private readonly Label _nameLabel;

        public InventorySlot()
        {
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

        }

        public void SetResource(ResourceObject resource)
        {
            _resource = resource;
            _icon.style.backgroundImage = new StyleBackground(resource.resourceSprite);
            _amountLabel.text = resource.resourceAmount.ToString();
            _nameLabel.text = resource.resourceName;
        }

        public void ClearSlot()
        {
            _resource = null;
            _icon.image = null;
            _amountLabel.text = string.Empty;
            _nameLabel.text = string.Empty;
        }

        public ResourceObject GetResource()
        {
            return _resource;
        }
    }
}