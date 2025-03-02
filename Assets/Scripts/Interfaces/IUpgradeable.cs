using Player;

namespace Interfaces
{
    public interface IUpgradeable
    {
        public bool CanApplyUpgrade(BaseUpgrade upgrade);
        void ApplyUpgrade(BaseUpgrade upgrade);
        UpgradeType GetUpgradeType();
    }
}