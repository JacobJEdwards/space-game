using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "ShipEngineUpgrade", menuName = "Upgrades/ShipEngineUpgrade")]
    public class ShipEngineUpgrade : ShipUpgrade, ISpaceMovementUpgrade
    {
        public float accelerationBonus = 1f;
        public float boostBonus = 1f;
        public float boostDeprecationRateBonus = 1f;
        public float boostRechargeRateBonus = 1f;
        public float handlingBonus = 1f;

        public float maxChargeBonus = 1f;
        public float speedBonus = 1f;

        public float SpeedBonus => speedBonus;
        public float HandlingBonus => handlingBonus;
        public float AccelerationBonus => accelerationBonus;
        public float BoostDeprecationRateBonus => boostDeprecationRateBonus;
        public float BoostRechargeRateBonus => boostRechargeRateBonus;
        public float MaxChargeBonus => maxChargeBonus;
        public float BoostBonus => boostBonus;
    }
}