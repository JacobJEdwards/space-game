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

        [SerializeField] private OxygenConfig playerOxygenConfig = null!;

        [Header("UI Settings")] [SerializeField]
        private MicroBar oxygenUI = null!;

        private UiManager _uiManager = null!;

        private bool _zeroGUIActive;

        private void Start()
        {
            Assert.IsNotNull(oxygen, "Oxygen is not assigned");
            Assert.IsNotNull(playerOxygenConfig, "Oxygen config is not assigned");
            Assert.IsNotNull(oxygenUI, "Oxygen UI is not assigned");

            _uiManager = UiManager.Instance;

            oxygenUI.Initialize(playerOxygenConfig.MaxOxygen);
            _uiManager.RegisterPanel(this);
            _uiManager.TransitionToState(UIState.ZeroG);
        }

        private void Update()
        {
            oxygenUI.UpdateBar(oxygen.CurrentOxygen);
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