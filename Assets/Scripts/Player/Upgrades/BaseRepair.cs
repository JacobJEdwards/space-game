#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Player.Upgrades;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class BaseRepair : ScriptableObject
    {
        public string upgradeName = null!;
        public string upgradeDescription = null!;
        public Sprite upgradeIcon = null!;
        public RepairType target;
        public BaseRepair? nextRepair;
        public List<UpgradeRequirement> requirements = new();
        public BaseUpgrade? nextUpgrade;

        public bool CanBeApplied(UpgradeManager manager)
        {
            return requirements.All(req => req.IsMet(manager));
        }
    }
}