using System.Collections.Generic;
using System.Linq;
using CollectableResources;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        public List<ResourceObject> resources = new();
        public event UnityAction<Inventory> OnInventoryChanged;

        public void AddResource(ResourceObject resource)
        {
            foreach (var t in resources.Where(t => t.resourceName == resource.resourceName))
            {
                t.resourceAmount += resource.resourceAmount;
                return;
            }

            resources.Add(resource);
            OnInventoryChanged?.Invoke(this);
        }

        public void RemoveResource(ResourceObject resource)
        {
            foreach (var t in resources.Where(t => t.resourceName == resource.resourceName))
            {
                t.resourceAmount -= resource.resourceAmount;
                if (t.resourceAmount <= 0)
                {
                    resources.Remove(t);
                }
                return;
            }
            OnInventoryChanged?.Invoke(this);
        }

        public ResourceObject GetResource(string resourceName)
        {
            return resources.FirstOrDefault(t => t.resourceName == resourceName);
        }

        public bool HasResource(string resourceName)
        {
            return resources.Any(t => t.resourceName == resourceName);
        }

        public void ClearInventory()
        {
            resources.Clear();
            OnInventoryChanged?.Invoke(this);
        }
    }
}