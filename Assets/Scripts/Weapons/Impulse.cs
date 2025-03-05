using Interfaces;
using Player;
using Player.Upgrades;
using UnityEngine;

namespace Weapons
{
    public class Impulse : MonoBehaviour, IRepairable
    {
        private bool _isRepaired;

        public bool CanApplyRepair(BaseRepair repair)
        {
            return repair is ImpulseRepair;
        }

        public void ApplyRepair(BaseRepair repair)
        {
            if (repair is not ImpulseRepair) return;

            _isRepaired = true;
        }

        public RepairType GetRepairType()
        {
            return RepairType.Impulse;
        }

        public bool IsRepaired()
        {
            return _isRepaired;
        }
    }
}