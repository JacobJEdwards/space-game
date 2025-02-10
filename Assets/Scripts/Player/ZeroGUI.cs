using Managers;
using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.Serialization;
using Interfaces;

namespace Player
{
public class ZeroGUI : MonoBehaviour, IUIPanel
{
    [FormerlySerializedAs("playerOxygen")]
    [Header("ZeroG Settings")]
    [SerializeField] private Oxygen oxygen;

    [SerializeField] private OxygenConfig playerOxygenConfig;

    [Header("UI Settings")]
    [SerializeField]
    private MicroBar oxygenUI;
    [SerializeField]
    private UiManager uiManager;

    private bool _zeroGUIActive;

    public UIState AssociatedState => UIState.ZeroG;

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
