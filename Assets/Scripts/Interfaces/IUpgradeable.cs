using Player;
using Player.Upgrades;

namespace Interfaces
{
    public interface IUpgradeable
    {
        public bool CanApplyUpgrade(BaseUpgrade upgrade);
        void ApplyUpgrade(BaseUpgrade upgrade);
        UpgradeType GetUpgradeType();
    }
}