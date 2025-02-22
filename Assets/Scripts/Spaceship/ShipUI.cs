using Interfaces;
using Managers;
using Microlight.MicroBar;
using Movement;
using UnityEngine;

namespace Spaceship
{
    public class ShipUI : MonoBehaviour, IUIPanel
    {
        [Header("Spaceship Settings")] [SerializeField]
        private SpaceMovement shipMovement;

        [SerializeField] private ShipShooting shipShooting;
        [SerializeField] private SpaceMovementConfig shipMovementConfig;

        [Header("UI Settings")] [SerializeField]
        private MicroBar overheatedUI;

        [SerializeField] private MicroBar boostUI;
        [SerializeField] private UiManager uiManager;

        private void Start()
        {
            overheatedUI.Initialize(shipShooting.LaserMaxCharge);
            boostUI.Initialize(shipMovementConfig.MaxBoostAmount);

            uiManager.RegisterPanel(this);
        }

        private void Update()
        {
            overheatedUI.UpdateBar(shipShooting.LaserCharge);
            boostUI.UpdateBar(shipMovement.CurrentBoostAmount);
        }

        public UIState AssociatedState => UIState.Ship;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}