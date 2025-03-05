#nullable enable

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
        public UnityEvent onInventoryChanged = new();

        private void Awake()
        {
            resources = new List<ResourceObject>();
        }

        public void AddResource(ResourceObject resource)
        {
            if (resources.Exists(t => t.resourceName == resource.resourceName))
            {
                var res = resources.Find(t => t.resourceName == resource.resourceName);

                res.resourceAmount += resource.resourceAmount;
                onInventoryChanged.Invoke();
                return;
            }

            resources.Add(resource);

            onInventoryChanged.Invoke();
        }

        public void RemoveResource(ResourceObject resource)
        {
            foreach (var t in resources.Where(t => t.resourceName == resource.resourceName))
            {
                t.resourceAmount -= resource.resourceAmount;
                if (t.resourceAmount <= 0) resources.Remove(t);
                return;
            }

            onInventoryChanged.Invoke();
        }

        public void RemoveResource(string resourceName, int amount)
        {
            foreach (var t in resources.Where(t => t.resourceName == resourceName))
            {
                t.resourceAmount -= amount;
                if (t.resourceAmount <= 0)
                    resources.Remove(t);

                onInventoryChanged.Invoke();
                return;
            }

            onInventoryChanged.Invoke();
        }

        public ResourceObject? GetResource(string resourceName)
        {
            return resources.FirstOrDefault(t => t.resourceName == resourceName);
        }

        public bool HasResource(string resourceName)
        {
            return resources.Any(t => t.resourceName == resourceName);
        }

        public bool HasResource(string resourceName, int amount)
        {
            return resources.Any(t => t.resourceName == resourceName && t.resourceAmount >= amount);
        }

        public void ClearInventory()
        {
            resources.Clear();
            onInventoryChanged.Invoke();
        }
    }
}