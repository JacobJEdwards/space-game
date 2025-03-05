using UnityEngine;

namespace Player.Upgrades
{
    [CreateAssetMenu(fileName = "PlayerHealthUpgrade", menuName = "Upgrades/PlayerHealthUpgrade")]
    public class PlayerHealthUpgrade : PlayerUpgrade
    {
        public float healthBonus = 1f;
        public float healthRegenerationBonus = 1f;
    }
}