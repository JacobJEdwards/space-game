using Interfaces;
using Player;
using Player.Upgrades;
using UnityEngine;

namespace Movement
{
    public class Thrusters : MonoBehaviour, IRepairable
    {
        private bool _isRepaired;

        public bool CanApplyRepair(BaseRepair repair)
        {
            return repair is ThrusterRepair;
        }

        public void ApplyRepair(BaseRepair repair)
        {
            if (repair is not ThrusterRepair) return;

            _isRepaired = true;
        }

        public RepairType GetRepairType()
        {
            return RepairType.Thrusters;
        }

        public bool IsRepaired()
        {
            return _isRepaired;
        }
    }
}