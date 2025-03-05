#nullable enable

using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Player.Upgrades
{
    public abstract class BaseUpgrade : ScriptableObject
    {
        public string upgradeName = null!;
        public string upgradeDescription = null!;
        public Sprite upgradeIcon = null!;
        public UpgradeType target;
        public BaseUpgrade? nextUpgrade;
        public List<UpgradeRequirement> requirements = new();

        public bool CanBeApplied(UpgradeManager manager)
        {
            return requirements.All(req => req.IsMet(manager));
        }
    }
}