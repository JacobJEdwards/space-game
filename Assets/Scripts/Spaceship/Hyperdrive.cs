using Interfaces;
using Player;
using Player.Upgrades;
using UnityEngine;

namespace Spaceship
{
    public class Hyperdrive : MonoBehaviour, IRepairable
    {
        private bool _isRepaired;

        public bool CanApplyRepair(BaseRepair repair)
        {
            return repair is HyperdriveRepair;
        }

        public void ApplyRepair(BaseRepair repair)
        {
            if (repair is not HyperdriveRepair) return;

            _isRepaired = true;
        }

        public RepairType GetRepairType()
        {
            return RepairType.Hyperdrive;
        }

        public bool IsRepaired()
        {
            return _isRepaired;
        }
    }
}