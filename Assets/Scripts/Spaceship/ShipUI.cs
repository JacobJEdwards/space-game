#nullable enable

using Interfaces;
using Managers;
using Microlight.MicroBar;
using Movement;
using Movement.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace Spaceship
{
    public class ShipUI : MonoBehaviour, IUIPanel
    {
        [Header("Spaceship Settings")] [SerializeField]
        private SpaceMovement shipMovement = null!;

        [SerializeField] private ShipShooting shipShooting = null!;
        [SerializeField] private SpaceMovementConfig shipMovementConfig = null!;

        [Header("UI Settings")] [SerializeField]
        private MicroBar overheatedUI = null!;
        [SerializeField] private MicroBar boostUI = null!;
        private UiManager _uiManager = null!;

        private void Start()
        {
            _uiManager = UiManager.Instance;
            Assert.IsNotNull(shipMovement, "Ship movement is not set!");
            Assert.IsNotNull(shipShooting, "Ship shooting is not set!");
            Assert.IsNotNull(shipMovementConfig, "Ship movement config is not set!");
            Assert.IsNotNull(overheatedUI, "Overheated UI is not set!");
            Assert.IsNotNull(boostUI, "Boost UI is not set!");
            Assert.IsNotNull(_uiManager, "UI Manager is not set!");

            overheatedUI.Initialize(shipShooting.LaserMaxCharge);
            boostUI.Initialize(shipMovementConfig.MaxBoostAmount);

            _uiManager.RegisterPanel(this);
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