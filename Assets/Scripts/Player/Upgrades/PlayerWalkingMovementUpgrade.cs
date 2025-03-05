using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "PlayerWalkingMovementUpgrade", menuName = "Upgrades/PlayerWalkingMovementUpgrade")]
    public class PlayerWalkingMovementUpgrade : PlayerUpgrade
    {
        public float jumpHeightBonus = 1f;
        public float speedBonus = 1f;
        public float sprintSpeedBonus = 1f;
    }
}