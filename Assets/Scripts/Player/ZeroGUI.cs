using Interfaces;
using Managers;
using Microlight.MicroBar;
using UnityEngine;

namespace Player
{
    public class ZeroGUI : MonoBehaviour, IUIPanel
    {
        [Header("ZeroG Settings")] [SerializeField]
        private Oxygen oxygen;

        [SerializeField] private OxygenConfig playerOxygenConfig;

        [Header("UI Settings")] [SerializeField]
        private MicroBar oxygenUI;

        [SerializeField] private UiManager uiManager;

        private bool _zeroGUIActive;

        private void Start()
        {
            oxygenUI.Initialize(playerOxygenConfig.MaxOxygen);
            uiManager.RegisterPanel(this);
            uiManager.TransitionToState(UIState.ZeroG);
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