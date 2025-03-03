using Player;

namespace Interfaces
{
    public interface IRepairable
    {
        public bool CanApplyRepair(BaseRepair repair);
        void ApplyRepair(BaseRepair repair);
        RepairType GetRepairType();
        public bool IsRepaired();
    }
}