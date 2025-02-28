#nullable enable

using Interfaces;
using Managers;
using Microlight.MicroBar;
using Unity.Assertions;
using UnityEngine;

namespace Player
{
    public class ZeroGUI : MonoBehaviour, IUIPanel
    {
        [Header("ZeroG Settings")] [SerializeField]
        private Oxygen oxygen = null!;
        [SerializeField]
        private Health health = null!;

        [Header("UI Settings")] [SerializeField]
        private MicroBar oxygenUI = null!;
        [SerializeField] private MicroBar healthUI = null!;

        private UiManager _uiManager = null!;

        private bool _zeroGUIActive;

        private void Start()
        {
            Assert.IsNotNull(oxygen, "Oxygen is not assigned");
            Assert.IsNotNull(health, "health is not assigned");
            Assert.IsNotNull(oxygenUI, "Oxygen UI is not assigned");

            _uiManager = UiManager.Instance;

            oxygenUI.Initialize(oxygen.MaxOxygen);
            healthUI.Initialize(health.MaxHealth);
            _uiManager.RegisterPanel(this);
            _uiManager.TransitionToState(UIState.ZeroG);
        }

        private void Update()
        {
            oxygenUI.UpdateBar(oxygen.CurrentOxygen);
            healthUI.UpdateBar(health.CurrentHealth);
        }

        public UIState AssociatedState => UIState.ZeroG;


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