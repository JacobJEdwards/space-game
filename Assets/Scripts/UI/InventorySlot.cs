using System;
using CollectableResources;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UI
{
    public class InventorySlot : VisualElement
    {
        private ResourceObject _resource;
        private readonly Image _icon;

        public InventorySlot()
        {
            _icon = new Image();
            Add(_icon);

            _icon.AddToClassList("slotIcon");
            AddToClassList("slotContainer");
        }

        public void SetResource(ResourceObject resource)
        {
            _resource = resource;
            _icon.image = resource.resourceSprite.texture;
        }

        public void ClearSlot()
        {
            _resource = null;
            _icon.image = null;
        }

        public ResourceObject GetResource()
        {
            return _resource;
        }
    }
}